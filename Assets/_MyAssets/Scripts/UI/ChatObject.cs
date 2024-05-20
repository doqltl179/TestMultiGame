using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatObject : MonoBehaviour {
    private RectTransform rectTransform;

    [SerializeField] private TextMeshProUGUI text;
    public Color TextColor {
        get => text.color;
        set => text.color = value;
    }
    public string Text {
        get => text.text;
        set {
            text.text = value;

            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, text.preferredHeight + 8);
        }
    }
    public float Width => text.preferredWidth;
    public float Height => text.preferredHeight;



    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }
}
