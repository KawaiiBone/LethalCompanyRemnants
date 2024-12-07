using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Remnants.Behaviours
{
    public class GrabbableObjsSpawnListBehaviour
    {
        #region Variables
        private bool _hasInitialized = false;
        private List<string> _bannedItemsNamesList = new List<string>();
        private static List<GrabbableObject> _spawnedGrabbableObjectList = new List<GrabbableObject>();
        private int _totalScrapValueInLevel = 0;
        private const string _unbannedItemName = "Key";
        private const string _shipGameObjName = "HangarShip";
        private const int _parentNestledSearchAmount = 4;
        #endregion

        #region Initialize 
        public void Initialize()
        {
            if (!_hasInitialized)
            {
                _hasInitialized = true;
                _bannedItemsNamesList = Remnants.Instance.RemnantsConfig.GetBannedFromRegisteringItemNames();
                _bannedItemsNamesList.RemoveAll(itemName => itemName == _unbannedItemName);
            }
        }
        #endregion

        #region Methods
        public void AddSpawnedGrabbableObject(GrabbableObject grabbableObject)
        {
            if (grabbableObject.itemProperties == null ||
                 _bannedItemsNamesList.FindIndex(x => x == grabbableObject.itemProperties.name || x == grabbableObject.itemProperties.itemName) != -1)
                return;
            _spawnedGrabbableObjectList.Add(grabbableObject);
        }

        public void CleanUpSpawnedGrabbableObjectList()
        {
            _spawnedGrabbableObjectList.RemoveAll(grabObj => grabObj == null || grabObj.itemProperties == null || grabObj.itemProperties.spawnPrefab == null);
        }

        public GrabbableObject[] GetGrabbableObjectsToSave()
        {
            var mls = Remnants.Instance.Mls;
            if (Remnants.Instance.RemnantsConfig.ShouldSaveRemnantItems.Value == false)
            {
                mls.LogInfo("Not saving remnant items.");
                //Get non remnant items from normal place
                GrabbableObject[] nonRemnantItemsArray = UnityEngine.Object.FindObjectsByType<GrabbableObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                return nonRemnantItemsArray;
            }
            CleanUpSpawnedGrabbableObjectList();
            GrabbableObject[] grabbableObjects = _spawnedGrabbableObjectList.Where(grabObj =>
            !(grabObj is RagdollGrabbableObject) && ((grabObj.isInShipRoom && grabObj.isInElevator) || (grabObj.isInElevator && grabObj.isPocketed))).ToArray();
            return grabbableObjects;
        }

        public KeyValuePair<GrabbableObject[], GrabbableObject[]> GetGrabbableObjectShipAndRest()
        {
            CleanUpSpawnedGrabbableObjectList();
            List<GrabbableObject> shipGrabbableObjects = _spawnedGrabbableObjectList.Where(grabObj =>
            !(grabObj is RagdollGrabbableObject) && ((grabObj.isInShipRoom && grabObj.isInElevator) || (grabObj.isInElevator && grabObj.isPocketed))).ToList();   
            Transform transformParent = null;

            shipGrabbableObjects.RemoveAll((grabObj) =>
            {
                transformParent = grabObj.transform.parent;
                for (int i = 0; i < _parentNestledSearchAmount; i++)
                {
                    if (transformParent == null)
                        return false;
                    if (transformParent.name == _shipGameObjName)
                        return true;
                    transformParent = transformParent.parent;
                }
                return false;
            });
            GrabbableObject[] restOfTheGrabObjs = _spawnedGrabbableObjectList.Where(grabObj => 
            (shipGrabbableObjects.FindIndex(grabShipObj => grabShipObj.NetworkObjectId == grabObj.NetworkObjectId) == -1)).ToArray();
            return new KeyValuePair<GrabbableObject[], GrabbableObject[]>(shipGrabbableObjects.ToArray(), restOfTheGrabObjs);
        }

        public GrabbableObject[] GetSpawnedGrabbableObjects()
        {
            var mls = Remnants.Instance.Mls;
            GrabbableObject[] grabbableObjects = null;
            CleanUpSpawnedGrabbableObjectList();
            grabbableObjects = _spawnedGrabbableObjectList.ToArray();
            return grabbableObjects;
        }

        public void AddScrapValueFromLevel(int scrapValue)
        {
            _totalScrapValueInLevel += scrapValue;
        }
        public void ResetTotalScrapValueInLevel()
        {
            _totalScrapValueInLevel = 0;
        }

        public void UpdateTotalScrapValue()
        {
            RoundManager.Instance.totalScrapValueInLevel = _totalScrapValueInLevel;
        }

        #endregion

    }
}
