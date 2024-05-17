using Mu3Library;
using Mu3Library.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyUI : MonoBehaviour {



    #region Action
    public void ReadyToggleValueChanged(bool value) {
        //NetworkTransmission.Instance.IsTheClientReadyServerRPC(value, )
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
    #endregion
}
