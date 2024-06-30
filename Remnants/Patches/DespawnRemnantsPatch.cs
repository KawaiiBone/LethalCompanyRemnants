using HarmonyLib;
using Remnants.Behaviours;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Unity.Netcode;

namespace Remnants.Patches
{
    internal class DespawnRemnantsPatch
    {
        #region Variables
        private static MethodInfo _firstMethodToFind = SymbolExtensions.GetMethodInfo(() => UnityEngine.Object.FindObjectsOfType<GrabbableObject>());
        private static bool _canUseTranspiler = true;
        #endregion


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

                if (!despawnALlItems && (skipShipRoom && (grabbableObject.isInShipRoom && grabbableObject.isInElevator)))
                    continue;


                if (grabbableObject.isHeld && grabbableObject.playerHeldBy != null)
                {
                    if (despawnALlItems)
                        grabbableObject.playerHeldBy.DropAllHeldItemsAndSync();
                    else if (grabbableObject.playerHeldBy.isInHangarShipRoom && grabbableObject.playerHeldBy.isInElevator)
                        continue;
                    else
                        grabbableObject.playerHeldBy.DropAllHeldItemsAndSync();
                }

                mls.LogInfo("Despawning " + grabbableObject.itemProperties.itemName + " from " + placeOfDespawning + '.');
                networkObject.Despawn();
            }
        }

        private static void DespawnRemnantItems(object[] __args)
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
            if (despawnAllItems || (StartOfRound.Instance.allPlayersDead && Remnants.Instance.RemnantsConfig.ShouldDespawnRemnantItemsOnPartyWipe.Value == true))
            {
                DespawnItems(itemsLocationBeh.GetShipRemnantItems(), !itemsLocationBeh.ShipObjectLocation.IsShipRoom, despawnAllItems, itemsLocationBeh.ShipObjectLocation.ObjectLocationsNames.Last());
            }
            DespawnItems(itemsLocationBeh.GetItemsInProps(), !itemsLocationBeh.PropObjectLocation.IsShipRoom, despawnAllItems, itemsLocationBeh.PropObjectLocation.ObjectLocationsNames.Last());
            DespawnItems(itemsLocationBeh.GetItemsInRoot(), !itemsLocationBeh.RootObjectLocation.IsShipRoom, despawnAllItems, "Root objects");
            remnantItemsBehaviour.RemoveDespawnedAndNullItems();
            GrabbableObject[] grabbableObjectsArray = RemnantItemsBehaviour.FoundRemnantItems.ConvertAll(remnantOBJ => remnantOBJ.GetComponent<GrabbableObject>()).ToArray();
            DespawnItems(grabbableObjectsArray, true, despawnAllItems, "Unknown place");
            remnantItemsBehaviour.RemoveDespawnedAndNullItems();
        }




        #endregion
        #region HarmonyMethods
        [HarmonyPatch(typeof(RoundManager), "DespawnPropsAtEndOfRound")]
        [HarmonyPostfix]
        private static void DespawnRemnantItemsAtEndOfRoundPatch(object[] __args)
        {
            if (_canUseTranspiler)
                return;

            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Patching Despawn Remnant Items AtEndOfRound.");
            DespawnRemnantItems(__args);
        }


        [HarmonyPatch(typeof(RoundManager), "DespawnPropsAtEndOfRound")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> DespawnPropsAtEndOfRoundTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var mls = Remnants.Instance.Mls;
            if (Remnants.Instance.RemnantsConfig.ShouldDespawnRemnantItemsOnPartyWipe.Value == false)
            {
                mls.LogWarning("Using config feature: Do not despawn remnant items on party wipe. This will not use the transpiler for cleaning up items, and may cause issues.");
                _canUseTranspiler = false;
                return instructions;
            }

            var codes = new List<CodeInstruction>(instructions);
            int indexFirstCall = -1;
            for (int i = 0; i < codes.Count; ++i)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].Calls(_firstMethodToFind))
                {
                    indexFirstCall = i;
                    break;
                }
            }

            if (indexFirstCall == -1)
            {
                mls.LogError("Could not find first call to edit, using old patching method to despawn remnant items.");
                _canUseTranspiler = false;
                return codes.AsEnumerable();
            }
            MethodInfo methodInfoGetAllItems = typeof(RemnantItemsLocationsBehaviour).GetMethod(nameof(RemnantItemsLocationsBehaviour.GetAllItems));
            codes.Insert(indexFirstCall + 1, new CodeInstruction(OpCodes.Call, methodInfoGetAllItems));
            mls.LogInfo("Transpiler succes with function: Despawn Props At End Of Round.");
            return codes.AsEnumerable();
        }


        [HarmonyPatch(typeof(GameNetworkManager), "Disconnect")]
        [HarmonyPostfix]

        public static void DespawnRemnantItemsOnDisconnect(GameNetworkManager __instance)
        {
            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Patching Despawn Remnant Items at disconnect.");
            if (!__instance.isDisconnecting && (StartOfRound.Instance == null))
                return;

            if (!__instance.isHostingGame)
                return;

            object[] objects = { true };
            DespawnRemnantItems(objects);
        }


        [HarmonyPatch(typeof(GameNetworkManager), "DisconnectProcess")]
        [HarmonyPrefix]
        public static void DespawnRemnantItemsOnDisconnectProcess(GameNetworkManager __instance)
        {
            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Patching Despawn Remnant Items at disconnect process.");

            if (!__instance.isHostingGame)
                return;

            object[] objects = { true };
            DespawnRemnantItems(objects);
        }


        [HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect")]
        [HarmonyPrefix]
        private static void DespawnRemnantItemsOnStartDisconnect()
        {
            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Patching ownership of Remnant Items at start disconnect.");
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
