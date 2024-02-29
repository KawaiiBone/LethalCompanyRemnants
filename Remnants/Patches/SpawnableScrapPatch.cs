using HarmonyLib;
using LethalLib.Modules;
using Remnants.Behaviours;
using Remnants.utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Remnants.Patches
{
    internal class SpawnableScrapPatch
    {

        #region Variables
        #endregion

        #region Methods
        #endregion

        #region HarmonyMethods
        [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
        [HarmonyPrefix]
        public static void SpawnScrapInLevelPatch(RoundManager __instance)
        {
            var mls = Remnants.Instance.Mls;
            mls.LogInfo("In SpawnScrapInLevelPatch function!");
            //Here we will delete all items that are banned, currently replaced by itemsscrap for easy of testing
            List<SpawnableItemWithRarity> spawnableScrapList = __instance.currentLevel.spawnableScrap;
            List<RemnantData> scrapItemDataList = RemnantDataListBehaviour.GetRemnantDataList();
            spawnableScrapList.RemoveAll(spawnableItem => scrapItemDataList.FindIndex(itemData => !itemData.ShouldSpawn && itemData.RemnantItemName == spawnableItem.spawnableItem.name) != -1);
            __instance.currentLevel.spawnableScrap = spawnableScrapList;
        }
        #endregion
    }
}
