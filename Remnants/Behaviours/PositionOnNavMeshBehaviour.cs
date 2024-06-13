using UnityEngine.AI;
using UnityEngine;

namespace Remnants.Behaviours
{
    internal class PositionOnNavMeshBehaviour
    {
        #region Variables
        private float _maxReachDistance = 6.0f;
        private float _minReachDistance = 0.125f;//limit is 0.165
        private float _movedReachDistance = 5.0f;
        private float _moveDistance = 1.0f;
        private float _yOffset = 1.0f;
        private const int _areaMask = -1;
        #endregion


        #region Initialize 
        public PositionOnNavMeshBehaviour(float maxReachDistance, float minReachDistance, float movedReachDistance, float moveDistance, float yOffset)
        {
            this._maxReachDistance = maxReachDistance;
            this._minReachDistance = minReachDistance;
            this._movedReachDistance = movedReachDistance;
            this._moveDistance = moveDistance;
            this._yOffset = yOffset;
        }
        #endregion

        #region PublicMethods
        public bool SetPositionOnNavMesh(Vector3 startPosition, out Vector3 newPosition)
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
                //mls.LogInfo("Position is already on navmesh.");
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
            if (isWithinMovedRange)
            {
                //mls.LogInfo("Moved position found on navmesh.");
                newPosition = movedNavHit.position;
                newPosition.y += _yOffset;
                return true;
            }
            else
            {
                mls.LogWarning("Moved position not found on navmesh, using older position on navmesh.");
                newPosition = maxRangeNavHit.position;
                newPosition.y += _yOffset;
                return true;
            }
        }

        public bool SetRandomOffsetOnNavmesh(Vector3 startPosition, out Vector3 newPosition)
        {
            var mls = Remnants.Instance.Mls;
            bool isWithinNavmeshRange = NavMesh.SamplePosition(startPosition, out NavMeshHit maxRangeNavHit, _minReachDistance, _areaMask);
            if (!isWithinNavmeshRange)
            {
                mls.LogWarning("Position is not on Navmesh.");
                newPosition = Vector3.zero;
                return false;
            }
            float distance = Mathf.Abs(_moveDistance);
            Vector3 randomPosition = new Vector3(UnityEngine.Random.Range(-distance, distance), 
                UnityEngine.Random.Range(-distance, distance), UnityEngine.Random.Range(-distance, distance));

            bool foundRandomPosition = NavMesh.SamplePosition(startPosition + randomPosition, out NavMeshHit randomNavHit, distance, _areaMask);
            if(!foundRandomPosition)
            {
                mls.LogWarning("Random position is not on Navmesh, using starting position.");
                newPosition = startPosition;
                return true;
            }
            newPosition = randomNavHit.position;
            return true;
        }
        #endregion
    }
}
