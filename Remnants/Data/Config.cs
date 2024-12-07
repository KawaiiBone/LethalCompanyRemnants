using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using LethalLib.Modules;
using Remnants.utilities;
using UnityEngine;
using LethalConfig;
using LethalConfig.ConfigItems.Options;
using LethalConfig.ConfigItems;

namespace Remnants.Data
{
    public class Config
    {
        #region Variables
        #region PublicData
        public ConfigEntry<int> MinRemnantRarity;
        public ConfigEntry<int> MaxRemnantRarity;
        public ConfigEntry<int> MinRemnantBatteryCharge;
        public ConfigEntry<int> MaxRemnantBatteryCharge;
        public List<ConfigEntry<int>> ConfigScrapDataList;
        public List<ConfigEntry<bool>> ConfigSuitsDataList;
        public ConfigEntry<bool> UseSpecificLevelRarities;
        public ConfigEntry<float> MaxRemnantItemCost;
        public ConfigEntry<int> SpawnRarityOfBody;
        public ConfigEntry<float> BodySpawnModifierRiskLevel;
        public ConfigEntry<int> RemnantScrapMinCostPercentage;
        public ConfigEntry<int> RemnantScrapMaxCostPercentage;
        public ConfigEntry<bool> ShouldSaveRemnantItems;
        public ConfigEntry<bool> ShouldDespawnRemnantItemsOnPartyWipe;
        public ConfigEntry<bool> ShouldAlwaysDespawnRemnantItems;
        public ConfigEntry<bool> ShouldBodiesBeScrap;
        public ConfigEntry<int> MinBodyScrapValue;
        public ConfigEntry<int> MaxBodyScrapValue;
        public Dictionary<Levels.LevelTypes, Tuple<int, int>> LevelRarities = new Dictionary<Levels.LevelTypes, Tuple<int, int>>();
        public Dictionary<string, Tuple<int, int>> CustomLevelRarities = new Dictionary<string, Tuple<int, int>>();
        public ConfigEntry<int> IncreasedScrapSpawnPool;
        public ConfigEntry<bool> UseLegacySpawning;
        public ConfigEntry<int> MinRemnantItemsSpawning;
        public ConfigEntry<int> MaxRemnantItemsSpawning;
        public ConfigEntry<float> RemnantItemsSpawningModifier;
        public ConfigEntry<int> MaxDuplicatesRemnantItems;
        public ConfigEntry<int> MinItemsFoundOnBodies;
        public ConfigEntry<int> MaxItemsFoundOnBodies;
        public ConfigEntry<bool> UseBeltBagTranspiler;
        public ConfigEntry<bool> UseTerminalScanItemsTranspiler;
        #endregion

        #region PrivateData
        private bool _hasInitialized = false;
        private List<ConfigEntry<int>> _minRemnantLevelRarities = new List<ConfigEntry<int>>();
        private List<ConfigEntry<int>> _maxRemnantLevelRarities = new List<ConfigEntry<int>>();
        private List<ConfigEntry<int>> _minRemnantCustomLevelRarities = new List<ConfigEntry<int>>();
        private List<ConfigEntry<int>> _maxRemnantCustomLevelRarities = new List<ConfigEntry<int>>();
        private List<ConfigDataValue<int>> _configRemnantDataPairList = new List<ConfigDataValue<int>>();
        private List<ConfigDataValue<int>> _configCustomLevelsRarities = new List<ConfigDataValue<int>>();
        private List<ConfigDataValue<bool>> _configSuitsDataList = new List<ConfigDataValue<bool>>();
        private ConfigEntry<string> _bannedNamesFromRegistering;
        private ConfigEntry<string> _overriddenScrapItems;
        private ConfigEntry<string> _bannedItemsFromSaving;
        private ConfigFile _configFile;
        private string _bannedPlanetName = "71 Gordion";
        private const string _LethalConfigName = "ainavt.lc.lethalconfig";
        #endregion

