using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendIcon : MonoBehaviour {
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;



    #region Utility
    public void SetIcon(Sprite image, string name) {
        icon.sprite = image;
        nameText.text = name;
    }
    #endregion
}
