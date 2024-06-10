using HarmonyLib;
using Remnants.utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Remnants.Patches
{
    internal class SpawnableScrapPatch
    {
        #region Variables
        private static int _currentLevelMinScrap = 0;
        private static int _currentLevelMaxScrap = 0;
        private static List<SpawnableItemWithRarity> _removedSpawnableItems = new List<SpawnableItemWithRarity>();
        #endregion

        #region HarmonyMethods
        [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
        [HarmonyPrefix]
        public static void SpawnScrapInLevelPatch(RoundManager __instance)
        {
            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Patching remnant items spawns.");
            //Here we will delete all items that are banned //Should only be temporary
            List<SpawnableItemWithRarity> spawnableScrapList = __instance.currentLevel.spawnableScrap;
            List<RemnantData> scrapItemDataList = Remnants.Instance.RemnantsConfig.GetRemnantItemList();
            _removedSpawnableItems = spawnableScrapList.Where(spawnableItem => scrapItemDataList.FindIndex(itemData => itemData.RarityInfo == 0 && itemData.RemnantItemName == spawnableItem.spawnableItem.itemName) != -1).ToList();
            spawnableScrapList.RemoveAll(spawnableItem => _removedSpawnableItems.Contains(spawnableItem));
            __instance.currentLevel.spawnableScrap = spawnableScrapList;
            //Increase pool size
            if (Remnants.Instance.RemnantsConfig.IncreasedScrapSpawnPool.Value <= 0 || Remnants.Instance.RemnantsConfig.UseLegacySpawning.Value == false)
                return;

            _currentLevelMinScrap = __instance.currentLevel.minScrap;
            _currentLevelMaxScrap = __instance.currentLevel.maxScrap;
            float poolSizeIncrease = 1;
            float poolSizeModifier = 100.0f / (float)Remnants.Instance.RemnantsConfig.IncreasedScrapSpawnPool.Value;
            if (Remnants.Instance.RemnantsConfig.UseSpecificLevelRarities.Value == false)
            {
                poolSizeIncrease = (float)Remnants.Instance.RemnantsConfig.MaxRemnantRarity.Value / poolSizeModifier;
            }
            else if (Remnants.Instance.RemnantsConfig.CustomLevelRarities.ContainsKey(__instance.currentLevel.PlanetName))
            {
                poolSizeIncrease = (float)Remnants.Instance.RemnantsConfig.CustomLevelRarities[__instance.currentLevel.PlanetName].Item2 / poolSizeModifier;
            }
            else
            {
                foreach (var moonRarity in Remnants.Instance.RemnantsConfig.LevelRarities)
                {
                    string moonName = __instance.currentLevel.PlanetName.Split(' ').Last();
                    if(moonRarity.Key.ToString().Contains(moonName))
                    {
                        poolSizeIncrease = (float)moonRarity.Value.Item2 / poolSizeModifier;
                        break;
                    }
                }
            }
            poolSizeIncrease = Mathf.Clamp(poolSizeIncrease, 1.0f, Remnants.Instance.RemnantsConfig.IncreasedScrapSpawnPool.Value);
            __instance.currentLevel.minScrap += (int)(poolSizeIncrease);
            __instance.currentLevel.maxScrap += (int)(poolSizeIncrease);
        }

        [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
        [HarmonyPostfix]
        public static void SpawnScrapInLevelEndPatch(RoundManager __instance)
        {
            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Patching end of Spawn Scrap.");
            //Reset spawnable list
            __instance.currentLevel.spawnableScrap.AddRange(_removedSpawnableItems);
            _removedSpawnableItems.Clear();

            //Reset pool size
            if (Remnants.Instance.RemnantsConfig.IncreasedScrapSpawnPool.Value <= 0 || Remnants.Instance.RemnantsConfig.UseLegacySpawning.Value == false)
                return;
            __instance.currentLevel.minScrap = _currentLevelMinScrap;
            __instance.currentLevel.maxScrap = _currentLevelMaxScrap;
        }
        #endregion
    }
}
