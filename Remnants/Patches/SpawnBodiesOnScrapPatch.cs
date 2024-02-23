using HarmonyLib;
using LethalLib.Modules;
using Remnants.Behaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using static LethalLib.Modules.Enemies;

namespace Remnants.Patches
{
    internal class SpawnBodiesOnScrapPatch
    {
        #region Variables
        private static int _bodyScrapValue = 5;
        private static float _SyncDelayTime = 11.0f;
        #endregion

        #region HarmonyMethods
        [HarmonyPatch(typeof(RoundManager), "waitForScrapToSpawnToSync")]//SpawnScrapInLevel//waitForScrapToSpawnToSync//SyncScrapValuesClientRpc
        [HarmonyPostfix]
        static void AddBodiesToScrap(object[] __args)
        {
            RoundManager __instance = RoundManager.Instance;
            var mls = Remnants.Instance._mls;
            NetworkObjectReference[] spawnedScrap = (NetworkObjectReference[])__args[0];
            List<NetworkObjectReference> bodyNetObjList = new List<NetworkObjectReference>();
            List<int> scrapValueList = new List<int>();
            mls.LogInfo("Enterred function AddBodiesToScrap");
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
                SpawnBodyBehaviour spawnBody = networkObject.GetComponent<SpawnBodyBehaviour>();
                if (grabbableObject == null || spawnBody != null)
                    continue;

                mls.LogInfo("Found " + grabbableObject.itemProperties.itemName + " in the AddBodiesToScrap.");

                if (Items.scrapItems.FindIndex(scrapItem => scrapItem.origItem.itemName == grabbableObject.itemProperties.itemName) == -1)
                    continue;

                mls.LogInfo("Adding body to " + grabbableObject.itemProperties.name);
                spawnBody = networkObject.gameObject.AddComponent<SpawnBodyBehaviour>();
                spawnBody.CreateEnemyBody();
                scrapValueList.Add(_bodyScrapValue);
            }

            if (bodyNetObjList.Count == 0)
                return;

            if (utilities.CoroutineHelper.Instance == null)
            {
                var helperObject = new GameObject("CoroutineHelper");
                helperObject.AddComponent<utilities.CoroutineHelper>();
            }

            utilities.CoroutineHelper.Instance.ExecuteAfterDelay(() => {
                __instance.SyncScrapValuesClientRpc(bodyNetObjList.ToArray(), scrapValueList.ToArray());
            }, _SyncDelayTime);
        }
        #endregion

    }
}