        #region PrivateConfigData
        private string _configFileName = "\\Remnants.cfg";
        private string _generalSection = "General";
        private string _generalBodySection = "GeneralBody";
        private string _levelsSection = "Levels";
        private string _customLevelsSection = "ModdedLevels";
        private string _otherSection = "Other";
        private string _remnantsSection = "Remnants";
        private string _spawningSection = "Spawning";
        private string _spawningLegacySection = "SpawningLegacy";
        private string _suitsSection = "Suits";
        private string _saveLoadSection = "Save/load";

        private const int _maxPercentage = 100;
        private const int _minPercentage = 1;
        private const int _maxRemnantItemsSpawned = 50;
        private const float _minItemCost = 5.0f;
        private const float _maxItemCost = 1000.0f;
        private const float _maxItemStoreCost = 10000.0f;
        private const float _maxModifierWithMoonThreath = 10.0f;
        private const int _maxDuplicatesOfARemnantItem = 15;
        private const int _maximumItemsFoundOnBody = 10;
        private const int _maximumScrapSpawnPoolIncrease = 30;
        private const int _defaultMinRarity = 1;
        private const int _defaultMaxRarity = 100;
        #endregion






        struct ConfigData
        {
            public string Name;
            public string Discription;
            public string StringValue;


            public ConfigData(string name, string discription, string stringValue)
            {
                this.Name = name;
                this.Discription = discription;
                this.StringValue = stringValue;
            }
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

            MinRemnantRarity = _configFile.Bind(_generalSection, "Min remnant item rarity", _defaultMinRarity, "Minimum chance of a remnant item spawning.");
            MinRemnantRarity.Value = Mathf.Clamp(MinRemnantRarity.Value, _minPercentage, _maxPercentage);

            MaxRemnantRarity = _configFile.Bind(_generalSection, "Max remnant item rarity", _defaultMaxRarity, "Maximum chance of a remnant item spawning.");
            MaxRemnantRarity.Value = Mathf.Clamp(MaxRemnantRarity.Value, MinRemnantRarity.Value, _maxPercentage);

            MinRemnantBatteryCharge = _configFile.Bind(_generalSection, "Min remnant battery charge", 20, "Minimum remnant item battery charge on first finding it.");
            MinRemnantBatteryCharge.Value = Mathf.Clamp(MinRemnantBatteryCharge.Value, _minPercentage, _maxPercentage);

            MaxRemnantBatteryCharge = _configFile.Bind(_generalSection, "Max remnant battery charge", 90, "Maximum remnant item battery charge on first finding it.");
            MaxRemnantBatteryCharge.Value = Mathf.Clamp(MaxRemnantBatteryCharge.Value, MinRemnantBatteryCharge.Value, _maxPercentage);

            RemnantScrapMinCostPercentage = _configFile.Bind(_generalSection, "Remnant item min scrap cost percentage", 5, "The min percentage of how much worth of scrap a remnant item is compared to its normal credit cost. \nFrom 0 percentage scrap cost to 1000 percentage.");
            RemnantScrapMinCostPercentage.Value = Mathf.Clamp(RemnantScrapMinCostPercentage.Value, 0, (int)_maxItemCost);

            RemnantScrapMaxCostPercentage = _configFile.Bind(_generalSection, "Remnant item max scrap cost percentage", 20, "The max percentage of how much worth of scrap a remnant item is compared to its normal credit cost. \nFrom 0 percentage scrap cost to 1000 percentage.");
            RemnantScrapMaxCostPercentage.Value = Mathf.Clamp(RemnantScrapMaxCostPercentage.Value, RemnantScrapMinCostPercentage.Value, (int)_maxItemCost);

            ShouldBodiesBeScrap = _configFile.Bind(_generalBodySection, "Should bodies be scrap", true, "When the bodies are a scrap they can be grabbed, have a scrap value and can be sold. \nIf not then it becomes a prop and cannot be interacted with.");

            SpawnRarityOfBody = _configFile.Bind(_generalBodySection, "Body spawn rarity", 2, "This number is the chance that a body spawns next to an remnant item.");
            SpawnRarityOfBody.Value = Mathf.Clamp(SpawnRarityOfBody.Value, 0, _maxPercentage);

