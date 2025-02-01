using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Remnants.Patches;
using Remnants.Behaviours;
using Remnants.Data;

namespace Remnants
{
    [BepInDependency("evaisa.lethallib", "0.16.1")]
    [BepInDependency("ainavt.lc.LethalConfig", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(modGUID, modName, modVersion)]

    public class Remnants : BaseUnityPlugin
    {
        #region Variables
        private const string modGUID = "KawaiiBone.Remnants";
        private const string modName = "Remnants";
        private const string modVersion = "1.4.0";

        public static Remnants Instance;
        private readonly Harmony _harmony = new Harmony(modGUID);
        internal ManualLogSource Mls;

        public RegisterBodiesSpawnBehaviour RegisterBodiesSpawn = new RegisterBodiesSpawnBehaviour();
        public Data.Config RemnantsConfig = new Data.Config();
        public RegisterBodySuitsBehaviour RegisterBodySuits = new RegisterBodySuitsBehaviour();
        public LoadAssetsBodies LoadBodyAssets = new LoadAssetsBodies();
        public SpawnBodiesBehaviour SpawningBodyBeh = new SpawnBodiesBehaviour();
        public RemnantItemsBehaviour RemnantItemsBeh = new RemnantItemsBehaviour();
        public RandomizeBatteriesBehaviour ItemsBatteriesBeh = new RandomizeBatteriesBehaviour();
        public SpawnRemnantItemsBehaviour SpawnRemnantItemsBeh = new SpawnRemnantItemsBehaviour();

        private RegisterItemsBehaviour _registerItemsBehaviour = new RegisterItemsBehaviour();
        private string[] _riskLevelArray = { "Safe", "D", "C", "B", "A", "S", "S+", "S++", "S+++" };
        public string[] RiskLevelArray
        {
            get { return _riskLevelArray; }
        }
        #endregion

        #region Initialize 
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            Mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            Mls.LogInfo("modGUID has started");
            RemnantsConfig.Initialize();
            _harmony.PatchAll(typeof(RemnantItemsPatch));
            _harmony.PatchAll(typeof(SpawnRemnantItemsPatch));
            _harmony.PatchAll(typeof(SpawnableScrapPatch));
            _harmony.PatchAll(typeof(BodySuitBehaviour));
            _harmony.PatchAll(typeof(RegisterSuitsPatch));
            _harmony.PatchAll(typeof(AddRemnantItemsToItemList));
            _harmony.PatchAll(typeof(EndRoundStatsPatch));
            _harmony.PatchAll(typeof(BeltBagTranspiler));
            _harmony.PatchAll(typeof(PartyWipeRemnantItemsTranspiler));
            _harmony.PatchAll(typeof(Remnants));
            _registerItemsBehaviour.Initialize();
            RegisterBodiesSpawn.Initialize();
            LoadBodyAssets.Initialize();
            RegisterBodySuits.Initialize();
            SpawningBodyBeh.Initialize();
            ItemsBatteriesBeh.Initialize();
            SpawnRemnantItemsBeh.Initialize();
            Mls.LogInfo("modGUID has loaded");
        }
        #endregion
    }
}
