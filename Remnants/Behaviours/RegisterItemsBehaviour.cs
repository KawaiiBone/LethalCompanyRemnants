using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.Netcode;
using Remnants.utilities;
using System.Reflection;

namespace Remnants.Behaviours
{
    internal class RegisterItemsBehaviour
    {
        #region Variables
        private bool _hasInitialized = false;
        private bool _isAddingItems = false;
        private List<string> _bannedItemsNamesList = new List<string>();
        private bool _useLegacySpawning = false;

        private const int _minSellValue = 1, _maxSellValue = 2;
        private const float _minCreditCost = 1f, _toFullCostMod = 2.5f;
        private const float _maxPercentage = 100.0f;
        private RemnantDataListBehaviour _remnantDataListBehaviour = new RemnantDataListBehaviour();
        private List<RemnantData> _remnantItemList = new List<RemnantData>();
        private RemnantItemsBehaviour _remnantItemsBehaviour = null;
        #endregion

        #region Initialize 
        public void Initialize()
        {
            if (!_hasInitialized)
            {
                _hasInitialized = true;
                _bannedItemsNamesList = Remnants.Instance.RemnantsConfig.GetBannedFromRegisteringItemNames();
                _remnantItemList = Remnants.Instance.RemnantsConfig.GetRemnantItemList(false);
                _useLegacySpawning = Remnants.Instance.RemnantsConfig.UseLegacySpawning.Value;
                _remnantItemsBehaviour = Remnants.Instance.RemnantItemsBeh;
                SceneManager.sceneLoaded += StoreItemsRegisterAsScrap;
            }
        }
        #endregion

        #region Methods
        private void StoreItemsRegisterAsScrap(Scene scene, LoadSceneMode mode)
        {
            var mls = Remnants.Instance.Mls;
            if (StartOfRound.Instance != null)
            {
                SceneManager.sceneLoaded -= StoreItemsRegisterAsScrap;
                _remnantDataListBehaviour.UpdateScrapDataList();
                _remnantItemList.Clear();
                return;
            }

            if (_isAddingItems)
                return;

            List<Item> allItems = Resources.FindObjectsOfTypeAll<Item>().Concat(UnityEngine.Object.FindObjectsByType<Item>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)).ToList();
            if (allItems.Count == 0 || allItems == null)
                return;

            mls.LogInfo("Loading in items.");
            _isAddingItems = true;
            AddStoreItemsToScrap(allItems);
            mls.LogInfo("Items loaded in.");
        }

        private void AddStoreItemsToScrap(List<Item> allItems)
        {
            var mls = Remnants.Instance.Mls;
            try
            {
                foreach (Item item in allItems)
                {
                    if (item == null)
                        continue;
  
                    if (HasBannedName(item))
                        continue;

                    if (_useLegacySpawning && IsAlreadyScrap(item))
                        continue;
                    else if (!_useLegacySpawning && IsAlreadyScrapOrRegistered(item))
                        continue;

                    if (IsPrefabIncorrect(item.spawnPrefab))
                    {
                        //mls.LogWarning(item.itemName + ": prefab is incorrect to be registered as scrap.");
                        continue;
                    }

                    int creditsworth = GetItemCreditsWorth(item);
                    if (creditsworth >= _minCreditCost)
                    {
                        int remnantsIndex = _remnantItemList.FindIndex(remnantData => remnantData.RemnantItemName == item.itemName);
                        int itemRarityInfo = -1;
                        if(remnantsIndex != -1)
                            itemRarityInfo = _remnantItemList[remnantsIndex].RarityInfo;

                        RegisterItemAsScrap(item, creditsworth, itemRarityInfo);
                    }
                }
            }
            catch (Exception e)
            {
                mls.LogError(e.ToString());
            }
            _isAddingItems = false;
        }

        private bool HasBannedName(Item item)
        {
            return _bannedItemsNamesList.FindIndex(x => x == item.name || x == item.itemName) != -1;
        }

        private bool IsAlreadyScrap(Item item)
        {
            return item.isScrap || Items.scrapItems.FindIndex(scrapItem => scrapItem.item.itemName == item.itemName || scrapItem.origItem.itemName == item.itemName) != -1;
        }

        private bool IsAlreadyScrapOrRegistered(Item item)
        {
            return item.isScrap || _remnantItemsBehaviour.NetworkRemnantItems.FindIndex(remnantItem => remnantItem.item.itemName == item.itemName
            || remnantItem.origItem.itemName == item.itemName) != -1;
        }

        private bool IsPrefabIncorrect(GameObject gameObject)
        {
            return gameObject == null || gameObject.GetComponent<NetworkObject>() == null;
        }

        private int GetItemCreditsWorth(Item item)
        {
            int shopItemsIndex = Items.shopItems.FindIndex(shopItem => shopItem.origItem.itemName == item.itemName);
            if (shopItemsIndex != -1)
              return Items.shopItems[shopItemsIndex].price;
            else
                return item.creditsWorth;
        }

