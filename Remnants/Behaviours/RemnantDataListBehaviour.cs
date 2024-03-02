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
    internal class RemnantDataListBehaviour
    {
        #region Variables
        private List<RemnantData> _RemnantItemsListData = new List<RemnantData>();
        private static string _fileName = "RemnantsSpawnableList.json";
        private static string _path = Application.persistentDataPath + "/" + "RemnantsSpawnableList.json";
        #endregion

        #region Initialize 

        #endregion

        #region Methods
        public void AddItemToDataList(string itemName)
        {

            if (_RemnantItemsListData.FindIndex(scrapData => scrapData.RemnantItemName == itemName) != -1)
                return;

            RemnantData scrapItemData = new RemnantData();
            scrapItemData.RemnantItemName = itemName;
            scrapItemData.ShouldSpawn = true;
            _RemnantItemsListData.Add(scrapItemData);
        }

        public void UpdateScrapDataList()
        {
            //Get data and check if it needs to be changed
            List<RemnantData> remnantDataList = Data.Config.GetRemnantItemList();
            if (remnantDataList.Count != 0)
                UpdateDataList(remnantDataList);
            WriteScrapDataList();
        }

        private void WriteScrapDataList()
        {
            Data.Config.SetRemnantItemList(_RemnantItemsListData);
            //string json = JsonConvert.SerializeObject(_RemnantItemsListData);
            //File.WriteAllText(_path, json);
        }

        private void UpdateDataList(List<RemnantData> remnantDataList)
        {
            var mls = Remnants.Instance.Mls;
            List<RemnantData> fileScrapDataList = remnantDataList;
            List<RemnantData> dataToAdd = new List<RemnantData>();
            //Update data in ScrapItemsListData
            for (int i = 0; i < _RemnantItemsListData.Count; i++)
            {
                int index = fileScrapDataList.FindIndex(scrapData => scrapData.RemnantItemName == _RemnantItemsListData[i].RemnantItemName);
                if (index != -1)
                {
                    _RemnantItemsListData[i].ShouldSpawn = fileScrapDataList[index].ShouldSpawn;
                }
            }
            //Find data to list if it is not found
            for (int j = 0; j < fileScrapDataList.Count; j++)
            {
                int index = _RemnantItemsListData.FindIndex(scrapData => scrapData.RemnantItemName == fileScrapDataList[j].RemnantItemName);
                if (index == -1)
                {
                    dataToAdd.Add(fileScrapDataList[j]);
                }

            }
            //Add data to list if it is not found
            foreach (var data in dataToAdd)
            {
                _RemnantItemsListData.Add(data);
            }
        }

        #endregion
    }
}
