using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Remnants.Behaviours
{
    public class RemnantItemsBehaviour
    {
        #region Variables
        private bool _hasInitialized = false;
        private List<GameObject> _remnantItemsObjects = new List<GameObject>();
        public List<GameObject> RemnantItems
        {
            get { return _remnantItemsObjects; }
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
        public void AddRemnantItemObject(GameObject gameObj)
        {
            if (gameObj == null)
                return;

            if (_remnantItemsObjects.Contains(gameObj))
                return;

            _remnantItemsObjects.Add(gameObj);
        }

        public void RemoveDespawnedAndNullItems()
        {
            _remnantItemsObjects.RemoveAll(remnantItem =>
            remnantItem == null ||
            remnantItem.GetComponent<NetworkObject>().IsSpawned == false
            );
        }
        #endregion
    }
}
