using HarmonyLib;
using LethalLib.Modules;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

namespace Remnants.Patches
{
    internal class AddRemnantItemsToItemList
    {
        #region Variables
        private static bool _hasAddedNetworkRemnantItems = false;
        #endregion

        #region HarmonyMethods
        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        [HarmonyPostfix]
        private static void AddNetworkRemnantToStartOfRoundList(StartOfRound __instance)
        {
            if (_hasAddedNetworkRemnantItems)
                return;
            _hasAddedNetworkRemnantItems = true;

            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Patch, adding network remnant items to startOfRound item list.");
            List<Items.ScrapItem> networkRemnantItems = Remnants.Instance.RemnantItemsBeh.NetworkRemnantItems;
            for (int i = 0; i < networkRemnantItems.Count; i++) 
            {
                __instance.allItemsList.itemsList.Add(networkRemnantItems[i].item);
            }

        }
        #endregion
    }
}
