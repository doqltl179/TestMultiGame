using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbyMemberIcon : MonoBehaviour {
    public Friend Info { get; private set; }

    [Space(20)]
    [SerializeField] private Image thumb;

    [Space(20)]
    [SerializeField] private TextMeshProUGUI infoName;

    [Space(20)]
    [SerializeField] private GameObject readyMark;



    #region Utility
    public async Task SetIcon(Friend info, bool ready = false) {
        thumb.sprite = await GameNetworkManager.Instance.GetThumbSprite(info.Id);

        infoName.text = info.Name;

        readyMark.SetActive(ready);

        Info = info;
    }
    #endregion
}
