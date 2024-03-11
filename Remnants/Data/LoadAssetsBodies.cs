﻿using BepInEx;
using BepInEx.Configuration;
using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Remnants.Data
{
    internal class LoadAssetsBodies
    {
        #region Variables
        private bool _hasInitialized = false;
        private string _pathPlugin = Paths.PluginPath;
        private string _assetBundleName = "bodies";
        private string _defaultBodyFileName = "DefaultBody";
        private string _prefabTypeName = ".prefab";
        private AssetBundle _assetBundleBodies = null;
        #endregion

        #region Initialize 
        public void Initialize()
        {
            if(!_hasInitialized)
            {
                _hasInitialized = true;
                LoadAssetBundle();
            }
        }
        #endregion

        #region Methods
        private void LoadAssetBundle()
        {
            var mls = Remnants.Instance.Mls;
            string filePath = Path.Combine(_pathPlugin, _assetBundleName);
            if(!File.Exists(filePath))
            {
                mls.LogError("Assetbundle " + _assetBundleName + " not found.");
                return;
            }

            _assetBundleBodies = AssetBundle.LoadFromFile(filePath);
            if(_assetBundleBodies == null)
            {
                mls.LogError("Failed to load Bodies.");
                return;
            }

            GameObject defaultBodyPrefab = _assetBundleBodies.LoadAsset<GameObject>(_defaultBodyFileName + _prefabTypeName);
            //Later on:
            //var prefabs = _assetBundleBodies.LoadAllAssets<GameObject>();
            if (defaultBodyPrefab == null)
            {
                mls.LogError("Failed to load: " + _defaultBodyFileName);
                return;
            }
           
            NetworkPrefabs.RegisterNetworkPrefab(defaultBodyPrefab);
        }
        #endregion
    }
}
