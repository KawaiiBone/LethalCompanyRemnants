using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using LethalLib.Modules;
using Unity.Netcode;
using BepInEx;

namespace Remnants.Behaviours
{
    internal class RegisterBodiesSpawnBehaviour
    {

        #region Variables
        private bool _hasInitialized = false;
        private bool _isRegistering = false;
        public static Dictionary<string, Dictionary<string, int>> PlanetsBodiesRarities = new Dictionary<string, Dictionary<string, int>>();
        #endregion

        #region Initialize 
        public void Initialize()
        {
            if (!_hasInitialized)
            {
                _hasInitialized = true;
                SceneManager.sceneLoaded += RegisterBodiesToMoons;
            }
        }
        #endregion

        #region Methods
        private void RegisterBodiesToMoons(Scene scene, LoadSceneMode mode)
        {
            var mls = Remnants.Instance.Mls;
            var startOfRound = StartOfRound.Instance;
            if (startOfRound == null || _isRegistering)
            {
                mls.LogInfo("startOfRound is null or is already registering bodies to moons.");
                return;
            }
            if (!Data.LoadAssetsBodies.HasLoadedAnyAssets)
            {
                mls.LogWarning("Did not load any body assets, skipping registering bodies");
                return;
            }


            mls.LogInfo("Registering bodies to moons");
            _isRegistering = true;
            List<string> planetNames = new List<string>();
            foreach (var level in startOfRound.levels)
            {
                if (HasIllegalCharacters(level.PlanetName) || PlanetsBodiesRarities.ContainsKey(level.PlanetName))
                    continue;

                RegisterBodiesToNewMoon(level);
                if(!Enum.TryParse(level.name, out Levels.LevelTypes a))
                    planetNames.Add(level.PlanetName);
            }
            Data.Config.SetCustomLevelsRarities(planetNames);
            SceneManager.sceneLoaded -= RegisterBodiesToMoons;
            _isRegistering = false;
        }

        public static void RegisterBodiesToNewMoon(SelectableLevel newLevel)
        {
            var mls = Remnants.Instance.Mls;
            mls.LogInfo("New moon found attempting to register bodies to moon: " + newLevel.PlanetName);
            PlanetsBodiesRarities.Add(newLevel.PlanetName, new Dictionary<string, int>());
            var enemiesAndBodiesNames = Data.LoadAssetsBodies.EnemiesAndBodiesNames;
            foreach (var enemyWithRarity in newLevel.Enemies)
            {
                if (enemyWithRarity.enemyType.isOutsideEnemy)
                    continue;

                int index = Mathf.Clamp(Array.FindIndex(enemiesAndBodiesNames, enemyBodyName => enemyBodyName.Key == enemyWithRarity.enemyType.enemyName), 0, Data.LoadAssetsBodies.EnemiesAndBodiesNames.Length - 1);
                if (PlanetsBodiesRarities[newLevel.PlanetName].TryGetValue(enemiesAndBodiesNames[index].Value, out int value))
                {
                    if (value < enemyWithRarity.rarity)
                        PlanetsBodiesRarities[newLevel.PlanetName][enemiesAndBodiesNames[index].Value] = enemyWithRarity.rarity;
                }
                else
                {
                    PlanetsBodiesRarities[newLevel.PlanetName][enemiesAndBodiesNames[index].Value] = enemyWithRarity.rarity;
                }
            }
            mls.LogInfo("Registered bodies to moon: " + newLevel.PlanetName);
        }

        public static bool HasIllegalCharacters(string name)
        {
            char[] chars = new char[] { '=', '\n', '\t', '\\', '\"', '\'', '[', ']'};
            if (name.IsNullOrWhiteSpace()) 
                return true;

            return name.IndexOfAny(chars) != -1;
        }

        #endregion
    }
}
