using System;
using UnityEngine;
using BepInEx;
using LethalLib.Modules;
using Remnants.utilities;
using System.Collections.Generic;
using Unity.Netcode;
using System.Linq;

namespace Remnants.Behaviours
{
    public class SpawnRemnantItemsBehaviour
    {
        #region Variables
        private bool _hasInitialized = false;
        private string[] _riskLevelArray = null;
        System.Random _random = new System.Random();
        private float _spawnBalanceModifier = 0.55f;
        private float _courotineDelayTAmount = 11.0f;//same length as in the game
        private RandomizeBatteriesBehaviour _randomizeBatteriesBeh = null;
        private int _maxPercentage = 100;
        private PositionOnNavMeshBehaviour _positionOnNavMeshBeh = new PositionOnNavMeshBehaviour(0, 0.125f, 0, 1.3f, 0);
        private List<Vector3> _bodySpawnPositions = new List<Vector3>();
        public List<Vector3> BodySpawnPositions
        {
            get { return _bodySpawnPositions; }
        }

        #endregion

        #region Initialize 
        public void Initialize()
        {
            if (!_hasInitialized)
            {
                _hasInitialized = true;
                _riskLevelArray = Remnants.Instance.RiskLevelArray;
                _randomizeBatteriesBeh = Remnants.Instance.ItemsBatteriesBeh;
            }
        }
        #endregion

