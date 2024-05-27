using Mu3Library;
using Mu3Library.Utility;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;

public class GameNetworkManager : MonoBehaviour {
    public static GameNetworkManager Instance {
        get {
            if(instance == null) {
                GameNetworkManager pm = FindObjectOfType<GameNetworkManager>();
                if(pm == null) {
                    GameObject resource = ResourceLoader.GetResource<GameObject>($"Network/{nameof(GameNetworkManager)}");
                    if(resource != null) {
                        GameObject go = Instantiate(resource);
                        DontDestroyOnLoad(go);

                        pm = go.GetComponent<GameNetworkManager>();
                    }
                }

                instance = pm;
            }

            return instance;
        }
    }
    private static GameNetworkManager instance;

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

        NetworkTransmission.Instance.OnClickReady += OnClickReady;
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

        NetworkTransmission.Instance.OnClickReady -= OnClickReady;

        if(NetworkManager.Singleton == null) {
            return;
        }
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;

        NetworkManager.Singleton.ConnectionApprovalCallback -= ConnectionApprovalCallback;
    }

    private void OnApplicationQuit() {
        Disconnect();
    }

    private void Start() {
        
    }

    #region Utility
    public void Init() {
        if(transport == null) {
            transport = GetComponent<FacepunchTransport>();
        }
    }

    public async void StartHost(int maxMembers, Action<bool> callback = null) {
        NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApprovalCallback;

        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnServerStopped += OnServerStopped;
        NetworkManager.Singleton.StartHost();
        CurrentLobby = await SteamMatchmaking.CreateLobbyAsync(maxMembers);
        CurrentLobby?.SetPublic();
        CurrentLobby?.SetJoinable(true);

        UpdateLobbyData("Scene", SceneLoader.Instance.CurrentLoadedScene.ToString());

        callback?.Invoke(CurrentLobby != null);
    }

    private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
        Debug.Log($"ClientNetworkId: {request.ClientNetworkId}, Approved: {response.Approved}, Reason: {response.Reason}");
    }

    public async void StartServer(int maxMembers, Action<bool> callback = null) {
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnServerStopped += OnServerStopped;
        NetworkManager.Singleton.StartServer();
        CurrentLobby = await SteamMatchmaking.CreateLobbyAsync(maxMembers);

        UpdateLobbyData("Scene", SceneLoader.Instance.CurrentLoadedScene.ToString());

        callback?.Invoke(CurrentLobby != null);
    }

    public void StartClient(SteamId steamId, Action<bool> callback = null) {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
        transport.targetSteamId = steamId;
        if(NetworkManager.Singleton.StartClient()) {
            Debug.Log("Client has started.");

            callback?.Invoke(true);
        }
        else {
            callback?.Invoke(false);
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

            NetworkManager.Singleton.ConnectionApprovalCallback -= ConnectionApprovalCallback;

            NetworkManager.Singleton.OnConnectionEvent -= OnConnectionEvent;
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

    public void Invite(ulong id) {
        if(CurrentLobby == null) return;

        Friend? friend = CurrentLobby.Value.Members.Where(t => t.Id == id).FirstOrDefault();
        if(friend != null) {
            if(CurrentLobby.Value.InviteFriend(id)) {
                Debug.Log($"Invite successed. id: {id}");
            }
            else {
                Debug.LogError($"Invite failed. id: {id}");
            }
        }
        else {
            Debug.Log($"ID not found. id: {id}");
        }
    }

    public void UpdateLobbyData(string key, string value) {
        if(CurrentLobby == null) return;

        CurrentLobby.Value.SetData(key, value);
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
            Disconnect(() => {
                if(SceneLoader.Instance.CurrentLoadedScene != SceneType.Main) {
                    SceneLoader.Instance.LoadScene(
                        SceneType.Main,
                        () => {
                            LoadingPanel.Instance.SetActive(true, 0.5f);
                            LoadingPanel.Instance.UpdateProgress();
                        },
                        () => {
                            LoadingPanel.Instance.SetActive(false, 0.5f);
                            LoadingPanel.Instance.StopProgressUpdate();
                        });
                }
            });
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
        UpdateLobbyData("Scene", SceneLoader.Instance.CurrentLoadedScene.ToString());

        Debug.Log("Joined lobby.");
    }

    private void OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId steamId) {
        Debug.Log($"Lobby was created. ip: {ip}, port: {port}, name: {steamId}");

        lobby.SetData("ip", ip.ToString());
        lobby.SetData("port", port.ToString());
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

        Debug.Log($"OnLobbyEntered. LobbyID: {lobby.Id}, OwnerName: {lobby.Owner.Name}, OwnerID: {lobby.Owner.Id}");

        CurrentLobby = lobby;
        UpdateLobbyData("Scene", SceneLoader.Instance.CurrentLoadedScene.ToString());

        memberInfos.Clear();
        foreach(Friend friend in lobby.Members) {
            memberInfos.Add(friend.Id, new MemberInfo() {
                IsMe = friend.IsMe,
                ID = friend.Id,
                Name = friend.Name,
                IsReady = false
            });
        }

        //SceneLoader.Instance.LoadScene(
        //    SceneType.Lobby,
        //    () => {
        //        LoadingPanel.Instance.SetActive(true, 0.5f);
        //        LoadingPanel.Instance.UpdateProgress();
        //    },
        //    () => {
        //        LoadingPanel.Instance.SetActive(false, 0.5f);
        //        LoadingPanel.Instance.StopProgressUpdate();
        //    });

        //string ipString = lobby.GetData("ip");
        //uint? ip = null;
        //try {
        //    uint parse = uint.Parse(ipString);
        //    ip = parse;
        //}
        //catch(Exception ex) {
        //    Debug.LogError(ex.ToString());
        //}
        //StartClient(ip == null ? 0 : ip.Value, (value) => {
        StartClient(lobby.Id, (value) => { 
            if(value) {
                SceneLoader.Instance.LoadScene(
                    SceneType.Lobby,
                    () => {
                        LoadingPanel.Instance.SetActive(true, 0.5f);
                        LoadingPanel.Instance.UpdateProgress();
                    },
                    () => {
                        LoadingPanel.Instance.SetActive(false, 0.5f);
                        LoadingPanel.Instance.StopProgressUpdate();
                    });
            }
            else {
                Debug.Log("Failed create lobby.");
            }
        });
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

        NetworkManager.Singleton.OnConnectionEvent += OnConnectionEvent;

        Debug.Log($"Lobby created. owner: {lobby.Owner.Name}");
    }

    private void OnConnectionEvent(NetworkManager manager, ConnectionEventData eventData) {
        Debug.Log($"OnConnectionEvent. EventType: {eventData.EventType}, ClientId: {eventData.ClientId}");
        switch(eventData.EventType) {
            case ConnectionEvent.ClientConnected: {

                }
                break;
            case ConnectionEvent.ClientDisconnected: {

                }
                break;
            case ConnectionEvent.PeerConnected: {

                }
                break;
            case ConnectionEvent.PeerDisconnected: {

                }
                break;
        }
    }

    private void OnClickReady(ulong id, bool value) {
        MemberInfo info = null;
        if(memberInfos.TryGetValue(id, out info)) {
            memberInfos[id].IsReady = value;
        }
        else {
            Debug.LogError($"Member not found. id: {id}");
        }
    }
    #endregion

    private class MemberInfo {
        public bool IsMe;
        public SteamId ID;
        public string Name;
        public bool IsReady;
    }
}
