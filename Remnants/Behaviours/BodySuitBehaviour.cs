﻿using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace Remnants.Behaviours
{
    internal class BodySuitBehaviour : NetworkBehaviour
    {
        #region Variables
        private int _saveSuitIndex = 0;
        public int saveSuitIndex
        { get { return _saveSuitIndex; } }
        #endregion

        #region Initialize 
        #endregion

        #region Methods
        public void UpdateSuit(int suitIndex)
        {
            _saveSuitIndex = suitIndex;
            if (!gameObject.name.Contains(Remnants.Instance.LoadBodyAssets.BannedPrefabTextureChange) && Remnants.Instance.RegisterBodySuits.SuitsIndexList.Count != 0)
            {
                SkinnedMeshRenderer skinnedMeshedRenderer = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
                Material suitMaterial = StartOfRound.Instance.unlockablesList.unlockables[_saveSuitIndex].suitMaterial;
                if (suitMaterial == null)
                    return;
                skinnedMeshedRenderer.material = suitMaterial;
                for (int i = 0; i < skinnedMeshedRenderer.materials.Length; i++)
                {
                    skinnedMeshedRenderer.materials[i] = suitMaterial;
                }
            }
        }
        #endregion

        #region networkMethods
        [HarmonyPrepare]
        [RuntimeInitializeOnLoadMethod]
        internal static void InitializeRPCS_GrabbableObject()
        {
            NetworkManager.__rpc_func_table.Add(3184508696u, __rpc_handler_3184508696);
            NetworkManager.__rpc_func_table.Add(2170264864u, __rpc_handler_2170264864);
        }



        [ServerRpc]
        public void SyncIndexSuitServerRpc(int indexSuit)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager == null || !networkManager.IsListening)
            {
                return;
            }

            if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
            {
                if (base.OwnerClientId != networkManager.LocalClientId)
                {
                    if (networkManager.LogLevel <= LogLevel.Normal)
                    {
                        Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
                    }

                    return;
                }

                ServerRpcParams serverRpcParams = default(ServerRpcParams);
                FastBufferWriter bufferWriter = __beginSendServerRpc(3184508696u, serverRpcParams, RpcDelivery.Reliable);
                BytePacker.WriteValueBitPacked(bufferWriter, indexSuit);
                __endSendServerRpc(ref bufferWriter, 3184508696u, serverRpcParams, RpcDelivery.Reliable);
            }

            if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsServer || networkManager.IsHost))
            {
                SyncIndexSuitClientRpc(indexSuit);
            }
        }

        [ClientRpc]
        public void SyncIndexSuitClientRpc(int indexSuit)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
                {
                    ClientRpcParams clientRpcParams = default(ClientRpcParams);
                    FastBufferWriter bufferWriter = __beginSendClientRpc(2170264864u, clientRpcParams, RpcDelivery.Reliable);
                    BytePacker.WriteValueBitPacked(bufferWriter, indexSuit);
                    __endSendClientRpc(ref bufferWriter, 2170264864u, clientRpcParams, RpcDelivery.Reliable);
                }

                if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
                {
                    UpdateSuit(indexSuit);
                }
            }
        }

        private static void __rpc_handler_3184508696(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
        {
            NetworkManager networkManager = target.NetworkManager;
            if ((object)networkManager == null || !networkManager.IsListening)
            {
                return;
            }

            if (rpcParams.Server.Receive.SenderClientId != target.OwnerClientId)
            {
                if (networkManager.LogLevel <= LogLevel.Normal)
                {
                    Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
                }
            }
            else
            {
                var rpcExecStage = Traverse.Create(target).Field("__rpc_exec_stage");
                ByteUnpacker.ReadValueBitPacked(reader, out int value);
                rpcExecStage.SetValue(__RpcExecStage.Server);
                ((BodySuitBehaviour)target).SyncIndexSuitServerRpc(value);
                rpcExecStage.SetValue(__RpcExecStage.None);
            }
        }


        private static void __rpc_handler_2170264864(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
        {
            NetworkManager networkManager = target.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                ByteUnpacker.ReadValueBitPacked(reader, out int value);
                var rpcExecStage = Traverse.Create(target).Field("__rpc_exec_stage");
                rpcExecStage.SetValue(__RpcExecStage.Client);
                ((BodySuitBehaviour)target).SyncIndexSuitClientRpc(value);
                rpcExecStage.SetValue(__RpcExecStage.None);
            }
        }
        #endregion
    }
}
