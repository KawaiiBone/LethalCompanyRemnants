using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions.Must;
using UnityEngine.SceneManagement;

namespace Remnants.Behaviours
{
    public class SpawnBodiesBehaviour
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
        private float _courotineDelayTAmount = 11.0f;//same length as in the game
        private string _propName = "Prop";
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

        #region PublicMethods
        public void SpawnBodiesOnItems(List<GameObject> itemsObjects)
        {
            var mls = Remnants.Instance.Mls;
            var roundManager = RoundManager.Instance;
            mls.LogInfo("Spawning bodies on items.");
            if (itemsObjects == null || itemsObjects.Count == 0)
            {
                mls.LogWarning("List to spawn bodies on null or is empty!");
                return;
            }     

            if(roundManager == null)
            {
                mls.LogWarning("Roundmanager not found!");
                return;
            }

            if (!Remnants.Instance.LoadBodyAssets.HasLoadedAnyAssets || Remnants.Instance.RemnantsConfig.SpawnRarityOfBody.Value == 0)
                return;

            List<Vector3> spawnPositions = new List<Vector3>();
            spawnPositions = itemsObjects.ConvertAll<Vector3>(gameObj => gameObj.transform.position);
            List<int> indexList = Remnants.Instance.RegisterBodySuits.SuitsIndexList;
            List<NetworkObjectReference> NetworkObjectReferenceList = new List<NetworkObjectReference>();
            List<int> scrapValueList = new List<int>();
            var prefabAndRarityList = CreatePrefabAndRarityList();
            int totalRarityValue = CalculateTotalRarityValue(prefabAndRarityList);
            float spawnChance = CalculateSpawnChance(StartOfRound.Instance.currentLevel.riskLevel);
            System.Random random = new System.Random();
            int maxPercentage = 101;
            bool willSpawnBody = false;
            foreach (var itemPosition in spawnPositions)
            {
                if (!willSpawnBody)
                    willSpawnBody = random.Next(maxPercentage) <= spawnChance;

                if (!willSpawnBody)
                    continue;

                if (!CalculatePositionOnNavMesh(itemPosition, out Vector3 spawnPosition))
                {
                    mls.LogWarning("Did not found place to spawn body, skipping it.");
                    continue;
                }

                NetworkObject netBodyObject = null;
                int bodyIndex = GetRandomBodyIndex(prefabAndRarityList, random.Next(totalRarityValue));
                if (Remnants.Instance.RemnantsConfig.ShouldBodiesBeScrap.Value == false)
                {
                    netBodyObject = SpawnBody(prefabAndRarityList[bodyIndex].Key, spawnPosition);
                }
                else
                {
                    netBodyObject = SpawnScrapBody(prefabAndRarityList[bodyIndex].Key, spawnPosition, roundManager.spawnedScrapContainer);
                    NetworkObjectReferenceList.Add(netBodyObject);
                    scrapValueList.Add(Remnants.Instance.RemnantsConfig.BodyScrapValue.Value);
                }

                if (indexList.Count != 0)
                {
                    int suitIndex = indexList[random.Next(indexList.Count)];
                    netBodyObject.GetComponent<BodyGrabbableObject>().SyncIndexSuitServerRpc(suitIndex);
                }
                willSpawnBody = false;
            }

            if (NetworkObjectReferenceList.Count == 0)
                return;

            //Here do courotine for sync scrap      
            var couroutine = utilities.CoroutineHelper.Instance;
            if (couroutine == null)
                couroutine = new GameObject().AddComponent<utilities.CoroutineHelper>();

            couroutine.ExecuteAfterDelay(() =>
            {
                roundManager.SyncScrapValuesClientRpc(NetworkObjectReferenceList.ToArray(), scrapValueList.ToArray());
            }
            , _courotineDelayTAmount);
        }
        #endregion

        #region PrivateMethods

        private NetworkObject SpawnBody(GameObject prefab, Vector3 spawnPosition)
        {
            GameObject defaultBody = UnityEngine.Object.Instantiate(prefab, spawnPosition, UnityEngine.Random.rotation, RoundManager.Instance.mapPropsContainer.transform);
            NetworkObject netObject = defaultBody.GetComponent<NetworkObject>();
            netObject.Spawn(true);
            return netObject;
        }

        private NetworkObject SpawnScrapBody(GameObject prefab, Vector3 spawnPosition, Transform parent)
        {
            GameObject defaultBody = UnityEngine.Object.Instantiate(prefab, spawnPosition, UnityEngine.Random.rotation, parent);
            NetworkObject netObject = defaultBody.GetComponent<NetworkObject>();
            netObject.Spawn();
            return netObject;
        }

        private List<KeyValuePair<GameObject, int>> CreatePrefabAndRarityList()
        {
            RegisterBodiesSpawnBehaviour registerBodiesSpawn = Remnants.Instance.RegisterBodiesSpawn;
            SelectableLevel currentLevel = StartOfRound.Instance.currentLevel;
            string planetName = currentLevel.PlanetName;
            if (registerBodiesSpawn.HasIllegalCharacters(currentLevel.PlanetName))
                planetName = registerBodiesSpawn.PlanetsBodiesRarities.First().Key;

            if (!registerBodiesSpawn.PlanetsBodiesRarities.ContainsKey(planetName))
                registerBodiesSpawn.RegisterBodiesToNewMoon(currentLevel);

            Dictionary<string, int> bodiesArray = registerBodiesSpawn.PlanetsBodiesRarities[planetName];
            IReadOnlyList<NetworkPrefab> prefabs = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs;

            List<NetworkPrefab> bodyPrefabs = null;
            if (Remnants.Instance.RemnantsConfig.ShouldBodiesBeScrap.Value == false)
            {
                bodyPrefabs = prefabs.Where(netObj => bodiesArray.ToList().FindIndex(name => (name.Key + _propName) == netObj.Prefab.name) != -1).ToList();
                return bodyPrefabs.ConvertAll(netPrefab =>
                new KeyValuePair<GameObject, int>(
                netPrefab.Prefab,
                bodiesArray[netPrefab.Prefab.name.Substring(0, netPrefab.Prefab.name.Length - _propName.Length)]
                ));
            }
            else
            {
                bodyPrefabs = prefabs.Where(netObj => bodiesArray.ContainsKey(netObj.Prefab.name)).ToList();
                return bodyPrefabs.ConvertAll(netPrefab =>
                new KeyValuePair<GameObject, int>(
                netPrefab.Prefab,
                bodiesArray[netPrefab.Prefab.name]
                ));
            }
        }

        private bool CalculatePositionOnNavMesh(Vector3 startPosition, out Vector3 newPosition)
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

        private int CalculateTotalRarityValue(List<KeyValuePair<GameObject, int>> prefabAndRarityList)
        {
            int totalRarityValue = 0;
            foreach (var prefabRarity in prefabAndRarityList)
            {
                totalRarityValue += prefabRarity.Value;
            }
            return totalRarityValue;
        }

        private int GetRandomBodyIndex(List<KeyValuePair<GameObject, int>> prefabAndRarityList, int randomNumber)
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

        private float CalculateSpawnChance(string riskLevelName)
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
