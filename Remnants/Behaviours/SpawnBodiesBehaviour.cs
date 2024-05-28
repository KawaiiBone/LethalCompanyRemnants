using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Remnants.Behaviours
{
    public class SpawnBodiesBehaviour
    {
        #region Variables
        private bool _hasInitialized = false;
        private float _courotineDelayTAmount = 11.0f;//same length as in the game
        private string _propName = "Prop";
        private List<GameObject> _propBodyObjects = new List<GameObject>();
        private List<GameObject> _scrapBodyObjects = new List<GameObject>();
        System.Random _random = new System.Random();
        private PositionOnNavMeshBehaviour _positionOnNavMeshBehaviour = new PositionOnNavMeshBehaviour(6.0f, 0.125f, 5.0f, 1.0f, 1.0f);
        private int _maxPercentage = 100;

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
        public void SpawnBodiesOnItems(List<GameObject> itemsObjects, bool alwaysSpawn = false)
        {
            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Spawning bodies.");
            if (itemsObjects == null || itemsObjects.Count == 0)
            {
                mls.LogWarning("List to spawn bodies on, is null or is empty!");
                return;
            }
            List<Vector3> positionsList = itemsObjects.ConvertAll<Vector3>(gameObj => gameObj.transform.position);
            SpawnBodiesOnPositions(positionsList, alwaysSpawn);
        }
        
         
        public void SpawnBodiesOnPositions(List<Vector3> positionList, bool alwaysSpawn = false)
        {
            var mls = Remnants.Instance.Mls;
            var roundManager = RoundManager.Instance;
            if (roundManager == null)
            {
                mls.LogWarning("Roundmanager not found!");
                return;
            }

            if (!Remnants.Instance.LoadBodyAssets.HasLoadedAnyAssets || Remnants.Instance.RemnantsConfig.SpawnRarityOfBody.Value == 0 || positionList.Count == 0)
                return;

            List<KeyValuePair<GameObject, int>> bodiesRarityList = CreatePrefabAndRarityList();
            if (bodiesRarityList == null || bodiesRarityList.Count == 0)
            {
                mls.LogWarning("No indoor enemies found on this moon, skipping body spawning");
                return;
            }

            List<NetworkObjectReference> NetworkObjectReferenceList = new List<NetworkObjectReference>();
            List<int> scrapValueList = new List<int>();
            List<int> suitsIndexList = Remnants.Instance.RegisterBodySuits.SuitsIndexList;
            int totalRarityValue = CalculateTotalRarityValue(bodiesRarityList);
            float spawnChance = CalculateSpawnChance(StartOfRound.Instance.currentLevel.riskLevel);
            bool ShouldBodiesBeScrap = Remnants.Instance.RemnantsConfig.ShouldBodiesBeScrap.Value;
            bool willSpawnBody = false;
            foreach (var itemPosition in positionList)
            {
                if (!alwaysSpawn)
                {
                    if (!willSpawnBody)
                        willSpawnBody = _random.Next(_maxPercentage) <= spawnChance;

                    if (!willSpawnBody)
                        continue;
                }

                if (!_positionOnNavMeshBehaviour.SetPositionOnNavMesh(itemPosition, out Vector3 spawnPosition))
                {
                    mls.LogWarning("Did not found place to spawn body, skipping it.");
                    continue;
                }

                NetworkObject netBodyObject = null;
                int bodyIndex = GetRandomBodyIndex(bodiesRarityList, _random.Next(totalRarityValue));
                if (!ShouldBodiesBeScrap)
                {
                    netBodyObject = SpawnPropBody(bodiesRarityList[bodyIndex].Key, spawnPosition);
                }
                else
                {
                    netBodyObject = SpawnScrapBody(bodiesRarityList[bodyIndex].Key, spawnPosition, roundManager.spawnedScrapContainer);
                    NetworkObjectReferenceList.Add(netBodyObject);
                    scrapValueList.Add(Remnants.Instance.RemnantsConfig.BodyScrapValue.Value);
                }

                if (suitsIndexList.Count != 0)
                {
                    int suitIndex = suitsIndexList[_random.Next(suitsIndexList.Count)];
                    if (!ShouldBodiesBeScrap)
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
            float spawnChanceMod = Remnants.Instance.RemnantsConfig.BodySpawnModifierRiskLevel.Value;
            float spawnChance = Remnants.Instance.RemnantsConfig.SpawnRarityOfBody.Value;
            int riskLevel = Array.IndexOf(Remnants.Instance.RiskLevelArray, riskLevelName);
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
