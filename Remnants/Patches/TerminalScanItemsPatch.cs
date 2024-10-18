using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using Remnants.Behaviours;

namespace Remnants.Patches
{
    internal class TerminalScanItemsPatch
    {
        #region Variables
        private static MethodInfo _findFirstGrabbableObjectMethod = SymbolExtensions.GetMethodInfo(() => UnityEngine.Object.FindObjectsOfType<GrabbableObject>());
        static FieldInfo _itemIsScrapField = AccessTools.Field(typeof(GrabbableObject), nameof(GrabbableObject.itemProperties));
        #endregion

        #region HarmonyMethods
        [HarmonyPatch(typeof(Terminal), "TextPostProcess")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> TerminalScanItemsTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var mls = Remnants.Instance.Mls;
            if (Remnants.Instance.RemnantsConfig.UseBeltBagTranspiler.Value == false)
            {
                mls.LogWarning("Terminal scan items feature disabled, terminal can now not find remnant items when scanning.");
                return instructions;
            }
            var codes = new List<CodeInstruction>(instructions);
            int indexCallGrabObjects = -1;
            for (int i = 0; i < codes.Count; ++i)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].Calls(_findFirstGrabbableObjectMethod))
                {
                    indexCallGrabObjects = i;
                    break;
                }
            }

            if (indexCallGrabObjects == -1)
            {
                mls.LogError("Could not find place to edit, unable to use scan remnant items in the terminal.");
                return codes.AsEnumerable();
            }
            //Insert the function, just after the default method to replace it
            codes.Insert(indexCallGrabObjects+1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RemnantItemsLocationsBehaviour), "GetAllItems")));
            mls.LogInfo("Transpiler succes with function: TextPostProcess for scanning remnant items via the terminal.");
            return codes.AsEnumerable();
        }

        #endregion
    }
}