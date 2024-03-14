using LethalLib.Modules;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Remnants.Data
{
    internal class LoadAssetsBodies
    {
        #region Variables
        private bool _hasInitialized = false;
        private string _assetBundleName = "remnants";
        private string _folderPluginName = "Remnants";
        private string _prefabTypeName = ".prefab";
        private string _thunderStoreFolderName = "KawaiiBone-Remnants";
        private AssetBundle _assetBundleBodies = null;
        public static string[] BodiesFileNamesArray = { "DefaultBody", "HeadBurstBody", "CoilHeadBody", "WebbedBody" };
        public static KeyValuePair<string, string>[] EnemiesAndBodiesNames = new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("Any", "DefaultBody"), new KeyValuePair<string, string>("Spring", "CoilHeadBody"), new KeyValuePair<string, string>("Bunker Spider", "WebbedBody"), new KeyValuePair<string, string>("Girl", "HeadBurstBody") };
        public static bool HasLoadedAnyAssets = false;
        #endregion

        #region Initialize 
        public void Initialize()
        {
            if (!_hasInitialized)
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
            HasLoadedAnyAssets = true;
        }
        #endregion
    }
}
