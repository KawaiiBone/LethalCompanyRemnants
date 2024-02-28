using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Remnants.Behaviours
{
    internal class RegisterBodyBehaviour : MonoBehaviour
    {

        #region Variables
        private bool _hasInitialized = false;
        private bool _isCreatingBody = false;
        private GameObject _prefab = null;
        private string _objectNameToFind = "ScavengerModel";
        private string _spawnBodyName = "ScavengerBody";
        private string _itemName = "BodyCorpse";
        private int _scrapCost = 5;
        private Item _bodyItem = null;
        private bool _isCreating = false;
        #endregion

        #region Initialize 
        public void Initialize()
        {
            if (!_hasInitialized)
            {
                _hasInitialized = true;
                SceneManager.sceneLoaded += CrteateItemAndRegisterBody;
            }
        }
        #endregion

        #region Methods

        private void CrteateItemAndRegisterBody(Scene scene, LoadSceneMode mode)
        {
            if (_isCreatingBody)
                return;

            var mls = Remnants.Instance._mls;
            List<GameObject> allItems = Resources.FindObjectsOfTypeAll<GameObject>().Concat(UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)).ToList();
            if (allItems == null)
            {
                mls.LogInfo("allItems Is Null!");
                return;
            }
            GameObject GameObjectToCopy = allItems.Find(gameObject => gameObject.name == _objectNameToFind);
            if (GameObjectToCopy == null)
            {
                mls.LogInfo(_objectNameToFind + " Not found!");
                return;
            }
            else
            {
                mls.LogInfo(_objectNameToFind + " found!");
            }
            _isCreatingBody = true;
            //_prefab = Instantiate(new GameObject());
            _prefab = Instantiate(GameObjectToCopy);
            _prefab.name = _spawnBodyName;

            if (_prefab == null)
            {
                mls.LogInfo("_prefab is null!");
            }
            _prefab.SetActive(true);
            CleanItemPrefab();
            SetUpRaggdoll();
            mls.LogInfo(_prefab.name + " Created");
            CreateItem();
            mls.LogInfo("Item: " +_bodyItem.name + " Created");

            Items.RegisterScrap(_bodyItem, 99, Levels.LevelTypes.All);
            mls.LogInfo("Registered scrap: " + _bodyItem.name);
            SceneManager.sceneLoaded -= CrteateItemAndRegisterBody;
        }

        private void CleanItemPrefab()
        {
            DeleteComp(_prefab.GetComponentInChildren<EnemyAICollisionDetect>());
            DeleteObj(_prefab.GetComponentInChildren<RandomPeriodicAudioPlayer>());
            DeleteObj(_prefab.GetComponentInChildren<RandomPeriodicAudioPlayer>());
            DeleteObj(_prefab.GetComponentInChildren<NetworkAnimator>());
            //_prefab.GetComponentInChildren<Animator>().SetBool("Dead", true);
        }

        private void SetUpRaggdoll()
        {
   
            Rigidbody[] rigidBodies = _prefab.GetComponentsInChildren<Rigidbody>();
            foreach (var rigidboy in rigidBodies)
            {
                rigidboy.useGravity = true;
            }
            Collider[] colliders = _prefab.GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
            {
                collider.enabled = true;
            }
            CharacterJoint[] characterJoints = _prefab.GetComponentsInChildren<CharacterJoint>();
            foreach (var characterJoint in characterJoints)
            {
                characterJoint.enableCollision = true;
            }

        }
        //I got this info from : https://github.com/MegaPiggy/LethalCompanyBuyableShotgunShells
        private void CreateItem()
        {
            var mls = Remnants.Instance._mls;
            _bodyItem = ScriptableObject.CreateInstance<Item>();
            //Maybe here copy the same from ragdollgrabable 
            _bodyItem.name = _itemName;
            _bodyItem.itemName = _itemName;
            _bodyItem.itemId = 7155;
            _bodyItem.isScrap = true;
            _bodyItem.creditsWorth = _scrapCost;
            _bodyItem.canBeGrabbedBeforeGameStart = true;
            _bodyItem.automaticallySetUsingPower = false;
            _bodyItem.batteryUsage = 300;
            _bodyItem.canBeInspected = false;
            _bodyItem.isDefensiveWeapon = false;
            _bodyItem.saveItemVariable = true;
            _bodyItem.syncGrabFunction = false;
            _bodyItem.twoHandedAnimation = true;
            _bodyItem.verticalOffset = 0.25f; // TEST
            //var prefab = _prefab;
            //var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //cube.transform.SetParent(prefab.transform, false);
            //cube.GetComponent<MeshRenderer>().sharedMaterial.shader = Shader.Find("HDRP/Lit");
            //prefab.AddComponent<BoxCollider>().size = Vector3.one * 2;
            //prefab.AddComponent<AudioSource>();
            var prefab = LethalLib.Modules.NetworkPrefabs.CreateNetworkPrefab("ScavengerBody");
            _prefab.transform.SetParent(prefab.transform, false);
             var prop = prefab.AddComponent<PhysicsProp>();
            prefab.AddComponent<MeshFilter>();
            prefab.AddComponent<MeshRenderer>();

            prop.itemProperties = _bodyItem;
            prop.itemProperties.meshVariants = new Mesh[0];
            prop.itemProperties.materialVariants = new Material[0];
            prop.grabbable = true;
            _bodyItem.spawnPrefab = prefab;
            prefab.tag = "PhysicsProp";
            prefab.layer = LayerMask.NameToLayer("Props");
            //.layer = LayerMask.NameToLayer("Props");
            try
            {
                GameObject scanNode = GameObject.Instantiate<GameObject>(Items.scanNodePrefab, _prefab.transform);
                scanNode.name = "ScanNode";
                scanNode.transform.localPosition = new Vector3(0f, 0f, 0f);
                scanNode.transform.localScale *= 2;
                ScanNodeProperties properties = scanNode.GetComponent<ScanNodeProperties>();
                properties.nodeType = 1;
                properties.headerText = "Body of unknown";
                properties.subText = "Cause of death: unknown";
            }
            catch (Exception e)
            {
                mls.LogError(e.ToString());
            }

            //_prefab.transform.localScale = Vector3.one / 2;
        }
        private void DeleteObj(MonoBehaviour comp)
        {
            var mls = Remnants.Instance._mls;
            if (comp != null)
            {
                mls.LogInfo("Atempting to delete from " + comp.name + " To delete obj: " + comp.gameObject.name);
                Destroy(comp.gameObject);
            }
            else
            {
                mls.LogInfo("Component is null");
            }
        }

        private void DeleteComp(MonoBehaviour comp)
        {
            var mls = Remnants.Instance._mls;
            if (comp != null)
            {
                mls.LogInfo("Atempting to delete " + comp.name);
                Destroy(comp);
              
            }
            else
            {
                mls.LogInfo("Component is null");
            }
        }
        #endregion
    }
}
