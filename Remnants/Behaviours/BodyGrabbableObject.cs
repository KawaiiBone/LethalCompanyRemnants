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
            var mls = Remnants.Instance.Mls;
            _bodySuitBehaviour = GetComponent<BodySuitBehaviour>();
            if (_bodySuitBehaviour == null)
                mls.LogError("Did not found BodySuitBehaviour.");

        }

        private void Start()
        {
            //base.Start();
            //for (int i = 0; i < propColliders.Length; i++)
            //{
            //    propColliders[i].includeLayers = -2621449;
            //}
            
            propColliders = base.gameObject.GetComponentsInChildren<Collider>();
 

            originalScale = base.transform.localScale;

            if (itemProperties.isScrap && RoundManager.Instance.mapPropsContainer != null)
            {
                radarIcon = UnityEngine.Object.Instantiate(StartOfRound.Instance.itemRadarIconPrefab, RoundManager.Instance.mapPropsContainer.transform).transform;
            }

            if (!itemProperties.isScrap)
            {
                HoarderBugAI.grabbableObjectsInMap.Add(base.gameObject);
            }

            originalScale = base.transform.localScale;
            if (itemProperties.itemSpawnsOnGround)
            {
                startFallingPosition = base.transform.position;
                if (base.transform.parent != null)
                {
                    startFallingPosition = base.transform.parent.InverseTransformPoint(startFallingPosition);
                }

                FallToGround();
            }
            else
            {
                fallTime = 1f;
                hasHitGround = true;
                reachedFloorTarget = true;
                targetFloorPosition = base.transform.localPosition;
            }

            MeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshRenderer>();
            for (int j = 0; j < componentsInChildren.Length; j++)
            {
                componentsInChildren[j].renderingLayerMask = 1u;
            }

            SkinnedMeshRenderer[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int k = 0; k < componentsInChildren2.Length; k++)
            {
                componentsInChildren2[k].renderingLayerMask = 1u;
            }

            EnablePhysics(true);
        }
        #endregion

        #region Methods
        public override void EquipItem()
        {
            base.EquipItem();
        }

        public override void Update()
        {
            base.Update();
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
            if (_bodySuitBehaviour != null)
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
            if (_bodySuitBehaviour != null)
                _bodySuitBehaviour.UpdateSuit(_saveSuitIndex);
        }


        public void SyncIndexSuit(int indexSuit)
        {
            if (_bodySuitBehaviour != null)
                _bodySuitBehaviour.SyncIndexSuitClientRpc(indexSuit);
        }
        #endregion
    }
}
