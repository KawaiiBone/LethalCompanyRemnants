using HarmonyLib;
using LethalLib.Modules;
using Remnants.Behaviours;
using Remnants.utilities;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Remnants.Patches
{
    internal class SpawnRemnantItemsPatch
    {
        #region HarmonyMethods
        [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
        [HarmonyPostfix]
        private static void PatchSpawnRemnantItems(RoundManager __instance)
        {
            if (Remnants.Instance.RemnantsConfig.UseLegacySpawning.Value == true)
                return;

            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Patching spawn remnant items.");
            Remnants.Instance.SpawnRemnantItemsBeh.SpawnRemnantItems(__instance); 
            Remnants.Instance.SpawningBodyBeh.SpawnBodiesOnPositions(Remnants.Instance.SpawnRemnantItemsBeh.BodySpawnPositions, true);
        }
        #endregion
    }
}
