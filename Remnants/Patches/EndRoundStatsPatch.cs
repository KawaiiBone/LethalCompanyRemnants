using HarmonyLib;
using Remnants.Behaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using Unity.Netcode;

namespace Remnants.Patches
{
    internal class EndRoundStatsPatch
    {
        #region Variables
        private static int SyncScrapValueCounter = 0;
        private static int _totalScrapValueInLevel = 0;
        #endregion
        #region HarmonyPatchMethods
        [HarmonyPatch(typeof(RoundManager), "SyncScrapValuesClientRpc")]
        [HarmonyPostfix]
        static void PatchSyncScrapValuesClientRpcScrapValue(object[] __args, RoundManager __instance)
        {
            var mls = Remnants.Instance.Mls;
            if (Remnants.Instance.RemnantsConfig.UseEndRoundPatchFix.Value == false)
                return;
            NetworkObjectReference[] spawnedScrap = (NetworkObjectReference[])__args[0];
            if (spawnedScrap == null)
            {
                mls.LogWarning("spawnedScrap IS NULL");
                return;
            }
            if(SyncScrapValueCounter % 2 == 0)
            {
                _totalScrapValueInLevel += (int)__instance.totalScrapValueInLevel;
            }
            SyncScrapValueCounter++;
        }


        [HarmonyPatch(typeof(StartOfRound), "ShipHasLeft")]
        [HarmonyPrefix]
        static void PatchShipHasLeft(object[] __args, RoundManager __instance)
        {
            var mls = Remnants.Instance.Mls;
            if (Remnants.Instance.RemnantsConfig.UseEndRoundPatchFix.Value == false)
                return;
            mls.LogInfo("Patching ShipHasLeft.");
            RoundManager.Instance.totalScrapValueInLevel = _totalScrapValueInLevel;
            _totalScrapValueInLevel = 0;
        }
        #endregion
    }
}
