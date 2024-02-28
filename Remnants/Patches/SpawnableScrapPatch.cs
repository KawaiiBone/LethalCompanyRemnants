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
        private static float _delayFunction = 11.0f;
        #endregion

        #region Methods
        #endregion

        #region HarmonyMethods
        [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
        [HarmonyPrefix]
        public static void SpawnScrapInLevelPatch(RoundManager __instance)
        {
            //
            var mls = Remnants.Instance._mls;
            mls.LogInfo("In SpawnScrapInLevelPatch function!");

            //IL_035a: Unknown result type (might be due to invalid IL or missing references)
            int num = (int)((float)__instance.AnomalyRandom.Next(__instance.currentLevel.minScrap, __instance.currentLevel.maxScrap) * __instance.scrapAmountMultiplier);
            if (StartOfRound.Instance.isChallengeFile)
            {
                int num2 = __instance.AnomalyRandom.Next(10, 30);
                num += num2;
                Debug.Log($"Anomaly random 0b: {num2}");
            }

            List<Item> ScrapToSpawn = new List<Item>();
            List<int> list = new List<int>();
            int num3 = 0;

            //Here we will delete all items that are banned, currently replaced by itemsscrap for easy of testing
            List<SpawnableItemWithRarity> spawnableScrapList = __instance.currentLevel.spawnableScrap;
            List<ScrapItemData> scrapItemDataList = ScrapDataListBehaviour.GetScrapItemDataList();
            spawnableScrapList.RemoveAll(spawnableItem => scrapItemDataList.FindIndex(itemData => itemData.Isbanned && itemData.ScrapItemName == spawnableItem.spawnableItem.name) != -1);


            List<int> list2 = new List<int>(spawnableScrapList.Count);
            for (int j = 0; j < spawnableScrapList.Count; j++)
            {
                if (j == __instance.increasedScrapSpawnRateIndex)
                {
                    list2.Add(100);
                }
                else
                {
                    list2.Add(spawnableScrapList[j].rarity);
                }
            }

            int[] weights = list2.ToArray();
            for (int k = 0; k < num; k++)
            {
                ScrapToSpawn.Add(spawnableScrapList[__instance.GetRandomWeightedIndex(weights)].spawnableItem);
            }

            Debug.Log($"Number of scrap to spawn: {ScrapToSpawn.Count}. minTotalScrapValue: {__instance.currentLevel.minTotalScrapValue}. Total value of items: {num3}.");
            RandomScrapSpawn randomScrapSpawn = null;
            RandomScrapSpawn[] source = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>();
            List<NetworkObjectReference> list3 = new List<NetworkObjectReference>();
            List<RandomScrapSpawn> usedSpawns = new List<RandomScrapSpawn>();
            int i;
            for (i = 0; i < ScrapToSpawn.Count; i++)
            {
                if (ScrapToSpawn[i] == null)
                {
                    Debug.Log("Error!!!!! Found null element in list ScrapToSpawn. Skipping it.");
                    continue;
                }

                List<RandomScrapSpawn> list4 = ((ScrapToSpawn[i].spawnPositionTypes != null && ScrapToSpawn[i].spawnPositionTypes.Count != 0) ? source.Where((RandomScrapSpawn x) => ScrapToSpawn[i].spawnPositionTypes.Contains(x.spawnableItems) && !x.spawnUsed).ToList() : source.ToList());
                if (list4.Count <= 0)
                {
                    Debug.Log("No tiles containing a scrap spawn with item type: " + ScrapToSpawn[i].itemName);
                    continue;
                }

                if (usedSpawns.Count > 0 && list4.Contains(randomScrapSpawn))
                {
                    list4.RemoveAll((RandomScrapSpawn x) => usedSpawns.Contains(x));
                    if (list4.Count <= 0)
                    {
                        usedSpawns.Clear();
                        i--;
                        continue;
                    }
                }

                randomScrapSpawn = list4[__instance.AnomalyRandom.Next(0, list4.Count)];
                usedSpawns.Add(randomScrapSpawn);
                Vector3 position;
                if (randomScrapSpawn.spawnedItemsCopyPosition)
                {
                    randomScrapSpawn.spawnUsed = true;
                    position = randomScrapSpawn.transform.position;
                }
                else
                {

                    position = __instance.GetRandomNavMeshPositionInBoxPredictable(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, __instance.navHit, __instance.AnomalyRandom) + Vector3.up * ScrapToSpawn[i].verticalOffset;
                }

                GameObject obj = UnityEngine.Object.Instantiate(ScrapToSpawn[i].spawnPrefab, position, Quaternion.identity, __instance.spawnedScrapContainer);
                GrabbableObject component = obj.GetComponent<GrabbableObject>();
                component.transform.rotation = Quaternion.Euler(component.itemProperties.restingRotation);
                component.fallTime = 0f;
                list.Add((int)((float)__instance.AnomalyRandom.Next(ScrapToSpawn[i].minValue, ScrapToSpawn[i].maxValue) * __instance.scrapValueMultiplier));
                num3 += list[list.Count - 1];
                component.scrapValue = list[list.Count - 1];
                NetworkObject component2 = obj.GetComponent<NetworkObject>();
                component2.Spawn();
                list3.Add(component2);
            }
            CoroutineHelper coroutineHelper = CoroutineHelper.Instance;
            if (coroutineHelper == null)
            {
                var obj = new GameObject("CoroutineHelper");
                coroutineHelper = obj.AddComponent<CoroutineHelper>();
            }
            object[] argument = new object[2] { list3.ToArray(), list.ToArray() };
            coroutineHelper.ExecuteAfterDelay(() =>
            {
                // The function you want to call after 11 seconds
                Traverse.Create<RoundManager>().Method("SyncScrapValuesClientRpc", argument);
            }, _delayFunction);
            return;
        }
        #endregion
    }
}
