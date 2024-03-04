using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.Netcode;

namespace Remnants.Behaviours
{
    internal class RegisterItemsBehaviour
    {
        #region Variables
        private bool _hasInitialized = false;
        private bool _isAddingItems = false;
        private List<string> _bannedItemsNamesList = new List<string>();

        private const int _minSellValue = 1, _maxSellValue = 2;
        private const float _minCreditCost = 4f;
        private RemnantDataListBehaviour _scrapDataListBehaviour = new RemnantDataListBehaviour();
        #endregion

        #region Initialize 
        public void Initialize()
        {
            if (!_hasInitialized)
            {
                _hasInitialized = true;
                SceneManager.sceneLoaded += StoreItemsRegisterAsScrap;
                _bannedItemsNamesList = Data.Config.GetBannedItemNames();
            }
        }
        #endregion

        #region Methods
        private void StoreItemsRegisterAsScrap(Scene scene)
        {
            StoreItemsRegisterAsScrap(scene, LoadSceneMode.Single);
        }
        private void StoreItemsRegisterAsScrap(Scene scene, LoadSceneMode mode)
        {
            var mls = Remnants.Instance.Mls;
            if (StartOfRound.Instance != null)
            {
                mls.LogInfo("In lobby, loading in no more items");
                SceneManager.sceneLoaded -= StoreItemsRegisterAsScrap;
                SceneManager.sceneUnloaded -= StoreItemsRegisterAsScrap;
                _scrapDataListBehaviour.UpdateScrapDataList();
                return;
            }

            if (_isAddingItems)
            {
                mls.LogInfo("Did not load items because items are already loading in.");
                return;
            }

            List<Item> allItems = Resources.FindObjectsOfTypeAll<Item>().Concat(UnityEngine.Object.FindObjectsByType<Item>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)).ToList();
            if (allItems.Count == 0)
            {
                mls.LogInfo("Did not load items because there are no items to load.");
                return;
            }

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
                System.Random random = new System.Random();
                foreach (Item item in allItems)
                {
                    if (item == null)
                        continue;

                    if (HasBannedName(item.name))
                        continue;

                    if (Items.scrapItems.FindIndex(scrapItem => scrapItem.item.name == item.name) != -1 || item.isScrap)
                        continue;

                    if (item.spawnPrefab.GetComponent<NetworkObject>() == null)
                    {
                        mls.LogWarning(item.name + ": NetworkObject is null, barring item from registering.");
                        continue;
                    }

                    if (item.creditsWorth > _minCreditCost)
                    {
                        int rarity = CalculateRarity(item.creditsWorth);
                        item.minValue = _minSellValue;
                        item.maxValue = _maxSellValue;
                        Items.RegisterScrap(item, rarity, Levels.LevelTypes.All);
                        mls.LogInfo("Added " + item.name + " as a scrap item.");
                        _scrapDataListBehaviour.AddItemToDataList(item.name);
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

        private int CalculateRarity(int itemCreditWorth)
        {
            int maxStoreScrapRarity = Data.Config.MaxRemnantRarity.Value;
            int minStoreScrapRarity = Data.Config.MinRemnantRarity.Value;
            float maxCreditValue = Data.Config.MaxRemnantItemCost.Value;
            float creditCapped = Mathf.Clamp(itemCreditWorth, _minCreditCost, maxCreditValue);
            float rarityPercentage = Mathf.Abs(((creditCapped / maxCreditValue) * maxStoreScrapRarity) - maxStoreScrapRarity);
            return Mathf.Clamp((int)rarityPercentage, minStoreScrapRarity, maxStoreScrapRarity);
        }
        #endregion
    }
}
