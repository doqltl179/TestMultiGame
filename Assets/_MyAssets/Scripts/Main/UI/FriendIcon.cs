using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendIcon : MonoBehaviour {
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;

    [Space(20)]
    [SerializeField] private GameObject readyObj;
    public bool IsReady {
        get => readyObj.activeSelf;
        set => readyObj.SetActive(value);
    }

    public ulong ID = 0;



    #region Utility
    public void SetIcon(Sprite image, string name) {
        icon.sprite = image;
        nameText.text = name;
    }
    #endregion
}
