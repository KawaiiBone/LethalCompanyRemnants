using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Remnants.Behaviours
{
    public class RandomizeBatteriesBehaviour
    {


        #region Variables
        private bool _hasInitialized = false;
        private int _minBatteryCharge = 0;
        private int _maxBatteryCharge = 0;
        private System.Random _random = new System.Random();
        #endregion

        #region Initialize 
        public void Initialize()
        {
            if (!_hasInitialized)
            {
                _hasInitialized = true;
                _minBatteryCharge = Remnants.Instance.RemnantsConfig.MinRemnantBatteryCharge.Value;
                _maxBatteryCharge = Remnants.Instance.RemnantsConfig.MaxRemnantBatteryCharge.Value;
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
                //if (grabbableObject == null) //is this necessary?
                //    continue;

                if (!grabbableObject.itemProperties.requiresBattery)
                    continue;

                if (!(grabbableObject.insertedBattery != null &&//is this necessary?
                    grabbableObject.isInFactory == true && !grabbableObject.isInShipRoom))
                    continue;

                int randomCharge = _random.Next(_minBatteryCharge, _maxBatteryCharge);
                grabbableObject.SyncBatteryServerRpc(randomCharge);
                mls.LogInfo("Has updated " + grabbableObject.itemProperties.name + " charge to " + grabbableObject.insertedBattery.charge);
            }
        }
        #endregion
    }
}
