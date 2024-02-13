using BepInEx;
using BepInEx.Logging;
using Unity.Netcode;
using Unity.Netcode.Components;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine;
using LethalLib.Modules;
using System.Media;
using System.Xml.Linq;

namespace Remnants
{
    [BepInDependency("evaisa.lethallib", "0.14.2")]
    [BepInPlugin(modGUID, modName, modVersion)]

    public class Remnants : BaseUnityPlugin
    {
        #region Variables
        private const string modGUID = "KawaiiBone.Remnants";
        private const string modName = "Remnants";
        private const string modVersion = "1.0.0";

        public static Remnants Instance;
        private readonly Harmony _harmony = new Harmony(modGUID);
        internal ManualLogSource _mls;

        private List<Item> _allItems;
        private bool _isAddingItems = false;
        private string[] _bannedItemsNames = new string[4] { "Clipboard", "StickyNote", "Binoculars", "MapDevice" };
        private const int _minStoreItemRarity = 1, _maxStoreItemRarity = 10;
        private const int _minSellValue = 1, _maxSellValue = 2;
        private const float _minCreditCost = 4f, _maxCreditCost = 300f;
        #endregion

        #region Initialize 
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            _mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            _mls.LogInfo("modGUID has started");
            _harmony.PatchAll(typeof(Remnants));
            SceneManager.sceneLoaded += StoreItemsRegisterAsScrap;
            _mls.LogInfo("modGUID has loaded");

        }
        #endregion

        #region Methods
        private void StoreItemsRegisterAsScrap(Scene scene, LoadSceneMode mode)
        {
            if (_isAddingItems)
            {
                _mls.LogInfo("Did not load items because items are already loading in.");
                return;
            }

            _allItems = Resources.FindObjectsOfTypeAll<Item>().Concat(UnityEngine.Object.FindObjectsByType<Item>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID)).ToList();
            if (_allItems.Count == 0)
            {
                _mls.LogInfo("Did not load items because there are no items to load.");
                return;
            }

            _mls.LogInfo("Loading in items.");
            _isAddingItems = true;
            AddStoreItemsToScrap();
            _mls.LogInfo("Items Are loaded in.");
            SceneManager.sceneLoaded -= StoreItemsRegisterAsScrap;
        }


        private void AddStoreItemsToScrap()
        {
            try
            {
                System.Random random = new System.Random();
                foreach (Item item in _allItems)
                {
                    if (item == null)
                        continue;

                    if (HasBannedName(item.name))
                        continue;

                    if (item.isScrap == false && item.creditsWorth > _minCreditCost)
                    {
                        int rarity = CalculateRarity(item.creditsWorth);
                        _mls.LogInfo(item.name + "rarity: " + rarity);
                       item.minValue = _minSellValue;
                        item.maxValue = _maxSellValue;       
                        Items.RegisterScrap(item, rarity, Levels.LevelTypes.All);
                        _mls.LogInfo("Added " + item.name + " as a scrap item.");
                    }
                }
            }
            catch (Exception e)
            {
                _mls.LogError(e.ToString());
            }
        }

        private bool HasBannedName(string name)
        {
            return Array.FindIndex(_bannedItemsNames, x => x == name) != -1;
        }

        private int CalculateRarity(int itemCreditWorth)
        {
            float creditCapped = Mathf.Clamp(itemCreditWorth, _minCreditCost, _maxCreditCost);
            float rarityPercentage = Mathf.Abs(((creditCapped / _maxCreditCost) * _maxStoreItemRarity) - _maxStoreItemRarity);
            return Mathf.Clamp((int)rarityPercentage, _minStoreItemRarity, _maxStoreItemRarity);
        }
        #endregion

        #region HarmonyMethods
        [HarmonyPatch(typeof(RoundManager), "waitForScrapToSpawnToSync")]
        [HarmonyPostfix]
        static void UpdateSpawnedScrapCharge()
        {
            var mls = Remnants.Instance._mls;
            System.Random random = new System.Random();
            var grabbableObjects = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<GrabbableObject>();
            foreach (GrabbableObject grObject in grabbableObjects)
            {
                if (grObject.insertedBattery != null && grObject.itemProperties.requiresBattery == true && grObject.isInFactory == true)
                {
                    grObject.insertedBattery.charge = (float)random.NextDouble();
                    mls.LogInfo("Has updated " + grObject.itemProperties.name + " charge.");
                }
            }
        }
        [HarmonyPatch(typeof(QuickMenuManager), "LeaveGameConfirm")]
        [HarmonyPostfix]
        //Whenever you landed on a moon (generated a world) and then you quit the game, you would get OccludeAudio errors of something being nullptr reference.
        //This has probably something to do with the GameNetworkManager and the store items being added to scrap.
        //So the fix this, i had to disable all the OccludeAudio when you leave the lobby/game session.
        //During playtesting, i have not noticed any sounds that are absent.
        static void SoundErrorPatch()
        {
            OccludeAudio[] OccludeAudiosArray = Resources.FindObjectsOfTypeAll<OccludeAudio>();
            foreach (var occludeAudio in OccludeAudiosArray)
            {
                occludeAudio.enabled = false;
            }
        }
        #endregion
    }
}
