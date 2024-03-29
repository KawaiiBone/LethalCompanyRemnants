﻿using LethalLib.Modules;
using Remnants.Data;
using Remnants.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Remnants.Behaviours
{
    internal class RegisterCustomMoonsData
    {
        #region Variables
        private bool _hasInitialized = false;
        private bool _isRegistering = false;
        private List<string> _customMoonsNames = new List<string>();
        #endregion

        #region Initialize 
        public void Initialize()
        {
            if (!_hasInitialized)
            {
                _hasInitialized = true;
                SceneManager.sceneLoaded += RegisterMoonsData;
            }
        }
        #endregion

        #region Methods
        private void RegisterMoonsData(Scene scene, LoadSceneMode mode)
        {
            var mls = Remnants.Instance.Mls;
            if (StartOfRound.Instance != null)
            {
                mls.LogInfo("StartOfRound found, registering no more moons data.");
                SceneManager.sceneLoaded -= RegisterMoonsData;
                UpdateConfigCustomMoonList();
                return;
            }

            if (_isRegistering)
            {
                mls.LogInfo("Already registering moons data.");
                return;
            }


            List<SelectableLevel> selectableLevels = Resources.FindObjectsOfTypeAll<SelectableLevel>().Concat(UnityEngine.Object.FindObjectsByType<SelectableLevel>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)).ToList();
            if (selectableLevels == null || selectableLevels.Count == 0)
            {
                mls.LogInfo("Did not load SelectableLevels because there are no SelectableLevels to load.");
                return;
            }

            _isRegistering = true;
            mls.LogInfo("Registering custom moons data.");
            foreach (var selectableLevel in selectableLevels)
            {
                if (!RegisterBodiesSpawnBehaviour.PlanetsBodiesRarities.ContainsKey(selectableLevel.PlanetName))
                {
                    RegisterBodiesSpawnBehaviour.RegisterBodiesToNewMoon(selectableLevel);
                    _customMoonsNames.Add(selectableLevel.PlanetName);
                }
            }
            mls.LogInfo("Custom moons data registered.");
            _isRegistering = false;
        }

        private void UpdateConfigCustomMoonList()
        {
            Data.Config.SetCustomLevelsRarities(_customMoonsNames);
        }
        #endregion
    }
}
