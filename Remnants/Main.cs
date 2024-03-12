using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Remnants.Patches;
using Remnants.Behaviours;
using UnityEngine;
using LethalLib.Modules;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using Remnants.Data;




namespace Remnants
{
    [BepInDependency("evaisa.lethallib", "0.14.2")]
    [BepInPlugin(modGUID, modName, modVersion)]

    public class Remnants : BaseUnityPlugin
    {
        #region Variables
        private const string modGUID = "KawaiiBone.Remnants";
        private const string modName = "Remnants";
        private const string modVersion = "1.2.1";

        public static Remnants Instance;
        private readonly Harmony _harmony = new Harmony(modGUID);
        internal ManualLogSource Mls;

        private RegisterItemsBehaviour _registerItemsBehaviour = new RegisterItemsBehaviour();
        private LoadAssetsBodies _loadAssetsBodies = new LoadAssetsBodies();
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
            _harmony.PatchAll(typeof(ScrapBatteryPatch));
            _harmony.PatchAll(typeof(SpawnBodiesPatch));
            _harmony.PatchAll(typeof(SpawnableScrapPatch));
            _harmony.PatchAll(typeof(OccludeAudioPatch));
            _harmony.PatchAll(typeof(SaveGamePatch));
            _harmony.PatchAll(typeof(DespawnRemnantsPatch));
            _harmony.PatchAll(typeof(Remnants));
            Data.Config.LoadConfig();
            _registerItemsBehaviour.Initialize();
            _loadAssetsBodies.Initialize();
            Mls.LogInfo("modGUID has loaded");
        }
        #endregion

  


    }
}
