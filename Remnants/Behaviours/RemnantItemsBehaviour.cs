using System.Collections.Generic;
using Unity.Netcode;
using LethalLib.Modules;
using UnityEngine;

namespace Remnants.Behaviours
{
    public class RemnantItemsBehaviour
    {
        #region Variables
        private bool _hasInitialized = false;
        private List<Items.ScrapItem> _networkRemnantItems = new List<Items.ScrapItem>();

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
