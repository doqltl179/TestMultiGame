using Mu3Library;
using Mu3Library.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUI : MonoBehaviour {
    [SerializeField] private Transform friendListRect;
    [SerializeField] private FriendIcon friendIconObj;
    private FriendIcon[] friendIcons;



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
}