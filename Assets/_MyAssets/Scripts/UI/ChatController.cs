using Mu3Library.Utility;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatController : MonoBehaviour {
    [SerializeField] private CanvasGroup canvasGroup;
    public float Alpha {
        get => canvasGroup.alpha;
        set => canvasGroup.alpha = value;
    }

    [Space(20)]
    [SerializeField] private TMP_InputField inputField;

    [Space(20)]
    [SerializeField] private ChatObject chatObj;
    [SerializeField] private RectTransform textParent;
    private List<ChatObject> messages = new List<ChatObject>();

    public bool IsDestroyed { get; private set; }



    private void Awake() {
        IsDestroyed = false;
    }

    private void OnDestroy() {
        IsDestroyed = true;
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.Return)) {
            if(!string.IsNullOrEmpty(inputField.text) && inputField.text != " ") {
                GameNetworkManager.Instance.SendChat(inputField.text);

                inputField.text = "";

                inputField.ActivateInputField();
                inputField.Select();
            }
        }
    }

    #region Utility
    public void Clear() {
        for(int i = 0; i < messages.Count; i++) {
            UnityObjectPoolManager.Instance.AddObject(messages[i]);
        }
        messages.Clear();
    }

    public void AddChat(string name, string message) {
        ChatObject chat = UnityObjectPoolManager.Instance.GetObject<ChatObject>();
        if(chat == null) {
            chat = Instantiate(chatObj);
        }
        chat.transform.SetParent(textParent);
        chat.gameObject.SetActive(true);

        chat.transform.localScale = Vector3.one;

        chat.Text = $"{name} : {message}";
    }

    public void AddChat(string message) {
        ChatObject chat = UnityObjectPoolManager.Instance.GetObject<ChatObject>();
        if(chat == null) {
            chat = Instantiate(chatObj);
        }
        chat.transform.SetParent(textParent);
        chat.gameObject.SetActive(true);

        chat.transform.localScale = Vector3.one;

        chat.Text = $"{message}";
    }
    #endregion
}
