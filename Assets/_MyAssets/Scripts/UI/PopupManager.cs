using DG.Tweening;
using Mu3Library.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour {
    public static PopupManager Instance {
        get {
            if(instance == null) {
                PopupManager pm = FindObjectOfType<PopupManager>();
                if(pm == null) {
                    GameObject resource = ResourceLoader.GetResource<GameObject>(nameof(PopupManager));
                    if(resource != null) {
                        GameObject go = Instantiate(resource);
                        DontDestroyOnLoad(go);

                        pm = go.GetComponent<PopupManager>();
                    }
                }

                instance = pm;
            }

            return instance;
        }
    }
    private static PopupManager instance;

    [SerializeField] private CanvasGroup canvasGroup;

    [Space(20)]
    [SerializeField] private GameObject popupObject;
    [SerializeField] private RectTransform popupRect;

    [Space(20)]
    [SerializeField] private RectTransform messageRect;
    [SerializeField] private TextMeshProUGUI messageTxt;
    private const float MessageRectHeightOffset = 80.0f;
    private const float MessageRectWidthOffset = -16.0f;

    [Space(20)]
    [SerializeField] private PopupButton btn01;
    [SerializeField] private PopupButton btn02;

    private Tween activeAnimationTween;



    #region Utility
    public void SetActive(bool active, float fadeTime = 0.0f) {
        if(fadeTime > 0.0f) {
            if(activeAnimationTween != null) activeAnimationTween.Kill();

            if(active) popupObject.SetActive(true);

            activeAnimationTween = DOTween.To(
                () => canvasGroup.alpha,
                (value) => canvasGroup.alpha = value,
                active ? 1.0f : 0.0f,
                fadeTime);
            if(!active) {
                activeAnimationTween.OnComplete(() => {
                    popupObject.SetActive(false);

                    activeAnimationTween = null;
                });
            }
            activeAnimationTween.Play();
        }
        else {
            popupObject.SetActive(active);
        }
    }

    public void SetPopup(string message, string text, Action onClick) {
        btn02.SetActive(false);

        SetMessage(message);

        btn01.SetOnClick(text, onClick);
    }

    public void SetPopup(string message, string text01, Action onClick01, string text02, Action onClick02) {
        btn02.SetActive(true);

        SetMessage(message);

        btn01.SetOnClick(text01, onClick01);
        btn02.SetOnClick(text02, onClick02);
    }
    #endregion

    private void SetMessage(string message) {
        //messageRect.sizeDelta = new Vector2(messageRect.sizeDelta.x, CalculatedMessageBoxHeight(message));
        messageTxt.text = message;
        messageRect.sizeDelta = new Vector2(messageRect.sizeDelta.x, messageTxt.preferredHeight);

        LayoutRebuilder.ForceRebuildLayoutImmediate(popupRect);
    }

    private float CalculatedMessageBoxHeight(string message) {
        string[] s = message.Split('\n');
        int lineCount = 0;
        for(int i = 0; i < s.Length; i++) {
            messageTxt.text = s[i];
            lineCount += Mathf.FloorToInt(messageTxt.preferredWidth / (messageRect.sizeDelta.x + MessageRectWidthOffset)) + 1;
        }

        return lineCount * messageTxt.fontSize + MessageRectHeightOffset;
    }

    [Serializable]
    private class PopupButton {
        public Button btn;
        public TextMeshProUGUI txt;



        #region Utility
        public void SetActive(bool active) {
            btn.gameObject.SetActive(active);
        }

        public void SetOnClick(string text, Action action) {
            btn.onClick.RemoveAllListeners();

            txt.text = text;
            btn.onClick.AddListener(() => {
                action.Invoke();
            });
        }
        #endregion
    }
}