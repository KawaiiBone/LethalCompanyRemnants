using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Remnants.Behaviours
{
    public class SpawnBodyBehaviour
    {
        #region Variables
        private bool _hasInitialized = false;
        private float _maxReachDistance = 6.0f;
        private float _minReachDistance = 0.125f;//Maybe limit is 0.165
        private float _movedReachDistance = 5.0f;
        private float _moveDistance = 1.0f;
        private int _areaMask = -1;
        private float _yOffset = 1.0f;
        private string[] _riskLevelArray = { "Safe", "D", "C", "B", "A", "S", "S+" };
        private float _spawnChance = 0.0f;
        private float spawnChanceModifier = 0.0f;
        #endregion

        #region Initialize 
        public void Initialize()
        {
            if (!_hasInitialized)
            {
                _hasInitialized = true;
                _spawnChance = Remnants.Instance.RemnantsConfig.SpawnRarityOfBody.Value;
                spawnChanceModifier = Remnants.Instance.RemnantsConfig.SpawnModifierRiskLevel.Value;
            }
        }
        #endregion
        #region Methods
        public bool CalculatePositionOnNavMesh(Vector3 startPosition, out Vector3 newPosition)
        {
            var mls = Remnants.Instance.Mls;
            bool isWithinMaxRange = NavMesh.SamplePosition(startPosition, out NavMeshHit maxRangeNavHit, _maxReachDistance, _areaMask);
            if (!isWithinMaxRange)
            {
                mls.LogWarning("Position is not in range of Navmesh.");
                newPosition = Vector3.zero;
                return false;
            }

            //mls.LogInfo("Distance from navmesh: " + maxRangeNavHit.distance);
            if (maxRangeNavHit.distance <= _minReachDistance)
            {
                mls.LogInfo("Position is already on navmesh.");
                newPosition = startPosition;
                newPosition.y += _yOffset;
                return true;
            }

            Vector3 positionYFlat = maxRangeNavHit.position;
            positionYFlat.y = startPosition.y;
            Vector3 heading = positionYFlat - startPosition;
            float distance = heading.magnitude;
            Vector3 direction = heading / distance;
            Vector3 movedPosition = maxRangeNavHit.position + (direction * _moveDistance);
            bool isWithinMovedRange = NavMesh.SamplePosition(movedPosition, out NavMeshHit movedNavHit, _movedReachDistance, _areaMask);
            if(isWithinMovedRange)
            {
                mls.LogInfo("Moved position found on navmesh.");
                newPosition = movedNavHit.position;
                newPosition.y += _yOffset;
                return true;
            }
            else
            {
                mls.LogInfo("Moved position not found on navmesh, using older position on navmesh.");
                newPosition = maxRangeNavHit.position;
                newPosition.y += _yOffset;
                return true;
            }
        }

        public int CalculateTotalRarityValue(List<KeyValuePair<GameObject, int>> prefabAndRarityList)
        {
            int totalRarityValue = 0;
            foreach (var prefabRarity in prefabAndRarityList)
            {
                totalRarityValue += prefabRarity.Value;
            }
            return totalRarityValue;
        }

        public int GetRandomBodyIndex(List<KeyValuePair<GameObject, int>> prefabAndRarityList, int randomNumber)
        {
            int totalValue = 0;
            for (int i = 0; i < prefabAndRarityList.Count; ++i)
            {
                totalValue += prefabAndRarityList[i].Value;
                if (totalValue > randomNumber)
                    return i;
            }
            return 0;
        }

        public float CalculateSpawnChance(string riskLevelName)
        {
            float spawnChance = _spawnChance;
            int riskLevel = Array.IndexOf(_riskLevelArray, riskLevelName);
            if (!Mathf.Approximately(spawnChanceModifier, 0.0f) && riskLevel != -1)
                spawnChance *= (riskLevel * spawnChanceModifier);

            return spawnChance;
        }
        #endregion
    }
}
