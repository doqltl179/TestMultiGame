using Mu3Library.Scene;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUI : SceneUI {
    [Space(20)]
    [SerializeField] private MainController mainController;

    [Space(20)]
    [SerializeField] private MainLayer_Main layer_main;
    [SerializeField] private MainLayer_LobbyList layer_lobbyList;
    [SerializeField] private MainLayer_Lobby layer_lobby;



    public override void OnFirstActivate() {
        layer_main.Alpha = 0.0f;
        layer_lobbyList.Alpha = 0.0f;
        layer_lobby.Alpha = 0.0f;

        layer_main.SetActive = false;
        layer_lobbyList.SetActive = false;
        layer_lobby.SetActive = false;

        Transition(layer_main);
    }

    #region Action
    public void OnClickBackToMain() {
        Transition(layer_main, 0.8f);
    }

    public void OnClickSinglePlay() {

    }

    public void OnClickMultiPlay() {
        Transition(layer_lobbyList, 0.8f);
    }

    public void OnClickSettings() {

    }

    public void OnClickQuit() {

    }
    #endregion
}
