using HarmonyLib;
using Remnants.Behaviours;
using Remnants.Data;
using Remnants.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.Diagnostics;


namespace Remnants.Patches
{
    internal class BeltBagTranspiler
    {
        #region Variables
        static FieldInfo _itemIsScrapField = AccessTools.Field(typeof(GrabbableObject), nameof(GrabbableObject.itemProperties));
        #endregion

        #region HarmonyMethods
        [HarmonyPatch(typeof(BeltBagItem), "ItemInteractLeftRight")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> SaveItemsInShipTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var mls = Remnants.Instance.Mls;
            if(Remnants.Instance.RemnantsConfig.UseBeltBagTranspiler.Value == false)
            {
                mls.LogWarning("Beltbag feature disabled, beltbag can now not pickup remnant items.");
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
                else if(indexOfFirstItemProperties > -1 && codes[i].opcode == OpCodes.Brtrue)
                {
                    indexOfReturnItemProperties = i;
                    break;
                }
            }

            if (indexOfFirstItemProperties == -1 || indexOfReturnItemProperties == -1)
            {
                mls.LogError("Could not find place in if statement to edit, unable to use BeltBagItem with remnant items.");
                return codes.AsEnumerable();
            }

            int amountOfCodeToNop = indexOfReturnItemProperties - indexOfFirstItemProperties;
            //Set operands to nop so it does not get used
            for (int i = 0; i < amountOfCodeToNop; ++i)
            {
                codes[i + indexOfFirstItemProperties].opcode = OpCodes.Nop;
            }
            //Change statement from true to false in the end, so it will fully ignore this part of the code
            codes[indexOfReturnItemProperties].opcode = OpCodes.Brfalse_S;
            //Insert value on the stack and add the function
            codes.Insert(indexOfFirstItemProperties, new CodeInstruction(OpCodes.Ldloc_1));
            codes.Insert(indexOfFirstItemProperties+1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BeltBagTranspiler), "CheckIsStoreOrRemnantItem", new Type[] { typeof(GrabbableObject)})));
            mls.LogInfo("Transpiler succes with function: ItemInteractLeftRight for BeltBagItem.");
            return codes.AsEnumerable();
        }



        public bool CheckIsStoreOrRemnantItem(GrabbableObject grabbableObject)
        {
            if (grabbableObject == null || grabbableObject.itemProperties == null)
                return false;
            if (!grabbableObject.itemProperties.isScrap)
                return true;
            List<RemnantData> remnantItemDataList = Remnants.Instance.RemnantsConfig.GetRemnantItemList(false);
            if (remnantItemDataList.FindIndex(configEntry => configEntry.RemnantItemName == grabbableObject.itemProperties.itemName
            || configEntry.RemnantItemName == grabbableObject.itemProperties.name) != -1)
                return true;
            return false;
        }
        #endregion

    }
}
