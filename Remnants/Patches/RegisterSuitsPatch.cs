using HarmonyLib;
using Remnants.utilities;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Remnants.Patches
{

    internal class RegisterSuitsPatch
    {
        #region Variables
        private static bool _hasRegistered = false;
        #endregion

        #region HarmonyMethods
        [HarmonyPatch(typeof(StartOfRound), "Start")]
        [HarmonyPostfix]
        private static void RegisterBodySuitsPatch(StartOfRound __instance)
        {
            if (_hasRegistered)
                return;

            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Patching register body suits.");
            Remnants.Instance.RegisterBodySuits.RegisterSuitsDataToConfig(__instance.unlockablesList.unlockables);
            _hasRegistered = true;
        }
        #endregion
    }
}
