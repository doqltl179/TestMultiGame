using Mu3Library;
using Mu3Library.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//-------------------------------------------------------------------------------
// ServerRpc => Client에서 호출하며 Server에서 실행.
// ClientRpc => Server에서 호출하며 Client에서 실행.
//-------------------------------------------------------------------------------
public class NetworkTransmission : NetworkBehaviour {
    [SerializeField] private NetworkObject characterObj;

    public Action<ulong, bool> OnClickReady;



    [ServerRpc(RequireOwnership = false)]
    public void RequestInstantiateCharacter_ServerRpc(ulong from, ulong clientId) {
        Debug.Log($"RequestInstantiate_ServerRpc. from: {from}");

        NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(characterObj, clientId, true, true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void Ready_ServerRpc(ulong from, bool value) {
        Debug.Log($"Ready_ServerRpc. from: {from}, value: {value}");

        Ready_ClientRpc(from, value);
    }

    [ClientRpc]
    public void Ready_ClientRpc(ulong from, bool value) {
        Debug.Log($"Ready_ClientRpc. from: {from}, value: {value}");

        OnClickReady?.Invoke(from, value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetReady_ServerRpc(ulong from, bool value) {
        Debug.Log($"ResetReady_ServerRpc. from: {from}, value: {value}");

        ResetReady_ClientRpc(from, value);
    }

    [ClientRpc]
    public void ResetReady_ClientRpc(ulong from, bool value) {
        Debug.Log($"ResetReady_ClientRpc. from: {from}, value: {value}");

        foreach(ulong id in GameNetworkManager.Instance.MemberIDs) {
            GameNetworkManager.Instance.SetReady(id, false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void MoveScene_ServerRpc(ulong from, SceneType scene) {
        Debug.Log($"MoveScene_ServerRpc. from: {from}, scene: {scene}");

        MoveScene_ClientRpc(from, scene);
    }

    [ClientRpc]
    public void MoveScene_ClientRpc(ulong from, SceneType scene) {
        Debug.Log($"MoveScene_ClientRpc. from: {from}, scene: {scene}");

        SceneLoader.Instance.LoadScene(
            scene,
            () => {
                LoadingPanel.Instance.SetActive(true, 0.5f);
                LoadingPanel.Instance.UpdateProgress();
            },
            () => {
                LoadingPanel.Instance.SetActive(false, 0.5f);
                LoadingPanel.Instance.StopProgressUpdate();
            });
    }
}
