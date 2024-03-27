using HarmonyLib;
using Remnants.Behaviours;
using Remnants.Data;
using Remnants.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Remnants.Patches
{
    internal class SpawnBodiesPatch
    {
        #region HarmonyMethods
        [HarmonyPatch(typeof(RoundManager), "waitForScrapToSpawnToSync")]
        [HarmonyPostfix]
        static void SpawnBodiesOnRemnantItemsPatch(object[] __args, RoundManager __instance)
        {
            var mls = Remnants.Instance.Mls;
            NetworkObjectReference[] spawnedScrap = (NetworkObjectReference[])__args[0];
            mls.LogInfo("Patching Spawn Bodies On Remnant Items.");
            if (spawnedScrap == null)
            {
                mls.LogInfo("spawnedScrap IS NULL");
                return;
            }

            if (!LoadAssetsBodies.HasLoadedAnyAssets || Data.Config.SpawnRarityOfBody.Value == 0)
                return;

            var prefabAndRarityList = CreatePrefabAndRarityList();
            int totalRarityValue = CalculateTotalRarityValue(prefabAndRarityList);
            List<RemnantData> scrapItemDataList = Data.Config.GetRemnantItemList();
            float spawnChance = CalculateSpawnChance();
            System.Random random = new System.Random();
            int maxPercentage = 101;
            bool willSpawnBody = false;
            List<NetworkObjectReference> NetworkObjectReferenceList = new List<NetworkObjectReference>();
            List<int> scrapValueList = new List<int>();
            for (int i = 0; i < spawnedScrap.Length; i++)
            {
                if (!willSpawnBody)
                    willSpawnBody = random.Next(maxPercentage) <= spawnChance;

                if (!willSpawnBody)
                    continue;

                if (!spawnedScrap[i].TryGet(out var networkObject))
                    continue;

                GrabbableObject grabbableObject = networkObject.GetComponent<GrabbableObject>();
                if (grabbableObject == null)
                    continue;

                if (scrapItemDataList.FindIndex(itemData => itemData.RemnantItemName == grabbableObject.itemProperties.name) == -1)
                    continue;

                Vector3 spawnPosition = CalculateNavSpawnPosition(grabbableObject.transform.position);
                if (spawnPosition == Vector3.zero)
                {
                    mls.LogWarning("Did not found place to spawn body, skipping it.");
                    continue;
                }

                int bodyIndex = GetRandomBodyIndex(prefabAndRarityList, totalRarityValue, random);
                if (Data.Config.ShouldBodiesBeScrap.Value == false)
                {
                    SpawnBody(prefabAndRarityList[bodyIndex].Key, spawnPosition);
                }
                else
                {
                    NetworkObjectReferenceList.Add(SpawnScrapBody(prefabAndRarityList[bodyIndex].Key, spawnPosition, __instance.spawnedScrapContainer));
                    scrapValueList.Add(Config.BodyScrapValue.Value);
                }
                willSpawnBody = false;
            }

            if (NetworkObjectReferenceList.Count == 0)
                return;

            //Here do courotine for sync scrap
            var couroutine = CoroutineHelper.Instance;
            if (couroutine == null)
            {
                couroutine = new GameObject().AddComponent<CoroutineHelper>();
            }

            couroutine.ExecuteAfterDelay(() =>
            {
                __instance.SyncScrapValuesClientRpc(NetworkObjectReferenceList.ToArray(), scrapValueList.ToArray());
            }
            , 11.0f);
        }
        #endregion
        #region Methods
        private static void SpawnBody(GameObject prefab, Vector3 spawnPosition)
        {
            spawnPosition.y = spawnPosition.y + 1.0f;
            GameObject defaultBody = UnityEngine.Object.Instantiate(prefab, spawnPosition, UnityEngine.Random.rotation, RoundManager.Instance.mapPropsContainer.transform);
            defaultBody.GetComponent<NetworkObject>().Spawn(true);
        }

        private static NetworkObject SpawnScrapBody(GameObject prefab, Vector3 spawnPosition, Transform parent)
        {
            spawnPosition.y = spawnPosition.y + 1.0f;
            GameObject defaultBody = UnityEngine.Object.Instantiate(prefab, spawnPosition, UnityEngine.Random.rotation, parent);
            defaultBody.GetComponent<NetworkObject>().Spawn();
            return defaultBody.GetComponent<NetworkObject>();
        }

        private static Vector3 CalculateNavSpawnPosition(Vector3 grabObjPos)
        {
            var mls = Remnants.Instance.Mls;
            float minDistance = 0.1f;
            float mediumDistance = 5.0f;
            float maxDistance = 6.0f;
            float moveDistance = 1.0f;
            int areaMask = -1;
            if (NavMesh.SamplePosition(grabObjPos, out NavMeshHit navOldHit, minDistance, areaMask))
            {
                if (Vector3.Distance(navOldHit.position, grabObjPos) < minDistance)
                {
                    mls.LogInfo("Already is on navmesh, spawning body on remnant item.");
                    return grabObjPos;
                }
            }

            if (NavMesh.SamplePosition(grabObjPos, out NavMeshHit navHit, maxDistance, areaMask))
            {
                Vector3 navHitYFlat = navHit.position;
                navHitYFlat.y = grabObjPos.y;
                Vector3 heading = navHitYFlat - grabObjPos;
                float distance = heading.magnitude;
                Vector3 direction = heading / distance;
                Vector3 position = navHit.position + (direction * moveDistance);
                if (NavMesh.SamplePosition(position, out NavMeshHit navNewHit, mediumDistance, areaMask))
                {
                    mls.LogInfo("Calculated and found position on navmesh, spawning body.");
                    return navNewHit.position;
                }
                else
                {
                    mls.LogInfo("Not found calculated position on navmesh using previous position, spawning body.");
                    return navHit.position;
                }
            }
            else
            {
                mls.LogWarning("No location found on navmesh, not spawning body.");
                return Vector3.zero;
            }
        }

        private static float CalculateSpawnChance()
        {
            float spawnChance = Data.Config.SpawnRarityOfBody.Value;
            float spawnBodyModifier = Data.Config.SpawnModifierRiskLevel.Value;
            string[] riskLevelArray = { "Safe", "D", "C", "B", "A", "S", "S+" };
            int riskLevel = Array.IndexOf(riskLevelArray, StartOfRound.Instance.currentLevel.riskLevel);
            if (!Mathf.Approximately(spawnBodyModifier, 0.0f) && riskLevel != -1)
                spawnChance *= (riskLevel * spawnBodyModifier);

            return spawnChance;
        }

        private static int GetRandomBodyIndex(List<KeyValuePair<GameObject, int>> prefabAndRarityList, int totalRarityValue, System.Random random)
        {
            int randomNumber = random.Next(totalRarityValue);
            int totalValue = 0;
            for (int i = 0; i < prefabAndRarityList.Count; ++i)
            {
                totalValue += prefabAndRarityList[i].Value;
                if (totalValue > randomNumber)
                    return i;
            }
            return 0;
        }

        private static int CalculateTotalRarityValue(List<KeyValuePair<GameObject, int>> prefabAndRarityList)
        {
            int totalRarityValue = 0;
            foreach (var prefabRarity in prefabAndRarityList)
            {
                totalRarityValue += prefabRarity.Value;
            }
            return totalRarityValue;
        }

        private static List<KeyValuePair<GameObject, int>> CreatePrefabAndRarityList()
        {
            SelectableLevel currentLevel = StartOfRound.Instance.currentLevel;
            string planetName = currentLevel.PlanetName;
            if (RegisterBodiesSpawnBehaviour.HasIllegalCharacters(currentLevel.PlanetName))
                planetName = RegisterBodiesSpawnBehaviour.PlanetsBodiesRarities.First().Key;

            if (!RegisterBodiesSpawnBehaviour.PlanetsBodiesRarities.ContainsKey(planetName))
                RegisterBodiesSpawnBehaviour.RegisterBodiesToNewMoon(currentLevel);

            Dictionary<string, int> bodiesArray = RegisterBodiesSpawnBehaviour.PlanetsBodiesRarities[planetName];
            IReadOnlyList<NetworkPrefab> prefabs = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs;

            List<NetworkPrefab> bodyPrefabs = null;
            if (Data.Config.ShouldBodiesBeScrap.Value == false)
            {
                string propName = "Prop";
                bodyPrefabs = prefabs.Where(netObj => bodiesArray.ToList().FindIndex(name => (name.Key + propName) == netObj.Prefab.name) != -1).ToList();
                return bodyPrefabs.ConvertAll(netPrefab =>
                new KeyValuePair<GameObject, int>(
                netPrefab.Prefab,
                bodiesArray[netPrefab.Prefab.name.Substring(0,netPrefab.Prefab.name.Length - propName.Length)]
                ));
            }
            else
            {
                bodyPrefabs = prefabs.Where(netObj => bodiesArray.ContainsKey(netObj.Prefab.name)).ToList();
                return bodyPrefabs.ConvertAll(netPrefab =>
                new KeyValuePair<GameObject, int>(
                netPrefab.Prefab,
                bodiesArray[netPrefab.Prefab.name]
                ));
            }


        }
        #endregion
    }
}
