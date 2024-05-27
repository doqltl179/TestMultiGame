using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyIcon : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI lobbyText;
    [SerializeField] private TextMeshProUGUI lobbyMemberText;

    public Action OnClickButton;



    #region Utility
    public void SetIcon(Lobby lobby, Action callback = null) {
        string ip = LobbyData.GetIP(lobby);
        string port = LobbyData.GetPort(lobby);
        string ownerName = LobbyData.GetOwnerName(lobby);

        string title = LobbyData.GetRoomTitle(lobby);
        string password = LobbyData.GetRoomPassword(lobby);

        if(string.IsNullOrEmpty(title)) {
            if(string.IsNullOrEmpty(ownerName)) {
                lobbyText.text = $"{lobby.Owner.Name}'s Lobby ({lobby.Id})";
            }
            else {
                lobbyText.text = $"{ownerName}'s Lobby ({lobby.Id})";
            }
        }
        else {
            lobbyText.text = $"{title} ({lobby.Id})";
        }
        lobbyMemberText.text = $"({lobby.MemberCount} / {lobby.MaxMembers})";

        OnClickButton = callback;
    }
    #endregion

    #region Action
    public void OnClicked() {
        OnClickButton?.Invoke();
    }
    #endregion
}
