using Mu3Library.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance {
        get {
            if(instance == null) {
                instance = FindObjectOfType<GameManager>();
                if(instance == null) {
                    Debug.LogError($"`{nameof(GameManager)}` not found.");
                }
            }

            return instance;
        }
    }
    private static GameManager instance;



    private void Awake() {
        KeyCodeInputCollector.Instance.InitCollectKeys();
    }

    private void Start() {
        Time.fixedDeltaTime = 1.0f / 144.0f;

        CameraManager.Instance.SetCamera(Camera.main);

        //HostMigrationManager.Instance.SetToHost();
        HostMigrationManager.Instance.SetupRelay(4);
    }
}
