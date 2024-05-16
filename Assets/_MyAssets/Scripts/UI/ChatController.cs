using Mu3Library.Utility;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatController : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI textObj;
    [SerializeField] private Transform textParent;
    private List<TextMeshProUGUI> messages = new List<TextMeshProUGUI>();



    private void Awake() {
        SteamP2P.Instance.OnChatReceived += OnChatReceived;
    }

    private void OnDestroy() {
        SteamP2P.Instance.OnChatReceived -= OnChatReceived;
    }

    #region Utility
    public void Clear() {
        for(int i = 0; i < messages.Count; i++) {
            UnityObjectPoolManager.Instance.AddObject(messages[i]);
        }
        messages.Clear();
    }
    #endregion

    #region Action
    private void OnChatReceived(string name, string message) {
        TextMeshProUGUI text = UnityObjectPoolManager.Instance.GetObject<TextMeshProUGUI>();
        if(text == null) {
            text = Instantiate(textObj);
        }
        text.transform.SetParent(textParent);
        text.gameObject.SetActive(true);

        text.text = $"{name}: {message}";
    }
    #endregion
}
