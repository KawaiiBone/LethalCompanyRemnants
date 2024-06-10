using System;
using System.Collections.Generic;
using Unity.Netcode;
using LethalLib.Modules;
using UnityEngine;
using System.Linq;

namespace Remnants.Behaviours
{
    public class RemnantItemsBehaviour
    {
        #region Variables
        private bool _hasInitialized = false;
        private static List<GameObject> _foundRemnantItemsObjects = new List<GameObject>();
        private List<Items.ScrapItem> _networkRemnantItems = new List<Items.ScrapItem>();
        public static List<GameObject> FoundRemnantItems
        {
            get { return _foundRemnantItemsObjects; }
        }

        public List<Items.ScrapItem> NetworkRemnantItems
        {
            get { return _networkRemnantItems; }
        }
        #endregion

        #region Initialize 
        public void Initialize()
        {
            if (!_hasInitialized)
            {
                _hasInitialized = true;
            }
        }
        #endregion

        #region Methods
        public void AddFoundRemnantItemObject(GameObject gameObj)
        {
            if (gameObj == null)
                return;

            if (_foundRemnantItemsObjects.Contains(gameObj))
                return;

            _foundRemnantItemsObjects.Add(gameObj);
        }

        public void RemoveDespawnedAndNullItems()
        {
            _foundRemnantItemsObjects.RemoveAll(remnantItem =>
            remnantItem == null ||
            remnantItem.GetComponent<NetworkObject>().IsSpawned == false
            );
        }

        public void AddNetworkRemnantItem(Items.ScrapItem scrapItem)
        {
            if (scrapItem == null)
                return;

            if (_networkRemnantItems.Contains(scrapItem))
                return;

            _networkRemnantItems.Add(scrapItem);
        }
        #endregion
    }
}
