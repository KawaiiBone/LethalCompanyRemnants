using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
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
            get {return _suitsIndexList;} 
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
            {
                mls.LogInfo("startOfRound is null or is already registering indexes of suits.");
                return;
            }
            _isRegistering = true;
            mls.LogInfo("Registering suits indexes.");
            for (int i = 0; i < startOfRound.unlockablesList.unlockables.Count; i++)
            {
                if (startOfRound.unlockablesList.unlockables[i].suitMaterial != null && !_suitsIndexList.Contains(i))
                {
                    mls.LogInfo("Register suit index of: " + startOfRound.unlockablesList.unlockables[i].suitMaterial.name);
                    _suitsIndexList.Add(i);
                }
            }
            mls.LogInfo("Suits indexes registered.");
            SceneManager.sceneLoaded -= RegisterSuitsIndexData;
            _isRegistering = false;
        }

        public void RegisterNewSuitsIndexData()
        {
            RegisterSuitsIndexData(SceneManager.GetActiveScene(),LoadSceneMode.Additive);
        }
        #endregion
    }
}
