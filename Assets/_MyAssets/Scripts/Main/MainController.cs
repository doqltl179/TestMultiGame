using Mu3Library.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour {
    [SerializeField] private MainUI mainUI;



    private void Awake() {
        GameNetworkManager.Instance.Init();

        KeyCodeInputCollector.Instance.InitCollectKeys();
    }

    private void Start() {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
        Time.fixedDeltaTime = 1.0f / Application.targetFrameRate;

        CameraManager.Instance.InitCamera();
    }
}
