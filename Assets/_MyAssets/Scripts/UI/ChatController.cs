using Mu3Library.Utility;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
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
    [SerializeField] private Transform textParent;
    private List<ChatObject> messages = new List<ChatObject>();



    private void Update() {
        if(!string.IsNullOrEmpty(inputField.text) && inputField.text != " " && Input.GetKeyDown(KeyCode.Return)) {
            AddChat(SteamClient.Name, inputField.text);

            NetworkTransmission.Instance.IWishToSendAChatServerRPC($"{SteamClient.Name} : {inputField.text}", NetworkManager.Singleton.LocalClientId);
            inputField.text = "";
        }
    }

    #region Utility
    public void Clear() {
        for(int i = 0; i < messages.Count; i++) {
            UnityObjectPoolManager.Instance.AddObject(messages[i]);
        }
        messages.Clear();
    }

    public void SendChat(string message, ulong fromWho, bool isServer) {
        if(!isServer) {

        }
    }
    #endregion

    #region Action
    private void AddChat(string name, string message) {
        ChatObject chat = UnityObjectPoolManager.Instance.GetObject<ChatObject>();
        if(chat == null) {
            chat = Instantiate(chatObj);
        }
        chat.transform.SetParent(textParent);
        chat.gameObject.SetActive(true);

        chat.Text = $"{name} : {message}";
    }
    #endregion
}
