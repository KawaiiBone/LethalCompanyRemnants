using HarmonyLib;
using Remnants.Behaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace Remnants.Patches
{
    public class SaveGameTranspiler
    {
        #region Variables
        private static MethodInfo _firstMethodToFind = SymbolExtensions.GetMethodInfo(() => UnityEngine.Object.FindObjectsByType<GrabbableObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        #endregion

        #region HarmonyMethods
        [HarmonyPatch(typeof(GameNetworkManager), "SaveItemsInShip")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> SaveItemsInShipTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var mls = Remnants.Instance.Mls;
            var codes = new List<CodeInstruction>(instructions);
            int indexFirstCall = -1;
            for (int i = 0; i < codes.Count; ++i)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].Calls(_firstMethodToFind))
                {
                    indexFirstCall = i;
                    break;
                }
            }

            if (indexFirstCall == -1)
            {
                mls.LogError("Could not find first call to edit, unable to save remnant items.");
                return codes.AsEnumerable();
            }
            MethodInfo proteinHitFoce = typeof(RemnantItemsLocationsBehaviour).GetMethod(nameof(RemnantItemsLocationsBehaviour.GetItemsToSave));
            codes.Insert(indexFirstCall + 1, new CodeInstruction(OpCodes.Call, proteinHitFoce));
            mls.LogInfo("Transpiler succes with function: Save Items In Ship.");
            return codes.AsEnumerable();
        }
        #endregion
    }
}
