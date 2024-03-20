using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using LethalLib.Modules;
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
        public static List<ConfigEntry<bool>> ConfigScrapDataList;
        public static ConfigEntry<bool> UseSpecificLevelRarities;
        public static List<ConfigEntry<int>> MinRemnantLevelRarities = new List<ConfigEntry<int>>();
        public static List<ConfigEntry<int>> MaxRemnantLevelRarities = new List<ConfigEntry<int>>();
        public static List<ConfigEntry<int>> MinRemnantCostumLevelRarities = new List<ConfigEntry<int>>();
        public static List<ConfigEntry<int>> MaxRemnantCostumLevelRarities = new List<ConfigEntry<int>>();
        private static ConfigEntry<string> _bannedNamesFromRegistering;
        public static ConfigEntry<float> MaxRemnantItemCost;
        public static ConfigEntry<int> SpawnRarityOfBody;
        public static ConfigEntry<float> SpawnModifierRiskLevel;
        public static ConfigEntry<bool> ShouldSaveRemnantItems;
        public static ConfigEntry<bool> ShouldDespawnRemnantItems;
        private static ConfigFile _configFile;
        private static string _configFileName = "\\Remnants.cfg";
        private static string _generalSection = "General";
        private static string _otherSection = "Other";
        private static string _remnantsSection = "Remnants";
        private static string _levelsSection = "Levels";
        private static string _costumLevelsSection = "ModdedLevels";
        private static string _saveLoadSection = "Save/load";
        private static List<ConfigDataValue<bool>> _configRemnantDataPairList = new List<ConfigDataValue<bool>>();
        private static List<ConfigDataValue<int>> _configCostumLevelsRarities = new List<ConfigDataValue<int>>();
        public static Dictionary<Levels.LevelTypes, Tuple<int, int>> LevelRarities = new Dictionary<Levels.LevelTypes, Tuple<int, int>>();
        public static Dictionary<string, Tuple<int, int>> CostumLevelRarities = new Dictionary<string, Tuple<int, int>>();


        struct ConfigData
        {
            public string Name;
            public string Discription;
            public string StringValue;
        }

        struct ConfigDataValue<T> where T : struct
        {
            public string Name;
            public string Discription;
            public T Value;
        }

        #endregion

        #region Initialize 
        #endregion

        #region Methods


        public static void LoadConfig()
        {
            LoadConfigData();


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

            SpawnRarityOfBody = _configFile.Bind(_otherSection, "Body spawn rarity", 5, "This number is the chance that a body spawns next to an remnant item.");
            SpawnRarityOfBody.Value = Mathf.Clamp(SpawnRarityOfBody.Value, 0, maxPercentage);

            SpawnModifierRiskLevel = _configFile.Bind(_otherSection, "Body spawn modifier per moon risk level", 1.2f, "By increasing this modifier you will increase the spawnchance of the body per risk level moon.");
            SpawnModifierRiskLevel.Value = Mathf.Clamp(SpawnModifierRiskLevel.Value, 0.0f, 10.0f);

            MinRemnantLevelRarities = new List<ConfigEntry<int>>();
            MaxRemnantLevelRarities = new List<ConfigEntry<int>>();
            LevelRarities = new Dictionary<Levels.LevelTypes, Tuple<int, int>>();
            UseSpecificLevelRarities = _configFile.Bind(_levelsSection, "Use level specific rarities", false);
            foreach (var moonName in Enum.GetNames(typeof(Levels.LevelTypes)))
            {
                if (moonName == Levels.LevelTypes.All.ToString() || moonName == Levels.LevelTypes.None.ToString()
                    || moonName == Levels.LevelTypes.Vanilla.ToString() || moonName == Levels.LevelTypes.Modded.ToString())
                    continue;

                MinRemnantLevelRarities.Add(_configFile.Bind(_levelsSection, moonName + " min remnant rarity", 5, "Minimum chance of a remnant item spawning on moon: " + moonName + " ."));
                MinRemnantLevelRarities.Last().Value = Mathf.Clamp(MinRemnantLevelRarities.Last().Value, minPercentage, maxPercentage);
                MaxRemnantLevelRarities.Add(_configFile.Bind(_levelsSection, moonName + " max remnant rarity", 25, "Maximum chance of a remnant item spawning on moon: " + moonName + " ."));
                MaxRemnantLevelRarities.Last().Value = Mathf.Clamp(MaxRemnantLevelRarities.Last().Value, MinRemnantLevelRarities.Last().Value, maxPercentage);
                LevelRarities.Add((Levels.LevelTypes)Enum.Parse(typeof(Levels.LevelTypes), moonName),
                    new Tuple<int, int>(MinRemnantLevelRarities.Last().Value, MaxRemnantLevelRarities.Last().Value));
            }

            ShouldSaveRemnantItems = _configFile.Bind(_saveLoadSection, "Save remnant items", true, "This ensures that the remnant items are saved in the ship when you reload the lobby.");
            ShouldDespawnRemnantItems = _configFile.Bind(_saveLoadSection, "Despawn remnant items on party wipe", true, "On party wipe all items are despawned from the ship, this ensures that remnant items also are despawned. \nIf you use a mod that prevents items from being despawned, here you can edit it too for remnant items.");

            ConfigScrapDataList = _configRemnantDataPairList.ConvertAll(itemData =>
                     _configFile.Bind(_remnantsSection, itemData.Name, true, itemData.Discription));


            MinRemnantCostumLevelRarities = new List<ConfigEntry<int>>();
            MaxRemnantCostumLevelRarities = new List<ConfigEntry<int>>();


            for (int i = 0; i < _configCostumLevelsRarities.Count; i += 2)
            {
                MinRemnantCostumLevelRarities.Add(_configFile.Bind(_costumLevelsSection, _configCostumLevelsRarities[i].Name, 5, _configCostumLevelsRarities[i].Discription));
                MinRemnantCostumLevelRarities.Last().Value = Mathf.Clamp(MinRemnantCostumLevelRarities.Last().Value, minPercentage, maxPercentage);
                MaxRemnantCostumLevelRarities.Add(_configFile.Bind(_costumLevelsSection, _configCostumLevelsRarities[i + 1].Name, 25, _configCostumLevelsRarities[i + 1].Discription));
                MaxRemnantCostumLevelRarities.Last().Value = Mathf.Clamp(MaxRemnantCostumLevelRarities.Last().Value, MinRemnantCostumLevelRarities.Last().Value, maxPercentage);
                CostumLevelRarities.Add(_configCostumLevelsRarities[i].Name,
                    new Tuple<int, int>(MinRemnantCostumLevelRarities.Last().Value, MaxRemnantCostumLevelRarities.Last().Value));
            }

            _configFile.Save();
        }

        private static void LoadConfigData()
        {
            var configRemnantSection = GetConfigSectionData(_remnantsSection);
            if (configRemnantSection != null || configRemnantSection.Count > 0)
            {
                _configRemnantDataPairList = configRemnantSection.ConvertAll(configData =>
                 new ConfigDataValue<bool>
                 {
                     Name = configData.Name,
                     Discription = configData.Discription,
                     Value = Convert.ToBoolean(configData.StringValue)
                 });
            }

            var configCostumMoonSection = GetConfigSectionData(_costumLevelsSection);
            if (configCostumMoonSection != null || configCostumMoonSection.Count > 0)
            {
                _configCostumLevelsRarities = configCostumMoonSection.ConvertAll(configDataInt =>
                    new ConfigDataValue<int>
                    {
                        Name = configDataInt.Name,
                        Discription = configDataInt.Discription,
                        Value = Convert.ToInt32(configDataInt.StringValue)
                    });
            }
        }


        public static List<string> GetBannedItemNames()
        {
            if (_bannedNamesFromRegistering.Value.IsNullOrWhiteSpace())
                return new List<string>();
            return _bannedNamesFromRegistering.Value.Split(',').ToList();
        }

        public static List<RemnantData> GetRemnantItemList()
        {
            _configFile.Reload();

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
          _configFile.Bind(_remnantsSection, remnantData.RemnantItemName, remnantData.ShouldSpawn, "By changing the value, you can choose whether the certain item spawns or not."));
            _configFile.Save();
        }

        public static void SetCostumLevelRarities(string costumMoonName, int minPercentage = 1, int maxPercentage = 100)
        {
            if (CostumLevelRarities.ContainsKey(costumMoonName))
                return;

            MinRemnantCostumLevelRarities.Add(_configFile.Bind(_costumLevelsSection, costumMoonName + " min remnant rarity", 5, "Minimum chance of a remnant item spawning on moon: " + costumMoonName + " ."));
            MinRemnantCostumLevelRarities.Last().Value = Mathf.Clamp(MinRemnantCostumLevelRarities.Last().Value, minPercentage, maxPercentage);
            MaxRemnantCostumLevelRarities.Add(_configFile.Bind(_costumLevelsSection, costumMoonName + " max remnant rarity", 25, "Maximum chance of a remnant item spawning on moon: " + costumMoonName + " ."));
            MaxRemnantCostumLevelRarities.Last().Value = Mathf.Clamp(MaxRemnantCostumLevelRarities.Last().Value, MinRemnantCostumLevelRarities.Last().Value, maxPercentage);
            CostumLevelRarities.Add(costumMoonName,
                new Tuple<int, int>(MinRemnantCostumLevelRarities.Last().Value, MaxRemnantCostumLevelRarities.Last().Value));
        }


        public static Dictionary<string, Tuple<int, int>> GetCostumLevelRarities()
        {
            _configFile.Reload();

            if (CostumLevelRarities == null || CostumLevelRarities.Count == 0)
                return new Dictionary<string, Tuple<int, int>>();


            for (int i = 0; i < MinRemnantCostumLevelRarities.Count; i++)
            {
                CostumLevelRarities[MinRemnantCostumLevelRarities[i].Definition.Key] = new Tuple<int, int>(MinRemnantCostumLevelRarities[i].Value, MaxRemnantCostumLevelRarities[i].Value);
            }

            return CostumLevelRarities;
        }


        private static List<ConfigData> GetConfigSectionData(string section)
        {
            List<ConfigData> list = new List<ConfigData>();
            if (!File.Exists(Paths.ConfigPath + _configFileName))
                return list;

            var mls = Remnants.Instance.Mls;
            StreamReader sr = new StreamReader(Paths.ConfigPath + _configFileName);
            string line = sr.ReadLine();
            bool hasFoundSection = false;

            while (line != null)
            {
                if(hasFoundSection && line.StartsWith("["))
                {
                    break;
                }
                else if (hasFoundSection)
                {
                    AddDataToList(sr, line, list);
                }
                else if (line == "[" + section + "]")
                {
                    hasFoundSection = true;
                    line = sr.ReadLine();
                    AddDataToList(sr, line, list);
                }
                line = sr.ReadLine();
            }
            sr.Close();
            return list;
        }

        private static void AddDataToList(StreamReader sr, string line, List<ConfigData> list)
        {
            string discriptionStart = "## ";
            string ignoreLine = "#";
            string itemValueDetect = " = ";
            ConfigData configData = new ConfigData();


            if (!line.StartsWith(discriptionStart))
                return;

            configData.Discription = line.Remove(0, discriptionStart.Length);
            while (line != null)
            {
                if (!line.StartsWith(ignoreLine))
                    break;
                line = sr.ReadLine();
            }

            var mls = Remnants.Instance.Mls;
            int startIndex = line.LastIndexOf(itemValueDetect);
            configData.Name = line.Remove(startIndex);
            configData.StringValue = line.Remove(0, startIndex + itemValueDetect.Length - 1);
            list.Add(configData);
        }


        #endregion
    }
}
