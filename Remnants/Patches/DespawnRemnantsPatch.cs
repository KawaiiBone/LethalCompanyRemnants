﻿using HarmonyLib;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Remnants.Patches
{
    internal class DespawnRemnantsPatch
    {
        #region HarmonyMethods
        [HarmonyPatch(typeof(RoundManager), "DespawnPropsAtEndOfRound")]
        [HarmonyPostfix]

        public static void DespawnRemnantItemsAtEndOfRound(object[] __args)
        {
            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Patching Despawn Remnant Items AtEndOfRound.");

            StartOfRound startOfRound = StartOfRound.Instance;
            if (startOfRound == null || !StartOfRound.Instance.allPlayersDead)
            {
                return;
            }

            var hangarShip = GameObject.Find("HangarShip");
            GrabbableObject[] remnantItemsArray = hangarShip.GetComponentsInChildren<GrabbableObject>().Where(grabObj => !(grabObj is RagdollGrabbableObject) && grabObj.isInShipRoom).ToArray();
            foreach (var remnantItem in remnantItemsArray)
            {
                if (!remnantItem.GetComponent<NetworkObject>().IsSpawned)
                    return;

                if (remnantItem.isHeld && remnantItem.playerHeldBy != null)
                    remnantItem.playerHeldBy.DropAllHeldItemsAndSync();

                remnantItem.GetComponent<NetworkObject>().Despawn();
            }
        }
        #endregion
    }
}
