using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using LethalLib.Modules;
using Remnants.utilities;
using UnityEngine;

namespace Remnants.Data
{
    public class Config
    {
        #region Variables
        private bool _hasInitialized = false;
        public ConfigEntry<int> MinRemnantRarity;
        public ConfigEntry<int> MaxRemnantRarity;
        public ConfigEntry<int> MinRemnantBatteryCharge;
        public ConfigEntry<int> MaxRemnantBatteryCharge;
        public List<ConfigEntry<int>> ConfigScrapDataList;
        public List<ConfigEntry<bool>> ConfigSuitsDataList;
        public ConfigEntry<bool> UseSpecificLevelRarities;
        public ConfigEntry<float> MaxRemnantItemCost;
        public ConfigEntry<int> SpawnRarityOfBody;
        public ConfigEntry<float> SpawnModifierRiskLevel;
        public ConfigEntry<int> RemnantScrapCostPercentage;
        public ConfigEntry<bool> ShouldSaveRemnantItems;
        public ConfigEntry<bool> ShouldDespawnRemnantItems;
        public ConfigEntry<bool> ShouldAlwaysDespawnRemnantItems;
        public ConfigEntry<bool> ShouldBodiesBeScrap;
        public ConfigEntry<int> BodyScrapValue;
        public Dictionary<Levels.LevelTypes, Tuple<int, int>> LevelRarities = new Dictionary<Levels.LevelTypes, Tuple<int, int>>();
        public Dictionary<string, Tuple<int, int>> CustomLevelRarities = new Dictionary<string, Tuple<int, int>>();

        private List<ConfigEntry<int>> _minRemnantLevelRarities = new List<ConfigEntry<int>>();
        private List<ConfigEntry<int>> _maxRemnantLevelRarities = new List<ConfigEntry<int>>();
        private List<ConfigEntry<int>> _minRemnantCustomLevelRarities = new List<ConfigEntry<int>>();
        private List<ConfigEntry<int>> _maxRemnantCustomLevelRarities = new List<ConfigEntry<int>>();
        private List<ConfigDataValue<int>> _configRemnantDataPairList = new List<ConfigDataValue<int>>();
        private List<ConfigDataValue<int>> _configCustomLevelsRarities = new List<ConfigDataValue<int>>();
        private List<ConfigDataValue<bool>> _configSuitsDataList = new List<ConfigDataValue<bool>>();
        private ConfigEntry<string> _bannedNamesFromRegistering;
        private ConfigEntry<string> _overriddenScrapItems;
        private ConfigFile _configFile;
        private string _configFileName = "\\Remnants.cfg";
        private string _generalSection = "General";
        private string _generalBodySection = "GeneralBody";
        private string _levelsSection = "Levels";
        private string _customLevelsSection = "ModdedLevels";
        private string _otherSection = "Other";
        private string _remnantsSection = "Remnants";
        private string _suitsSection = "Suits";
        private string _saveLoadSection = "Save/load";
        private string _bannedPlanetName = "71 Gordion";

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
        public void Initialize()
        {
            if (!_hasInitialized)
            {
                _hasInitialized = true;
                LoadConfig();
            }
        }
        #endregion

        #region Methods
        private void LoadConfig()
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

            RemnantScrapCostPercentage = _configFile.Bind(_generalSection, "Remnant item scrap cost percentage", 5, "The percentage of how much worth of scrap a remnant item is compared to its normal credit cost. \nFrom 0 percentage scrap cost to 1000 percentage.");
            RemnantScrapCostPercentage.Value = Mathf.Clamp(RemnantScrapCostPercentage.Value, 0, (int)maxItemCost);

            ShouldBodiesBeScrap = _configFile.Bind(_generalBodySection, "Should bodies be scrap", true, "When the bodies are a scrap they can be grabbed, have a scrap value and can be sold. \nIf not then it becomes a prop and cannot be interacted with.");

