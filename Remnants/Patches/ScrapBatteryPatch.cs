using HarmonyLib;
using Unity.Netcode;

namespace Remnants.Patches
{
    internal class ScrapBatteryPatch
    {
        #region Variables
        #endregion


        #region HarmonyMethods
        [HarmonyPatch(typeof(RoundManager), "waitForScrapToSpawnToSync")]
        [HarmonyPostfix]
        static void UpdateSpawnedScrapCharge(object[] __args)
        {
            var mls = Remnants.Instance.Mls;
            NetworkObjectReference[] spawnedScrap = (NetworkObjectReference[])__args[0];
            mls.LogInfo("Patching battery of remnant spawned items.");
            if (spawnedScrap == null)
            {
                mls.LogInfo("spawnedScrap IS NULL");
                return;
            }

            System.Random random = new System.Random();
            for (int i = 0; i < spawnedScrap.Length; i++)
            {
                if (!spawnedScrap[i].TryGet(out var networkObject))
                    continue;

                GrabbableObject grabbableObject = networkObject.GetComponent<GrabbableObject>();
                if (grabbableObject == null)
                    continue;

                if (!grabbableObject.itemProperties.requiresBattery)
                    continue;

                if (!(grabbableObject.insertedBattery != null &&
                    grabbableObject.isInFactory == true && !grabbableObject.isInShipRoom))
                    continue;
                 
                int minCharge = Remnants.Instance.RemnantsConfig.MinRemnantBatteryCharge.Value;
                int maxCharge = Remnants.Instance.RemnantsConfig.MaxRemnantBatteryCharge.Value;
                int randomCharge = random.Next(minCharge, maxCharge);
                grabbableObject.SyncBatteryServerRpc(randomCharge);
                mls.LogInfo("Has updated " + grabbableObject.itemProperties.name + " charge to " + grabbableObject.insertedBattery.charge);
            }
        }
        #endregion
    }
}
