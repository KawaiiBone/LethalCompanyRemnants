﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Remnants.Patches;
using Remnants.Behaviours;
using Remnants.Data;
using UnityEngine;
using System.Collections.Generic;

namespace Remnants
{
    [BepInDependency("evaisa.lethallib", "0.15.1")]
    [BepInPlugin(modGUID, modName, modVersion)]

    public class Remnants : BaseUnityPlugin
    {
        #region Variables
        private const string modGUID = "KawaiiBone.Remnants";
        private const string modName = "Remnants";
        private const string modVersion = "1.2.13";

        public static Remnants Instance;
        private readonly Harmony _harmony = new Harmony(modGUID);
        internal ManualLogSource Mls;

        public RegisterBodiesSpawnBehaviour  RegisterBodiesSpawn = new RegisterBodiesSpawnBehaviour();
        public Data.Config RemnantsConfig = new Data.Config();
        public RegisterBodySuitsBehaviour RegisterBodySuits = new RegisterBodySuitsBehaviour();
        public LoadAssetsBodies LoadBodyAssets = new LoadAssetsBodies();
        public SpawnBodiesBehaviour SpawningBodyBeh = new SpawnBodiesBehaviour();
        public RemnantItemsBehaviour RemnantItemsBeh = new RemnantItemsBehaviour();
        public RandomizeBatteriesBehaviour ItemsBatteriesBeh = new RandomizeBatteriesBehaviour();
        public RegisterItemLocationsBehaviour RegisterItemLocationsBeh = new RegisterItemLocationsBehaviour();

        private RegisterItemsBehaviour _registerItemsBehaviour = new RegisterItemsBehaviour();
        private RegisterCustomMoonsData _registerCustomMoonsData = new RegisterCustomMoonsData();
        #endregion

        #region Initialize 
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            Mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            Mls.LogInfo("modGUID has started");
            _harmony.PatchAll(typeof(RemnantItemsPatch));
            _harmony.PatchAll(typeof(SpawnableScrapPatch));
            _harmony.PatchAll(typeof(SaveGamePatch));
            _harmony.PatchAll(typeof(DespawnRemnantsPatch));
            _harmony.PatchAll(typeof(BodyGrabbableObject));
            _harmony.PatchAll(typeof(Remnants));
            RemnantsConfig.Initialize();
            _registerItemsBehaviour.Initialize();
             RegisterBodiesSpawn.Initialize();
            _registerCustomMoonsData.Initialize();
            LoadBodyAssets.Initialize();
            RegisterBodySuits.Initialize();
            SpawningBodyBeh.Initialize();
            ItemsBatteriesBeh.Initialize();
            RegisterItemLocationsBeh.Initialize();
            Mls.LogInfo("modGUID has loaded");
        }
        #endregion
    }
}