            SpawnRarityOfBody = _configFile.Bind(_generalBodySection, "Body spawn rarity", 2, "This number is the chance that a body spawns next to an remnant item.");
            SpawnRarityOfBody.Value = Mathf.Clamp(SpawnRarityOfBody.Value, 0, maxPercentage);

            SpawnModifierRiskLevel = _configFile.Bind(_generalBodySection, "Body spawn modifier per moon risk level", 1.2f, "By increasing this modifier you will increase the spawnchance of the body per risk level moon.");
            SpawnModifierRiskLevel.Value = Mathf.Clamp(SpawnModifierRiskLevel.Value, 0.0f, 10.0f);

            BodyScrapValue = _configFile.Bind(_generalBodySection, "Scrap value of the bodies", 5, "The scrap value of the bodies that this mod spawns. \nThis only works if the bodies are set to scrap.");
            BodyScrapValue.Value = Mathf.Clamp(BodyScrapValue.Value, 0, (int)maxItemCost);

            MaxRemnantItemCost = _configFile.Bind(_otherSection, "Max value to calculate rarity", 400.0f, "This value helps calculating the spawn rarity, the rarity is calculated by the credit cost of the shop item. \nThis caps the maximum cost of an item and setting it as min spawn rarity if it is the same or higher than this value. \nThe more an item cost, the less spawn chance/spawn rarity it has.");
            MaxRemnantItemCost.Value = Mathf.Clamp(MaxRemnantItemCost.Value, minItemCost, maxItemCost);

            _bannedNamesFromRegistering = _configFile.Bind(_otherSection, "Item list banned from registering as scrap", "Clipboard,StickyNote,Binoculars,MapDevice,Key,Error", "List of items that are barred from registering as scrap/remnant item. \nThese default items are there to avoid adding scrap that are left out of the vanilla version, don't work, or cause crashes. \nTo add more names to the list, be sure to add a comma between names.");
            _overriddenScrapItems = _configFile.Bind(_otherSection, "Scrap item list to be used as remnant items", "Example scrap,Scrap-example", "In here you can add scrap items to be treated as remnant items, to spawn bodies on and to randomize batteries. \nTo add more names to the list, be sure to add a comma between names.");

            _minRemnantLevelRarities = new List<ConfigEntry<int>>();
            _maxRemnantLevelRarities = new List<ConfigEntry<int>>();
            LevelRarities = new Dictionary<Levels.LevelTypes, Tuple<int, int>>();
            UseSpecificLevelRarities = _configFile.Bind(_levelsSection, "Use level specific rarities", false);
            foreach (var moonName in Enum.GetNames(typeof(Levels.LevelTypes)))
            {
                if (moonName == Levels.LevelTypes.All.ToString() || moonName == Levels.LevelTypes.None.ToString()
                    || moonName == Levels.LevelTypes.Vanilla.ToString() || moonName == Levels.LevelTypes.Modded.ToString())
                    continue;

                _minRemnantLevelRarities.Add(_configFile.Bind(_levelsSection, moonName + " min remnant rarity", 5, "Minimum chance of a remnant item spawning on moon: " + moonName + " ."));
                _minRemnantLevelRarities.Last().Value = Mathf.Clamp(_minRemnantLevelRarities.Last().Value, minPercentage, maxPercentage);
                _maxRemnantLevelRarities.Add(_configFile.Bind(_levelsSection, moonName + " max remnant rarity", 25, "Maximum chance of a remnant item spawning on moon: " + moonName + " ."));
                _maxRemnantLevelRarities.Last().Value = Mathf.Clamp(_maxRemnantLevelRarities.Last().Value, _minRemnantLevelRarities.Last().Value, maxPercentage);
                LevelRarities.Add((Levels.LevelTypes)Enum.Parse(typeof(Levels.LevelTypes), moonName),
                    new Tuple<int, int>(_minRemnantLevelRarities.Last().Value, _maxRemnantLevelRarities.Last().Value));
            }

