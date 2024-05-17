using Mu3Library;
using Mu3Library.Utility;
//using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUI : MonoBehaviour {
    [SerializeField] private Transform friendListRect;
    [SerializeField] private FriendIcon friendIconObj;
    private FriendIcon[] friendIcons;
    //private Dictionary<CSteamID, FriendInfo> friendInfos = new Dictionary<CSteamID, FriendInfo>();



    #region Utility
    //public void SetFriendIconsAll() {
    //    if(friendInfos.Count == 0) {
    //        Debug.Log("Friends not found.");

    //        return;
    //    }

    //    if(friendIcons != null && friendIcons.Length > 0) {
    //        for(int i = 0; i < friendIcons.Length; i++) {
    //            UnityObjectPoolManager.Instance.AddObject(friendIcons[i]);
    //        }
    //    }
    //    friendIcons = new FriendIcon[friendInfos.Count];

    //    FriendInfo tempInfo;
    //    int index = 0;
    //    foreach(CSteamID id in friendInfos.Keys) {
    //        FriendIcon icon = UnityObjectPoolManager.Instance.GetObject<FriendIcon>();
    //        if(icon == null) {
    //            icon = Instantiate(friendIconObj);
    //            icon.transform.SetParent(friendListRect);
    //        }
    //        icon.gameObject.SetActive(true);

    //        tempInfo = friendInfos[id];
    //        icon.SetIcon(tempInfo.AvatarSprite, tempInfo.Name);

    //        friendIcons[index] = icon;
    //        index++;
    //    }
    //}

    //public void InitializeFriendList() {
    //    int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
    //    FriendInfo tempInfo;
    //    for(int i = 0; i < friendCount; i++) {
    //        CSteamID friendSteamID = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);

    //        if(friendInfos.TryGetValue(friendSteamID, out tempInfo)) {

    //        }
    //        else {
    //            string name = SteamFriends.GetFriendPersonaName(friendSteamID);
    //            EPersonaState state = SteamFriends.GetFriendPersonaState(friendSteamID);
    //            int avatarInt = SteamFriends.GetMediumFriendAvatar(friendSteamID);
    //            FriendInfo info = new FriendInfo {
    //                Name = name,
    //                State = state,
    //                AvatarInt = avatarInt, 
    //            };

    //            friendInfos.Add(friendSteamID, info);

    //            tempInfo = info;
    //        }
    //        tempInfo.State = SteamFriends.GetFriendPersonaState(friendSteamID);
    //    }
    //}
    #endregion

    #region Action
    public void Network_StartHost(int maxMembers) {
        GameNetworkManager.Instance.StartHost(maxMembers, (value) => {
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

    public void Network_Join() {

    }
    #endregion

    //private class FriendInfo {
    //    public string Name;
    //    public EPersonaState State;
    //    public int AvatarInt;

    //    public Texture2D Avatar {
    //        get {
    //            if(avatar == null) {
    //                avatar = GetSteamImageAsTexture2D(AvatarInt);
    //            }

    //            return avatar;
    //        }
    //    }
    //    private Texture2D avatar = null;

    //    public Sprite AvatarSprite {
    //        get {
    //            if(avatarSprite == null) {
    //                avatarSprite = Sprite.Create(Avatar, new Rect(Vector2.zero, new Vector2(Avatar.width, Avatar.height)), Vector2.one * 0.5f);
    //            }

    //            return avatarSprite;
    //        }
    //    }
    //    private Sprite avatarSprite = null;



    //    private Texture2D GetSteamImageAsTexture2D(int iImage) {
    //        Texture2D texture = null;
    //        uint imageWidth;
    //        uint imageHeight;
    //        bool success = SteamUtils.GetImageSize(iImage, out imageWidth, out imageHeight);

    //        if(success) {
    //            byte[] image = new byte[imageWidth * imageHeight * 4];
    //            success = SteamUtils.GetImageRGBA(iImage, image, (int)(imageWidth * imageHeight * 4));
    //            if(success) {
    //                texture = new Texture2D((int)imageWidth, (int)imageHeight, TextureFormat.RGBA32, false, true);
    //                texture.LoadRawTextureData(image);
    //                texture.Apply();
    //            }
    //        }

    //        return texture;
    //    }
    //}
}