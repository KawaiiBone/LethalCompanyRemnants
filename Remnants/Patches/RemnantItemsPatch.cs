using HarmonyLib;
using Remnants.utilities;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Remnants.Patches
{
    internal class RemnantItemsPatch
    {
        #region HarmonyMethods
        [HarmonyPatch(typeof(RoundManager), "waitForScrapToSpawnToSync")]
        [HarmonyPostfix]
        static void PatchRemnantItems(object[] __args)
        {
            var mls = Remnants.Instance.Mls;
            NetworkObjectReference[] spawnedScrap = (NetworkObjectReference[])__args[0];
            mls.LogInfo("Patching remnant items.");
            if (spawnedScrap == null)
            {
                mls.LogWarning("spawnedScrap IS NULL");
                return;
            }
            
            var remnantItemsBehaviour = Remnants.Instance.RemnantItemsBeh;
            var itemsBatteriesBeh = Remnants.Instance.ItemsBatteriesBeh;
            var spawnBodiesBeh = Remnants.Instance.SpawningBodyBeh;
            List<GameObject> newRemnantItemsList = new List<GameObject>();
            List<RemnantData> scrapItemDataList = Remnants.Instance.RemnantsConfig.GetRemnantItemList();
            List<string> itemScrapList = Remnants.Instance.RemnantsConfig.GetOverriddenScrapItems();
    
            for (int i = 0; i < spawnedScrap.Length; i++)
            {
                if (!spawnedScrap[i].TryGet(out var networkObject))
                    continue;

                GrabbableObject grabbableObject = networkObject.GetComponent<GrabbableObject>();
                if (grabbableObject == null)
                    continue;

                if (itemScrapList.FindIndex(scrapName => scrapName == grabbableObject.itemProperties.itemName || scrapName == grabbableObject.itemProperties.name) != -1)
                    newRemnantItemsList.Add(grabbableObject.gameObject);
                else if (scrapItemDataList.FindIndex(itemData => itemData.RemnantItemName == grabbableObject.itemProperties.itemName || itemData.RemnantItemName == grabbableObject.itemProperties.name) == -1)
                    continue;
                else
                {
                    newRemnantItemsList.Add(grabbableObject.gameObject);
                }
            }
            itemsBatteriesBeh.RandomizeItemsBattery(newRemnantItemsList);
            spawnBodiesBeh.SpawnBodiesOnItems(newRemnantItemsList);
       
        }
        #endregion
    }
}
