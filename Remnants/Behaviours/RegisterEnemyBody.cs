using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Remnants.Behaviours
{
    internal class RegisterEnemyBody : MonoBehaviour
    {

        #region Variables
        private bool _hasInitialized = false;
        private bool _isAddingEnemy = false;
        private const string _enemyName = "Masked";
        public static string EnemyNameBody = "MaskedBody";
        public static EnemyType EnemyBody = null;
        #endregion

        #region Initialize 
        public void Initialize()
        {
            if (!_hasInitialized)
            {
                _hasInitialized = true;
                //SceneManager.sceneLoaded += RegisterEnemy;
            }
        }
        #endregion

        #region Methods

        private void RegisterEnemy(Scene scene, LoadSceneMode mode)
        {
            if (_isAddingEnemy)
                return;

            var mls = Remnants.Instance._mls;
            List<EnemyType> allEnemyTypes = Resources.FindObjectsOfTypeAll<EnemyType>().Concat(UnityEngine.Object.FindObjectsByType<EnemyType>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)).ToList();

            if(allEnemyTypes == null || allEnemyTypes.Count == 0)
            {
                mls.LogInfo("Cant register enemy, allEnemyTypes are not loaded in yet.");
                return;
            }

            if (_isAddingEnemy)
                return;

            _isAddingEnemy = true;
            foreach (var enemy in allEnemyTypes)
            {
                mls.LogInfo(enemy.enemyName);
                if (enemy.enemyName == _enemyName)
                {
                    EnemyBody = Instantiate(enemy);
                    break;
                }
            }
            if(EnemyBody == null)
            {
                mls.LogInfo("Cant register enemy, maskedEnemy is null!");
                return;
            }

            EnemyBody.enemyName = EnemyNameBody;
            Enemies.RegisterEnemy(EnemyBody, 0, Levels.LevelTypes.All,null, null);
            SceneManager.sceneLoaded -= RegisterEnemy;
        }
        #endregion
    }
}