            BodySpawnModifierRiskLevel = _configFile.Bind(_generalBodySection, "Body spawn modifier per moon risk level", 1.2f, "By increasing this modifier you will increase the spawnchance of the body per risk level moon.");
            BodySpawnModifierRiskLevel.Value = Mathf.Clamp(BodySpawnModifierRiskLevel.Value, 0.0f, _maxModifierWithMoonThreath);

            MinBodyScrapValue = _configFile.Bind(_generalBodySection, "Min scrap value of the bodies", 8, "The minimum scrap value of the bodies that this mod spawns. \nThis only works if the bodies are set to scrap.");
            MinBodyScrapValue.Value = Mathf.Clamp(MinBodyScrapValue.Value, 0, (int)_maxItemCost);

            MaxBodyScrapValue = _configFile.Bind(_generalBodySection, "Max scrap value of the bodies", 25, "The maximum scrap value of the bodies that this mod spawns. \nThis only works if the bodies are set to scrap.");
            MaxBodyScrapValue.Value = Mathf.Clamp(MaxBodyScrapValue.Value, MinBodyScrapValue.Value, (int)_maxItemCost);

            MaxRemnantItemCost = _configFile.Bind(_otherSection, "Max value to calculate rarity", 400.0f, "This value helps calculating the spawn rarity, the rarity is calculated by the credit cost of the shop item. \nThis caps the maximum cost of an item and setting it as min spawn rarity if it is the same or higher than this value. \nThe more an item cost, the less spawn chance/spawn rarity it has.");
            MaxRemnantItemCost.Value = Mathf.Clamp(MaxRemnantItemCost.Value, _minItemCost, _maxItemStoreCost);

            _bannedNamesFromRegistering = _configFile.Bind(_otherSection, "Item list banned from registering as scrap", "Clipboard,StickyNote,Binoculars,MapDevice,Key,Error", "List of items that are barred from registering as scrap/remnant item. \nThese default items are there to avoid adding scrap that are left out of the vanilla version, don't work, or cause crashes. \nTo add more names to the list, be sure to add a comma between names.");
            _overriddenScrapItems = _configFile.Bind(_otherSection, "Scrap item list to be used as remnant items", "Example scrap,Scrap-example", "In here you can add scrap items to be treated as remnant items, to spawn bodies on and to randomize batteries. \nTo add more names to the list, be sure to add a comma between names.");
            UseBeltBagTranspiler = _configFile.Bind(_otherSection, "Beltbag can store remnant items", true, "Make the beltbag item able to store remnant items. You can disable this feature to make other mods for the beltbag item more compatible.");
            UseTerminalScanItemsTranspiler = _configFile.Bind(_otherSection, "Terminal can scan remnant items", true, "Make the Terminal able to  scan remnant items. You can disable this feature to make other mods that interact with the terminal more compatible.");

            MinRemnantItemsSpawning = _configFile.Bind(_spawningSection, "Minimum remnant items spawned on a moon", 3, "The minimum remnant items that can spawn on a moon. \nThis value gets increased by the threat level a moon has, along the down below modifier.");
            MinRemnantItemsSpawning.Value = Mathf.Clamp(MinRemnantItemsSpawning.Value, 0, _maxRemnantItemsSpawned);
            MaxRemnantItemsSpawning = _configFile.Bind(_spawningSection, "Maximum remnant items spawned on a moon", 8, "The maximum remnant items that can spawn on a moon. \nThis value gets increased by the threat level a moon has, along the down below modifier.");
            MaxRemnantItemsSpawning.Value = Mathf.Clamp(MaxRemnantItemsSpawning.Value, MinRemnantItemsSpawning.Value, _maxRemnantItemsSpawned);

            RemnantItemsSpawningModifier = _configFile.Bind(_spawningSection, "Remnant items spawn modifier", 1.0f, "A modifier that increases the spawn pool of remnant items in relative to the moon threat level. \nPutting the value under zero, will disable this feature and will always use the normal spawn amount.");
            RemnantItemsSpawningModifier.Value = Mathf.Clamp(RemnantItemsSpawningModifier.Value, -1.0f, _maxModifierWithMoonThreath);

