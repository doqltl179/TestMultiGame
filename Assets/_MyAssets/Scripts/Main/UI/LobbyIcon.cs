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
        lobbyText.text = $"{lobby.Owner.Name}'s Lobby ({lobby.Id})";
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
