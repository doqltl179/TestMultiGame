using Mu3Library.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbyIcon : AnimationButton {
    public Steamworks.Data.Lobby Lobby { get; private set; }

    [Space(20)]
    [SerializeField] private GameObject lockObj;

    [Space(20)]
    [SerializeField] private TextMeshProUGUI lobbyTitle;

    [Space(20)]
    [SerializeField] private TextMeshProUGUI lobbyMembers;

    [Space(20)]
    [SerializeField] private Image blockImage;
    [SerializeField] private Image hoverImage;
    [SerializeField] private Image selectImage;

    [Space(20)]
    [SerializeField, Range(0.01f, 1.0f)] private float hoverAnimationTime = 0.2f;

    private Action OnClickAction;

    private IEnumerator hoverAnimationCoroutine = null;



    #region Utility
    public void SetIcon(Steamworks.Data.Lobby lobby, Action onClick = null) {
        lockObj.SetActive(!string.IsNullOrEmpty(LobbyData.GetLobbyPassword(lobby)));
        
        string lobbyTitleText = LobbyData.GetLobbyTitle(lobby);
        if(string.IsNullOrEmpty(lobbyTitleText)) {
            lobbyTitleText = LobbyData.GetOwnerName(lobby);
            if(string.IsNullOrEmpty(lobbyTitleText)) {
                lobbyTitleText = $"{lobby.Owner.Name}'s Lobby";
            }
        }
        lobbyTitle.text = lobbyTitleText;

        lobbyMembers.text = $"({lobby.MemberCount}/{lobby.MaxMembers})";

        OnClickAction = onClick;

        ButtonEnabled = lobby.MemberCount < lobby.MaxMembers;

        Lobby = lobby;
    }
    #endregion

    public override void OnButtonEnabledChanged(bool value) {
        base.OnButtonEnabledChanged(value);

        blockImage?.gameObject.SetActive(!value);
    }

    public override void OnClick() {
        base.OnClick();

        OnClickAction?.Invoke();
    }

    protected override void PointerEnterAnimation(PointerEventData data) {
        if(hoverAnimationCoroutine != null) StopCoroutine(hoverAnimationCoroutine);

        hoverAnimationCoroutine = HoverStartAnimationCoroutine();
        StartCoroutine(hoverAnimationCoroutine);
    }

    protected override void PointerExitAnimation(PointerEventData data) {
        if(hoverAnimationCoroutine != null) StopCoroutine(hoverAnimationCoroutine);

        hoverAnimationCoroutine = HoverEndAnimationCoroutine();
        StartCoroutine(hoverAnimationCoroutine);
    }

    private IEnumerator HoverStartAnimationCoroutine() {
        hoverImage.gameObject.SetActive(true);

        float startAlpha = hoverImage.color.a;
        float animationTime = hoverAnimationTime * (1.0f - startAlpha);
        float timer = 0.0f;
        while(timer < animationTime) {
            timer += Time.deltaTime;

            hoverImage.color = UtilFunc.GetChangedAlphaColor(hoverImage.color, Mathf.Lerp(startAlpha, 1.0f, timer / animationTime));

            yield return null;
        }

        hoverAnimationCoroutine = null;
    }

    private IEnumerator HoverEndAnimationCoroutine() {
        float startAlpha = hoverImage.color.a;
        float animationTime = hoverAnimationTime * startAlpha;
        float timer = 0.0f;
        while(timer < animationTime) {
            timer += Time.deltaTime;

            hoverImage.color = UtilFunc.GetChangedAlphaColor(hoverImage.color, Mathf.Lerp(startAlpha, 0.0f, timer / animationTime));

            yield return null;
        }

        hoverImage.gameObject.SetActive(false);

        hoverAnimationCoroutine = null;
    }
}
