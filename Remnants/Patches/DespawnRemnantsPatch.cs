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
        #region Variables
        private static string _shipObjName = "HangarShip";
        private static string _environmentObjName = "Environment";
        private static string _propObjName = "Props";
        #endregion
        #region Methods
        private static void DespawnItems(GrabbableObject[] grabbableObjects, bool skipShipRoom, string placeOfDespawning)
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

                if (skipShipRoom && grabbableObject.isInShipRoom)
                    continue;

                if (grabbableObject.isHeld && grabbableObject.playerHeldBy != null)
                    grabbableObject.playerHeldBy.DropAllHeldItemsAndSync();

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

            if (!GameNetworkManager.Instance.isHostingGame)
                return;

            //Despawn remnant items in ship
            if ((StartOfRound.Instance.allPlayersDead || despawnAllItems) && Remnants.Instance.RemnantsConfig.ShouldDespawnRemnantItems.Value == true)
            {
                var hangarShip = GameObject.Find(_shipObjName);
                GrabbableObject[] remnantShipItemsArray = hangarShip.GetComponentsInChildren<GrabbableObject>().Where(
                    grabObj => (!(grabObj is RagdollGrabbableObject) && grabObj.isInShipRoom) &&
                    Items.scrapItems.FindIndex(scrapItem => scrapItem.item.itemName == grabObj.itemProperties.itemName) != -1).ToArray();
                DespawnItems(remnantShipItemsArray, false, _shipObjName);
            }
            //Despawn remnant items in root objects
            var rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            GameObject[] grabbableObjects = rootGameObjects.Where(gmObject => gmObject.GetComponent<GrabbableObject>() != null).ToArray(); ;
            GrabbableObject[] remnantRootItemsArray = grabbableObjects.Select(gameObj => gameObj.GetComponent<GrabbableObject>()).ToArray();
            DespawnItems(remnantRootItemsArray, true, "root objects");
            //Despawn remnant items in Environment Props array
            GameObject environmentObj = rootGameObjects.ToList().Find(gameObject => gameObject.name == _environmentObjName);
            GameObject propObj = environmentObj.transform.Find(_propObjName).gameObject;
            GrabbableObject[] grabObjArray = propObj.GetComponentsInChildren<GrabbableObject>();
            DespawnItems(grabObjArray, true, _propObjName + " in " + _environmentObjName);
        }

        [HarmonyPatch(typeof(GameNetworkManager), "Disconnect")]
        [HarmonyPostfix]
        public static void DespawnRemnantItemsOnDisconnect(GameNetworkManager __instance)
        {
            if (!__instance.isHostingGame)
                return;

            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Patching Despawn Remnant Items at disconnect.");
            object[] objects = { true };
            DespawnRemnantItemsEndOfRound(objects);
        }
        #endregion
    }
}