            MaxDuplicatesRemnantItems = _configFile.Bind(_spawningSection, "Maximum duplicates can spawn", 4, "The maximum duplicates of a remnant item that can spawn on a moon. \nDo note that the spawning of remnant items will stop when it has used up all maximum duplicates.");
            MaxDuplicatesRemnantItems.Value = Mathf.Clamp(MaxDuplicatesRemnantItems.Value, 1, _maxDuplicatesOfARemnantItem);


            MinItemsFoundOnBodies = _configFile.Bind(_spawningSection, "Minimum remnant items found on a body", 1, "The Minimum remnant items found on a body.");
            MinItemsFoundOnBodies.Value = Mathf.Clamp(MinItemsFoundOnBodies.Value, 1, _maximumItemsFoundOnBody);

            MaxItemsFoundOnBodies = _configFile.Bind(_spawningSection, "Maximum remnant items found on a body", 4, "The maximum remnant items found on a body.");
            MaxItemsFoundOnBodies.Value = Mathf.Clamp(MaxItemsFoundOnBodies.Value, MinItemsFoundOnBodies.Value, _maximumItemsFoundOnBody);

            UseLegacySpawning = _configFile.Bind(_spawningLegacySection, "Use legacy spawning", false, "Chooses if you want to use the older version of spawning remnant items. \nThe older version spawns along the normal scrap, which can be in lockers. \nWhile this is active this means that the new version will be disabled.");
            IncreasedScrapSpawnPool = _configFile.Bind(_spawningLegacySection, "Max increase scrap spawn pool", 15, "Increases the total scrap spawn pool to accommodate the remnant items spawning along the scrap items. \nThis is intended to make sure you get enough scrap value per moon.");
            IncreasedScrapSpawnPool.Value = Mathf.Clamp(IncreasedScrapSpawnPool.Value, 0, _maximumScrapSpawnPoolIncrease);

            _minRemnantLevelRarities = new List<ConfigEntry<int>>();
            _maxRemnantLevelRarities = new List<ConfigEntry<int>>();
            LevelRarities = new Dictionary<Levels.LevelTypes, Tuple<int, int>>();
            UseSpecificLevelRarities = _configFile.Bind(_levelsSection, "Use level specific rarities", false);
            foreach (var moonName in Enum.GetNames(typeof(Levels.LevelTypes)))
            {
                if (moonName == Levels.LevelTypes.All.ToString() || moonName == Levels.LevelTypes.None.ToString()
                    || moonName == Levels.LevelTypes.Vanilla.ToString() || moonName == Levels.LevelTypes.Modded.ToString())
                    continue;

                _minRemnantLevelRarities.Add(_configFile.Bind(_levelsSection, moonName + " min remnant rarity", _defaultMinRarity, "Minimum chance of a remnant item spawning on moon: " + moonName + "."));
                _minRemnantLevelRarities.Last().Value = Mathf.Clamp(_minRemnantLevelRarities.Last().Value, _minPercentage, _maxPercentage);
                _maxRemnantLevelRarities.Add(_configFile.Bind(_levelsSection, moonName + " max remnant rarity", _defaultMaxRarity, "Maximum chance of a remnant item spawning on moon: " + moonName + "."));
                _maxRemnantLevelRarities.Last().Value = Mathf.Clamp(_maxRemnantLevelRarities.Last().Value, _minRemnantLevelRarities.Last().Value, _maxPercentage);
                LevelRarities.Add((Levels.LevelTypes)Enum.Parse(typeof(Levels.LevelTypes), moonName),
                    new Tuple<int, int>(_minRemnantLevelRarities.Last().Value, _maxRemnantLevelRarities.Last().Value));
            }

            ShouldSaveRemnantItems = _configFile.Bind(_saveLoadSection, "Save remnant items", true, "This ensures that the remnant items are saved in the ship when you reload the lobby.");
            ShouldDespawnRemnantItemsOnPartyWipe = _configFile.Bind(_saveLoadSection, "Despawn remnant items on party wipe", true, "On party wipe all items are despawned from the ship, this ensures that remnant items also are despawned. \nIf you use a mod that prevents items from being despawned, here you can edit it too for remnant items. \nThis will not use the transpiler for cleaning up items, and may cause issues.");
            ShouldAlwaysDespawnRemnantItems = _configFile.Bind(_saveLoadSection, "Always despawn remnant items", false, "Despawns all remnant items when going away from moons, even if it is in the shiproom and you are holding it.");
            _bannedItemsFromSaving = _configFile.Bind(_saveLoadSection, "Item list banned from saving", "Clipboard,StickyNote,Binoculars,MapDevice,Error", "List of items that are barred saving on the ship. \nThese default items are there to avoid issues with saving items on the ship. \nTo add more names to the list, be sure to add a comma between names.");

