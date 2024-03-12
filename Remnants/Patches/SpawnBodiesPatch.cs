using HarmonyLib;
using Remnants.Data;
using Remnants.utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            if (LoadAssetsBodies.BodiesFileNamesArray == null || LoadAssetsBodies.BodiesFileNamesArray.Length == 0)
                return;

            IReadOnlyList<NetworkPrefab> prefabs = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs;
            NetworkPrefab[] bodyPrefabs = prefabs.ToList().Where(netObj => Array.FindIndex(LoadAssetsBodies.BodiesFileNamesArray , nameFile => nameFile == netObj.Prefab.name) != -1 ).ToArray();
            List<RemnantData> scrapItemDataList = Data.Config.GetRemnantItemList();
            System.Random random = new System.Random();
            int maxPercentage = 101;
            int spawnChance = Data.Config.SpawnRarityOfBody.Value;
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

                Vector3 spawnPosition = grabbableObject.transform.position;
                spawnPosition.y = spawnPosition.y + 1.0f;
                GameObject defaultBody = UnityEngine.Object.Instantiate(bodyPrefabs[random.Next(bodyPrefabs.Length)].Prefab, spawnPosition, UnityEngine.Random.rotation , RoundManager.Instance.mapPropsContainer.transform);
                defaultBody.GetComponent<NetworkObject>().Spawn(true);
                willSpawnBody = false;
            }
        }
        #endregion

    }
}
