using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using LethalLib.Modules;
using BepInEx;
using System.Linq;

namespace Remnants.Behaviours
{
    public class RegisterBodiesSpawnBehaviour
    {
        #region Variables
        private bool _hasInitialized = false;
        private bool _isRegisteringToMoons = false;
        private bool _isRegisteringToCostumMoons = false;
        public Dictionary<string, Dictionary<string, int>> PlanetsBodiesRarities = new Dictionary<string, Dictionary<string, int>>();
        private char[] _illegalChars = new char[] { '=', '\n', '\t', '\\', '\"', '\'', '[', ']' };
        private List<string> _customMoonsNames = new List<string>();
        #endregion

        #region Initialize 
        public void Initialize()
        {
            if (!_hasInitialized)
            {
                _hasInitialized = true;
                SceneManager.sceneLoaded += RegisterBodiesToMoons;
                SceneManager.sceneLoaded += RegisterBodiesToCostumMoons;
            }
        }
        #endregion

        #region PrivateMethods
        private void RegisterBodiesToMoons(Scene scene, LoadSceneMode mode)
        {
            var mls = Remnants.Instance.Mls;
            var startOfRound = StartOfRound.Instance;
            if (startOfRound == null || _isRegisteringToMoons)
                return;
            if (!Remnants.Instance.LoadBodyAssets.HasLoadedAnyAssets)
                return;

            mls.LogInfo("Registering bodies to moons");
            _isRegisteringToMoons = true;
            RegisterMoonsData(startOfRound.levels);
            SceneManager.sceneLoaded -= RegisterBodiesToMoons;
            _isRegisteringToMoons = false;
        }

        private void RegisterBodiesToCostumMoons(Scene scene, LoadSceneMode mode)
        {
            var mls = Remnants.Instance.Mls;
            if (StartOfRound.Instance != null)
            {
                SceneManager.sceneLoaded -= RegisterBodiesToCostumMoons;
                Remnants.Instance.RemnantsConfig.SetCustomLevelsRarities(_customMoonsNames);
                return;
            }

            if (_isRegisteringToCostumMoons)
                return;

            List<SelectableLevel> selectableLevels = Resources.FindObjectsOfTypeAll<SelectableLevel>().Concat(UnityEngine.Object.FindObjectsByType<SelectableLevel>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)).ToList();
            if (selectableLevels == null || selectableLevels.Count == 0)
                return;

            _isRegisteringToCostumMoons = true;
            mls.LogInfo("Registering custom moons data.");
            foreach (var selectableLevel in selectableLevels)
            {

                if (!HasIllegalCharacters(selectableLevel.PlanetName) && !PlanetsBodiesRarities.ContainsKey(selectableLevel.PlanetName))
                {
                    RegisterBodiesToNewMoon(selectableLevel);
                    _customMoonsNames.Add(selectableLevel.PlanetName);
                }
            }
            mls.LogInfo("Custom moons data registered.");
            _isRegisteringToCostumMoons = false;
        }

        private void RegisterMoonsData(SelectableLevel[] levels)
        {
            List<string> planetNames = new List<string>();
            foreach (var level in levels)
            {
                if (HasIllegalCharacters(level.PlanetName) || PlanetsBodiesRarities.ContainsKey(level.PlanetName))
                    continue;

                RegisterBodiesToNewMoon(level);
                if (!Enum.TryParse(level.name, out Levels.LevelTypes a))
                    planetNames.Add(level.PlanetName);
            }
            Remnants.Instance.RemnantsConfig.SetCustomLevelsRarities(planetNames);
        }
        #endregion

        #region PublicMethods
        public void RegisterBodiesToNewMoon(SelectableLevel newLevel)
        {
            var mls = Remnants.Instance.Mls;
            mls.LogInfo("New moon found attempting to register bodies to moon: " + newLevel.PlanetName);
            PlanetsBodiesRarities.Add(newLevel.PlanetName, new Dictionary<string, int>());
            var enemiesAndBodiesNames = Remnants.Instance.LoadBodyAssets.EnemiesAndBodiesNames;
            foreach (var enemyWithRarity in newLevel.Enemies)
            {
                if (enemyWithRarity.enemyType.isOutsideEnemy)
                    continue;
                //If it can't find the enemy in the array, then that means it will be defaulted which is index 0, first in array
                int index = Mathf.Clamp(Array.FindIndex(enemiesAndBodiesNames, enemyBodyName => enemyBodyName.Key == enemyWithRarity.enemyType.enemyName), 0, enemiesAndBodiesNames.Length - 1);
                if (PlanetsBodiesRarities[newLevel.PlanetName].TryGetValue(enemiesAndBodiesNames[index].Value, out int rarityValue))
                {
                    if (rarityValue < enemyWithRarity.rarity)
                        PlanetsBodiesRarities[newLevel.PlanetName][enemiesAndBodiesNames[index].Value] = enemyWithRarity.rarity;
                }
                else
                {
                    PlanetsBodiesRarities[newLevel.PlanetName][enemiesAndBodiesNames[index].Value] = enemyWithRarity.rarity;
                }
            }
            mls.LogInfo("Registered bodies to moon: " + newLevel.PlanetName);
        }

        public bool HasIllegalCharacters(string name)
        {
            if (name.IsNullOrWhiteSpace()) 
                return true;

            return name.IndexOfAny(_illegalChars) != -1;
        }

       
        #endregion
    }
}
