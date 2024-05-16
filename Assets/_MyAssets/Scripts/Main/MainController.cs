using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour {
    [SerializeField] private MainUI mainUI;



    private void Start() {
        mainUI.InitializeFriendList();
        mainUI.SetFriendIconsAll();
    }
}
