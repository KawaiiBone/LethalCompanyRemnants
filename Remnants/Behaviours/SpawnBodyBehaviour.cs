using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Remnants.Behaviours
{
    internal class SpawnBodyBehaviour : MonoBehaviour
    {
        #region Variables
        private GameObject _spawnedGameObject = null;
        private float _yOffset = 2.0f;
        private bool _isSpawning = false;
        #endregion

        #region Initialize
        #endregion

        #region Methods
        public NetworkObject CreateAndSpawnBody()
        {
            if (_isSpawning)
                return null;

            _isSpawning = true;
            var mls = Remnants.Instance._mls;
            StartOfRound startOfRound = FindObjectOfType<StartOfRound>();
            if (startOfRound == null)
            {
                mls.LogInfo("StartOfRound not found!");
                return null;
            }
            mls.LogInfo("StartOfRound found!");
            //CREATE body
            GameObject bodyGameObject = null;
            Array values = Enum.GetValues(typeof(CauseOfDeath));
            System.Random random = new System.Random();
            CauseOfDeath causeOfDeath = (CauseOfDeath)values.GetValue(random.Next(values.Length));
            //This is an almost full copy of the SpawnDeadBody funtion  in playerControllerB
            Transform parent = null;
            Vector3 position = transform.localPosition;

            position.y += _yOffset;
            _spawnedGameObject = UnityEngine.Object.Instantiate(startOfRound.playerRagdolls[0], position, Quaternion.identity, parent);//to test
            DeadBodyInfo component = _spawnedGameObject.GetComponent<DeadBodyInfo>();
            component.overrideSpawnPosition = true; // this fixes the spawning issue

            component.parentedToShip = false;
            component.playerObjectId = 0;//startOfRound.allPlayerObjects.Length - 1;
            DeadBodyInfo deadBody = component;
            Rigidbody[] componentsInChildren = _spawnedGameObject.GetComponentsInChildren<Rigidbody>();
            mls.LogInfo("componentsInChildren: " + componentsInChildren.Length);
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                componentsInChildren[i].velocity = Vector3.zero;

            }

            bodyGameObject = UnityEngine.Object.Instantiate(startOfRound.ragdollGrabbableObjectPrefab, _spawnedGameObject.transform);
            Component[] componentsSpawnPrefab = startOfRound.ragdollGrabbableObjectPrefab.GetComponents(typeof(Component));
            _spawnedGameObject.transform.SetParent(bodyGameObject.transform, false);

            bodyGameObject.GetComponent<RagdollGrabbableObject>().bodyID.Value = component.playerObjectId;

            RagdollGrabbableObject ragdollGrabbableObject = bodyGameObject.GetComponent<RagdollGrabbableObject>();
            ragdollGrabbableObject.propColliders = _spawnedGameObject.GetComponentsInChildren<Collider>();

            ragdollGrabbableObject.ragdoll = deadBody;
            ragdollGrabbableObject.ragdoll.grabBodyObject = ragdollGrabbableObject;
            ragdollGrabbableObject.parentObject = ragdollGrabbableObject.ragdoll.bodyParts[5].transform;
            ragdollGrabbableObject.transform.SetParent(ragdollGrabbableObject.ragdoll.bodyParts[5].transform);


            ragdollGrabbableObject.EnablePhysics(enable: true);
            ragdollGrabbableObject.EnableItemMeshes(enable: true);
            ragdollGrabbableObject.isHeld = false;
            ragdollGrabbableObject.isPocketed = false;
            ragdollGrabbableObject.testBody = true;//TEST

            mls.LogInfo("ragdollGrabbableObject.propColliders: " + ragdollGrabbableObject.propColliders.Length);

            ScanNodeProperties componentInChildren = component.gameObject.GetComponentInChildren<ScanNodeProperties>();
            componentInChildren.headerText = "Body of " + "unknown";
            CauseOfDeath causeOfDeath2 = (CauseOfDeath)causeOfDeath;
            componentInChildren.subText = "Cause of death: " + causeOfDeath2;
            deadBody.causeOfDeath = causeOfDeath2;
            if (causeOfDeath2 == CauseOfDeath.Bludgeoning || causeOfDeath2 == CauseOfDeath.Mauling || causeOfDeath2 == CauseOfDeath.Gunshots)
            {
                deadBody.MakeCorpseBloody();

            }

            mls.LogInfo("SpawnedCorpse");
            NetworkObject networkObject = bodyGameObject.GetComponent<NetworkObject>();
            networkObject.SpawnWithObservers = true;
            networkObject.DontDestroyWithOwner = true;
            return networkObject;
        }
        #endregion
    }
}

