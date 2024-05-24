using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendIcon : MonoBehaviour {
    public ulong ID { get; private set; }

    [SerializeField] private Button button;
    public bool UseButton {
        get => button.enabled;
        set {
            button.transition = value ? Selectable.Transition.ColorTint : Selectable.Transition.None;

            button.enabled = value;
        }
    }
    public Action OnClick;

    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;

    [Space(20)]
    [SerializeField] private GameObject readyObj;
    public bool IsReady {
        get => readyObj.activeSelf;
        set => readyObj.SetActive(value);
    }



    #region Utility
    public void SetIcon(ulong id, Sprite image, string name) {
        ID = id;

        icon.sprite = image;
        nameText.text = name;
    }

    public void RemoveAllButtonAction() => OnClick = null;
    #endregion

    #region Action
    public void OnClickButton() {
        OnClick?.Invoke();
    }
    #endregion
}
