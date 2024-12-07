using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using Remnants.Behaviours;

namespace Remnants.Patches
{
    internal class TerminalScanItemsTranspiler
    {
        #region Variables
        private static MethodInfo _findFirstGrabbableObjecArraytMethod = SymbolExtensions.GetMethodInfo(() => UnityEngine.Object.FindObjectsOfType<GrabbableObject>());
        #endregion

        #region HarmonyMethods
        [HarmonyPatch(typeof(Terminal), "TextPostProcess")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> TextPostProcessScanItemsTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var mls = Remnants.Instance.Mls;
            if (Remnants.Instance.RemnantsConfig.UseTerminalScanItemsTranspiler.Value == false)
            {
                mls.LogWarning("Terminal scan items feature disabled, terminal can now not find remnant items when scanning.");
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
                mls.LogError("Could not find place to edit, unable to use scan remnant items in the terminal.");
                return codes.AsEnumerable();
            }
            //Insert the function, just after the default method to replace it
            codes.Insert(indexCallGrabObjects+1, new CodeInstruction(OpCodes.Call, typeof(GrabbableObjsSpawnListBehaviour).GetMethod(nameof(GrabbableObjsSpawnListBehaviour.GetSpawnedGrabbableObjects))));
            mls.LogInfo("Transpiler succes with function: TextPostProcess for scanning remnant items via the terminal.");
            return codes.AsEnumerable();
        }

        #endregion
    }
}