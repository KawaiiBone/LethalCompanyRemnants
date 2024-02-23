﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace Remnants.Patches
{
    internal class ScrapBatteryPatch
    {
        #region HarmonyMethods
        [HarmonyPatch(typeof(RoundManager), "waitForScrapToSpawnToSync")]//SpawnScrapInLevel//waitForScrapToSpawnToSync//SyncScrapValuesClientRpc
        [HarmonyPostfix]
        static void UpdateSpawnedScrapCharge(object[] __args)
        {
            var mls = Remnants.Instance._mls;
            NetworkObjectReference[] spawnedScrap = (NetworkObjectReference[])__args[0];
            mls.LogInfo("Enterred function UpdateSpawnedScrapCharge");
            if (spawnedScrap == null)
            {
                mls.LogInfo("spawnedScrap IS NULL");
                return;
            }

            System.Random random = new System.Random();
            for (int i = 0; i < spawnedScrap.Length; i++)
            {
                if (!spawnedScrap[i].TryGet(out var networkObject))
                    continue;

                GrabbableObject grabbableObject = networkObject.GetComponent<GrabbableObject>();
                if (grabbableObject == null)
                    continue;

                mls.LogInfo("Found " + grabbableObject.itemProperties.name + " in the world.");

                if (!grabbableObject.itemProperties.requiresBattery)
                    continue;

                mls.LogInfo(grabbableObject.itemProperties.name + " 0");

                if (!(grabbableObject.insertedBattery != null &&
                    grabbableObject.isInFactory == true && !grabbableObject.isInShipRoom))
                    continue;

                mls.LogInfo(grabbableObject.itemProperties.name + " 1");
                int randomCharge = random.Next(1, 100);
                mls.LogInfo("Charge " + randomCharge);
                grabbableObject.SyncBatteryServerRpc(randomCharge);
                mls.LogInfo("Has updated " + grabbableObject.itemProperties.name + " charge to " + grabbableObject.insertedBattery.charge);
            }
        }
        #endregion
    }
}