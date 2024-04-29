using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Remnants.Behaviours
{
    public class SpawnBodiesBehaviour
    {
        #region Variables
        private bool _hasInitialized = false;
        private float _maxReachDistance = 6.0f;
        private float _minReachDistance = 0.125f;//limit is 0.165
        private float _movedReachDistance = 5.0f;
        private float _moveDistance = 1.0f;
        private int _areaMask = -1;
        private float _yOffset = 1.0f;
        private string[] _riskLevelArray = { "Safe", "D", "C", "B", "A", "S", "S+" };
        private float _courotineDelayTAmount = 11.0f;//same length as in the game
        private string _propName = "Prop";
        private List<GameObject> _propBodyObjects = new List<GameObject>();
        private List<GameObject> _scrapBodyObjects = new List<GameObject>();
        #endregion

        #region Initialize 
        public void Initialize()
        {
            if (!_hasInitialized)
            {
                _hasInitialized = true;
                SceneManager.sceneLoaded += CollectBodiesFromNetworkObjects;
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
                mls.LogWarning("List to spawn bodies on, is null or is empty!");
                return;
            }

            if (roundManager == null)
            {
                mls.LogWarning("Roundmanager not found!");
                return;
            }

            if (!Remnants.Instance.LoadBodyAssets.HasLoadedAnyAssets || Remnants.Instance.RemnantsConfig.SpawnRarityOfBody.Value == 0)
                return;

            var prefabAndRarityList = CreatePrefabAndRarityList();
            if (prefabAndRarityList == null || prefabAndRarityList.Count == 0)
            {
                mls.LogWarning("No indoor enemies found on this moon, skipping body spawning");
                return;
            }

            List<NetworkObjectReference> NetworkObjectReferenceList = new List<NetworkObjectReference>();
            List<int> scrapValueList = new List<int>();
            List<Vector3> spawnPositions = itemsObjects.ConvertAll<Vector3>(gameObj => gameObj.transform.position);
            List<int> suitsIndexList = Remnants.Instance.RegisterBodySuits.SuitsIndexList;
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
                    netBodyObject = SpawnPropBody(prefabAndRarityList[bodyIndex].Key, spawnPosition);
                }
                else
                {
                    netBodyObject = SpawnScrapBody(prefabAndRarityList[bodyIndex].Key, spawnPosition, roundManager.spawnedScrapContainer);
                    NetworkObjectReferenceList.Add(netBodyObject);
                    scrapValueList.Add(Remnants.Instance.RemnantsConfig.BodyScrapValue.Value);
                }

                if (suitsIndexList.Count != 0)
                {
                    int suitIndex = suitsIndexList[random.Next(suitsIndexList.Count)];
                    if (Remnants.Instance.RemnantsConfig.ShouldBodiesBeScrap.Value == false)
                    {
                        BodySuitBehaviour bodySuit = netBodyObject.GetComponent<BodySuitBehaviour>();
                        bodySuit.SyncIndexSuitClientRpc(suitIndex);
                    }
                    else
                    {
                        BodyGrabbableObject bodyGrabbableObject = netBodyObject.GetComponent<BodyGrabbableObject>();
                        bodyGrabbableObject.SyncIndexSuit(suitIndex);
                    }
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

        private NetworkObject SpawnPropBody(GameObject prefab, Vector3 spawnPosition)
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
            if (Remnants.Instance.RemnantsConfig.ShouldBodiesBeScrap.Value == false)
            {
                var selectedBodies = _propBodyObjects.Where(prefab => bodiesArray.ContainsKey(prefab.name.Substring(0, prefab.name.Length - _propName.Length))).ToList();
                return selectedBodies.ConvertAll(prefab => 
                new KeyValuePair<GameObject, int>(
                prefab,
                bodiesArray[prefab.name.Substring(0, prefab.name.Length - _propName.Length)]
                ));
            }
            else
            {
                var selectedBodies = _scrapBodyObjects.Where(prefab => bodiesArray.ContainsKey(prefab.name)).ToList();
                return selectedBodies.ConvertAll(prefab =>
                new KeyValuePair<GameObject, int>(
                prefab,
                bodiesArray[prefab.name]
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
            if (isWithinMovedRange)
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
            float spawnChanceMod = Remnants.Instance.RemnantsConfig.SpawnModifierRiskLevel.Value;
            float spawnChance = Remnants.Instance.RemnantsConfig.SpawnRarityOfBody.Value;
            int riskLevel = Array.IndexOf(_riskLevelArray, riskLevelName);
            if (!Mathf.Approximately(spawnChanceMod, 0.0f) && riskLevel != -1)
                spawnChance *= (riskLevel * spawnChanceMod);

            return spawnChance;
        }

        private void CollectBodiesFromNetworkObjects(Scene scene, LoadSceneMode mode)
        {
            var mls = Remnants.Instance.Mls;
            var gameNetworkManager = GameNetworkManager.Instance;
            if (gameNetworkManager == null || gameNetworkManager.isDisconnecting)
            {
                _propBodyObjects.Clear();
                _scrapBodyObjects.Clear();
                return;
            }

            if (!gameNetworkManager.isHostingGame)
                return;

            if (_propBodyObjects.Count != 0 && _scrapBodyObjects.Count != 0)
                return;

            List<string> bodyObjNames = Remnants.Instance.LoadBodyAssets.EnemiesAndBodiesNames.Select(EnemyAndBodyName => EnemyAndBodyName.Value).ToList();
            IReadOnlyList<NetworkPrefab> netPrefabs = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs;

            var propBodyPrefabs = netPrefabs.Where(netObj => bodyObjNames.FindIndex(name => (name + _propName) == netObj.Prefab.name) != -1).ToList();
            _propBodyObjects = propBodyPrefabs.ConvertAll(netObj => netObj.Prefab);

            var scrapBodyPrefabs = netPrefabs.Where(netObj => bodyObjNames.Contains(netObj.Prefab.name)).ToList();
            _scrapBodyObjects = scrapBodyPrefabs.ConvertAll(netObj => netObj.Prefab);
        }
        #endregion
    }
}
