using Mu3Library;
using Mu3Library.Log;
using Mu3Library.Scene;
using Mu3Library.Utility;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using Steamworks.ServerList;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public Lobby[] LobbyList { get; private set; }
    public int LobbyCount => LobbyList == null ? 0 : LobbyList.Length;

    public Lobby[] FilterLobbyList { get; private set; }
    public int FilterLobbyCount => FilterLobbyList == null ? 0 : FilterLobbyList.Length;

    public Lobby? CurrentLobby { get; private set; } = null;
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
    private object[] param = null;

    private Dictionary<ulong, SteamUserInfoStruct> userInfos = new Dictionary<ulong, SteamUserInfoStruct>();

    public Action<bool> OnReadyAll;

    [Space(20)]
    [SerializeField] private NetworkObject networkTransmissionObj;
    private NetworkTransmission networkTransmission;

    private ChatController chatController;

    [Space(20)]
    [SerializeField] private LogCapture logCaptureObj;
    private LogCapture logCapture;



    private void Awake() {
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;

        SteamMatchmaking.OnChatMessage += OnChatMessage;
        
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;

        if(logCapture == null) {
            logCapture = Instantiate(logCaptureObj);
            DontDestroyOnLoad(logCapture.gameObject);
        }
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

    }

    public async void StartHost(int maxMembers, Action<bool> callback = null) {
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnServerStopped += OnServerStopped;
        NetworkManager.Singleton.StartHost();
        CurrentLobby = await SteamMatchmaking.CreateLobbyAsync(maxMembers);
        CurrentLobby?.SetPublic();
        CurrentLobby?.SetJoinable(true);
        CurrentLobby?.SetGameServer(CurrentLobby.Value.Owner.Id);

        if(networkTransmission == null) {
            NetworkObject no = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(networkTransmissionObj);

            DontDestroyOnLoad(no.gameObject);

            networkTransmission = no.GetComponent<NetworkTransmission>();
        }

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
        CurrentLobby = null;
        FilterLobbyList = null;

        if(userInfos != null && userInfos.Count > 0) {
            foreach(var key in userInfos.Keys) {
                userInfos[key].DestroyImages();
            }
            userInfos = new Dictionary<ulong, SteamUserInfoStruct>();
        }

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
        }

        NetworkManager.Singleton.OnConnectionEvent -= OnConnectionEvent;

        networkTransmission = null;
        networkPlayerObject = null;

        NetworkManager.Singleton.Shutdown(true);
        Debug.Log("Disconnected.");

        callback?.Invoke();
    }

    public async void JoinRequest(Lobby lobby) {
        await lobby.Join();
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

    public async Task LoadAllLobby() {
        Lobby[] list = await SteamMatchmaking.LobbyList.RequestAsync();
        if(list == null) {
            Debug.Log("Lobby not found.");

            LobbyList = new Lobby[0];
        }
        else {
            LobbyList = list;
        }
    }

    public void SetFilterLobby(bool hidePwd, bool hideFull) {
        Lobby[] result = LobbyList.Where(t => !LobbyData.GetIsPrivate(t)).ToArray();

        if(hidePwd) {
            result = result.Where(t => string.IsNullOrEmpty(LobbyData.GetLobbyPassword(t))).ToArray();
        }

        if(hideFull) {
            result = result.Where(t => t.MemberCount < t.MaxMembers).ToArray();
        }

        FilterLobbyList = result;
    }

    public async Task<Sprite> GetThumbSprite(ulong id) {
        SteamUserInfoStruct info = null;
        if(userInfos.TryGetValue(id, out info)) {
            if(info.ThumbSprite == null) {
                await info.CreateThumb();
            }

            return info.ThumbSprite;
        }

        return null;
    }

    public bool GetReady(ulong id) {
        return false;
    }

    public void SetReady(ulong id, bool value) {

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
                if(SceneLoader.Instance.CurrentSceneType != SceneType.Main) {
                    SceneLoader.Instance.LoadScene(SceneType.Main);
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

        ulong id = ulong.Parse(LobbyData.GetOwnerId(lobby));
        if(id == friendId.Id) {
            Debug.Log("Owner leaved.");

            Disconnect(() => {
                SceneLoader.Instance.LoadScene(SceneType.Main);
            });
        }
        else {
            Debug.Log("Member leaved.");
        }
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend friendId) {
        Debug.Log($"OnLobbyMemberJoined. [{friendId.Name}] joined.");

        userInfos.Add(friendId.Id, new SteamUserInfoStruct() { Info = friendId });
    }

    private void OnLobbyEntered(Lobby lobby) {
        if(NetworkManager.Singleton.IsHost) {
            return;
        }

        Debug.Log($"OnLobbyEntered. LobbyID: {lobby.Id}, OwnerName: {lobby.Owner.Name}, OwnerID: {lobby.Owner.Id}");

        CurrentLobby = lobby;

        foreach(var info in lobby.Members) {
            userInfos.Add(info.Id, new SteamUserInfoStruct() { Info = info });
        }

        StartClient(lobby.Owner.Id, (value) => {
            if(value) {
                SceneLoader.Instance.LoadScene(SceneType.Lobby);
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

        if(param != null) {
            // Set Lobby Data

            param = null;
        }

        userInfos.Add(lobby.Owner.Id, new SteamUserInfoStruct() { Info = lobby.Owner });

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
}

public class SteamUserInfoStruct {
    public Friend Info;
    public Texture2D ThumbImage;
    public Sprite ThumbSprite;



    public async Task CreateThumb() {
        Image? image = await Info.GetMediumAvatarAsync();
        if(image != null) {
            ThumbImage = new Texture2D((int)image.Value.Width, (int)image.Value.Height, TextureFormat.RGBA32, false, true);
            ThumbImage.LoadRawTextureData(image.Value.Data);
            ThumbImage.Apply();

            ThumbSprite = Sprite.Create(ThumbImage, new Rect(0, 0, ThumbImage.width, ThumbImage.height), Vector2.zero);
        }
    }

    public void DestroyImages() {
        if(ThumbImage != null) UnityEngine.Object.Destroy(ThumbImage);
        if(ThumbSprite != null) UnityEngine.Object.Destroy(ThumbSprite);
    }
}