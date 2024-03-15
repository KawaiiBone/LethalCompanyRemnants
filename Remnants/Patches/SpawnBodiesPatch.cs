using HarmonyLib;
using Remnants.Behaviours;
using Remnants.Data;
using Remnants.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Remnants.Patches
{
    internal class SpawnBodiesPatch
    {
        #region HarmonyMethods
        [HarmonyPatch(typeof(RoundManager), "waitForScrapToSpawnToSync")]
        [HarmonyPostfix]
        static void SpawnBodiesOnRemnantItemsPatch(object[] __args)
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

            Dictionary<string, int> bodiesArray = null;
            if (!RegisterBodiesSpawnRarities.PlanetsBodiesRarities.ContainsKey(StartOfRound.Instance.currentLevel.PlanetName))        
                RegisterBodiesSpawnRarities.RegisterBodiesToNewMoon(StartOfRound.Instance.currentLevel);

            bodiesArray = RegisterBodiesSpawnRarities.PlanetsBodiesRarities[StartOfRound.Instance.currentLevel.PlanetName];
            IReadOnlyList<NetworkPrefab> prefabs = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs;
            var bodyPrefabs = prefabs.Where(netObj => bodiesArray.ContainsKey(netObj.Prefab.name)).ToList();
            List<KeyValuePair<GameObject, int>> prefabAndRarityList = bodyPrefabs.ConvertAll(netPrefab =>
            new KeyValuePair<GameObject, int>(
                netPrefab.Prefab,
                bodiesArray[netPrefab.Prefab.name]
                ));
            int totalRarityValue = CalculateTotalRarityValue(prefabAndRarityList);
            List<RemnantData> scrapItemDataList = Data.Config.GetRemnantItemList();
            float spawnChance = CalculateSpawnChance();
            System.Random random = new System.Random();
            int maxPercentage = 101;
            bool willSpawnBody = false;
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

                int bodyIndex = GetRandomBodyIndex(prefabAndRarityList, totalRarityValue, random);
                SpawnBody(prefabAndRarityList[bodyIndex].Key, grabbableObject.transform.position);
                willSpawnBody = false;
            }
        }


        private static void SpawnBody(GameObject prefab, Vector3 spawnPosition)
        {
            spawnPosition.y = spawnPosition.y + 1.0f;
            GameObject defaultBody = UnityEngine.Object.Instantiate(prefab, spawnPosition, UnityEngine.Random.rotation, RoundManager.Instance.mapPropsContainer.transform);
            defaultBody.GetComponent<NetworkObject>().Spawn(true);
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
        #endregion

    }
}
