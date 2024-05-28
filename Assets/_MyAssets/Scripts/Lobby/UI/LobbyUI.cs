using Mu3Library;
using Mu3Library.Utility;
using Steamworks;
using Steamworks.Data;
using Steamworks.ServerList;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class LobbyUI : MonoBehaviour {
    [SerializeField] private NetworkObject networkTransmissionObj;
    private NetworkTransmission networkTransmission;

    [Space(20)]
    [SerializeField] private FriendIcon friendIconObj;

    [Space(20)]
    [SerializeField] private Transform lobbyFriendRect;
    private List<FriendIcon> lobbyFriendIcons = new List<FriendIcon>();

    [Space(20)]
    [SerializeField] private GameObject invitePopObject;
    [SerializeField] private Transform inviteFriendRect;
    private List<FriendIcon> inviteFriendIcons = new List<FriendIcon>();

    private Dictionary<ulong, FriendInfo> friendInfos = new Dictionary<ulong, FriendInfo>();



    private void Awake() {
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
    }

    private void OnDestroy() {
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;

        if(networkTransmission != null) networkTransmission.OnClickReady -= OnClickReady;
    }

    private IEnumerator Start() {
        invitePopObject.SetActive(false);

        if(GameNetworkManager.Instance.CurrentLobby != null) {
            foreach(Friend friend in GameNetworkManager.Instance.CurrentLobby.Value.Members) {
                AddLobbyFriendIcon(friend);
            }
        }

        if(networkTransmission == null) {
            if(NetworkManager.Singleton.IsHost) {
                NetworkObject go = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(networkTransmissionObj);
                NetworkTransmission transmission = go.GetComponent<NetworkTransmission>();

                networkTransmission = transmission;
            }
            else {
                WaitForSeconds wait = new WaitForSeconds(1.0f);
                const int maxCount = 10;
                int count = 0;
                while(count < maxCount) {
                    networkTransmission = FindObjectOfType<NetworkTransmission>();
                    if(networkTransmission != null) break;

                    count++;

                    yield return wait;
                }

                if(networkTransmission == null) {
                    Debug.Log($"[{nameof(NetworkTransmission)}] not found.");

                    Disconnect();

                    yield break;
                }
            }
        }

        networkTransmission.OnClickReady += OnClickReady;

        Debug.Log($"IsHost: {NetworkManager.Singleton.IsHost}, IsServer: {NetworkManager.Singleton.IsServer}, IsClient: {NetworkManager.Singleton.IsClient}");
    }

    private async void AddInviteFriendIcon(Friend friendId) {
        FriendIcon icon = UnityObjectPoolManager.Instance.GetObject<FriendIcon>();
        if(icon == null) {
            icon = Instantiate(friendIconObj);
        }
        icon.transform.SetParent(inviteFriendRect);
        icon.transform.localScale = Vector3.one;

        FriendInfo info = null;
        if(friendInfos.TryGetValue(friendId.Id, out info)) {

        }
        else {
            FriendInfo friendInfo = new FriendInfo();
            friendInfo.Name = friendId.Name;

            Image? image = await friendId.GetMediumAvatarAsync();
            if(image != null) {
                Texture2D tex = new Texture2D((int)image.Value.Width, (int)image.Value.Height, TextureFormat.RGBA32, false, true);
                tex.LoadRawTextureData(image.Value.Data);
                tex.Apply();

                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);

                friendInfo.Texture = tex;
                friendInfo.Sprite = sprite;
            }

            friendInfos.Add(friendId.Id, friendInfo);

            info = friendInfo;
        }
        icon.SetIcon(friendId.Id, info.Sprite, info.Name);
        icon.RemoveAllButtonAction();
        icon.UseButton = true;
        icon.IsReady = false;
        icon.gameObject.SetActive(true);

        icon.OnClick += () => {
            GameNetworkManager.Instance.Invite(friendId.Id);
        };

        inviteFriendIcons.Add(icon);
    }

    private async void AddLobbyFriendIcon(Friend friendId) {
        FriendIcon icon = UnityObjectPoolManager.Instance.GetObject<FriendIcon>();
        if(icon == null) {
            icon = Instantiate(friendIconObj);
        }
        icon.transform.SetParent(lobbyFriendRect);
        icon.transform.localScale = Vector3.one;

        FriendInfo info = null;
        if(friendInfos.TryGetValue(friendId.Id, out info)) {

        }
        else {
            FriendInfo friendInfo = new FriendInfo();
            friendInfo.Name = friendId.Name;

            Image? image = await friendId.GetMediumAvatarAsync();
            if(image != null) {
                Texture2D tex = new Texture2D((int)image.Value.Width, (int)image.Value.Height, TextureFormat.RGBA32, false, true);
                tex.LoadRawTextureData(image.Value.Data);
                tex.Apply();

                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);

                friendInfo.Texture = tex;
                friendInfo.Sprite = sprite;
            }

            friendInfos.Add(friendId.Id, friendInfo);

            info = friendInfo;
        }
        icon.SetIcon(friendId.Id, info.Sprite, info.Name);
        icon.UseButton = false;
        icon.IsReady = false;
        icon.gameObject.SetActive(true);

        lobbyFriendIcons.Add(icon);
    }

    #region Utility
    public void ClearFriendIcons() {
        for(int i = 0; i < lobbyFriendIcons.Count; i++) {
            UnityObjectPoolManager.Instance.AddObject(lobbyFriendIcons[i]);
        }
        lobbyFriendIcons.Clear();
    }
    #endregion

    #region Action
    public void OnClickReady() {
        Friend? local = GameNetworkManager.Instance.LocalID;
        if(local != null) {
            bool ready = !GameNetworkManager.Instance.GetReady(local.Value.Id);
            networkTransmission.Ready_ServerRpc(local.Value.Id, ready);

            GameNetworkManager.Instance.SetReady(local.Value.Id, ready);
        }
    }

    private void OnClickReady(ulong id, bool value) {
        FriendIcon icon = lobbyFriendIcons.Where(t => t.ID == id).FirstOrDefault();
        if(icon != null) {
            icon.IsReady = value;
        }
        else {
            Debug.LogError($"Member not found. id: {id}");
        }
    }

    public void OnClickInvite() {
        foreach(Friend id in SteamFriends.GetFriends()) {
            AddInviteFriendIcon(id);
        }

        invitePopObject.SetActive(true);
    }

    public void CloseInvite() {
        for(int i = 0; i < inviteFriendIcons.Count; i++) {
            UnityObjectPoolManager.Instance.AddObject(inviteFriendIcons[i]);
        }
        inviteFriendIcons.Clear();

        invitePopObject.SetActive(false);
    }

    public void Disconnect() {
        GameNetworkManager.Instance.Disconnect(() => {
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

    private void OnLobbyMemberLeave(Lobby lobby, Friend friendId) {
        int index = lobbyFriendIcons.FindIndex(t => t.ID == friendId.Id);
        if(index >= 0) {
            FriendIcon icon = lobbyFriendIcons[index];
            lobbyFriendIcons.RemoveAt(index);
            UnityObjectPoolManager.Instance.AddObject(icon);
        }
        else {
            Debug.LogError($"FriendIcon not found. id: {friendId.Id}, name: {friendId.Name}");
        }
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend friendId) {
        //Debug.Log($"[{friendId.Name}] joined.");

        AddLobbyFriendIcon(friendId);
    }
    #endregion

    private class FriendInfo {
        public Texture2D Texture;
        public Sprite Sprite;
        public string Name;
    }
}
