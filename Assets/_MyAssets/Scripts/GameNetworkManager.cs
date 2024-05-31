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
                instance = FindObjectOfType<GameNetworkManager>();
                if(instance == null) {
                    GameObject obj = ResourceLoader.GetResource<GameObject>($"Network/{nameof(GameNetworkManager)}");
                    if(obj != null) {
                        GameObject go = Instantiate(obj);

                        DontDestroyOnLoad(go);

                        instance = go.GetComponent<GameNetworkManager>();
                    }
                }
            }

            return instance;
        }
    }
    private static GameNetworkManager instance = null;

    [SerializeField] private FacepunchTransport transport;

    [Space(20)]
    [SerializeField] private NetworkObject networkPlayerObj;
    private NetworkPlayerObject networkPlayerObject;

    public Lobby? CurrentLobby { get; private set; } = null;
    private Dictionary<ulong, MemberInfo> memberInfos = new Dictionary<ulong, MemberInfo>();
    public ulong[] MemberIDs => memberInfos.Keys.ToArray();
    public Friend? LocalID {
        get {
            if(CurrentLobby == null) {
                Debug.Log("Lobby not exist.");

                return null;
            }

            foreach(var member in CurrentLobby.Value.Members) {
                if(member.IsMe) return member;
            }

            Debug.Log("My ID not found in Lobby.");

            return null;
        }
    }

    public Action<bool> OnReadyAll;

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

        //NetworkManager.Singleton.ConnectionApprovalCallback -= ConnectionApprovalCallback;
    }

    private void OnApplicationQuit() {
        Disconnect();
    }

    private void Start() {
        //transport.OnTransportEvent += OnTransportEvent;
    }

    //private void OnTransportEvent(NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload, float receiveTime) {
    //    //Debug.Log($"OnTransportEvent. type: {eventType}, clientId: {clientId}, receiveTime: {receiveTime}, payload: {Encoding.ASCII.GetString(payload.Array, payload.Offset, payload.Count)}");

    //    StringBuilder payloadString = new StringBuilder();
    //    if(payload.Array != null) {
    //        for(int i = 0; i < payload.Array.Length; i++) {
    //            payloadString.Append($"{payload.Array[i]} ");
    //        }
    //    }
    //    Debug.Log($"OnTransportEvent. type: {eventType}, clientId: {clientId}, receiveTime: {receiveTime}, payload: {payloadString}");
    //}

    #region Utility
    public void Init() {

    }

    public async void StartHost(int maxMembers, Action<bool> callback = null) {
        //NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApprovalCallback;
        //NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;

        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnServerStopped += OnServerStopped;
        NetworkManager.Singleton.StartHost();
        CurrentLobby = await SteamMatchmaking.CreateLobbyAsync(maxMembers);
        CurrentLobby?.SetPublic();
        CurrentLobby?.SetJoinable(true);
        CurrentLobby?.SetGameServer(CurrentLobby.Value.Owner.Id);

        UpdateLobbyData("Scene", SceneLoader.Instance.CurrentLoadedScene.ToString());

        if(networkPlayerObject == null) {
            GameObject go = Instantiate(NetworkManager.Singleton.NetworkConfig.PlayerPrefab);
            //DontDestroyOnLoad(go);

            NetworkPlayerObject npo = go.GetComponent<NetworkPlayerObject>();

            networkPlayerObject = npo;
        }
        if(!networkPlayerObject.IsSpawned) {
            networkPlayerObject.NetworkObject.SpawnAsPlayerObject(0);
            networkPlayerObject.NetworkObject.DestroyWithScene = false;
            Debug.Log(networkPlayerObject.NetworkObject.TrySetParent(transform));
        }

        callback?.Invoke(CurrentLobby != null);
    }

    //private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
    //    Debug.Log($"ConnectionApprovalCallback. ClientNetworkId: {request.ClientNetworkId}, Approved: {response.Approved}, Reason: {response.Reason}");
    //}

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
        transport.Initialize(NetworkManager.Singleton);
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

            //NetworkManager.Singleton.ConnectionApprovalCallback -= ConnectionApprovalCallback;
        }
        else {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            //NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }

        NetworkManager.Singleton.OnConnectionEvent -= OnConnectionEvent;

        if(networkPlayerObject != null && networkPlayerObject.IsSpawned) {
            networkPlayerObject.NetworkObject.Despawn(false);

            Debug.Log($"NetworkPlayerObject despawn.");
        }

        NetworkManager.Singleton.Shutdown(true);
        Debug.Log("Disconnected.");

        callback?.Invoke();
    }

    public void SendChat(string message) {
        CurrentLobby?.SendChatString(message);
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

    public bool GetReady(ulong id) {
        MemberInfo info = null;
        if(memberInfos.TryGetValue(id, out info)) {

        }
        else {
            Debug.LogError($"Member not found. id: {id}");
        }

        return info != null ? info.IsReady : false;
    }

    public void SetReady(ulong id, bool value) {
        MemberInfo info = null;
        if(memberInfos.TryGetValue(id, out info)) {
            info.IsReady = value;

            OnReadyAll?.Invoke(!memberInfos.Any(t => t.Value.IsReady == false));
        }
        else {
            Debug.LogError($"Member not found. id: {id}");
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
        Debug.Log($"OnClientDisconnectCallback. clientId: {clientId}");

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
        Debug.Log($"OnClientConnectedCallback. clientId: {clientId}");
    }

    private void OnServerStopped(bool obj) {
        Debug.Log($"OnServerStopped. Host Stoped. value: {obj}");

        NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
    }

    private void OnServerStarted() {
        Debug.Log("OnServerStarted. Host Started.");
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

        Debug.Log("OnGameLobbyJoinRequested.");
    }

    private void OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId steamId) {
        Debug.Log($"OnLobbyGameCreated. ip: {ip}, port: {port}, name: {steamId}");

        NetworkManager.Singleton.OnConnectionEvent += OnConnectionEvent;

        lobby.SetData(LobbyData.key_ip, ip.ToString());
        lobby.SetData(LobbyData.key_port, port.ToString());
        lobby.SetData(LobbyData.key_ownerId, steamId.ToString());
        lobby.SetData(LobbyData.key_ownerName, lobby.Owner.Name);
        lobby.SetData(LobbyData.key_roomTitle, "Test Lobby Title");

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("_____ Lobby Datas _____");
        foreach(var item in lobby.Data) {
            sb.AppendLine($"Key: {item.Key} || Value: {item.Value}");
        }
        Debug.Log(sb.Length > 1 ? sb.ToString() : "Data not exist.");
    }

    // Friend send you an steam invite
    private void OnLobbyInvite(Friend friendId, Lobby lobby) {
        Debug.Log($"OnLobbyInvite. Invite from [{friendId.Name}].");
    }

    private void OnLobbyMemberLeave(Lobby lobby, Friend friendId) {
        Debug.Log($"OnLobbyMemberLeave. [{friendId.Name}] leaved.");

        MemberInfo info = null;
        if(memberInfos.TryGetValue(friendId.Id.Value, out info)) {
            memberInfos.Remove(friendId.Id.Value);

            OnReadyAll?.Invoke(!memberInfos.Any(t => t.Value.IsReady == false));
        }
        else {
            Debug.LogWarning($"Friend not exist. name: {friendId.Name}");
        }

        ulong id = ulong.Parse(LobbyData.GetOwnerId(lobby));
        if(id == friendId.Id) {
            Debug.Log("Owner leaved.");

            Disconnect(() => {
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
            });
        }
        else {
            Debug.Log("Member leaved.");
        }
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend friendId) {
        Debug.Log($"OnLobbyMemberJoined. [{friendId.Name}] joined.");

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

        OnReadyAll?.Invoke(!memberInfos.Any(t => t.Value.IsReady == false));
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
        //StartClient(lobby.Id, (value) => { 
        StartClient(lobby.Owner.Id, (value) => {
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
                Debug.Log("Failed enter lobby.");
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

        Debug.Log($"OnLobbyCreated. owner: {lobby.Owner.Name}");
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
    #endregion

    private class MemberInfo {
        public bool IsMe;
        public SteamId ID;
        public string Name;
        public bool IsReady;
    }
}