            ShouldSaveRemnantItems = _configFile.Bind(_saveLoadSection, "Save remnant items", true, "This ensures that the remnant items are saved in the ship when you reload the lobby.");
            ShouldDespawnRemnantItems = _configFile.Bind(_saveLoadSection, "Despawn remnant items on party wipe", true, "On party wipe all items are despawned from the ship, this ensures that remnant items also are despawned. \nIf you use a mod that prevents items from being despawned, here you can edit it too for remnant items.");
            ShouldAlwaysDespawnRemnantItems = _configFile.Bind(_saveLoadSection, "Always despawn remnant items", false, "Despawns all remnant items when going away from moons, even if it is in the shiproom and you are holding it.");

            ConfigScrapDataList = _configRemnantDataPairList.ConvertAll(itemData =>
                     _configFile.Bind(_remnantsSection, itemData.Name, -1, itemData.Discription));
            for (int i = 0; i < ConfigScrapDataList.Count; i++)
            {
                ConfigScrapDataList[i].Value = Mathf.Clamp(ConfigScrapDataList[i].Value, -1, maxPercentage);
            }


            ConfigSuitsDataList = _configSuitsDataList.ConvertAll(itemData =>
                     _configFile.Bind(_suitsSection, itemData.Name, true, itemData.Discription));

            _minRemnantCustomLevelRarities = new List<ConfigEntry<int>>();
            _maxRemnantCustomLevelRarities = new List<ConfigEntry<int>>();
            for (int i = 0; i < _configCustomLevelsRarities.Count; i += 2)
            {
                _minRemnantCustomLevelRarities.Add(_configFile.Bind(_customLevelsSection, _configCustomLevelsRarities[i].Name, 5, _configCustomLevelsRarities[i].Discription));
                _minRemnantCustomLevelRarities.Last().Value = Mathf.Clamp(_minRemnantCustomLevelRarities.Last().Value, minPercentage, maxPercentage);
                _maxRemnantCustomLevelRarities.Add(_configFile.Bind(_customLevelsSection, _configCustomLevelsRarities[i + 1].Name, 25, _configCustomLevelsRarities[i + 1].Discription));
                _maxRemnantCustomLevelRarities.Last().Value = Mathf.Clamp(_maxRemnantCustomLevelRarities.Last().Value, _minRemnantCustomLevelRarities.Last().Value, maxPercentage);

                CustomLevelRarities.Add(RemoveEndSentence(_configCustomLevelsRarities[i].Name, " min remnant rarity"),
                    new Tuple<int, int>(_minRemnantCustomLevelRarities.Last().Value, _maxRemnantCustomLevelRarities.Last().Value));
            }

            _configFile.Save();
        }

