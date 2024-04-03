using HarmonyLib;
using LethalLib.Modules;
using System.Linq;
using System.Xml.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Remnants.Patches
{
    internal class DespawnRemnantsPatch
    {
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
            if ((StartOfRound.Instance.allPlayersDead || despawnAllItems) && Data.Config.ShouldDespawnRemnantItems.Value == true)
            {
                var hangarShip = GameObject.Find("HangarShip");
                GrabbableObject[] remnantItemsArray = hangarShip.GetComponentsInChildren<GrabbableObject>().Where(
                    grabObj => (!(grabObj is RagdollGrabbableObject) && grabObj.isInShipRoom) &&
                    Items.scrapItems.FindIndex(scrapItem => scrapItem.item.itemName == grabObj.itemProperties.itemName) != -1).ToArray();
                foreach (var remnantItem in remnantItemsArray)
                {
                    if (!remnantItem.GetComponent<NetworkObject>().IsSpawned)
                        continue;

                    if (remnantItem.isHeld && remnantItem.playerHeldBy != null)
                        remnantItem.playerHeldBy.DropAllHeldItemsAndSync();

                    remnantItem.GetComponent<NetworkObject>().Despawn();
                    mls.LogInfo("Dispawning " + remnantItem.itemProperties.itemName + " from shiproom.");
                }
            }
            //Despawn remnant items in root objects
            GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            GameObject[] toDespawnRemnantItems = rootGameObjects.Where(gmObject => gmObject.GetComponent<GrabbableObject>() != null).ToArray();
            foreach (var remnantObject in toDespawnRemnantItems)
            {
  
                NetworkObject networkObject = remnantObject.GetComponent<NetworkObject>();
                if (!networkObject.IsSpawned)
                    continue;

                GrabbableObject grabbableObject = remnantObject.GetComponent<GrabbableObject>();
                if (grabbableObject.isInShipRoom)
                    continue;

                if (grabbableObject.isHeld && grabbableObject.playerHeldBy != null)
                        grabbableObject.playerHeldBy.DropAllHeldItemsAndSync();

                mls.LogInfo("Despawning " + grabbableObject.itemProperties.itemName + " from root objects.");
                networkObject.Despawn();
            }
            //Despawn remnant items in Environment Props array
            GameObject environmentObj = rootGameObjects.ToList().Find(gameObject => gameObject.name == "Environment");
            GameObject propObj = null;
            foreach (Transform transformChild in environmentObj.transform)
            {
                if(transformChild.name == "Props")
                {
                    propObj = transformChild.gameObject;
                    break;
                }
            }
            GrabbableObject[] grabObjArray = propObj.GetComponentsInChildren<GrabbableObject>();
            foreach (var grabObj in grabObjArray)
            {

                NetworkObject networkObject = grabObj.GetComponent<NetworkObject>();
                if (!networkObject.IsSpawned)
                    continue;

                if (grabObj.isInShipRoom)
                    continue;

                if (grabObj.isHeld && grabObj.playerHeldBy != null)
                    grabObj.playerHeldBy.DropAllHeldItemsAndSync();

                mls.LogInfo("Despawning " + grabObj.itemProperties.itemName + " from environment props.");
                networkObject.Despawn();
            }
        }

        [HarmonyPatch(typeof(GameNetworkManager), "Disconnect")]
        [HarmonyPostfix]
        public static void DespawnRemnantItemsOnDisconnect(GameNetworkManager __instance)
        {
            if (!__instance.isHostingGame)
                return;

            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Patching Despawn Remnant Items at disconnect.");
            object[] objects ={true};
            DespawnRemnantItemsEndOfRound(objects);
        }

        #endregion
    }
}
