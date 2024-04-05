using Remnants.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Remnants.Behaviours
{
    public class BodyGrabbableObject : GrabbableObject
    {
        #region Variables
        private int _saveSuitIndex = 0;
        #endregion

        #region Initialize 
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

        protected /*internal*/ override string __getTypeName()
        {
            return "BodyGrabbableObject";
        }

        public override int GetItemDataToSave()
        {
            if (!itemProperties.saveItemVariable)
            {
                Debug.LogError("GetItemDataToSave is being called on " + itemProperties.itemName + ", which does not have saveItemVariable set true.");
            }

            return _saveSuitIndex;
        }

        public override void LoadItemSaveData(int saveData)
        {
            if (!itemProperties.saveItemVariable)
            {
                Debug.LogError("LoadItemSaveData is being called on " + itemProperties.itemName + ", which does not have saveItemVariable set true.");
            }
            UpdateSuit(saveData);
        }

        public void UpdateSuit(int suitIndex)
        {
            _saveSuitIndex = suitIndex;
            Remnants.Instance.Mls.LogError(itemProperties.spawnPrefab.name);
            if (!LoadAssetsBodies.BannedPrefabTexturesChange.Contains(itemProperties.spawnPrefab.name))
            {
                SkinnedMeshRenderer skinnedMeshedRenderer = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
                Material suitMaterial = StartOfRound.Instance.unlockablesList.unlockables[Remnants.Instance.RegisterBodySuits.SuitsIndexList[_saveSuitIndex]].suitMaterial;
                skinnedMeshedRenderer.material = suitMaterial;
                for (int i = 0; i < skinnedMeshedRenderer.materials.Length; i++)
                {
                    skinnedMeshedRenderer.materials[i] = suitMaterial;
                }
                Remnants.Instance.Mls.LogError("Changed texture of: " + itemProperties.spawnPrefab.name);
            }
        }
        #endregion
    }
}
