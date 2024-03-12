using BepInEx;
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
        private string _assetBundleName = "remnants";
        private string _prefabTypeName = ".prefab";
        private string _thunderStoreFolderName = "KawaiiBone-Remnants";
        private AssetBundle _assetBundleBodies = null;
        public static string[] BodiesFileNamesArray = { "DefaultBody", "HeadBurstBody", "CoilHeadBody", "WebbedBody" };
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
            string filePath = Path.Combine(Paths.PluginPath, _assetBundleName);
            if (!File.Exists(filePath))
            {
                filePath = Path.Combine(Paths.PluginPath, _thunderStoreFolderName, _assetBundleName);
                if(!File.Exists(filePath))
                {
                    mls.LogError("Assetbundle " + _assetBundleName + " not found.");
                    return;
                }
            }

            _assetBundleBodies = AssetBundle.LoadFromFile(filePath);
            if(_assetBundleBodies == null)
            {
                mls.LogError("Failed to load: " + _assetBundleName);
                return;
            }

            foreach (var bodyFileName in BodiesFileNamesArray)
            {
                LoadAndRegisterAsset(bodyFileName, _prefabTypeName);
            }
            //Later on:
            //var prefabs = _assetBundleBodies.LoadAllAssets<GameObject>();
        }

        private void LoadAndRegisterAsset(string assetName, string assetType)
        {
            var mls = Remnants.Instance.Mls;
            GameObject defaultBodyPrefab = _assetBundleBodies.LoadAsset<GameObject>(assetName + assetType);
            if (defaultBodyPrefab == null)
            {
                mls.LogError("Failed to load: " + assetName);
                return;
            }
            else
            {
                mls.LogInfo("Loaded asset: " + defaultBodyPrefab.name);
            }

            NetworkPrefabs.RegisterNetworkPrefab(defaultBodyPrefab);
        }
        #endregion
    }
}
