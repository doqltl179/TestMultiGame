using Mu3Library.Utility;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : GenericSingleton<GameManager> {




    private void Awake() {
        if(SteamManager.Initialized) {
            string name = SteamFriends.GetPersonaName();

            Debug.Log(name);
        }

        KeyCodeInputCollector.Instance.InitCollectKeys();
    }

    private void Start() {
        Time.fixedDeltaTime = 1.0f / 144.0f;

        CameraManager.Instance.SetCamera(Camera.main);
    }
}