            ConfigScrapDataList = _configRemnantDataPairList.ConvertAll(itemData =>
                     _configFile.Bind(_remnantsSection, itemData.Name, -1, "Set here the spawn rarity.\n -1 is the default using its store credits cost to calculate the rarity.\n 0 Is preventing from spawning it, and 1 to 100 is its costum rarity to spawn."));
            for (int i = 0; i < ConfigScrapDataList.Count; i++)
            {
                ConfigScrapDataList[i].Value = Mathf.Clamp(ConfigScrapDataList[i].Value, -1, _maxPercentage);
            }


            ConfigSuitsDataList = _configSuitsDataList.ConvertAll(itemData =>
                     _configFile.Bind(_suitsSection, itemData.Name, true, itemData.Discription));

            _minRemnantCustomLevelRarities = new List<ConfigEntry<int>>();
            _maxRemnantCustomLevelRarities = new List<ConfigEntry<int>>();
            for (int i = 0; i < _configCustomLevelsRarities.Count; i += 2)
            {
                _minRemnantCustomLevelRarities.Add(_configFile.Bind(_customLevelsSection, _configCustomLevelsRarities[i].Name, _defaultMinRarity, _configCustomLevelsRarities[i].Discription));
                _minRemnantCustomLevelRarities.Last().Value = Mathf.Clamp(_minRemnantCustomLevelRarities.Last().Value, _minPercentage, _maxPercentage);
                _maxRemnantCustomLevelRarities.Add(_configFile.Bind(_customLevelsSection, _configCustomLevelsRarities[i + 1].Name, _defaultMaxRarity, _configCustomLevelsRarities[i + 1].Discription));
                _maxRemnantCustomLevelRarities.Last().Value = Mathf.Clamp(_maxRemnantCustomLevelRarities.Last().Value, _minRemnantCustomLevelRarities.Last().Value, _maxPercentage);

                CustomLevelRarities.Add(RemoveEndSentence(_configCustomLevelsRarities[i].Name, " min remnant rarity"),
                    new Tuple<int, int>(_minRemnantCustomLevelRarities.Last().Value, _maxRemnantCustomLevelRarities.Last().Value));
            }

            var mls = Remnants.Instance.Mls;
          
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(_LethalConfigName))
            {
                mls.LogInfo("LethalConfig found, creating lethal config items");
                CreateLethalConfigItems();
            }
            else
            {
                mls.LogInfo("LethalConfig not found");
            }

            _configFile.Save();
        }

