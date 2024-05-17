using Mu3Library.Utility;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameNetworkManager : GenericSingleton<GameNetworkManager> {
    private FacepunchTransport transport;

    public Lobby? CurrentLobby { get; private set; } = null;



    private void Awake() {
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;

        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }

    private void OnDestroy() {
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated -= OnLobbyGameCreated;

        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;

        if(NetworkManager.Singleton == null) {
            return;
        }
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
    }

    private void OnApplicationQuit() {
        Disconnect();
    }

    #region Utility
    public void Init() {
        if(transport == null) {
            transport = FindObjectOfType<FacepunchTransport>();
            if(transport == null) {
                FacepunchTransport t = gameObject.AddComponent<FacepunchTransport>();

                transport = t;
            }
        }

        if(NetworkManager.Singleton == null) {
            NetworkManager networkManager = gameObject.AddComponent<NetworkManager>();

            NetworkConfig config = new NetworkConfig();
            config.NetworkTransport = transport;

            networkManager.NetworkConfig = config;
        }
    }

    public async void StartHost(int maxMembers, Action<bool> callback = null) {
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.StartHost();
        CurrentLobby = await SteamMatchmaking.CreateLobbyAsync(maxMembers);

        callback?.Invoke(CurrentLobby != null);
    }

    public void StartClient(SteamId steamId) {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
        transport.targetSteamId = steamId;
        if(NetworkManager.Singleton.StartClient()) {
            Debug.Log("Client has started.");
        }
    }

    public void Disconnect(Action callback = null) {
        CurrentLobby?.Leave();
        CurrentLobby = null;
        if(NetworkManager.Singleton == null) {
            return;
        }

        if(NetworkManager.Singleton.IsHost) {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        }
        else {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            //NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }
        NetworkManager.Singleton.Shutdown(true);
        Debug.Log("Disconnected.");

        callback?.Invoke();
    }
    #endregion

    #region Action
    private void OnClientDisconnectCallback(ulong clientId) {
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        if(clientId == 0) {
            Disconnect();
        }
    }

    private void OnClientConnectedCallback(ulong clientId) {
        NetworkTransmission.Instance.AddMeToDictionaryServerRPC(SteamClient.SteamId, SteamClient.Name, clientId);

        NetworkTransmission.Instance.IsTheClientReadyServerRPC(false, clientId);
        Debug.Log($"Client has connected. clientId: {clientId}");
    }

    private void OnServerStarted() {
        Debug.Log("Host Started.");
    }

    // When you accept the invite for join on a friend.
    private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId steamId) {
        RoomEnter joinedLobby = await lobby.Join();
        if(joinedLobby != RoomEnter.Success) {
            Debug.LogError("Failed to create lobby.");

            return;
        }

        CurrentLobby = lobby;
        Debug.Log("Joined lobby.");
    }

    private void OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId steamId) {
        Debug.Log("Lobby was created.");
        FindObjectOfType<ChatController>()?.SendChat("Lobby was created.", NetworkManager.Singleton.LocalClientId, true);
    }

    // Friend send you an steam invite
    private void OnLobbyInvite(Friend friendId, Lobby lobby) {
        Debug.Log($"Invite from [{friendId.Name}].");
    }

    private void OnLobbyMemberLeave(Lobby lobby, Friend friendId) {
        Debug.Log($"[{friendId.Name}] leaved.");
        FindObjectOfType<ChatController>()?.SendChat($"[{friendId.Name}] leaved.", friendId.Id, true);
        NetworkTransmission.Instance.RemoveMeFromDictionaryServerRPC(friendId.Id);
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend friendId) {
        Debug.Log($"[{friendId.Name}] joined.");
    }

    private void OnLobbyEntered(Lobby lobby) {
        if(NetworkManager.Singleton.IsHost) {
            return;
        }

        StartClient(CurrentLobby.Value.Owner.Id);
    }

    private void OnLobbyCreated(Result result, Lobby lobby) {
        if(result != Result.OK) {
            Debug.LogError("Lobby was not created.");

            return;
        }

        lobby.SetPublic();
        lobby.SetJoinable(true);
        lobby.SetGameServer(lobby.Owner.Id);
        Debug.Log($"Lobby created. owner: {lobby.Owner.Name}");
        NetworkTransmission.Instance.AddMeToDictionaryServerRPC(SteamClient.SteamId, SteamClient.Name, NetworkManager.Singleton.LocalClientId);
    }
    #endregion
}
