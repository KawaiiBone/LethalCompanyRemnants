﻿using LethalLib.Modules;
using Remnants.Behaviours;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Remnants.Data
{
    internal class LoadAssetsBodies
    {
        #region Variables
        private bool _hasInitialized = false;
        private bool _isLoadingBundle = false;
        private string _assetBundleName = "remnants";
        private string _prefabTypeName = ".prefab";
        private string _assetTypeName = ".asset";
        private string _iconSpriteName = "ScrapItemIcon2";
        private string _dropSoundName = "BodyCollision2";
        private string _grabSoundName = "StartJump";
        private string _playerRagdollLayer = "PlayerRagdoll";
        private AssetBundle _assetBundleBodies = null;
        public static string[] BodiesFileNamesArray = { "DefaultBodyProp", "HeadBurstBodyProp", "CoilHeadBodyProp", "WebbedBodyProp" };
        public static string[] BodiesItemsFileNamesArray = { "DefaultBodyItem", "HeadBurstBodyItem", "CoilBodyItem", "WebbedBodyItem" };
        public static KeyValuePair<string, string>[] EnemiesAndBodiesNames = new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("Any", "DefaultBody"), new KeyValuePair<string, string>("Spring", "CoilHeadBody"), new KeyValuePair<string, string>("Bunker Spider", "WebbedBody"), new KeyValuePair<string, string>("Girl", "HeadBurstBody") };
        public static bool HasLoadedAnyAssets = false;
        #endregion

        #region Initialize 
        public void Initialize()
        {
            if (!_hasInitialized)
            {
                _hasInitialized = true;
                SceneManager.sceneLoaded += LoadAssetBundle;
            }
        }
        #endregion

        #region Methods
        private void LoadAssetBundle(Scene scene, LoadSceneMode mode)
        {
            var mls = Remnants.Instance.Mls;
            //This is here to check if the right resources
            Sprite iconSprite = Resources.FindObjectsOfTypeAll<Sprite>().Concat(UnityEngine.Object.FindObjectsByType<Sprite>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)).
             ToList().Find(sprite => sprite.name == _iconSpriteName);
            if (_isLoadingBundle || iconSprite == null)
            {
                return;
            }

            _isLoadingBundle = true;
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(assemblyFolder, _assetBundleName);
            if (!File.Exists(filePath))
            {
                mls.LogError("Assetbundle " + _assetBundleName + " not found.");
                return;
            }

            _assetBundleBodies = AssetBundle.LoadFromFile(filePath);
            if (_assetBundleBodies == null)
            {
                mls.LogError("Failed to load: " + _assetBundleName);
                return;
            }

            var audioClips = Resources.FindObjectsOfTypeAll<AudioClip>().Concat(UnityEngine.Object.FindObjectsByType<AudioClip>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)).ToList();
            AudioClip dropSFX = audioClips.Find(audioClip => audioClip.name == _dropSoundName);
            AudioClip grabSFX = audioClips.Find(audioClip1 => audioClip1.name == _grabSoundName);
            foreach (var itemFileName in BodiesItemsFileNamesArray)
            {
                LoadAndRegisterBodyItemAsset(itemFileName, _assetTypeName, iconSprite, dropSFX, grabSFX);
            }

            foreach (var bodyFileName in BodiesFileNamesArray)
            {
                LoadAndRegisterBodyPropAsset(bodyFileName, _prefabTypeName);
            }
            //Later on:
            //var prefabs = _assetBundleBodies.LoadAllAssets<GameObject>();
            SceneManager.sceneLoaded -= LoadAssetBundle;
        }

        private void LoadAndRegisterBodyPropAsset(string assetName, string assetType)
        {
            var mls = Remnants.Instance.Mls;
            GameObject bodyPrefab = _assetBundleBodies.LoadAsset<GameObject>(assetName + assetType);
            if (bodyPrefab == null)
            {
                mls.LogError("Failed to load: " + assetName);
                return;
            }
            else
            {
                mls.LogInfo("Loaded asset: " + bodyPrefab.name);
            }
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(bodyPrefab);
            HasLoadedAnyAssets = true;
        }

        private void LoadAndRegisterBodyItemAsset(string assetName, string assetType, Sprite icon, AudioClip dropSFX, AudioClip grabSFX)
        {
            var mls = Remnants.Instance.Mls;
            Item bodyItem = _assetBundleBodies.LoadAsset<Item>(assetName + assetType);
            if (bodyItem == null)
            {
                mls.LogError("Failed to load: " + assetName);
                return;
            }
            else
            {
                mls.LogInfo("Loaded asset: " + bodyItem.name);
            }
            bodyItem.itemIcon = icon;
            bodyItem.dropSFX = dropSFX;
            bodyItem.grabSFX = grabSFX;
            bodyItem.spawnPrefab.AddComponent<BodyMovementBehaviour>();
            Items.RegisterItem(bodyItem);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(bodyItem.spawnPrefab);
            HasLoadedAnyAssets = true;
        }
        #endregion
    }
}
