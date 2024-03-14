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
            _isRegistering = true;
            var enemiesAndBodiesNames = Data.LoadAssetsBodies.EnemiesAndBodiesNames;
            foreach (var level in startOfRound.levels)
            {
                PlanetsBodiesRarities.Add(level.PlanetName, new Dictionary<string, int>());
                foreach (var enemyWithRarity in level.Enemies)
                {
                    if (enemyWithRarity.enemyType.isOutsideEnemy)
                        continue;

                    int index = Mathf.Clamp( Array.FindIndex(enemiesAndBodiesNames, enemyBodyName => enemyBodyName.Key == enemyWithRarity.enemyType.enemyName), 0, Data.LoadAssetsBodies.EnemiesAndBodiesNames.Length-1);           
                    if (PlanetsBodiesRarities[level.PlanetName].TryGetValue(enemiesAndBodiesNames[index].Value, out int value))
                    {
                        if(value < enemyWithRarity.rarity)
                            PlanetsBodiesRarities[level.PlanetName][enemiesAndBodiesNames[index].Value] = enemyWithRarity.rarity;
                    }
                    else
                    {
                        PlanetsBodiesRarities[level.PlanetName][enemiesAndBodiesNames[index].Value] = enemyWithRarity.rarity;
                    }
                }
            }
            SceneManager.sceneLoaded -= RegisterBodiesToMoons;
            _isRegistering = false;
        }
        #endregion
    }
}
