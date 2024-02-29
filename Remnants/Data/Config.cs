﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using DunGen.Tags;
using LethalLib;
using Remnants.utilities;
using UnityEngine;

namespace Remnants.Data
{
    internal class Config
    {
        #region Variables
        public static ConfigEntry<int> MinRemnantRarity;
        public static ConfigEntry<int> MaxRemnantRarity;
        public static ConfigEntry<int> MinRemnantBatteryCharge;
        public static ConfigEntry<int> MaxRemnantBatteryCharge;
        private static ConfigEntry<string> _bannedNamesFromRegistering;
        public static List<ConfigEntry<bool>> ConfigScrapDataList;
        public static ConfigEntry<float> MaxRemnantItemCost;
        private static ConfigFile _configFile;
        public static string _configFileName = "\\Remnants.cfg";
        private static string _generalSection = "General";
        private static string _otherSection = "Other";
        private static string _remnantsSection = "Remnants";
        #endregion

        #region Initialize 
        #endregion

        #region Methods
        public static void UpdateConfig()
        {

        }
        public static void LoadConfig()
        {
            _configFile = new ConfigFile(Paths.ConfigPath + _configFileName, true);
            const int maxPercentage = 100;
            const int minPercentage = 1;
            const float minItemCost = 5.0f;
            const float maxItemCost = 10000.0f;


            MinRemnantRarity = _configFile.Bind(_generalSection, "Min remnant item rarity", 5, "Minimum chance of a remnant item spawning.");
            MinRemnantRarity.Value = Mathf.Clamp(MinRemnantRarity.Value, minPercentage, maxPercentage);

            MaxRemnantRarity = _configFile.Bind(_generalSection, "Max remnant item rarity", 25, "Maximum chance of a remnant item spawning.");
            MaxRemnantRarity.Value = Mathf.Clamp(MaxRemnantRarity.Value, MinRemnantRarity.Value, maxPercentage);

            MinRemnantBatteryCharge = _configFile.Bind(_generalSection, "Min remnant battery charge", 20, "Minimum remnant item battery charge on first finding it.");
            MinRemnantBatteryCharge.Value = Mathf.Clamp(MinRemnantBatteryCharge.Value, minPercentage, maxPercentage);

            MaxRemnantBatteryCharge = _configFile.Bind(_generalSection, "Max remnant battery charge", 90, "Maximum remnant item battery charge on first finding it.");
            MaxRemnantBatteryCharge.Value = Mathf.Clamp(MaxRemnantBatteryCharge.Value, MinRemnantBatteryCharge.Value, maxPercentage);

            _bannedNamesFromRegistering = _configFile.Bind(_generalSection, "Item list banned from registering as scrap", "Clipboard,StickyNote,Binoculars,MapDevice", "List of items that are barred from registering as scrap/remnant item. \nThese default items are there to avoid adding scrap that are left out of the vanilla version, don't work, or cause crashes. \nTo add more names to the list, be sure to add a comma between names.");

            MaxRemnantItemCost = _configFile.Bind(_otherSection, "Max value to calculate rarity", 400.0f, "This value exists to calculate the spawn rarity of specific remnant items. \nThis rarity is determined by their original store cost. \nThe more expensive an item is, the less chance it has to spawn. \nThe value below caps the max cost of items in service of the calculation of an item's rarity. \nThe default value has already been optimized.");
            MaxRemnantItemCost.Value = Mathf.Clamp(MaxRemnantItemCost.Value, minItemCost, maxItemCost);

            _configFile.Save();
        }

        public static List<string> GetBannedItemNames()
        {
            if (_bannedNamesFromRegistering.Value.IsNullOrWhiteSpace())
                return new List<string>();
            return _bannedNamesFromRegistering.Value.Split(',').ToList();
        }

        public static List<RemnantData> GetRemnantItemList()
        {
            if (ConfigScrapDataList == null || ConfigScrapDataList.Count == 0)
                return new List<RemnantData>();

            return ConfigScrapDataList.ConvertAll(configEntry =>
            new RemnantData
            {
                RemnantItemName = configEntry.Definition.Key,
                ShouldSpawn = configEntry.Value
            });
        }

        public static void SetRemnantItemList(List<RemnantData> remnantDataList)
        {
            ConfigScrapDataList = remnantDataList.ConvertAll(remnantData =>
          _configFile.Bind(_remnantsSection, remnantData.RemnantItemName, remnantData.ShouldSpawn, "By changing the value above, you can choose whether the certain item spawns or not."));
            _configFile.Save();
        }
        #endregion
    }
}
