using Mu3Library;
using Mu3Library.Utility;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LobbyController : MonoBehaviour {
    [SerializeField] private LobbyUI lobbyUI;



    private void Awake() {
        KeyCodeInputCollector.Instance.InitCollectKeys();
    }

    private void Start() {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
        Time.fixedDeltaTime = 1.0f / Application.targetFrameRate;

        CameraManager.Instance.InitCamera();
    }

    #region Action
    public void StartGame() {
        SceneLoader.Instance.LoadScene(
            SceneType.Game,
            () => {
                LoadingPanel.Instance.SetActive(true, 0.5f);
                LoadingPanel.Instance.UpdateProgress();
            },
            () => {
                LoadingPanel.Instance.SetActive(false, 0.5f);
                LoadingPanel.Instance.StopProgressUpdate();
            });
    }
    #endregion
}
