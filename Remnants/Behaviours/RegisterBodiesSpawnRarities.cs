using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace Remnants.Behaviours
{
    internal class RegisterBodiesSpawnRarities
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
            var enemiesAndBodiesNames = Data.LoadAssetsBodies.EnemiesAndBodiesNames;
            foreach (var level in startOfRound.levels)
            {
                if (PlanetsBodiesRarities.ContainsKey(level.PlanetName))
                    continue;

                RegisterBodiesToNewMoon(level);

            //    PlanetsBodiesRarities.Add(level.PlanetName, new Dictionary<string, int>());
            //    foreach (var enemyWithRarity in level.Enemies)
            //    {
            //        if (enemyWithRarity.enemyType.isOutsideEnemy)
            //            continue;

            //        int index = Mathf.Clamp(Array.FindIndex(enemiesAndBodiesNames, enemyBodyName => enemyBodyName.Key == enemyWithRarity.enemyType.enemyName), 0, Data.LoadAssetsBodies.EnemiesAndBodiesNames.Length - 1);
            //        if (PlanetsBodiesRarities[level.PlanetName].TryGetValue(enemiesAndBodiesNames[index].Value, out int value))
            //        {
            //            if (value < enemyWithRarity.rarity)
            //                PlanetsBodiesRarities[level.PlanetName][enemiesAndBodiesNames[index].Value] = enemyWithRarity.rarity;
            //        }
            //        else
            //        {
            //            PlanetsBodiesRarities[level.PlanetName][enemiesAndBodiesNames[index].Value] = enemyWithRarity.rarity;
            //        }
            //    }
            //    mls.LogInfo("Registered bodies to moon: " + level.PlanetName);
            }
            SceneManager.sceneLoaded -= RegisterBodiesToMoons;
            _isRegistering = false;
        }

        public static void RegisterBodiesToNewMoon(SelectableLevel newLevel)
        {
            //if (PlanetsBodiesRarities.ContainsKey(newLevel.PlanetName))
            //    return;
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
        #endregion
    }
}
