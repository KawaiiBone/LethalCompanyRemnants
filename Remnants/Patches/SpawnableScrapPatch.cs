using HarmonyLib;
using Remnants.utilities;
using System.Collections.Generic;

namespace Remnants.Patches
{
    internal class SpawnableScrapPatch
    {
        #region HarmonyMethods
        [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
        [HarmonyPrefix]
        public static void SpawnScrapInLevelPatch(RoundManager __instance)
        {
            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Patching remntant items spawns.");
            //Here we will delete all items that are banned
            List<SpawnableItemWithRarity> spawnableScrapList = __instance.currentLevel.spawnableScrap;
            List<RemnantData> scrapItemDataList = Remnants.Instance.RemnantsConfig.GetRemnantItemList();
            spawnableScrapList.RemoveAll(spawnableItem => scrapItemDataList.FindIndex(itemData => !itemData.ShouldSpawn && itemData.RemnantItemName == spawnableItem.spawnableItem.name) != -1);
            __instance.currentLevel.spawnableScrap = spawnableScrapList;
        }
        #endregion
    }
}