        private void LoadConfigData()
        {
            var configRemnantSection = GetConfigSectionData(_remnantsSection);
            if (configRemnantSection != null || configRemnantSection.Count > 0)
            {
                _configRemnantDataPairList = configRemnantSection.ConvertAll(configData =>
                 new ConfigDataValue<int>
                 {
                     Name = configData.Name,
                     Discription = configData.Discription,
                     Value = Convert.ToInt32(configData.StringValue)
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

            var configSuitsSection = GetConfigSectionData(_suitsSection);
            if (configSuitsSection != null || configSuitsSection.Count > 0)
            {
                _configSuitsDataList = configSuitsSection.ConvertAll(configDataSuit =>
                 new ConfigDataValue<bool>
                 {
                     Name = configDataSuit.Name,
                     Discription = configDataSuit.Discription,
                     Value = Convert.ToBoolean(configDataSuit.StringValue)
                 });
            }
        }


        public List<string> GetBannedItemNames()
        {
            if (_bannedNamesFromRegistering.Value.IsNullOrWhiteSpace())
                return new List<string>();
            return _bannedNamesFromRegistering.Value.Split(',').ToList();
        }
        public List<string> GetOverriddenScrapItems()
        {
            if (_overriddenScrapItems.Value.IsNullOrWhiteSpace())
                return new List<string>();
            return _overriddenScrapItems.Value.Split(',').ToList();
        }

        public List<RemnantData> GetRemnantItemList(bool reloadConfig = true)
        {
            if (reloadConfig)
                _configFile.Reload();

            if (ConfigScrapDataList == null || ConfigScrapDataList.Count == 0)
                return new List<RemnantData>();

            return ConfigScrapDataList.ConvertAll(configEntry =>
            new RemnantData
            {
                RemnantItemName = configEntry.Definition.Key,
                RarityInfo = configEntry.Value
            });
        }


        public void SetRemnantItemList(List<RemnantData> remnantDataList)
        {
            ConfigScrapDataList = remnantDataList.ConvertAll(remnantData =>
         _configFile.Bind(_remnantsSection, remnantData.RemnantItemName, remnantData.RarityInfo, "By changing the value you can choose what kind of spawn rarity it is.\n -1 is the default using its store credits cost to calculate the rarity.\n 0 Is preventing from spawning it, and 1 to 100 is its costum rarity to spawn."));
            _configFile.Save();
        }

        public List<SuitData> GetSuitsList()
        {
            //_configFile.Reload();

            if (ConfigSuitsDataList == null || ConfigSuitsDataList.Count == 0)
                return new List<SuitData>();

            return ConfigSuitsDataList.ConvertAll(configEntry =>
            new SuitData
            {
                SuitName = configEntry.Definition.Key,
                UseSuit = configEntry.Value
            });
        }

        public void SetSuitsList(List<SuitData> suitDataList)
        {
            ConfigSuitsDataList = suitDataList.ConvertAll(suitData =>
         _configFile.Bind(_suitsSection, suitData.SuitName, suitData.UseSuit, "By changing the value, you can choose whether this suit is used on a body or not."));
            _configFile.Save();
        }



        public void SetCustomLevelsRarities(List<string> customMoonNames, int minPercentage = 1, int maxPercentage = 100)
        {
            foreach (var customMoonName in customMoonNames)
            {
                if (CustomLevelRarities.ContainsKey(customMoonName) || customMoonName == _bannedPlanetName)
                    continue;

                _minRemnantCustomLevelRarities.Add(_configFile.Bind(_customLevelsSection, customMoonName + " min remnant rarity", 5, "Minimum chance of a remnant item spawning on moon: " + customMoonName + " ."));
                _minRemnantCustomLevelRarities.Last().Value = Mathf.Clamp(_minRemnantCustomLevelRarities.Last().Value, minPercentage, maxPercentage);
                _maxRemnantCustomLevelRarities.Add(_configFile.Bind(_customLevelsSection, customMoonName + " max remnant rarity", 25, "Maximum chance of a remnant item spawning on moon: " + customMoonName + " ."));
                _maxRemnantCustomLevelRarities.Last().Value = Mathf.Clamp(_maxRemnantCustomLevelRarities.Last().Value, _minRemnantCustomLevelRarities.Last().Value, maxPercentage);
                CustomLevelRarities.Add(customMoonName,
                    new Tuple<int, int>(_minRemnantCustomLevelRarities.Last().Value, _maxRemnantCustomLevelRarities.Last().Value));
            }

            _configFile.Save();
        }

        public Dictionary<string, Tuple<int, int>> GetCustomLevelRarities()
        {
            _configFile.Reload();

            if (CustomLevelRarities == null || CustomLevelRarities.Count == 0)
                return new Dictionary<string, Tuple<int, int>>();

            for (int i = 0; i < _minRemnantCustomLevelRarities.Count; i++)
            {
                CustomLevelRarities[_minRemnantCustomLevelRarities[i].Definition.Key] = new Tuple<int, int>(_minRemnantCustomLevelRarities[i].Value, _maxRemnantCustomLevelRarities[i].Value);
            }

            return CustomLevelRarities;
        }


        private string RemoveEndSentence(string fullSentence, string toRemoveAtEnd)
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

        private List<ConfigData> GetConfigSectionData(string section)
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

        private void AddDataToList(StreamReader sr, string line, List<ConfigData> list)
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
