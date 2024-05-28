using System.Collections.Generic;
using UnityEngine;

namespace Remnants.Behaviours
{
    public class RandomizeBatteriesBehaviour
    {


        #region Variables
        private bool _hasInitialized = false;
        private System.Random _random = new System.Random();
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
        public void RandomizeItemsBattery(List<GameObject> itemsObjects = null)
        {
            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Randomizing items batteries.");
            if (itemsObjects == null)
            {
                mls.LogWarning("List to randomize batteries is null!");
                return;
            }
               
            foreach (var ItemObj in itemsObjects)
            {
                if (ItemObj == null)
                    continue;

                GrabbableObject grabbableObject = ItemObj.GetComponent<GrabbableObject>();
                if (!grabbableObject.itemProperties.requiresBattery)
                    continue;

                if (!(grabbableObject.insertedBattery != null &&//is this necessary?
                    grabbableObject.isInFactory == true && !grabbableObject.isInShipRoom))
                    continue;

                int randomCharge = _random.Next(Remnants.Instance.RemnantsConfig.MinRemnantBatteryCharge.Value, Remnants.Instance.RemnantsConfig.MaxRemnantBatteryCharge.Value);
                grabbableObject.SyncBatteryServerRpc(randomCharge);
                mls.LogInfo("Has updated " + grabbableObject.itemProperties.itemName + " charge to " + grabbableObject.insertedBattery.charge);
            }
        }

        public void RandomizeItemBattery(GameObject itemObject = null)
        {
            var mls = Remnants.Instance.Mls;
            if (itemObject == null)
                return;

            GrabbableObject grabbableObject = itemObject.GetComponent<GrabbableObject>();
            if (grabbableObject== null || !grabbableObject.itemProperties.requiresBattery)
                return;

            if (!(grabbableObject.insertedBattery != null &&
                grabbableObject.isInFactory == true && !grabbableObject.isInShipRoom))
                return;

            int randomCharge = _random.Next(Remnants.Instance.RemnantsConfig.MinRemnantBatteryCharge.Value, Remnants.Instance.RemnantsConfig.MaxRemnantBatteryCharge.Value);
            grabbableObject.SyncBatteryServerRpc(randomCharge);
            mls.LogInfo("Has updated " + grabbableObject.itemProperties.itemName + " charge to " + grabbableObject.insertedBattery.charge);

        }
        #endregion
    }
}
