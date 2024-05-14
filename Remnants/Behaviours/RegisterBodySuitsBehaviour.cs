using Remnants.utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Remnants.Behaviours
{
    public class RegisterBodySuitsBehaviour
    {
        #region Variables
        private bool _hasInitialized = false;
        private List<int> _suitsIndexList = new List<int>();
        private List<SuitData> _suitsData = new List<SuitData>();
        public List<int> SuitsIndexList
        {
            get { return _suitsIndexList; }
        }
        #endregion

        #region Initialize 
        public void Initialize()
        {
            if (!_hasInitialized)
            {
                _hasInitialized = true;
                _suitsData = Remnants.Instance.RemnantsConfig.GetSuitsList();
            }
        }
        #endregion

        #region Methods

        public void RegisterSuitsDataToConfig(List<UnlockableItem> unlockableItemsList)
        {
            RegisterSuitsData(unlockableItemsList);
            Remnants.Instance.RemnantsConfig.SetSuitsList(_suitsData);
        }

        private void RegisterSuitsData(List<UnlockableItem> unlockableItemsList)
        {
            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Registering suits data.");
            //Here I should get the list of suits that are saved.
            //Then, I should add an if to see if the name is in the list and If it it check if it is banned or not
            List<string> registeredSuitsNames = new List<string>();
            for (int i = 0; i < unlockableItemsList.Count; i++)
            {
                if (unlockableItemsList[i].suitMaterial != null && !_suitsIndexList.Contains(i))
                {
                    string suitName = unlockableItemsList[i].unlockableName;
                    int suitDataIndex = _suitsData.FindIndex(suitData => suitData.SuitName == suitName);
                    if (registeredSuitsNames.Contains(suitName))
                        continue;

                    if (suitDataIndex == -1)
                    {
                        _suitsIndexList.Add(i);
                        _suitsData.Add(new SuitData() { SuitName = suitName, UseSuit = true });
                        mls.LogInfo("Register new suit data of: " + suitName);
                        registeredSuitsNames.Add(suitName);
                    }
                    else if (_suitsData[suitDataIndex].UseSuit)
                    {
                        _suitsIndexList.Add(i);
                        mls.LogInfo("Register suit data of: " + suitName);
                        registeredSuitsNames.Add(suitName);
                    }
                }
            }
            mls.LogInfo("Suits data registered.");
        }
        #endregion
    }
}