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

        private const int _minSellValue = 1, _maxSellValue = 2;
        private const float _minCreditCost = 1f, _toFullCostMod = 2.5f;
        private const float _maxPercentage = 100.0f;
        private RemnantDataListBehaviour _remnantDataListBehaviour = new RemnantDataListBehaviour();
        private List<RemnantData> _remnantItemList = new List<RemnantData>();
        private RemnantItemsBehaviour _remnantItemsBehaviour = null;
        private float _creditsWorthMinPercentage = 0;
        private float _creditsWorthMaxPercentage = _maxPercentage;

        //Delegates
        private Func<Item, bool> _checkIsScrapFunc = null;
        private Action<Item, bool, int, int> _createRemnantItemAction = null;
        #endregion

        #region Initialize 
        public void Initialize()
        {
            if (!_hasInitialized)
            {
                _hasInitialized = true;
                _bannedItemsNamesList = Remnants.Instance.RemnantsConfig.GetBannedFromRegisteringItemNames();
                _remnantItemList = Remnants.Instance.RemnantsConfig.GetRemnantItemList(false);
                bool useLegacySpawning = Remnants.Instance.RemnantsConfig.UseLegacySpawning.Value;
                bool UseSpecificLevelRarities = Remnants.Instance.RemnantsConfig.UseSpecificLevelRarities.Value;
                _remnantItemsBehaviour = Remnants.Instance.RemnantItemsBeh;
                _creditsWorthMinPercentage = (float)Remnants.Instance.RemnantsConfig.RemnantScrapMinCostPercentage.Value / _maxPercentage;
                _creditsWorthMaxPercentage = (float)Remnants.Instance.RemnantsConfig.RemnantScrapMaxCostPercentage.Value / _maxPercentage;
                if (useLegacySpawning)
                {
                    _checkIsScrapFunc = IsAlreadyScrap;
                    if (UseSpecificLevelRarities)
                        _createRemnantItemAction = CreateMoonSpecificLegacyRemnantItem;
                    else
                        _createRemnantItemAction = CreateMoonGeneralLegacyRemnantItem;
                }
                else
                {
                    _checkIsScrapFunc = IsAlreadyScrapOrRegistered;
                    if (UseSpecificLevelRarities)
                        _createRemnantItemAction = CreateMoonSpecificRemnantItem;
                    else
                        _createRemnantItemAction = CreateMoonGeneralRemnantItem;
                }
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
            _isAddingItems = false;
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

                    if (_checkIsScrapFunc(item))
                        continue;

                    if (IsPrefabIncorrect(item.spawnPrefab))
                        continue;

                    int creditsworth = GetItemCreditsWorth(item);
                    if (creditsworth >= _minCreditCost)
                    {
                        int remnantsIndex = _remnantItemList.FindIndex(remnantData => remnantData.RemnantItemName == item.itemName);
                        int itemRarityInfo = -1;
                        if (remnantsIndex != -1)
                            itemRarityInfo = _remnantItemList[remnantsIndex].RarityInfo;

                        RegisterItemAsScrap(item, creditsworth, itemRarityInfo);
                    }
                }
            }
            catch (Exception e)
            {
                mls.LogError(e.ToString());
            }
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

        private void RegisterItemAsScrap(Item item, int creditsWorth, int itemRarityInfo)
        {
            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Registering " + item.itemName + " as scrap.");
            bool useRarityByCredits = itemRarityInfo == -1 || itemRarityInfo == 0;
            _createRemnantItemAction(item, useRarityByCredits, itemRarityInfo, creditsWorth);
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


        private void CreateMoonGeneralLegacyRemnantItem(Item item, bool useRarityByCredits, int itemRarityInfo, int creditsWorth)
        {
            int rarity = CalculateGeneralRarity(useRarityByCredits, itemRarityInfo, creditsWorth);
            Items.RegisterScrap(item, rarity, Levels.LevelTypes.All);
            UpdateScrapItemData(item, creditsWorth);
        }

        private void CreateMoonGeneralRemnantItem(Item item, bool useRarityByCredits, int itemRarityInfo, int creditsWorth)
        {
            int rarity = CalculateGeneralRarity(useRarityByCredits, itemRarityInfo, creditsWorth);
            var scrapItem = new Items.ScrapItem(item, rarity, Levels.LevelTypes.All);
            scrapItem = CreateScrapitemData(scrapItem, creditsWorth);
            _remnantItemsBehaviour.AddNetworkRemnantItem(scrapItem);
        }

        private void CreateMoonSpecificRemnantItem(Item item, bool useRarityByCredits, int itemRarityInfo, int creditsWorth)
        {
            KeyValuePair<Dictionary<Levels.LevelTypes, int>, Dictionary<string, int>> pairMoonrarities = CreateMoonSpecificRarities(item, useRarityByCredits, itemRarityInfo, creditsWorth);
            var scrapItem = new Items.ScrapItem(item, pairMoonrarities.Key, pairMoonrarities.Value);
            scrapItem = CreateScrapitemData(scrapItem, creditsWorth);
            _remnantItemsBehaviour.AddNetworkRemnantItem(scrapItem);
        }

        private void CreateMoonSpecificLegacyRemnantItem(Item item, bool useRarityByCredits, int itemRarityInfo, int creditsWorth)
        {
            KeyValuePair<Dictionary<Levels.LevelTypes, int>, Dictionary<string, int>> pairMoonrarities = CreateMoonSpecificRarities(item, useRarityByCredits, itemRarityInfo, creditsWorth);
            Items.RegisterScrap(item, pairMoonrarities.Key, pairMoonrarities.Value);
            UpdateScrapItemData(item, creditsWorth);
        }

        private void UpdateScrapItemData(Item item, int creditsWorth)
        {
            int scrapItemIndex = Items.scrapItems.FindIndex(scrapItem => scrapItem.item.itemName == item.itemName || scrapItem.origItem.itemName == item.itemName);
            Items.scrapItems[scrapItemIndex].item.minValue = Mathf.Clamp((int)(creditsWorth * _toFullCostMod * _creditsWorthMinPercentage), _minSellValue, int.MaxValue);
            Items.scrapItems[scrapItemIndex].item.maxValue = Mathf.Clamp((int)(creditsWorth * _toFullCostMod * _creditsWorthMaxPercentage), _maxSellValue, int.MaxValue);
            Items.scrapItems[scrapItemIndex].item.itemSpawnsOnGround = true;
            LethalLib.Modules.Utilities.FixMixerGroups(Items.scrapItems[scrapItemIndex].item.spawnPrefab);
            GrabbableObject grabbable = Items.scrapItems[scrapItemIndex].item.spawnPrefab.GetComponentInChildren<GrabbableObject>();
            if (grabbable != null)
                grabbable.isInFactory = true;
        }

        private Items.ScrapItem CreateScrapitemData(Items.ScrapItem scrapItem, int creditsWorth)
        {
            string name = Assembly.GetCallingAssembly().GetName().Name;
            scrapItem.modName = name;
            scrapItem.item.minValue = Mathf.Clamp((int)(creditsWorth * _creditsWorthMinPercentage), 0, int.MaxValue);
            scrapItem.item.maxValue = Mathf.Clamp((int)(creditsWorth * _creditsWorthMaxPercentage), 0, int.MaxValue);
            scrapItem.item.itemSpawnsOnGround = true;
            LethalLib.Modules.Utilities.FixMixerGroups(scrapItem.item.spawnPrefab);
            GrabbableObject grabbable = scrapItem.item.spawnPrefab.GetComponentInChildren<GrabbableObject>();
            if (grabbable != null)
                grabbable.isInFactory = true;

            return scrapItem;
        }


        private int CalculateGeneralRarity(bool useRarityByCredits, int itemRarityInfo, int creditsWorth)
        {
            int rarity = 0;
            if (useRarityByCredits)
                rarity = CalculateRarityByCredits(creditsWorth, Remnants.Instance.RemnantsConfig.MinRemnantRarity.Value, Remnants.Instance.RemnantsConfig.MaxRemnantRarity.Value);
            else
                rarity = CalculateRarityOfItem(itemRarityInfo, Remnants.Instance.RemnantsConfig.MinRemnantRarity.Value, Remnants.Instance.RemnantsConfig.MaxRemnantRarity.Value);
            return rarity;
        }

        private KeyValuePair<Dictionary<Levels.LevelTypes, int>, Dictionary<string, int>> CreateMoonSpecificRarities(Item item, bool useRarityByCredits, int itemRarityInfo, int creditsWorth)
        {
            Dictionary<Levels.LevelTypes, int> levelRarities = new Dictionary<Levels.LevelTypes, int>();
            Dictionary<string, int> customLevelRarities = new Dictionary<string, int>();
            if (useRarityByCredits)
            {
                foreach (var levelRarity in Remnants.Instance.RemnantsConfig.LevelRarities)
                    levelRarities.Add(levelRarity.Key, CalculateRarityByCredits(creditsWorth, levelRarity.Value.Item1, levelRarity.Value.Item2));

                foreach (var customLevelRarity in Remnants.Instance.RemnantsConfig.CustomLevelRarities)
                    customLevelRarities.Add(customLevelRarity.Key, CalculateRarityByCredits(creditsWorth, customLevelRarity.Value.Item1, customLevelRarity.Value.Item2));
            }
            else
            {
                foreach (var levelRarity in Remnants.Instance.RemnantsConfig.LevelRarities)
                    levelRarities.Add(levelRarity.Key, CalculateRarityOfItem(itemRarityInfo, levelRarity.Value.Item1, levelRarity.Value.Item2));

                foreach (var customLevelRarity in Remnants.Instance.RemnantsConfig.CustomLevelRarities)
                    customLevelRarities.Add(customLevelRarity.Key, CalculateRarityOfItem(itemRarityInfo, customLevelRarity.Value.Item1, customLevelRarity.Value.Item2));
            }
            return new KeyValuePair<Dictionary<Levels.LevelTypes, int>, Dictionary<string, int>>(levelRarities, customLevelRarities);
        }
        #endregion
    }
}