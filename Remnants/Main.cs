using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Remnants.Patches;
using System.Reflection;
using Remnants.Behaviours;
using UnityEditor;
using System.IO;




namespace Remnants
{
    [BepInDependency("evaisa.lethallib", "0.14.2")]
    [BepInPlugin(modGUID, modName, modVersion)]

    public class Remnants : BaseUnityPlugin
    {
        #region Variables
        private const string modGUID = "KawaiiBone.Remnants";
        private const string modName = "Remnants";
        private const string modVersion = "1.1.3";

        public static Remnants Instance;
        private readonly Harmony _harmony = new Harmony(modGUID);
        internal ManualLogSource Mls;

        private RegisterItemsBehaviour _registerItemsBehaviour = new RegisterItemsBehaviour();
        private RegisterBodyBehaviour _registerBodyBehaviour = new RegisterBodyBehaviour();
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
            _harmony.PatchAll(typeof(SpawnableScrapPatch));
            _harmony.PatchAll(typeof(Remnants));
            Data.Config.LoadConfig();
            _registerItemsBehaviour.Initialize();
            //_registerBodyBehaviour.Initialize();
            Mls.LogInfo("modGUID has loaded");
        }
        #endregion

        #region Methods
        #endregion

        #region HarmonyMethods
        [HarmonyPatch(typeof(QuickMenuManager), "LeaveGameConfirm")]
        [HarmonyPostfix]
        //Whenever you landed on a moon (generated a world) and then you quit the game, you would get OccludeAudio errors of something being nullptr reference.
        //This has probably something to do with the GameNetworkManager and the store items being added to scrap.
        //So the fix this, i had to disable all the OccludeAudio when you leave the lobby/game session.
        //During playtesting, i have not noticed any sounds that are absent.
        static void SoundErrorPatch()
        {
            OccludeAudio[] OccludeAudiosArray = Resources.FindObjectsOfTypeAll<OccludeAudio>();
            foreach (var occludeAudio in OccludeAudiosArray)
            {
                occludeAudio.enabled = false;
            }
        }

        //This is to fix the grab error where you cant grab the body correctly
        /*[HarmonyPatch(typeof(RagdollGrabbableObject), "Update")]
        [HarmonyPrefix]
        static void GrabbableBodyPatch(ref bool ___foundRagdollObject)
        {
            ___foundRagdollObject = true;

        }*/

        #endregion
    }
}