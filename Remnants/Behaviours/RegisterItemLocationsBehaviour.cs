using LethalLib.Modules;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Remnants.Behaviours
{
    public class RegisterItemLocationsBehaviour
    {

        #region Variables
        private bool _hasInitialized = false;
        private bool _isRegistering = false;
        private ObjectLocationData _shipObjectLocation = new ObjectLocationData(new string[] { "Environment", "HangarShip" }, null, true);
        private ObjectLocationData _propObjectLocation = new ObjectLocationData(new string[] { "Environment", "Props" }, null, false);
        private ObjectLocationData _rootObjectLocation = new ObjectLocationData(new string[] { }, null, false);
        public ObjectLocationData ShipObjectLocation
        {
            get { return _shipObjectLocation; }
        }
        public ObjectLocationData PropObjectLocation
        {
            get { return _propObjectLocation; }
        }
        public ObjectLocationData RootObjectLocation
        {
            get { return _rootObjectLocation; }
        }
        public struct ObjectLocationData
        {
            public string[] ObjectLocationsNames;
            public GameObject LocationObject;
            public bool IsShipRoom;

            public ObjectLocationData(string[] objectLocationsNames, GameObject locationObject, bool isShipRoom)
            {
                this.ObjectLocationsNames = objectLocationsNames;
                this.LocationObject = locationObject;
                this.IsShipRoom = isShipRoom;
            }
        }

        #endregion

        #region Initialize 
        public void Initialize()
        {
            if (!_hasInitialized)
            {
                _hasInitialized = true;
                SceneManager.sceneLoaded += RegisterItemsLocations;
            }
        }
        #endregion

        #region PublicMethods
        public GrabbableObject[] GetShipRemnantItems()
        {
            GrabbableObject[] remnantItemsArray = null;
            var mls = Remnants.Instance.Mls;

            remnantItemsArray = _shipObjectLocation.LocationObject.GetComponentsInChildren<GrabbableObject>().Where(grabObj =>
           (!(grabObj is RagdollGrabbableObject) && (grabObj.isInShipRoom || grabObj.isInElevator))).ToArray();
            remnantItemsArray = remnantItemsArray.Where(remnantItem => remnantItem.itemProperties.isScrap &&
            Items.scrapItems.FindIndex(scrapItem => scrapItem.item.itemName == remnantItem.itemProperties.itemName ||
            scrapItem.origItem.itemName == remnantItem.itemProperties.itemName) != -1).ToArray();
            return remnantItemsArray;
        }

        public GrabbableObject[] GetShipItems()
        {
            GrabbableObject[] remnantItemsArray = null;
            remnantItemsArray = _shipObjectLocation.LocationObject.GetComponentsInChildren<GrabbableObject>().Where(grabObj =>
            !(grabObj is RagdollGrabbableObject)).ToArray();
            return remnantItemsArray;
        }

        public GrabbableObject[] GetItemsInProps()
        {
            GrabbableObject[] remnantItemsArray = null;
            remnantItemsArray = _propObjectLocation.LocationObject.GetComponentsInChildren<GrabbableObject>();
            return remnantItemsArray;
        }

        public GrabbableObject[] GetItemsInRoot()
        {
            GrabbableObject[] remnantItemsArray = null;
            var rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            GameObject[] grabbableObjects = rootGameObjects.Where(gmObject => gmObject.GetComponent<GrabbableObject>() != null).ToArray(); ;
            remnantItemsArray = grabbableObjects.Select(gameObj => gameObj.GetComponent<GrabbableObject>()).ToArray();
            return remnantItemsArray;
        }
        #endregion

        #region PrivateMethods
        private void RegisterItemsLocations(Scene scene, LoadSceneMode mode)
        {
            if (_shipObjectLocation.LocationObject != null && _propObjectLocation.LocationObject != null)
                return;

            var startOfRound = StartOfRound.Instance;
            if (startOfRound == null || _isRegistering)
                return;

            _isRegistering = true;
            var rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            _shipObjectLocation.LocationObject = RegisterLocationObject(_shipObjectLocation.ObjectLocationsNames, rootGameObjects);
            _propObjectLocation.LocationObject = RegisterLocationObject(_propObjectLocation.ObjectLocationsNames, rootGameObjects);
            _isRegistering = false;
        }

        private GameObject RegisterLocationObject(string[] objectLocationsNames, GameObject[] rootObjects)
        {
            GameObject objectLocation = Array.Find(rootObjects, gameOBJ => gameOBJ.name == objectLocationsNames.First());
            for (int i = 1; i < objectLocationsNames.Length; i++)
            {
                objectLocation = objectLocation.transform.Find(objectLocationsNames[i]).gameObject;
            }
            return objectLocation;
        }
        #endregion
    }
}
