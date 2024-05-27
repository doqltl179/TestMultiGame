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
        uint ip = 0;
        ushort port = 0;
        SteamId id = new SteamId();
        if(lobby.GetGameServer(ref ip, ref port, ref id)) {
            lobbyText.text = $"{lobby.Owner.Name}'s Lobby ({lobby.Id}) || ip: {ip}, port: {port}, id: {id}";
            lobbyMemberText.text = $"({lobby.MemberCount} / {lobby.MaxMembers})";
        }
        else {
            lobbyText.text = $"{lobby.Owner.Name}'s Lobby ({lobby.Id})";
            lobbyMemberText.text = $"({lobby.MemberCount} / {lobby.MaxMembers})";
        }

        OnClickButton = callback;
    }
    #endregion

    #region Action
    public void OnClicked() {
        OnClickButton?.Invoke();
    }
    #endregion
}
