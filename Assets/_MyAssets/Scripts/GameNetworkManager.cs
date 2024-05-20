using Mu3Library.Utility;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using Steamworks.ServerList;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

public class GameNetworkManager : GenericSingleton<GameNetworkManager> {
    private FacepunchTransport transport;

    public Lobby? CurrentLobby { get; private set; } = null;
    private Dictionary<ulong, MemberInfo> memberInfos = new Dictionary<ulong, MemberInfo>();

    private ChatController chatController;



    private void Awake() {
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;

        SteamMatchmaking.OnChatMessage += OnChatMessage;

        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }

    private void OnDestroy() {
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated -= OnLobbyGameCreated;

        SteamMatchmaking.OnChatMessage -= OnChatMessage;

        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;

        if(NetworkManager.Singleton == null) {
            return;
        }
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
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
        NetworkManager.Singleton.OnServerStopped += OnServerStopped;
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
            //NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
        }
        else {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            //NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }
        NetworkManager.Singleton.Shutdown(true);
        Debug.Log("Disconnected.");

        callback?.Invoke();
    }

    public void SendChat(string message) {
        CurrentLobby?.SendChatString(message);
    }

    public void Ready() {
        if(CurrentLobby == null) return;

        MemberInfo info = memberInfos.Where(t => t.Value.IsMe).FirstOrDefault().Value;
        if(info != null) {
            NetworkTransmission.Instance.Ready_ServerRpc(info.ID, !info.IsReady);
        }
        else {
            Debug.LogError("Not exist in members myself.");

            return;
        }
    }
    #endregion

    #region Action
    private void OnChatMessage(Lobby lobby, Friend friendId, string message) {
        if(lobby.Id == CurrentLobby?.Id) {
            if(chatController == null || chatController.IsDestroyed) {
                chatController = FindObjectOfType<ChatController>();
                if(chatController == null) {
                    Debug.Log("ChatBox not found.");

                    return;
                }
            }

            chatController.AddChat(friendId.Name, message);
        }
        else {
            Debug.LogError($"Looby id not equel. current: {CurrentLobby?.Id}, from: {lobby.Id}");
        }
    }

    private void OnClientDisconnectCallback(ulong clientId) {
        Debug.Log($"Client has disconnected. clientId: {clientId}");

        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;

        if(clientId == 0) {
            Disconnect();
        }
    }

    private void OnClientConnectedCallback(ulong clientId) {
        Debug.Log($"Client has connected. clientId: {clientId}");
    }

    private void OnServerStopped(bool obj) {
        Debug.Log($"Host Stoped. value: {obj}");

        NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
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
        Debug.Log($"Lobby was created. ip: {ip}, port: {port}, name: {steamId}");
    }

    // Friend send you an steam invite
    private void OnLobbyInvite(Friend friendId, Lobby lobby) {
        Debug.Log($"Invite from [{friendId.Name}].");
    }

    private void OnLobbyMemberLeave(Lobby lobby, Friend friendId) {
        Debug.Log($"[{friendId.Name}] leaved.");

        MemberInfo info = null;
        if(memberInfos.TryGetValue(friendId.Id.Value, out info)) {
            memberInfos.Remove(friendId.Id.Value);
        }
        else {
            Debug.LogWarning($"Friend not exist. name: {friendId.Name}");
        }
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend friendId) {
        Debug.Log($"[{friendId.Name}] joined.");

        MemberInfo info = null;
        if(memberInfos.TryGetValue(friendId.Id.Value, out info)) {
            Debug.LogWarning($"Friend already joined. name: {friendId.Name}");
        }
        else {
            memberInfos.Add(friendId.Id.Value, new MemberInfo() { 
                IsMe = friendId.IsMe, 
                ID = friendId.Id, 
                Name = friendId.Name, 
                IsReady = false 
            });
        }
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

        memberInfos.Clear();
        memberInfos.Add(lobby.Owner.Id.Value, new MemberInfo() { 
            IsMe = true, 
            ID = lobby.Owner.Id, 
            Name = lobby.Owner.Name,
            IsReady = false });

        Debug.Log($"Lobby created. owner: {lobby.Owner.Name}");
    }
    #endregion

    private class MemberInfo {
        public bool IsMe;
        public SteamId ID;
        public string Name;
        public bool IsReady;
    }
}
