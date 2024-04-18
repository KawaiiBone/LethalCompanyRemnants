using HarmonyLib;
using Remnants.Behaviours;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Remnants.Patches
{
    internal class SaveGamePatch
    {
        #region HarmonyMethods
        [HarmonyPatch(typeof(GameNetworkManager), "SaveItemsInShip")]
        [HarmonyPrefix]
        //This just copies the entire function just to replace on how it find items to save.
        //In time I will learn CodeInstruction so I can replace this in a more clean way.
        private static bool PatchSaveItems(GameNetworkManager __instance)
        {
            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Patching save game");
            if (Remnants.Instance.RemnantsConfig.ShouldSaveRemnantItems.Value == false)
            {
                mls.LogInfo("Skipping this mod version of saving and using default version.");
                return true;
            }
            //The game cannot detect remnant items with simplemeans as GameObject.Find<GrabbableObject>() 
            //So this is the best way to find it by this mod self
            var itemsLocationBeh = Remnants.Instance.RegisterItemLocationsBeh;
            var grabbableObjectsList = itemsLocationBeh.GetShipItems().Where(grabObj => !(grabObj is RagdollGrabbableObject) && (grabObj.isInShipRoom && grabObj.isInElevator)).ToList();
            //Get all remnant items that should be in ship from root
            GrabbableObject[] remnantRootItemsArray = itemsLocationBeh.GetItemsInRoot().Where(gameObj => gameObj.GetComponent<GrabbableObject>()).ToArray();
            remnantRootItemsArray = remnantRootItemsArray.Where(grabObj => !(grabObj is RagdollGrabbableObject) && (grabObj.isInShipRoom && grabObj.isInElevator)).ToArray();
            //Get all remnant items that should be in ship from  Environment Props 
            GrabbableObject[] grabObjArray = itemsLocationBeh.GetItemsInProps().Where(grabObj => !(grabObj is RagdollGrabbableObject) && (grabObj.isInShipRoom && grabObj.isInElevator)).ToArray();
            grabbableObjectsList.AddRange(remnantRootItemsArray);
            grabbableObjectsList.AddRange(grabObjArray);
            var array = grabbableObjectsList.ToArray();

            if (array == null || array.Length == 0)
            {
                ES3.DeleteKey("shipGrabbableItemIDs", __instance.currentSaveFileName);
                ES3.DeleteKey("shipGrabbableItemPos", __instance.currentSaveFileName);
                ES3.DeleteKey("shipScrapValues", __instance.currentSaveFileName);
                ES3.DeleteKey("shipItemSaveData", __instance.currentSaveFileName);
                return false;
            }
            else
            {
                if (StartOfRound.Instance.isChallengeFile)
                {
                    return false;
                }

                List<int> list = new List<int>();
                List<Vector3> list2 = new List<Vector3>();
                List<int> list3 = new List<int>();
                List<int> list4 = new List<int>();
                int num = 0;
                for (int i = 0; i < array.Length && i <= StartOfRound.Instance.maxShipItemCapacity; i++)
                {
                    if ((!StartOfRound.Instance.allItemsList.itemsList.Contains(array[i].itemProperties) /*&& array[i].GetComponent<BodyMovementBehaviour>() == null*/) || array[i].deactivated)
                    {
                        continue;
                    }

                    if (array[i].itemProperties.spawnPrefab == null)
                    {
                        mls.LogError("Item '" + array[i].itemProperties.itemName + "' has no spawn prefab set!");
                    }
                    else
                    {
                        if (array[i].itemUsedUp)
                        {
                            continue;
                        }

                        for (int j = 0; j < StartOfRound.Instance.allItemsList.itemsList.Count; j++)
                        {
                            if (StartOfRound.Instance.allItemsList.itemsList[j] == array[i].itemProperties /*|| array[i].GetComponent<BodyMovementBehaviour>() != null*/)
                            {
                                mls.LogInfo("Adding item to save: " + array[i].itemProperties.itemName);
                                list.Add(j);
                                list2.Add(array[i].transform.position);
                                break;
                            }
                        }

                        if (array[i].itemProperties.isScrap)
                        {
                            list3.Add(array[i].scrapValue);
                        }

                        if (array[i].itemProperties.saveItemVariable)
                        {
                            try
                            {
                                num = array[i].GetItemDataToSave();
                            }
                            catch
                            {
                                mls.LogError($"An error occured while getting item data to save for item type: {array[i].itemProperties}; gameobject '{array[i].gameObject.name}'");
                            }

                            list4.Add(num);
                            mls.LogInfo($"Saved data for item type: {array[i].itemProperties.itemName} - {num}");
                        }
                    }
                }

                try
                {
                    ES3.Save<Vector3[]>("shipGrabbableItemPos", list2.ToArray(), __instance.currentSaveFileName);
                    ES3.Save<int[]>("shipGrabbableItemIDs", list.ToArray(), __instance.currentSaveFileName);
                    if (list3.Count > 0)
                    {
                        ES3.Save<int[]>("shipScrapValues", list3.ToArray(), __instance.currentSaveFileName);
                    }
                    else
                    {
                        ES3.DeleteKey("shipScrapValues", __instance.currentSaveFileName);
                    }

                    if (list4.Count > 0)
                    {
                        ES3.Save<int[]>("shipItemSaveData", list4.ToArray(), __instance.currentSaveFileName);
                    }
                    else
                    {
                        ES3.DeleteKey("shipItemSaveData", __instance.currentSaveFileName);
                    }
                }
                catch (System.IO.IOException)
                {
                    mls.LogInfo("The file is open elsewhere or there is not enough storage space");
                    return true;
                }
                catch (System.Security.SecurityException)
                {
                    mls.LogInfo("Could not save as you do not have the required permissions");
                    return true;
                }
                mls.LogInfo("Items succesfully saved, skipping original version");
                return false;
            }
        }
        #endregion
    }
}
