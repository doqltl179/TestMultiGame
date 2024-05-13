using Mu3Library.Utility;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class HostMigrationManager : MonoBehaviour {
    public static HostMigrationManager Instance {
        get {
            if(instance == null) {
                instance = FindObjectOfType<HostMigrationManager>();
                if(instance == null) {
                    Debug.LogError($"`{nameof(HostMigrationManager)}` not found.");
                }
            }

            return instance;
        }
    }
    private static HostMigrationManager instance;

    [SerializeField] private UnityTransport transport;



    public void SetToHost() {
        //NetworkManager.Singleton.StartHost();

        SetupRelay(4);
    }

    public async void SetupRelay(int playerCount) {
        try {
            if(NetworkManager.Singleton.StartHost()) {

            }

            await UnityServices.InitializeAsync();

            var allocation = await Relay.Instance.CreateAllocationAsync(playerCount);
            var joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"Join Code: {joinCode}");

            SetRelayServerData(allocation);

            NetworkManager.Singleton.StartHost();

            Debug.Log($"IsHost: {NetworkManager.Singleton.IsServer}, IsClient: {NetworkManager.Singleton.IsClient}");
        }
        catch(System.Exception ex) {
            Debug.LogError(ex.ToString());
        }
    }

    private void SetRelayServerData(Allocation allocation) {
        transport.SetRelayServerData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);
    }
}
