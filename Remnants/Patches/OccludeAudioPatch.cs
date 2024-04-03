using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace Remnants.Patches
{
    internal class OccludeAudioPatch
    {
        #region HarmonyMethods
        [HarmonyPatch(typeof(NetworkManager), "ShutdownInternal")]
        [HarmonyPrefix]
        //Whenever you landed on a moon (generated a world) and then you quit the game, you would get OccludeAudio errors of something being nullptr reference.
        //This has probably something to do with the GameNetworkManager and the store items being added to scrap.
        //So the fix this, i had to disable all the OccludeAudio when you leave the lobby/game session.
        //During playtesting, i have not noticed any sounds that are absent.
        static void SoundErrorEndPatch()
        {

            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Patching SoundErrorEndPatch");
            OccludeAudio[] OccludeAudiosArray = Resources.FindObjectsOfTypeAll<OccludeAudio>();
            foreach (var occludeAudio in OccludeAudiosArray)
            {
                occludeAudio.enabled = false;
            }
        }

        [HarmonyPatch(typeof(NetworkManager), "Initialize")]
        [HarmonyPrefix]
        static void SoundErrorStartPatch()
        {
            var mls = Remnants.Instance.Mls;
            mls.LogInfo("Patching SoundErrorStartPatch");
            OccludeAudio[] OccludeAudiosArray = Resources.FindObjectsOfTypeAll<OccludeAudio>();
            foreach (var occludeAudio in OccludeAudiosArray)
            {
                occludeAudio.enabled = true;
            }
        }


        #endregion
    }
}
