using System.Linq;
using UnityEngine;

namespace Remnants.Behaviours
{
    internal class BodyMovementBehaviour : MonoBehaviour
    {
        #region Variables
        private bool _hasInitialized = false;
        private GrabbableObject _grabbableObject = null;
        private Rigidbody[] _rigidbodies = null;
        private string _skipRigidBodyName = "ScanNode";
        private bool _isInStasis = false;
        private float _onGroundTimer = 0.0f;
        private float _onHeldTimer = 0.0f;
        private const float _onGroundMoveDuration = 2.0f;
        private const float _heldMoveDuration = 0.0f;
        #endregion

        #region Initialize 
        private void Awake()
        {
            var mls = Remnants.Instance.Mls;
            _grabbableObject = GetComponent<GrabbableObject>();
            if (_grabbableObject == null)
            {
                mls.LogWarning("Did not found GrabbableObject.");
                return;
            }

            _rigidbodies = _grabbableObject.GetComponentsInChildren<Rigidbody>();
            if (_rigidbodies == null)
            {
                mls.LogWarning("Rigidbodies is null.");
                return;
            }

            _rigidbodies = _rigidbodies.Where(rigidbody => rigidbody.gameObject.name != _skipRigidBodyName).ToArray();
            _hasInitialized = true;
        }
        #endregion

        #region Methods
        private void Update()
        {
            if (!_hasInitialized)
                return;
            //UpdateConstantMovement();
            UpdateMovementAtStart();
        }

        private void UpdateMovementAtStart()
        {
            if (_isInStasis)
                return;

            _onGroundTimer += Time.deltaTime;
            //Set in stasis after x time
            if (_onGroundTimer > _onGroundMoveDuration)
            {
                //Set in stasis
                SetStasis(true);
                _isInStasis = true;
            }
        }

        private void UpdateConstantMovement()
        {
            //Check if its held
            if (_grabbableObject.isHeld)
            {
                //Check if just held
                _onHeldTimer += Time.deltaTime;
                _onGroundTimer = 0.0f;
                if (_isInStasis && _onHeldTimer < _heldMoveDuration)
                {
                    //Set out of stasis
                    SetStasis(false);
                    _isInStasis = false;
                }//Set in stasis after x time
                if (!_isInStasis && _onHeldTimer > _heldMoveDuration)
                {
                    //Set in stasis
                    SetStasis(true);
                    _isInStasis = true;
                }
            }
            else if (!_grabbableObject.isHeld)
            {
                //check just released
                _onGroundTimer += Time.deltaTime;
                _onHeldTimer = 0.0f;
                if (_isInStasis && _onGroundTimer < _onGroundMoveDuration)
                {
                    //Set out of stasis
                    _isInStasis = false;
                    SetStasis(false);
                }//Set in stasis after x time
                else if (!_isInStasis && _onGroundTimer > _onGroundMoveDuration)
                {
                    //Set in stasis
                    SetStasis(true);
                    _isInStasis = true;
                }
            }
        }
        private void SetStasis(bool shouldBeStatis)
        {
            foreach (var rigidbody in _rigidbodies)
            {
                rigidbody.isKinematic = shouldBeStatis;
            }
        }
        #endregion
    }
}
