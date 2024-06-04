using Mu3Library.Scene;
using Mu3Library.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : SceneController {
    [Space(20)]
    [SerializeField] private MainUI mainUi;



    public override void OnSceneLoad() {
        GameNetworkManager.Instance.Init();

        mainUi.OnFirstActivate();

        SceneLoadedCompletely = true;
    }
}
