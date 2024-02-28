using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode.Components;
using UnityEngine.SceneManagement;
using UnityEngine;
using Remnants.utilities;
using System.IO;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace Remnants.Behaviours
{
    internal class ScrapDataListBehaviour
    {
        #region Variables
        private List<ScrapItemData> _scrapItemsListData = new List<ScrapItemData>();
        private static string _fileName = "RemnantsSpawnableList.json";
        private static string _path = Application.persistentDataPath + "/" + "RemnantsSpawnableList.json";
        #endregion

        #region Initialize 

        #endregion

        #region Methods
        public void AddItemToDataList(string itemName)
        {

            if (_scrapItemsListData.FindIndex(scrapData => scrapData.ScrapItemName == itemName) != -1)
                return;

            ScrapItemData scrapItemData = new ScrapItemData();
            scrapItemData.ScrapItemName = itemName;
            scrapItemData.Isbanned = false;
            _scrapItemsListData.Add(scrapItemData);
        }

        public void UpdateScrapDataList()
        {
            var mls = Remnants.Instance._mls;
            //Get data and check if it needs to be changed
            mls.LogInfo(_path);
            if (File.Exists(_path))
            {
                UpdateDataList();

            }
            WriteScrapDataList();
        }

        private void WriteScrapDataList()
        {
            string json = JsonConvert.SerializeObject(_scrapItemsListData);
            File.WriteAllText(_path, json);
        }

        private void UpdateDataList()
        {
            var mls = Remnants.Instance._mls;
            List<ScrapItemData> fileScrapDataList = GetScrapItemDataList();
            List<ScrapItemData> dataToAdd = new List<ScrapItemData>();
            //Update data in ScrapItemsListData
            for (int i = 0; i < _scrapItemsListData.Count; i++)
            {
                int index = fileScrapDataList.FindIndex(scrapData => scrapData.ScrapItemName == _scrapItemsListData[i].ScrapItemName);
                if (index != -1)
                {
                    _scrapItemsListData[i].Isbanned = fileScrapDataList[index].Isbanned;
                }
            }
            //Find data to list if it is not found
            for (int j = 0; j < fileScrapDataList.Count; j++)
            {
                int index = _scrapItemsListData.FindIndex(scrapData => scrapData.ScrapItemName == fileScrapDataList[j].ScrapItemName);
                if (index == -1)
                {
                    dataToAdd.Add(fileScrapDataList[j]);
                }

            }
            //Add data to list if it is not found
            foreach (var data in dataToAdd)
            {
                _scrapItemsListData.Add(data);
            }
        }

        public static List<ScrapItemData> GetScrapItemDataList()
        {
            if (File.Exists(_path))
            {
                return JsonConvert.DeserializeObject<List<ScrapItemData>>(File.ReadAllText(_path));
            }
            else
            {
                var mls = Remnants.Instance._mls;
                mls.LogError(_fileName + " file not found!");
                return new List<ScrapItemData>();
            }

        }
        #endregion
    }
}
