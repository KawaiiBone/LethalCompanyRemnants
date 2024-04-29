using Remnants.utilities;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Remnants.Behaviours
{
    public class RegisterBodySuitsBehaviour
    {
        #region Variables
        private bool _hasInitialized = false;
        private bool _isRegistering = false;
        private List<int> _suitsIndexList = new List<int>();
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
                SceneManager.sceneLoaded += RegisterSuitsIndexData;
            }
        }
        #endregion

        #region Methods

        private void RegisterSuitsIndexData(Scene scene, LoadSceneMode mode)
        {
            var mls = Remnants.Instance.Mls;
            var startOfRound = StartOfRound.Instance;
            if (startOfRound == null || _isRegistering)
                return;

            _isRegistering = true;
            mls.LogInfo("Registering suits indexes.");
            //Here I should get the list of suits that are saved.
            //Then, I should add an if to see if the name is in the list and If it it check if it is banned or not
            List<SuitData> suitsDataList = Remnants.Instance.RemnantsConfig.GetSuitsList();
            for (int i = 0; i < startOfRound.unlockablesList.unlockables.Count; i++)
            {
                if (startOfRound.unlockablesList.unlockables[i].suitMaterial != null && !_suitsIndexList.Contains(i))
                {
                    string suitName = startOfRound.unlockablesList.unlockables[i].suitMaterial.name;
                    int suitDataIndex = suitsDataList.FindIndex(suitData => suitData.SuitName == suitName);
                    if (suitDataIndex == -1)
                    {
                        _suitsIndexList.Add(i);
                        suitsDataList.Add(new SuitData() { SuitName = suitName, UseSuit = true });
                        mls.LogInfo("Register suit index of: " + suitName);
                    }
                    else if (suitsDataList[suitDataIndex].UseSuit)
                    {
                        _suitsIndexList.Add(i);
                        mls.LogInfo("Register suit index of: " + suitName);
                    }
                }
            }
            mls.LogInfo("Suits indexes registered.");
            SceneManager.sceneLoaded -= RegisterSuitsIndexData;
            Remnants.Instance.RemnantsConfig.SetSuitsList(suitsDataList);
            _isRegistering = false;
        }

        public void RegisterNewSuitsIndexData()
        {
            RegisterSuitsIndexData(SceneManager.GetActiveScene(), LoadSceneMode.Additive);
        }
        #endregion
    }
}