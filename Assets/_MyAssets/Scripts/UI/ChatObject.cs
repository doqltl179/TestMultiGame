using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatObject : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI text;
    public Color TextColor {
        get => text.color;
        set => text.color = value;
    }
    public string Text {
        get => text.text;
        set => text.text = value;
    }
    public float Width => text.preferredWidth;
}
