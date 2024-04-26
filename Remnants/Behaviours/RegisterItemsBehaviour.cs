using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.Netcode;
using Remnants.utilities;
using System.IO.Ports;

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
        private RemnantDataListBehaviour _scrapDataListBehaviour = new RemnantDataListBehaviour();
        #endregion

        #region Initialize 
        public void Initialize()
        {
            if (!_hasInitialized)
            {
                _hasInitialized = true;
                _bannedItemsNamesList = Remnants.Instance.RemnantsConfig.GetBannedItemNames();
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
                _scrapDataListBehaviour.UpdateScrapDataList();
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
            var remnantItemList = Remnants.Instance.RemnantsConfig.GetRemnantItemList(false);
            try
            {
                foreach (Item item in allItems)
                {
                    if (item == null)
                        continue;
  
                    if (HasBannedName(item.name))
                        continue;
          
                    if (IsAlreadyScrap(item))
                        continue;
           
                    if (IsPrefabIncorrect(item.spawnPrefab))
                    {
                        mls.LogWarning(item.name + ": prefab is incorrect to be registered as scrap.");
                        continue;
                    }

                    int creditsworth = GetItemCreditsWorth(item);
                    if (creditsworth >= _minCreditCost)
                    {
                        int remnantsIndex = remnantItemList.FindIndex(remnantData => remnantData.RemnantItemName == item.name);
                        int itemRarityInfo = -1;
                        if(remnantsIndex != -1)
                            itemRarityInfo = remnantItemList[remnantsIndex].RarityInfo;

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

        private bool HasBannedName(string name)
        {
            return _bannedItemsNamesList.FindIndex(x => x == name) != -1;
        }

        private bool IsAlreadyScrap(Item item)
        {
            return item.isScrap || Items.scrapItems.FindIndex(scrapItem => scrapItem.item.itemName == item.itemName || scrapItem.origItem.itemName == item.itemName) != -1;
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
            float creditWorthPercentage = (float)Remnants.Instance.RemnantsConfig.RemnantScrapCostPercentage.Value / _maxPercentage;
            item.minValue = Mathf.Clamp((int)(creditsWorth * _toFullCostMod * creditWorthPercentage), _minSellValue, int.MaxValue);
            item.maxValue = Mathf.Clamp((int)(creditsWorth * _toFullCostMod * creditWorthPercentage), _maxSellValue, int.MaxValue);
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

                Items.RegisterScrap(item, rarity, Levels.LevelTypes.All);
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
                Items.RegisterScrap(item, levelRarities, customLevelRarities);
            }
            mls.LogInfo("Added " + item.itemName + " as a scrap item.");
            _scrapDataListBehaviour.AddItemToDataList(item.name);
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