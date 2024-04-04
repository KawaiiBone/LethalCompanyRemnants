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
    class Config
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
        public static List<ConfigEntry<int>> MinRemnantCustomLevelRarities = new List<ConfigEntry<int>>();
        public static List<ConfigEntry<int>> MaxRemnantCustomLevelRarities = new List<ConfigEntry<int>>();
        private static ConfigEntry<string> _bannedNamesFromRegistering;
        public static ConfigEntry<float> MaxRemnantItemCost;
        public static ConfigEntry<int> SpawnRarityOfBody;
        public static ConfigEntry<float> SpawnModifierRiskLevel;
        public static ConfigEntry<int> RemnantScrapCostPercentage;
        public static ConfigEntry<bool> ShouldSaveRemnantItems;
        public static ConfigEntry<bool> ShouldDespawnRemnantItems;
        public static ConfigEntry<bool> ShouldBodiesBeScrap;
        public static ConfigEntry<int> BodyScrapValue;
        private static ConfigFile _configFile;
        private static string _configFileName = "\\Remnants.cfg";
        private static string _generalSection = "General";
        private static string _generalBodySection = "GeneralBody";
        private static string _levelsSection = "Levels";
        private static string _customLevelsSection = "ModdedLevels";
        private static string _otherSection = "Other";
        private static string _remnantsSection = "Remnants";
        private static string _saveLoadSection = "Save/load";
        private static string _bannedPlanetName = "71 Gordion";
        private static List<ConfigDataValue<bool>> _configRemnantDataPairList = new List<ConfigDataValue<bool>>();
        private static List<ConfigDataValue<int>> _configCustomLevelsRarities = new List<ConfigDataValue<int>>();
        public static Dictionary<Levels.LevelTypes, Tuple<int, int>> LevelRarities = new Dictionary<Levels.LevelTypes, Tuple<int, int>>();
        public static Dictionary<string, Tuple<int, int>> CustomLevelRarities = new Dictionary<string, Tuple<int, int>>();


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

            RemnantScrapCostPercentage = _configFile.Bind(_generalSection, "Remnant item scrap cost percentage", 0, "The percentage of how much worth of scrap a remnant item is compared to its normal credit cost. \nFrom 0 percentage scrap cost to 1000 percentage.");
            RemnantScrapCostPercentage.Value = Mathf.Clamp(RemnantScrapCostPercentage.Value, 0, (int)maxItemCost);

            ShouldBodiesBeScrap = _configFile.Bind(_generalBodySection, "Should bodies be scrap", true, "When the bodies are a scrap they can be grabbed, have a scrap value and can be sold. \nIf not then it becomes a prop and cannot be interacted with.");

            SpawnRarityOfBody = _configFile.Bind(_generalBodySection, "Body spawn rarity", 2, "This number is the chance that a body spawns next to an remnant item.");
            SpawnRarityOfBody.Value = Mathf.Clamp(SpawnRarityOfBody.Value, 0, maxPercentage);

                        SpawnModifierRiskLevel = _configFile.Bind(_generalBodySection, "Body spawn modifier per moon risk level", 1.2f, "By increasing this modifier you will increase the spawnchance of the body per risk level moon.");
            SpawnModifierRiskLevel.Value = Mathf.Clamp(SpawnModifierRiskLevel.Value, 0.0f, 10.0f);

            BodyScrapValue = _configFile.Bind(_generalBodySection, "Scrap value of the bodies", 5, "The scrap value of the bodies that this mod spawns. \nThis only works if the bodies are set to scrap.");
            BodyScrapValue.Value = Mathf.Clamp(BodyScrapValue.Value, 0, (int)maxItemCost);

            MaxRemnantItemCost = _configFile.Bind(_otherSection, "Max value to calculate rarity", 400.0f, "This value exists to calculate the spawn rarity of specific remnant items. \nThis rarity is determined by their original store cost. \nThe more expensive an item is, the less chance it has to spawn. \nThe value below caps the max cost of items in service of the calculation of an item's rarity. \nThe default value has already been optimized.");
            MaxRemnantItemCost.Value = Mathf.Clamp(MaxRemnantItemCost.Value, minItemCost, maxItemCost);

            _bannedNamesFromRegistering = _configFile.Bind(_otherSection, "Item list banned from registering as scrap", "Clipboard,StickyNote,Binoculars,MapDevice,Key", "List of items that are barred from registering as scrap/remnant item. \nThese default items are there to avoid adding scrap that are left out of the vanilla version, don't work, or cause crashes. \nTo add more names to the list, be sure to add a comma between names.");

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


            MinRemnantCustomLevelRarities = new List<ConfigEntry<int>>();
            MaxRemnantCustomLevelRarities = new List<ConfigEntry<int>>();
            for (int i = 0; i < _configCustomLevelsRarities.Count; i += 2)
            {
                MinRemnantCustomLevelRarities.Add(_configFile.Bind(_customLevelsSection, _configCustomLevelsRarities[i].Name, 5, _configCustomLevelsRarities[i].Discription));
                MinRemnantCustomLevelRarities.Last().Value = Mathf.Clamp(MinRemnantCustomLevelRarities.Last().Value, minPercentage, maxPercentage);
                MaxRemnantCustomLevelRarities.Add(_configFile.Bind(_customLevelsSection, _configCustomLevelsRarities[i + 1].Name, 25, _configCustomLevelsRarities[i + 1].Discription));
                MaxRemnantCustomLevelRarities.Last().Value = Mathf.Clamp(MaxRemnantCustomLevelRarities.Last().Value, MinRemnantCustomLevelRarities.Last().Value, maxPercentage);

                CustomLevelRarities.Add(RemoveEndSentence(_configCustomLevelsRarities[i].Name, " min remnant rarity"),
                    new Tuple<int, int>(MinRemnantCustomLevelRarities.Last().Value, MaxRemnantCustomLevelRarities.Last().Value));
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

            var configCustomMoonSection = GetConfigSectionData(_customLevelsSection);
            if (configCustomMoonSection != null || configCustomMoonSection.Count > 0)
            {
                _configCustomLevelsRarities = configCustomMoonSection.ConvertAll(configDataInt =>
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

        public static void SetCustomLevelsRarities(List<string> customMoonNames, int minPercentage = 1, int maxPercentage = 100)
        {
            foreach (var customMoonName in customMoonNames)
            {
                if (CustomLevelRarities.ContainsKey(customMoonName) || customMoonName == _bannedPlanetName)
                    continue;

                MinRemnantCustomLevelRarities.Add(_configFile.Bind(_customLevelsSection, customMoonName + " min remnant rarity", 5, "Minimum chance of a remnant item spawning on moon: " + customMoonName + " ."));
                MinRemnantCustomLevelRarities.Last().Value = Mathf.Clamp(MinRemnantCustomLevelRarities.Last().Value, minPercentage, maxPercentage);
                MaxRemnantCustomLevelRarities.Add(_configFile.Bind(_customLevelsSection, customMoonName + " max remnant rarity", 25, "Maximum chance of a remnant item spawning on moon: " + customMoonName + " ."));
                MaxRemnantCustomLevelRarities.Last().Value = Mathf.Clamp(MaxRemnantCustomLevelRarities.Last().Value, MinRemnantCustomLevelRarities.Last().Value, maxPercentage);
                CustomLevelRarities.Add(customMoonName,
                    new Tuple<int, int>(MinRemnantCustomLevelRarities.Last().Value, MaxRemnantCustomLevelRarities.Last().Value));
            }

            _configFile.Save();
        }


        public static Dictionary<string, Tuple<int, int>> GetCustomLevelRarities()
        {
            _configFile.Reload();

            if (CustomLevelRarities == null || CustomLevelRarities.Count == 0)
                return new Dictionary<string, Tuple<int, int>>();

            for (int i = 0; i < MinRemnantCustomLevelRarities.Count; i++)
            {
                CustomLevelRarities[MinRemnantCustomLevelRarities[i].Definition.Key] = new Tuple<int, int>(MinRemnantCustomLevelRarities[i].Value, MaxRemnantCustomLevelRarities[i].Value);
            }

            return CustomLevelRarities;
        }


        private static string RemoveEndSentence(string fullSentence, string toRemoveAtEnd)
        {
            if (fullSentence.EndsWith(toRemoveAtEnd))
            {
                for (int i = 0; i < fullSentence.Length; i++)
                {
                    if (fullSentence[i] != toRemoveAtEnd[0])
                        continue;

                    if (i + toRemoveAtEnd.Length == fullSentence.Length)
                        return fullSentence.Substring(0, i);
                }
            }
            return fullSentence;
        }

        private static List<ConfigData> GetConfigSectionData(string section)
        {
            List<ConfigData> list = new List<ConfigData>();
            if (!File.Exists(Paths.ConfigPath + _configFileName))
                return list;

            StreamReader sr = new StreamReader(Paths.ConfigPath + _configFileName);
            string line = sr.ReadLine();
            bool hasFoundSection = false;

            while (line != null)
            {
                if (hasFoundSection && line.StartsWith("["))
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
