using Mu3Library.Scene;
using Mu3Library.Utility;
using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MainLayer_Lobby : SceneUILayer {
    [SerializeField] private Transform lobbyMemberIconParent;
    [SerializeField] private LobbyMemberIcon lobbyMemberIconObj;
    private Dictionary<ulong, LobbyMemberIcon> lobbyMembers = new Dictionary<ulong, LobbyMemberIcon>();



    public override void OnActivate() {
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
    }

    public override void OnDeActivate() {
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;

        if(lobbyMembers != null && lobbyMembers.Count > 0) {
            foreach(var key in lobbyMembers.Keys) {
                UnityObjectPoolManager.Instance.AddObject(lobbyMembers[key]);
            }

            lobbyMembers = new Dictionary<ulong, LobbyMemberIcon>();
        }
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend info) {
        AddLobbyMember(info);
    }

    private void OnLobbyMemberLeave(Lobby lobby, Friend info) {
        RemoveLobbyMember(info);
    }

    #region Utility
    public async void SetLobbyMembers(Friend[] members) {
        if(lobbyMembers != null && lobbyMembers.Count > 0) {
            foreach(var key in lobbyMembers.Keys) {
                UnityObjectPoolManager.Instance.AddObject(lobbyMembers[key]);
            }

            lobbyMembers = new Dictionary<ulong, LobbyMemberIcon>();
        }

        for(int i = 0; i < members.Length; i++) {
            await AddLobbyMember(members[i]);
        }
    }

    public async Task AddLobbyMember(Friend member) {
        LobbyMemberIcon temp;
        if(lobbyMembers.TryGetValue(member.Id, out temp)) {
            Debug.Log($"This member already joined. name: {member.Name}");

            return;
        }

        LobbyMemberIcon icon = UnityObjectPoolManager.Instance.GetObject<LobbyMemberIcon>();
        if(icon == null) {
            icon = Instantiate(lobbyMemberIconObj);
        }
        icon.transform.SetParent(lobbyMemberIconParent);
        icon.gameObject.SetActive(true);

        icon.transform.localScale = Vector3.one;

        await icon.SetIcon(member);

        lobbyMembers.Add(member.Id, icon);
    }

    public void RemoveLobbyMember(Friend member) {
        LobbyMemberIcon icon;
        if(lobbyMembers.TryGetValue(member.Id, out icon)) {
            UnityObjectPoolManager.Instance.AddObject(icon);

            lobbyMembers.Remove(member.Id);
        }
        else {
            Debug.Log($"Member not found. name: {member.Name}");
        }
    }
    #endregion
}
