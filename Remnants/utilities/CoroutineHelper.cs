using System;
using System.Collections;
using UnityEngine;

namespace Remnants.utilities
{
    //I got this class from https://github.com/fardin2000/MonsterDrops
    //This helps me with easily syncing items in Lethal Company
    internal class CoroutineHelper : MonoBehaviour
    {
        #region Variables
        public static CoroutineHelper Instance { get; private set; }
        #endregion
        #region Initialize
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
        #endregion

        #region Methods
        public void ExecuteAfterDelay(Action action, float delay)
        {
            StartCoroutine(DelayedExecution(action, delay));
        }

        private IEnumerator DelayedExecution(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action.Invoke();
        }
        #endregion

    }
}
