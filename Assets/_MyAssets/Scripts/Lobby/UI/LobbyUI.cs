using Mu3Library;
using Mu3Library.Utility;
using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyUI : MonoBehaviour {
    [SerializeField] private Transform friendListRect;
    [SerializeField] private FriendIcon friendIconObj;
    private List<FriendIcon> friendIcons = new List<FriendIcon>();
    private Dictionary<ulong, FriendInfo> friendInfos = new Dictionary<ulong, FriendInfo>();



    private void Awake() {
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
    }

    private void OnDestroy() {
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
    }

    private void Start() {
        if(GameNetworkManager.Instance.CurrentLobby != null) {
            foreach(Friend f in GameNetworkManager.Instance.CurrentLobby.Value.Members) {
                OnLobbyMemberJoined(GameNetworkManager.Instance.CurrentLobby.Value, f);
            }
        }
    }

    #region Utility
    public void ClearFriendIcons() {
        for(int i = 0; i < friendIcons.Count; i++) {
            friendIcons[i].ID = 0;
            UnityObjectPoolManager.Instance.AddObject(friendIcons[i]);
        }
        friendIcons.Clear();
    }
    #endregion

    #region Action
    public void OnClickReady() {
        GameNetworkManager.Instance.Ready();
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
        int index = friendIcons.FindIndex(t => t.ID == friendId.Id);
        if(index >= 0) {
            FriendIcon icon = friendIcons[index];
            icon.ID = 0;
            friendIcons.RemoveAt(index);
            UnityObjectPoolManager.Instance.AddObject(icon);
        }
    }

    private async void OnLobbyMemberJoined(Lobby lobby, Friend friendId) {
        Debug.Log($"[{friendId.Name}] joined.");

        FriendIcon icon = UnityObjectPoolManager.Instance.GetObject<FriendIcon>();
        if(icon == null) {
            icon = Instantiate(friendIconObj);
        }
        icon.transform.SetParent(friendListRect);
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
        icon.SetIcon(info.Sprite, info.Name);
        icon.ID = friendId.Id;
        icon.gameObject.SetActive(true);

        friendIcons.Add(icon);
    }
    #endregion

    private class FriendInfo {
        public Texture2D Texture;
        public Sprite Sprite;
        public string Name;
    }
}
