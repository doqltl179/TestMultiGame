using Mu3Library.Utility;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameController : MonoBehaviour {
    [SerializeField] private NetworkObject networkTransmissionObj;
    private NetworkTransmission networkTransmission;

    [SerializeField] private NetworkObject charactorObj;
    private CharacterController player;



    private void OnDestroy() {
        GameNetworkManager.Instance.OnReadyAll -= OnReadyAll;
    }

    private IEnumerator Start() {
        CameraManager.Instance.InitCamera();

        while(SceneLoader.Instance.IsLoading) {
            yield return null;
        }

        if(networkTransmission == null) {
            if(NetworkManager.Singleton.IsHost) {
                NetworkObject go = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(networkTransmissionObj);
                NetworkTransmission transmission = go.GetComponent<NetworkTransmission>();

                networkTransmission = transmission;

                GameNetworkManager.Instance.OnReadyAll += OnReadyAll;
            }
            else {
                WaitForSeconds wait = new WaitForSeconds(0.5f);
                const int maxCount = 20;
                int count = 0;
                while(count < maxCount) {
                    networkTransmission = FindObjectOfType<NetworkTransmission>();
                    if(networkTransmission != null) break;

                    count++;

                    yield return wait;
                }

                if(networkTransmission == null) {
                    Debug.LogWarning($"[{nameof(NetworkTransmission)}] not found.");

                    //Disconnect();

                    yield break;
                }
            }
        }

        if(player == null) {
            NetworkObject go = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(charactorObj, NetworkManager.Singleton.LocalClientId, true, true);
            CharacterController charactor = go.GetComponent<CharacterController>();

            player = charactor;
        }

        Ready();

        Debug.Log($"IsHost: {NetworkManager.Singleton.IsHost}, IsServer: {NetworkManager.Singleton.IsServer}, IsClient: {NetworkManager.Singleton.IsClient}");
    }

    private void Ready() {
        Friend? local = GameNetworkManager.Instance.LocalID;
        if(local != null) {
            bool ready = !GameNetworkManager.Instance.GetReady(local.Value.Id);
            networkTransmission.Ready_ServerRpc(local.Value.Id, ready);

            GameNetworkManager.Instance.SetReady(local.Value.Id, ready);
        }
    }

    #region Action
    private void OnReadyAll(bool value) {
        Debug.Log("All players loaded [Game] scene.");
    }
    #endregion
}