        private void LoadConfigData()
        {
            var configRemnantSection = GetConfigSectionData(_remnantsSection);
            if (configRemnantSection != null || configRemnantSection.Count > 0)
            {
                //Adding a safe conversion from the previous setting
                string stringValueFalse = "false";
                string stringValueTrue = "true";
                string stringDefaultValue = "-1";
                for (int i = 0; i < configRemnantSection.Count; i++)
                {
                    if (configRemnantSection[i].StringValue.Contains(stringValueTrue) || configRemnantSection[i].StringValue.Contains(stringValueFalse))
                    {
                        ConfigData configData = configRemnantSection[i];
                        configData.StringValue = stringDefaultValue;
                        configRemnantSection[i] = configData;
                    }
                }

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

        private void CreateLethalConfigItems()
        {
            //General section
            var minRemnantRaritySlider = new IntSliderConfigItem(MinRemnantRarity, new IntSliderOptions  {Min = _minPercentage, Max = _maxPercentage});
            LethalConfigManager.AddConfigItem(minRemnantRaritySlider);
            var maxRemnantRaritySlider = new IntSliderConfigItem(MaxRemnantRarity, new IntSliderOptions { Min = MinRemnantRarity.Value, Max = _maxPercentage });
            LethalConfigManager.AddConfigItem(maxRemnantRaritySlider);

            var minRemnantBatteryChargeSlider = new IntSliderConfigItem(MinRemnantBatteryCharge, new IntSliderOptions { Min = _minPercentage, Max = _maxPercentage });
            LethalConfigManager.AddConfigItem(minRemnantBatteryChargeSlider);
            var maxRemnantBatteryChargeSlider = new IntSliderConfigItem(MaxRemnantBatteryCharge, new IntSliderOptions { Min = MinRemnantBatteryCharge.Value, Max = _maxPercentage });
            LethalConfigManager.AddConfigItem(maxRemnantBatteryChargeSlider);

            var remnantScrapMinCostPercentageSlider = new IntSliderConfigItem(RemnantScrapMinCostPercentage, new IntSliderOptions { Min = 0, Max = (int)_maxItemCost });
            LethalConfigManager.AddConfigItem(remnantScrapMinCostPercentageSlider);
            var remnantScrapMaxCostPercentageSlider = new IntSliderConfigItem(RemnantScrapMaxCostPercentage, new IntSliderOptions { Min = RemnantScrapMinCostPercentage.Value, Max = (int)_maxItemCost });
            LethalConfigManager.AddConfigItem(remnantScrapMaxCostPercentageSlider);
            //GeneralBody section
            var shouldBodiesBeScrapCheckBox = new BoolCheckBoxConfigItem(ShouldBodiesBeScrap, false);
            LethalConfigManager.AddConfigItem(shouldBodiesBeScrapCheckBox);

            var spawnRarityOfBodySlider = new IntSliderConfigItem(SpawnRarityOfBody, new IntSliderOptions { Min = 0, Max = _maxPercentage, RequiresRestart = false });
            LethalConfigManager.AddConfigItem(spawnRarityOfBodySlider);

            var bodySpawnModifierRiskLeveSlider = new FloatSliderConfigItem(BodySpawnModifierRiskLevel, new FloatSliderOptions { Min = 0.0f, Max = _maxModifierWithMoonThreath, RequiresRestart = false });
            LethalConfigManager.AddConfigItem(bodySpawnModifierRiskLeveSlider);
           
            var minBodyScrapValueSlider = new IntSliderConfigItem(MinBodyScrapValue, new IntSliderOptions { Min = 0, Max = (int)_maxItemCost, RequiresRestart = false });
            LethalConfigManager.AddConfigItem(minBodyScrapValueSlider);

            var maxBodyScrapValueSlider = new IntSliderConfigItem(MaxBodyScrapValue, new IntSliderOptions { Min = MinBodyScrapValue.Value, Max = (int)_maxItemCost, RequiresRestart = false });
            LethalConfigManager.AddConfigItem(maxBodyScrapValueSlider);
            //Spawning section
            var minRemnantItemsSpawningSlider = new IntSliderConfigItem(MinRemnantItemsSpawning, new IntSliderOptions { Min = 1, Max = _maxRemnantItemsSpawned, RequiresRestart = false });
            LethalConfigManager.AddConfigItem(minRemnantItemsSpawningSlider);
            var maxRemnantItemsSpawningSlider = new IntSliderConfigItem(MaxRemnantItemsSpawning, new IntSliderOptions { Min = MinRemnantItemsSpawning.Value, Max = _maxRemnantItemsSpawned, RequiresRestart = false });
            LethalConfigManager.AddConfigItem(maxRemnantItemsSpawningSlider);

            var remnantItemsSpawningModifierSlider = new FloatSliderConfigItem(RemnantItemsSpawningModifier, new FloatSliderOptions { Min = -1.0f, Max = _maxModifierWithMoonThreath, RequiresRestart = false });
            LethalConfigManager.AddConfigItem(remnantItemsSpawningModifierSlider);

            var maxDuplicatesRemnantItemsSlider = new IntSliderConfigItem(MaxDuplicatesRemnantItems, new IntSliderOptions { Min = 1, Max = _maxDuplicatesOfARemnantItem, RequiresRestart = false });
            LethalConfigManager.AddConfigItem(maxDuplicatesRemnantItemsSlider);

            var minItemsFoundOnBodiesSlider = new IntSliderConfigItem(MinItemsFoundOnBodies, new IntSliderOptions { Min = 1, Max = _maximumItemsFoundOnBody, RequiresRestart = false });
            LethalConfigManager.AddConfigItem(minItemsFoundOnBodiesSlider);
            var maxItemsFoundOnBodiesSlider = new IntSliderConfigItem(MaxItemsFoundOnBodies, new IntSliderOptions { Min = MinItemsFoundOnBodies.Value, Max = _maximumItemsFoundOnBody, RequiresRestart = false });
            LethalConfigManager.AddConfigItem(maxItemsFoundOnBodiesSlider);
            //Legacy spawning section
            var useLegacySpawningBox = new BoolCheckBoxConfigItem(UseLegacySpawning);
            LethalConfigManager.AddConfigItem(useLegacySpawningBox);

            var increasedScrapSpawnPoolSlider = new IntSliderConfigItem(IncreasedScrapSpawnPool, new IntSliderOptions { Min = 0, Max = _maximumScrapSpawnPoolIncrease, RequiresRestart = false });
            LethalConfigManager.AddConfigItem(increasedScrapSpawnPoolSlider);
            //Saving section
            var shouldSaveRemnantItemsBox = new BoolCheckBoxConfigItem(ShouldSaveRemnantItems);
            LethalConfigManager.AddConfigItem(shouldSaveRemnantItemsBox);

            var shouldDespawnRemnantItemsOnPartyWipeBox = new BoolCheckBoxConfigItem(ShouldDespawnRemnantItemsOnPartyWipe);
            LethalConfigManager.AddConfigItem(shouldDespawnRemnantItemsOnPartyWipeBox);

            var shouldAlwaysDespawnRemnantItemsBox = new BoolCheckBoxConfigItem(ShouldAlwaysDespawnRemnantItems);
            LethalConfigManager.AddConfigItem(shouldAlwaysDespawnRemnantItemsBox);

            var bannedItemsFromSavingInput = new TextInputFieldConfigItem(_bannedItemsFromSaving);
            LethalConfigManager.AddConfigItem(bannedItemsFromSavingInput);
            //Other section
            var maxRemnantItemCostSlider = new FloatSliderConfigItem(MaxRemnantItemCost, new FloatSliderOptions { Min = _minItemCost, Max = _maxItemStoreCost });
            LethalConfigManager.AddConfigItem(maxRemnantItemCostSlider);

            var bannedNamesFromRegisteringInput = new TextInputFieldConfigItem(_bannedNamesFromRegistering);
            LethalConfigManager.AddConfigItem(bannedNamesFromRegisteringInput);

            var overriddenScrapItemsInput = new TextInputFieldConfigItem(_overriddenScrapItems, false);
            LethalConfigManager.AddConfigItem(overriddenScrapItemsInput);
            
            var UseBeltBagTranspilerCheckBox =new BoolCheckBoxConfigItem(UseBeltBagTranspiler, true);
            LethalConfigManager.AddConfigItem(UseBeltBagTranspilerCheckBox);

            var UseTerminalScanItemsTranspilerCheckBox = new BoolCheckBoxConfigItem(UseTerminalScanItemsTranspiler, true);
            LethalConfigManager.AddConfigItem(UseTerminalScanItemsTranspilerCheckBox);
            //Remnant items section
            for (int i = 0; i < ConfigScrapDataList.Count; i++)
            {
                var remnantItemSpawnData = new IntSliderConfigItem(ConfigScrapDataList[i], new IntSliderOptions { Min = -1, Max = _maxPercentage });
                LethalConfigManager.AddConfigItem(remnantItemSpawnData);
            }

            //Moons vanilla section
            var useSpecificLevelRaritiesInput = new BoolCheckBoxConfigItem(UseSpecificLevelRarities);
            LethalConfigManager.AddConfigItem(useSpecificLevelRaritiesInput);

            for (int i = 0; i < _minRemnantLevelRarities.Count; i++)
            {
                var minRemnantLevelRaritiesSlider = new IntSliderConfigItem(_minRemnantLevelRarities[i], new IntSliderOptions { Min = _minPercentage, Max = _maxPercentage});
                var maxRemnantLevelRaritiesSlider = new IntSliderConfigItem(_maxRemnantLevelRarities[i], new IntSliderOptions { Min = _minRemnantLevelRarities[i].Value, Max = _maxPercentage });
                LethalConfigManager.AddConfigItem(minRemnantLevelRaritiesSlider);
                LethalConfigManager.AddConfigItem(maxRemnantLevelRaritiesSlider);
            }
            //Costum Moons section
            for (int i = 0; i < _minRemnantCustomLevelRarities.Count; i++)
            {
                var minRemnantLevelRaritiesSlider = new IntSliderConfigItem(_minRemnantCustomLevelRarities[i], new IntSliderOptions { Min = _minPercentage, Max = _maxPercentage });
                var maxRemnantLevelRaritiesSlider = new IntSliderConfigItem(_maxRemnantCustomLevelRarities[i], new IntSliderOptions { Min = _minRemnantCustomLevelRarities[i].Value, Max = _maxPercentage });
                LethalConfigManager.AddConfigItem(minRemnantLevelRaritiesSlider);
                LethalConfigManager.AddConfigItem(maxRemnantLevelRaritiesSlider);
            }
        }



        public List<string> GetBannedFromRegisteringItemNames()
        {
            if (_bannedNamesFromRegistering.Value.IsNullOrWhiteSpace())
                return new List<string>();
            return _bannedNamesFromRegistering.Value.Split(',').ToList();
        }

        public List<string> GetBannedFromSavingItemNames()
        {
            if (_bannedItemsFromSaving.Value.IsNullOrWhiteSpace())
                return new List<string>();
            return _bannedItemsFromSaving.Value.Split(',').ToList();
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
         _configFile.Bind(_remnantsSection, remnantData.RemnantItemName, remnantData.RarityInfo, "Set here the spawn rarity.\n -1 is the default using its store credits cost to calculate the rarity.\n 0 Is preventing from spawning it, and 1 to 100 is its costum rarity to spawn."));
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

                _minRemnantCustomLevelRarities.Add(_configFile.Bind(_customLevelsSection, customMoonName + " min remnant rarity", _defaultMinRarity, "Minimum chance of a remnant item spawning on moon: " + customMoonName + "."));
                _minRemnantCustomLevelRarities.Last().Value = Mathf.Clamp(_minRemnantCustomLevelRarities.Last().Value, minPercentage, maxPercentage);
                _maxRemnantCustomLevelRarities.Add(_configFile.Bind(_customLevelsSection, customMoonName + " max remnant rarity", _defaultMaxRarity, "Maximum chance of a remnant item spawning on moon: " + customMoonName + "."));
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

        
       public bool CheckIsStoreOrRemnantItem(GrabbableObject grabbableObject)
        {
            var mls = Remnants.Instance.Mls;
            if (grabbableObject == null || grabbableObject.itemProperties == null)
            {
                mls.LogError("GrabbableObject is null!");
                return false;
            }
            if (!grabbableObject.itemProperties.isScrap)
            {
                mls.LogError("GrabbableObject is not scrap!");
                return true;
            }
            mls.LogInfo("ConfigScrapDataList lengt: " + ConfigScrapDataList.Count);
            foreach (var item in ConfigScrapDataList)//IS EMPTY? :O
            {
                mls.LogInfo(item.Definition.Key);
            }

            mls.LogWarning(grabbableObject.itemProperties.itemName + " " + grabbableObject.itemProperties.name);


             if (ConfigScrapDataList.FindIndex(configEntry => configEntry.Definition.Key == grabbableObject.itemProperties.itemName 
             || configEntry.Definition.Key == grabbableObject.itemProperties.name) != -1)

            {
                mls.LogError("GrabbableObject is a remnant item!");
                return true;
            }
            mls.LogError("GrabbableObject is a normal scrap item!");
            return false;
        }


        #endregion
    }
}
