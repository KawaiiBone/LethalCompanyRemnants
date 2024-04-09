using HarmonyLib;
using Remnants.Behaviours;
using Remnants.Data;
using Remnants.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Remnants.Patches
{
    internal class SpawnBodiesPatch
    {
        #region Variables 
        private static float _courotineDelayTAmount = 11.0f;//same length as in the game
        private static string _propName = "Prop";
        #endregion

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

            if (!Remnants.Instance.LoadBodyAssets.HasLoadedAnyAssets || Remnants.Instance.RemnantsConfig.SpawnRarityOfBody.Value == 0)
                return;

            var prefabAndRarityList = CreatePrefabAndRarityList();
            SpawnBodyBehaviour SpawningBody = Remnants.Instance.SpawningBody;
            int totalRarityValue = SpawningBody.CalculateTotalRarityValue(prefabAndRarityList);
            float spawnChance = SpawningBody.CalculateSpawnChance(StartOfRound.Instance.currentLevel.riskLevel);
            List<RemnantData> scrapItemDataList = Remnants.Instance.RemnantsConfig.GetRemnantItemList();
            System.Random random = new System.Random();
            int maxPercentage = 101;
            bool willSpawnBody = false;
            List<int> indexList = Remnants.Instance.RegisterBodySuits.SuitsIndexList;
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

                if (!SpawningBody.CalculatePositionOnNavMesh(grabbableObject.transform.position, out Vector3 spawnPosition))
                {
                    mls.LogWarning("Did not found place to spawn body, skipping it.");
                    continue;
                }

                NetworkObject netBodyObject = null;
                int bodyIndex = SpawningBody.GetRandomBodyIndex(prefabAndRarityList, random.Next(totalRarityValue));
                if (Remnants.Instance.RemnantsConfig.ShouldBodiesBeScrap.Value == false)
                {
                    netBodyObject = SpawnBody(prefabAndRarityList[bodyIndex].Key, spawnPosition);
                }
                else
                {
                    netBodyObject = SpawnScrapBody(prefabAndRarityList[bodyIndex].Key, spawnPosition, __instance.spawnedScrapContainer);
                    NetworkObjectReferenceList.Add(netBodyObject);
                    scrapValueList.Add(Remnants.Instance.RemnantsConfig.BodyScrapValue.Value);
                }
               
                if(indexList.Count != 0)
                {
                    int suitIndex = random.Next(indexList.Count);
                    netBodyObject.GetComponent<BodyGrabbableObject>().SyncIndexSuitServerRpc(suitIndex);
                }
                willSpawnBody = false;
            }

            if (NetworkObjectReferenceList.Count == 0)
                return;

            //Here do courotine for sync scrap
            var couroutine = CoroutineHelper.Instance;
            if (couroutine == null)
                couroutine = new GameObject().AddComponent<CoroutineHelper>();

            couroutine.ExecuteAfterDelay(() =>
            {
                __instance.SyncScrapValuesClientRpc(NetworkObjectReferenceList.ToArray(), scrapValueList.ToArray());
            }
            , _courotineDelayTAmount);
        }
        #endregion
        #region Methods
        private static NetworkObject SpawnBody(GameObject prefab, Vector3 spawnPosition)
        {
            GameObject defaultBody = UnityEngine.Object.Instantiate(prefab, spawnPosition, UnityEngine.Random.rotation, RoundManager.Instance.mapPropsContainer.transform);
            NetworkObject netObject = defaultBody.GetComponent<NetworkObject>();
            netObject.Spawn(true);
            return netObject;
        }

        private static NetworkObject SpawnScrapBody(GameObject prefab, Vector3 spawnPosition, Transform parent)
        {
            GameObject defaultBody = UnityEngine.Object.Instantiate(prefab, spawnPosition, UnityEngine.Random.rotation, parent);
            NetworkObject netObject = defaultBody.GetComponent<NetworkObject>();
            netObject.Spawn();
            return netObject;
        }

        private static List<KeyValuePair<GameObject, int>> CreatePrefabAndRarityList()
        {
            RegisterBodiesSpawnBehaviour registerBodiesSpawn = Remnants.Instance.RegisterBodiesSpawn;
            SelectableLevel currentLevel = StartOfRound.Instance.currentLevel;
            string planetName = currentLevel.PlanetName;
            if (registerBodiesSpawn.HasIllegalCharacters(currentLevel.PlanetName))
                planetName = registerBodiesSpawn.PlanetsBodiesRarities.First().Key;

            if (!registerBodiesSpawn.PlanetsBodiesRarities.ContainsKey(planetName))
                registerBodiesSpawn.RegisterBodiesToNewMoon(currentLevel);

            Dictionary<string, int> bodiesArray = registerBodiesSpawn.PlanetsBodiesRarities[planetName];
            IReadOnlyList<NetworkPrefab> prefabs = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs;

            List<NetworkPrefab> bodyPrefabs = null;
            if (Remnants.Instance.RemnantsConfig.ShouldBodiesBeScrap.Value == false)
            {
                bodyPrefabs = prefabs.Where(netObj => bodiesArray.ToList().FindIndex(name => (name.Key + _propName) == netObj.Prefab.name) != -1).ToList();
                return bodyPrefabs.ConvertAll(netPrefab =>
                new KeyValuePair<GameObject, int>(
                netPrefab.Prefab,
                bodiesArray[netPrefab.Prefab.name.Substring(0, netPrefab.Prefab.name.Length - _propName.Length)]
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
