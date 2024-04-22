using HarmonyLib;
using LethalLib.Modules;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Remnants.Patches
{
    internal class DespawnRemnantsPatch
    {
        #region Methods
        private static void DespawnItems(GrabbableObject[] grabbableObjects, bool skipShipRoom, bool despawnALlItems, string placeOfDespawning)
        {
            var mls = Remnants.Instance.Mls;
            if (grabbableObjects == null || grabbableObjects.Length == 0)
            {
                mls.LogInfo("Could not find grabbableObjects in: " + placeOfDespawning);
                return;
            }
            foreach (var grabbableObject in grabbableObjects)
            {
                if (grabbableObject == null)
                    continue;

                NetworkObject networkObject = grabbableObject.GetComponent<NetworkObject>();
                if (networkObject == null || !networkObject.IsSpawned)
                    continue;

                if (!despawnALlItems && (skipShipRoom && (grabbableObject.isInShipRoom && /*||*/ grabbableObject.isInElevator)))
                    continue;


                if (grabbableObject.isHeld && grabbableObject.playerHeldBy != null)
                {
                    if (despawnALlItems)
                        grabbableObject.playerHeldBy.DropAllHeldItemsAndSync();
                    else if (grabbableObject.playerHeldBy.isInHangarShipRoom && /*||*/ grabbableObject.playerHeldBy.isInElevator)
                        continue;
                    else
                        grabbableObject.playerHeldBy.DropAllHeldItemsAndSync();
                }

                mls.LogInfo("Despawning " + grabbableObject.itemProperties.itemName + " from " + placeOfDespawning + '.');
                networkObject.Despawn();
            }
        }
        #endregion
        #region HarmonyMethods
        [HarmonyPatch(typeof(RoundManager), "DespawnPropsAtEndOfRound")]
        [HarmonyPostfix]

        public static void DespawnRemnantItemsEndOfRound(object[] __args)
        {
            var mls = Remnants.Instance.Mls;
            bool despawnAllItems = (bool)__args[0];
            mls.LogInfo("Patching Despawn Remnant Items AtEndOfRound.");
            StartOfRound startOfRound = StartOfRound.Instance;
            if (startOfRound == null)
                return;

            if (!NetworkManager.Singleton.IsServer)
                return;

            if (Remnants.Instance.RemnantsConfig.ShouldAlwaysDespawnRemnantItems.Value == true)
                despawnAllItems = true;
            //The game cannot detect remnant items with simple means as GameObject.Find<GrabbableObject>() 
            //So this is the best way to find it by this mod self
            var remnantItemsBehaviour = Remnants.Instance.RemnantItemsBeh;
            var itemsLocationBeh = Remnants.Instance.RegisterItemLocationsBeh;
            if (despawnAllItems || (StartOfRound.Instance.allPlayersDead && Remnants.Instance.RemnantsConfig.ShouldDespawnRemnantItems.Value == true))
            {
                DespawnItems(itemsLocationBeh.GetShipRemnantItems(), !itemsLocationBeh.ShipObjectLocation.IsShipRoom, despawnAllItems, itemsLocationBeh.ShipObjectLocation.ObjectLocationsNames.Last());
            }
            DespawnItems(itemsLocationBeh.GetItemsInProps(), !itemsLocationBeh.PropObjectLocation.IsShipRoom, despawnAllItems, itemsLocationBeh.PropObjectLocation.ObjectLocationsNames.Last());
            DespawnItems(itemsLocationBeh.GetItemsInRoot(), !itemsLocationBeh.RootObjectLocation.IsShipRoom, despawnAllItems, "Root objects");
            remnantItemsBehaviour.RemoveDespawnedAndNullItems();
            GrabbableObject[] grabbableObjectsArray = remnantItemsBehaviour.RemnantItems.ConvertAll(remnantOBJ => remnantOBJ.GetComponent<GrabbableObject>()).ToArray();
            DespawnItems(grabbableObjectsArray, true, despawnAllItems, "Unknown place");
            remnantItemsBehaviour.RemoveDespawnedAndNullItems();
        }

        [HarmonyPatch(typeof(GameNetworkManager), "DisconnectProcess")]
        [HarmonyPrefix]
        public static void DespawnRemnantItemsOnDisconnect(GameNetworkManager __instance)
        {
            if (!__instance.isHostingGame)
                return;

            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Patching Despawn Remnant Items at disconnect.");
            object[] objects = { true };
            DespawnRemnantItemsEndOfRound(objects);
        }


        [HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect")]
        [HarmonyPrefix]
        private static void DespawnRemnantItemsOnStartDisconnect()
        {
            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Patching Despawn Remnant Items at start disconnect.");
            //Get all remnant items that should be in ship from root
            var itemsLocationBeh = Remnants.Instance.RegisterItemLocationsBeh;
            var grabbableObjectsList = itemsLocationBeh.GetShipItems().ToList();
            grabbableObjectsList.AddRange(itemsLocationBeh.GetItemsInProps());
            grabbableObjectsList.AddRange(itemsLocationBeh.GetItemsInRoot());

            foreach (var grabbableObject in grabbableObjectsList)
            {
                var networkOBJ = grabbableObject.GetComponent<NetworkObject>();
                if (networkOBJ != null)
                {
                    networkOBJ.DontDestroyWithOwner = false;
                    mls.LogInfo(grabbableObject.name + " DontDestroyWithOwner to false");
                }
            }
        }
        #endregion
    }
}