        #region PublicMethods
        public void SpawnRemnantItems(RoundManager roundManager)
        {
            var mls = Remnants.Instance.Mls;
            if (roundManager == null)
                return;
          
            List<Items.ScrapItem> networkRemnantItems = GetAvailableNetworkRemnantItems();
            int minRemnantItemsOnBody = Remnants.Instance.RemnantsConfig.MinItemsFoundOnBodies.Value;
            int maxRemnantItemsOnBody = Remnants.Instance.RemnantsConfig.MaxItemsFoundOnBodies.Value;
            int amountRemnantItemsToSpawn = CalculateAmountItemsToSpawn(roundManager);
            List<KeyValuePair<string, int>> remnantItemsBaseContainer = CreateRemnantItemsBaseContainer(roundManager, networkRemnantItems);
            List<KeyValuePair<string, int>> remnantItemsContainer = CreateRemnantItemsContainer(remnantItemsBaseContainer);
            int totalRarity = CreateTotalRarity(remnantItemsContainer);
            List<NetworkObjectReference> NetworkObjectReferenceList = new List<NetworkObjectReference>();
            List<int> scrapValueList = new List<int>();
            _bodySpawnPositions.Clear();
            float spawnChance = CalculateBodySpawnChance(StartOfRound.Instance.currentLevel.riskLevel);
            bool hasNotSpawnedItemOnABody = true;
            int currentAmountItemsSpawnOnBody = 0;  
            for (int spawnIndex = 0; spawnIndex < amountRemnantItemsToSpawn; spawnIndex++)
            {
                if (totalRarity <= 0 || remnantItemsContainer.Count == 0)
                    break;

                KeyValuePair<string, int> remnantItemToSpawnData = GetRandomSpawnData(remnantItemsContainer, totalRarity);
                if (remnantItemToSpawnData.Key.IsNullOrWhiteSpace())
                {
                    mls.LogError("Remnant item spawn data not found");
                    break;
                }

                totalRarity -= remnantItemToSpawnData.Value;
                remnantItemsContainer.Remove(remnantItemToSpawnData);
                var remnantItemToSpawn = networkRemnantItems.Find(networkRemnantItem => networkRemnantItem.item.itemName == remnantItemToSpawnData.Key);
                Vector3 spawnPosition = Vector3.zero;
                if(currentAmountItemsSpawnOnBody <= 0 || _bodySpawnPositions.Count == 0)
                {
                    int randomInsideAiNode = roundManager.AnomalyRandom.Next(0, roundManager.insideAINodes.Length);
                    Vector3 randomNavMeshPositionInBoxPredictable = roundManager.GetRandomNavMeshPositionInBoxPredictable(roundManager.insideAINodes[randomInsideAiNode].transform.position, 8f, roundManager.navHit, roundManager.AnomalyRandom, -1);
                    spawnPosition = randomNavMeshPositionInBoxPredictable;
                    hasNotSpawnedItemOnABody = true;          
                }
                else if(currentAmountItemsSpawnOnBody > 0 && _positionOnNavMeshBeh.SetRandomOffsetOnNavmesh(_bodySpawnPositions.Last(), out spawnPosition))
                {
                    currentAmountItemsSpawnOnBody--;
                    hasNotSpawnedItemOnABody = false;
                }
                else
                {
                    mls.LogWarning("Something went wrong with spawning remnant items spawning positions.");
                    continue;
                }

                if(hasNotSpawnedItemOnABody && _random.Next(_maxPercentage) <= spawnChance)
                {
                    currentAmountItemsSpawnOnBody = _random.Next(minRemnantItemsOnBody, maxRemnantItemsOnBody) - 1;
                    _bodySpawnPositions.Add(spawnPosition);
                }

                GameObject gameObj = UnityEngine.Object.Instantiate(remnantItemToSpawn.item.spawnPrefab, spawnPosition, Quaternion.identity, roundManager.spawnedScrapContainer);
                var networkObj = gameObj.GetComponent<NetworkObject>();
                networkObj.Spawn();
                int rarity = (int)((float)_random.Next(remnantItemToSpawn.item.minValue, remnantItemToSpawn.item.maxValue) * roundManager.scrapAmountMultiplier);
                _randomizeBatteriesBeh.RandomizeItemBattery(gameObj);
                NetworkObjectReferenceList.Add(networkObj);
                scrapValueList.Add(rarity);
                Remnants.Instance.RemnantItemsBeh.AddFoundRemnantItemObject(gameObj);
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
        private List<Items.ScrapItem> GetAvailableNetworkRemnantItems()
        {
            List<Items.ScrapItem> networkRemnantItems = Remnants.Instance.RemnantItemsBeh.NetworkRemnantItems;
            List<RemnantData> remnantItemDataList = Remnants.Instance.RemnantsConfig.GetRemnantItemList(false);
            networkRemnantItems.RemoveAll(spawnableItem => remnantItemDataList.FindIndex(itemData => itemData.RarityInfo == 0 &&
           (itemData.RemnantItemName == spawnableItem.origItem.itemName || itemData.RemnantItemName == spawnableItem.item.itemName)) != -1);
            return networkRemnantItems;
        }

        private int CalculateAmountItemsToSpawn(RoundManager roundManager)
        {
            int minRemnantItemsSpawn = Remnants.Instance.RemnantsConfig.MinRemnantItemsSpawning.Value;
            int maxRemnantItemsSpawn = Remnants.Instance.RemnantsConfig.MaxRemnantItemsSpawning.Value;
            float spawnRemnantItemsModifier = Remnants.Instance.RemnantsConfig.RemnantItemsSpawningModifier.Value;
            int riskLevel = 0;
            riskLevel = Mathf.Clamp(Array.IndexOf(_riskLevelArray, roundManager.currentLevel.riskLevel), 0, _riskLevelArray.Length);
            float remnantItemsAmountModifier = roundManager.scrapAmountMultiplier;
            remnantItemsAmountModifier = remnantItemsAmountModifier * ((riskLevel * _spawnBalanceModifier) * spawnRemnantItemsModifier);
            return (int)((float)_random.Next(minRemnantItemsSpawn, maxRemnantItemsSpawn) * remnantItemsAmountModifier);
        }

        private List<KeyValuePair<string, int>> CreateRemnantItemsBaseContainer(RoundManager roundManager, List<Items.ScrapItem> networkRemnantItems)
        {
            var mls = Remnants.Instance.Mls;
            bool useSpecificLevelRarities = Remnants.Instance.RemnantsConfig.UseSpecificLevelRarities.Value;       
            List<KeyValuePair<string, int>> remnantItemsBaseContainer = new List<KeyValuePair<string, int>>();
            if (useSpecificLevelRarities)
            {
                foreach (var remnantNetworkItem in networkRemnantItems)
                {
                    if (remnantNetworkItem.customLevelRarities.ContainsKey(roundManager.currentLevel.PlanetName))
                    {
                        remnantItemsBaseContainer.Add(new KeyValuePair<string, int>(remnantNetworkItem.item.itemName,
                            remnantNetworkItem.customLevelRarities[roundManager.currentLevel.PlanetName]));
                    }
                    else if (remnantNetworkItem.customLevelRarities.ContainsKey(roundManager.currentLevel.name))
                    {
                        remnantItemsBaseContainer.Add(new KeyValuePair<string, int>(remnantNetworkItem.item.itemName,
                            remnantNetworkItem.customLevelRarities[roundManager.currentLevel.name]));
                    }
                    else if (Enum.TryParse(roundManager.currentLevel.name, out Levels.LevelTypes level))
                    {
                        remnantItemsBaseContainer.Add(new KeyValuePair<string, int>(remnantNetworkItem.item.itemName,
                            remnantNetworkItem.levelRarities[level]));
                    }
                }
            }
            else
            {
                foreach (var remnantNetworkItem in networkRemnantItems)
                {
                    remnantItemsBaseContainer.Add(new KeyValuePair<string, int>(remnantNetworkItem.item.itemName,
                        remnantNetworkItem.levelRarities[Levels.LevelTypes.All]));
                }
            }
            return remnantItemsBaseContainer;
        }

        private List<KeyValuePair<string, int>> CreateRemnantItemsContainer(List<KeyValuePair<string, int>> remnantItemsBaseContainer)
        {
            int maxRemnantItemDuplicates = Remnants.Instance.RemnantsConfig.MaxDuplicatesRemnantItems.Value;
            List<KeyValuePair<string, int>> remnantItemsContainer = new List<KeyValuePair<string, int>>();
            for (int i = 0; i < maxRemnantItemDuplicates; i++)
            {
                remnantItemsContainer.AddRange(remnantItemsBaseContainer);
            }
            return remnantItemsContainer;
        }

        private int CreateTotalRarity(List<KeyValuePair<string, int>>  remnantItemsContainer)
        {
            int totalRarity = 0;
            foreach (var remnantItemData in remnantItemsContainer)
            {
                totalRarity += remnantItemData.Value;
            }
            return totalRarity;
        }

        private KeyValuePair<string, int> GetRandomSpawnData(List<KeyValuePair<string, int>> remnantItemsContainer, int totalRarity)
        {
            //via seed randomly pick an item from the container
            int randomRarity = _random.Next(0, totalRarity);
            int currentTotalRarity = 0;
            KeyValuePair<string, int> remnantItemToSpawnData = new KeyValuePair<string, int>("", 0);
            foreach (var remnantItemData in remnantItemsContainer)
            {
                if (currentTotalRarity <= randomRarity)
                {
                    remnantItemToSpawnData = remnantItemData;
                    return remnantItemToSpawnData;
                }
                currentTotalRarity += remnantItemData.Value;
            }
            return remnantItemToSpawnData;
        }
        private float CalculateBodySpawnChance(string riskLevelName)
        {
            float spawnChanceMod = Remnants.Instance.RemnantsConfig.BodySpawnModifierRiskLevel.Value;
            float spawnChance = Remnants.Instance.RemnantsConfig.SpawnRarityOfBody.Value;
            int riskLevel = Array.IndexOf(Remnants.Instance.RiskLevelArray, riskLevelName);
            if (!Mathf.Approximately(spawnChanceMod, 0.0f) && riskLevel != -1)
                spawnChance *= (riskLevel * spawnChanceMod);

            return spawnChance;
        }
        #endregion
    }
}