        private void RegisterItemAsScrap(Item item, int creditsWorth, int itenRarityInfo)
        {
            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Registering " + item.itemName + " as scrap.");
            float creditWorthMinPercentage = (float)Remnants.Instance.RemnantsConfig.RemnantScrapMinCostPercentage.Value / _maxPercentage;
            float creditWorthMaxPercentage = (float)Remnants.Instance.RemnantsConfig.RemnantScrapMaxCostPercentage.Value / _maxPercentage;
            if(_useLegacySpawning)
            {
                item.minValue = Mathf.Clamp((int)(creditsWorth * _toFullCostMod * creditWorthMinPercentage), _minSellValue, int.MaxValue);
                item.maxValue = Mathf.Clamp((int)(creditsWorth * _toFullCostMod * creditWorthMaxPercentage), _maxSellValue, int.MaxValue);
            }
            item.itemSpawnsOnGround = true;
            LethalLib.Modules.Utilities.FixMixerGroups(item.spawnPrefab);
            GrabbableObject grabbable = item.spawnPrefab.GetComponentInChildren<GrabbableObject>();
            if (grabbable != null)
                grabbable.isInFactory = true;

            bool useRarityByCredits = itenRarityInfo == -1 || itenRarityInfo == 0;

            if (Remnants.Instance.RemnantsConfig.UseSpecificLevelRarities.Value == false)
            {
                int rarity = 0;
                if (useRarityByCredits)
                    rarity = CalculateRarityByCredits(creditsWorth, Remnants.Instance.RemnantsConfig.MinRemnantRarity.Value, Remnants.Instance.RemnantsConfig.MaxRemnantRarity.Value);
                else
                    rarity = CalculateRarityOfItem(itenRarityInfo, Remnants.Instance.RemnantsConfig.MinRemnantRarity.Value, Remnants.Instance.RemnantsConfig.MaxRemnantRarity.Value);
                //Use new spawning version via legacy or by hand
                if (_useLegacySpawning)
                {
                    Items.RegisterScrap(item, rarity, Levels.LevelTypes.All);
                }
                else
                {
                    var scrapItem = new Items.ScrapItem(item, rarity, Levels.LevelTypes.All);
                    string name = Assembly.GetCallingAssembly().GetName().Name;
                    scrapItem.modName = name;
                    scrapItem.item.minValue = Mathf.Clamp((int)(creditsWorth /** _toFullCostMod*/ * creditWorthMinPercentage), 0, int.MaxValue);
                    scrapItem.item.maxValue = Mathf.Clamp((int)(creditsWorth /** _toFullCostMod*/ * creditWorthMaxPercentage), 0, int.MaxValue);
                    _remnantItemsBehaviour.AddNetworkRemnantItem(scrapItem);
                }
            }
            else
            {
                Dictionary<Levels.LevelTypes, int> levelRarities = new Dictionary<Levels.LevelTypes, int>();
                foreach (var levelRarity in Remnants.Instance.RemnantsConfig.LevelRarities)
                {
                    if (useRarityByCredits)
                        levelRarities.Add(levelRarity.Key, CalculateRarityByCredits(creditsWorth, levelRarity.Value.Item1, levelRarity.Value.Item2));
                    else
                        levelRarities.Add(levelRarity.Key, CalculateRarityOfItem(itenRarityInfo, levelRarity.Value.Item1, levelRarity.Value.Item2));
                }

                Dictionary<string, int> customLevelRarities = new Dictionary<string, int>();
                foreach (var customLevelRarity in Remnants.Instance.RemnantsConfig.CustomLevelRarities)
                {
                    if (useRarityByCredits)
                        customLevelRarities.Add(customLevelRarity.Key, CalculateRarityByCredits(creditsWorth, customLevelRarity.Value.Item1, customLevelRarity.Value.Item2));
                    else
                        customLevelRarities.Add(customLevelRarity.Key, CalculateRarityOfItem(itenRarityInfo, customLevelRarity.Value.Item1, customLevelRarity.Value.Item2));
                }
                //Use new spawning version via legacy or by hand
                if (_useLegacySpawning)
                {
                    Items.RegisterScrap(item, levelRarities, customLevelRarities);
                }
                else
                {
                    var scrapItem = new Items.ScrapItem(item, levelRarities, customLevelRarities);
                    string name = Assembly.GetCallingAssembly().GetName().Name;
                    scrapItem.modName = name;
                    scrapItem.item.minValue = Mathf.Clamp((int)(creditsWorth /** _toFullCostMod*/ * creditWorthMinPercentage), 0, int.MaxValue);
                    scrapItem.item.maxValue = Mathf.Clamp((int)(creditsWorth /** _toFullCostMod*/ * creditWorthMaxPercentage), 0, int.MaxValue);
                    _remnantItemsBehaviour.AddNetworkRemnantItem(scrapItem);
                }
            }
            mls.LogInfo("Added " + item.itemName + " as a scrap item.");
            _remnantDataListBehaviour.AddItemToDataList(item.itemName);
        }

        private int CalculateRarityByCredits(int itemCreditWorth, int minStoreScrapRarity, int maxStoreScrapRarity)
        {
            float maxCreditValue = Remnants.Instance.RemnantsConfig.MaxRemnantItemCost.Value;
            float creditCapped = Mathf.Clamp(itemCreditWorth, _minCreditCost, maxCreditValue);
            float rarityPercentage = Mathf.Abs(((creditCapped / maxCreditValue) * maxStoreScrapRarity) - maxStoreScrapRarity);
            return Mathf.Clamp((int)rarityPercentage, minStoreScrapRarity, maxStoreScrapRarity);
        }

        private int CalculateRarityOfItem(int itemRarity, int minRarity, int maxRarity)
        {
            int diffRarity = maxRarity - minRarity;
            int calculatedRarity = (int)(((float)diffRarity / _maxPercentage) * itemRarity) + minRarity;
            return calculatedRarity;
        }
        #endregion
    }
}