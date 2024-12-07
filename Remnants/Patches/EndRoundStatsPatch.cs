using HarmonyLib;
using Remnants.Behaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace Remnants.Patches
{
    internal class EndRoundStatsPatch
    {
        #region Variables
        private static MethodInfo _findFirstGrabbableObjecArraytMethod = SymbolExtensions.GetMethodInfo(() => UnityEngine.Object.FindObjectsOfType<GrabbableObject>());
        #endregion

        #region HarmonyTranspilerMethods
        [HarmonyPatch(typeof(StartOfRound), "GetValueOfAllScrap")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> GetValueOfAllScrapTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var mls = Remnants.Instance.Mls;
            if (Remnants.Instance.RemnantsConfig.UseTerminalScanItemsTranspiler.Value == false)
            {
                mls.LogWarning("Report scrap items collected feature disabled, report will now not show accurate results with this mod.");
                return instructions;
            }
            var codes = new List<CodeInstruction>(instructions);
            int indexCallGrabObjects = -1;
            for (int i = 0; i < codes.Count; ++i)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].Calls(_findFirstGrabbableObjecArraytMethod))
                {
                    indexCallGrabObjects = i;
                    break;
                }
            }

            if (indexCallGrabObjects == -1)
            {
                mls.LogError("Could not find place to edit, unable to show accurate results for report.");
                return codes.AsEnumerable();
            }
            //Insert the function, just after the default method to replace it
            codes.Insert(indexCallGrabObjects + 1, new CodeInstruction(OpCodes.Call, typeof(GrabbableObjsSpawnListBehaviour).GetMethod(nameof(GrabbableObjsSpawnListBehaviour.GetSpawnedGrabbableObjects))));
            mls.LogInfo("Transpiler succes with function: GetValueOfAllScrap for showing accurate results for the report.");
            return codes.AsEnumerable();
        }
        #endregion
        #region HarmonyPatchMethods
        [HarmonyPatch(typeof(RoundManager), "SyncScrapValuesClientRpc")]
        [HarmonyPostfix]
        static void PatchSyncScrapValuesClientRpcScrapValue(object[] __args, RoundManager __instance)
        {
            var mls = Remnants.Instance.Mls;
            NetworkObjectReference[] spawnedScrap = (NetworkObjectReference[])__args[0];
            if (spawnedScrap == null)
            {
                mls.LogWarning("spawnedScrap IS NULL");
                return;
            }
            Remnants.Instance.GrabbableObjsSpawnListBeh.AddScrapValueFromLevel((int)__instance.totalScrapValueInLevel);
        }


        [HarmonyPatch(typeof(StartOfRound), "ShipHasLeft")]
        [HarmonyPrefix]
        static void PatchShipHasLeft(object[] __args, RoundManager __instance)
        {
            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Patching ShipHasLeft.");
            Remnants.Instance.GrabbableObjsSpawnListBeh.UpdateTotalScrapValue();
            Remnants.Instance.GrabbableObjsSpawnListBeh.ResetTotalScrapValueInLevel();
        }
        #endregion
    }
}
