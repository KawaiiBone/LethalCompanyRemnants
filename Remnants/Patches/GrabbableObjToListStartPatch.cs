using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace Remnants.Patches
{
    internal class GrabbableObjToListStartPatch
    {
        #region Variables

        #endregion

        #region HarmonyMethods
        [HarmonyPatch(typeof(NetworkBehaviour), "OnNetworkSpawn")]
        [HarmonyPostfix]
        private static void AddGrabbableObjectPostStart(NetworkBehaviour __instance)
        {
            var mls = Remnants.Instance.Mls;
            NetworkManager networkManager = __instance.NetworkManager;
            if (networkManager.IsClient || networkManager.IsHost)
            {
                if (__instance is GrabbableObject)
                {
                    GrabbableObject grabbableObject = __instance as GrabbableObject;
                    Remnants.Instance.GrabbableObjsSpawnListBeh.AddSpawnedGrabbableObject(grabbableObject);
                }
            }
        }
        #endregion
    }
}
