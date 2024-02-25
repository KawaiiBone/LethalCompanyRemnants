using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace Remnants.Behaviours
{
    internal class RegisterItemsBehaviour
    {
        #region Variables
        private bool _hasInitialized = false;
        private bool _isAddingItems = false;
        private string[] _bannedItemsNames = new string[4] { "Clipboard", "StickyNote", "Binoculars", "MapDevice" };
        private const int _minStoreItemRarity = 5, _maxStoreItemRarity = 25;
        private const int _minSellValue = 1, _maxSellValue = 2;
        private const float _minCreditCost = 4f, _maxCreditCost = 300f;
        #endregion

        #region Initialize 
        public void Initialize()
        {
            if(!_hasInitialized)
            {
                _hasInitialized = true;
                SceneManager.sceneLoaded += StoreItemsRegisterAsScrap;
            }
        }
        #endregion

        #region Methods
        private void StoreItemsRegisterAsScrap(Scene scene, LoadSceneMode mode)
        {
            var mls = Remnants.Instance._mls;
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
            mls.LogInfo("Items Are loaded in.");
            SceneManager.sceneLoaded -= StoreItemsRegisterAsScrap;

    
        }

        private void AddStoreItemsToScrap(List<Item> allItems)
        {
            var mls = Remnants.Instance._mls;
            try
            {
                System.Random random = new System.Random();
                foreach (Item item in allItems)
                {
                    if (item == null)
                        continue;

                    if (HasBannedName(item.name) || Items.scrapItems.FindIndex(scrapItem => scrapItem.item.name == item.name) != -1)
                    {
                        mls.LogInfo("Barred from registerring: " + item.name);
                        continue;
                    }

                    if ((item.isScrap == false && item.creditsWorth > _minCreditCost))
                    {
                        int rarity = CalculateRarity(item.creditsWorth);
                        mls.LogInfo(item.name + " rarity: " + rarity);
                        item.minValue = _minSellValue;
                        item.maxValue = _maxSellValue;
                        Items.RegisterScrap(item, rarity, Levels.LevelTypes.All);
                        mls.LogInfo("Added " + item.name + " as a scrap item.");
                    }
                }
            }
            catch (Exception e)
            {
                mls.LogError(e.ToString());
            }
        }

        private bool HasBannedName(string name)
        {
            return Array.FindIndex(_bannedItemsNames, x => x == name) != -1;
        }

        private int CalculateRarity(int itemCreditWorth)
        {
            float creditCapped = Mathf.Clamp(itemCreditWorth, _minCreditCost, _maxCreditCost);
            float rarityPercentage = Mathf.Abs(((creditCapped / _maxCreditCost) * _maxStoreItemRarity) - _maxStoreItemRarity);
            return Mathf.Clamp((int)rarityPercentage, _minStoreItemRarity, _maxStoreItemRarity);
        }



 
    
        #endregion
    }
}
