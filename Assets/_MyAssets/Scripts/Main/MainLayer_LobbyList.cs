using Mu3Library.Scene;
using Mu3Library.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainLayer_LobbyList : SceneUILayer {
    [SerializeField] private GameObject lobbyLoading;

    [Space(20)]
    [SerializeField] private GameObject filterObj;
    private bool savedHidePwdIsOn;
    private bool savedHideFullIsOn;

    [Space(20)]
    [SerializeField] private Transform lobbyIconParent;
    [SerializeField] private LobbyIcon lobbyIconObj;
    private LobbyIcon[] lobbyIconList;

    [Space(20)]
    [SerializeField] private Toggle hidePwdToggle;
    [SerializeField] private Toggle hideFullToggle;



    public override void OnActivate() {
        filterObj.SetActive(false);

        hidePwdToggle.isOn = false;
        hideFullToggle.isOn = false;

        RefreshLobbyList();
    }

    public override void OnDeActivate() {
        if(lobbyIconList != null && lobbyIconList.Length > 0) {
            for(int i = 0; i < lobbyIconList.Length; i++) {
                UnityObjectPoolManager.Instance.AddObject(lobbyIconList[i]);
            }
            lobbyIconList = null;
        }
    }

    private async void RefreshLobbyList() {
        lobbyLoading.SetActive(true);

        if(lobbyIconList != null && lobbyIconList.Length > 0) {
            for(int i = 0; i < lobbyIconList.Length; i++) {
                UnityObjectPoolManager.Instance.AddObject(lobbyIconList[i]);
            }
            lobbyIconList = null;
        }

        await GameNetworkManager.Instance.LoadAllLobby();
        GameNetworkManager.Instance.SetFilterLobby(hidePwdToggle.isOn, hideFullToggle.isOn);

        if(GameNetworkManager.Instance.FilterLobbyCount > 0) {
            lobbyIconList = new LobbyIcon[GameNetworkManager.Instance.FilterLobbyCount];

            for(int i = 0; i < GameNetworkManager.Instance.FilterLobbyCount; i++) {
                LobbyIcon icon = UnityObjectPoolManager.Instance.GetObject<LobbyIcon>();
                if(icon == null) {
                    icon = Instantiate(lobbyIconObj);
                }

                icon.transform.SetParent(lobbyIconParent);
                icon.gameObject.SetActive(true);

                icon.transform.localScale = Vector3.one;

                icon.SetIcon(GameNetworkManager.Instance.FilterLobbyList[i]);

                lobbyIconList[i] = icon;
            }
        }

        lobbyLoading.SetActive(false);
    }

    #region Action
    public void OnClickRefresh() {
        RefreshLobbyList();
    }

    public void OnClickFilter() {
        savedHidePwdIsOn = hidePwdToggle.isOn;
        savedHideFullIsOn = hideFullToggle.isOn;

        filterObj.SetActive(true);
    }

    public void OnClickFilterApply() {
        RefreshLobbyList();

        filterObj.SetActive(false);
    }

    public void OnClickFilterCancel() {
        hidePwdToggle.isOn = savedHidePwdIsOn;
        hideFullToggle.isOn = savedHideFullIsOn;

        filterObj.SetActive(false);
    }
    #endregion
}
