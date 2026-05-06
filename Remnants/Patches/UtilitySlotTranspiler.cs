using GameNetcodeStuff;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Remnants.Patches
{
    internal class UtilitySlotTranspiler
    {
        static FieldInfo _itemIsScrapField = AccessTools.Field(typeof(GrabbableObject), nameof(GrabbableObject.itemProperties));
        private static List<string> _remnantUtilityNameList = null;

        [HarmonyPatch(typeof(PlayerControllerB), "FirstEmptyItemSlot")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> FirstEmptyItemSlotTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            //rather then !attemptingGrab.itemProperties.isScrap, it should be (attemptingGrab is FlashlightItem  || !attemptingGrab.itemProperties.isScrap)
            var mls = Remnants.Instance.Mls;
            if (Remnants.Instance.RemnantsConfig.UseBeltBagTranspiler.Value == false)
            {
                mls.LogWarning("Remnant flashlight slot feature disabled, flashlight slot can now not pickup remnant flashlight items.");
                return instructions;
            }
            var codes = new List<CodeInstruction>(instructions);
            int indexOfFirstItemProperties = -1;
            int indexOfReturnItemProperties = -1;
            for (int i = 0; i < codes.Count; ++i)
            {
                if (indexOfFirstItemProperties == -1 && codes[i].LoadsField(_itemIsScrapField))
                {
                    indexOfFirstItemProperties = i;
                }
                else if (indexOfFirstItemProperties > -1 && codes[i].opcode == OpCodes.Brtrue)
                {
                    indexOfReturnItemProperties = i;
                    break;
                }
            }

            if (indexOfFirstItemProperties == -1 || indexOfReturnItemProperties == -1)
            {
                mls.LogError("Could not find place in if statement to edit, unable to use remnant flashlight slot feature.");
                return codes.AsEnumerable();
            }

            int amountOfCodeToNop = indexOfReturnItemProperties - indexOfFirstItemProperties;
            //Set operands to nop so it does not get used
            for (int i = 0; i < amountOfCodeToNop; ++i)
            {
                codes[i + indexOfFirstItemProperties].opcode = OpCodes.Nop;
            }
            codes.Insert(indexOfFirstItemProperties,
                new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.Method(
                        typeof(UtilitySlotTranspiler),
                        nameof(IsRemnantFlashlightOrScrap),
                        new[] { typeof(GrabbableObject) }
                    )));
            mls.LogInfo("Transpiler succes with function: FirstEmptyItemSlot for remnant flashlight item slot.");

            return codes.AsEnumerable();
        }


        public static bool IsRemnantFlashlightOrScrap(GrabbableObject attemptingGrab)
        {
            if (_remnantUtilityNameList == null)
            {
                List<string> remnantItems = Remnants.Instance.RemnantsConfig.GetRemnantItemList(false).Select(c => c.RemnantItemName.ToLower()).ToList();
                List<string> bannedList = Remnants.Instance.RemnantsConfig.GetBannedFromUtilitySlotItemNames().ConvertAll(c => c.ToLower());
                _remnantUtilityNameList = remnantItems.Except(bannedList).ToList();      
            }

            return !((_remnantUtilityNameList.FindIndex(ul => ul == attemptingGrab.itemProperties.itemName.ToLower()
            || ul == attemptingGrab.itemProperties.name.ToLower()) != -1) || !attemptingGrab.itemProperties.isScrap);
        }
    }
}
