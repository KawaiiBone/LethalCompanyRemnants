using HarmonyLib;
using LethalLib.Modules;
using Remnants.Data;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Remnants.Behaviours
{
    public class BodyGrabbableObject : GrabbableObject
    {
        #region Variables
        private int _saveSuitIndex = 0;
        private BodySuitBehaviour _bodySuitBehaviour = null;

        #endregion

        #region Initialize 
        void Awake()
        {
            _bodySuitBehaviour = GetComponent<BodySuitBehaviour>();
        }
        #endregion

        #region Methods
        public override void EquipItem()
        {
            base.EquipItem();
        }

        protected override void __initializeVariables()
        {
            base.__initializeVariables();
        }

        protected override string __getTypeName()
        {
            return "BodyGrabbableObject";
        }

        public override int GetItemDataToSave()
        {
            if (!itemProperties.saveItemVariable)
            {
                Debug.LogError("GetItemDataToSave is being called on " + itemProperties.itemName + ", which does not have saveItemVariable set true.");
            }
            _saveSuitIndex = _bodySuitBehaviour.saveSuitIndex;
            return _saveSuitIndex;
        }

        public override void LoadItemSaveData(int saveData)
        {
            if (!itemProperties.saveItemVariable)
            {
                Debug.LogError("LoadItemSaveData is being called on " + itemProperties.itemName + ", which does not have saveItemVariable set true.");
            }
            _saveSuitIndex = saveData;
            _bodySuitBehaviour.UpdateSuit(_saveSuitIndex);
        }


        public void SyncIndexSuit(int indexSuit)
        {
            _bodySuitBehaviour.SyncIndexSuitClientRpc(indexSuit);
        }
        #endregion


    }
}
