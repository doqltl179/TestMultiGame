using Mu3Library;
using Mu3Library.Utility;
using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MainUI : MonoBehaviour {
    [SerializeField] private Transform lobbyListRect;
    [SerializeField] private LobbyIcon lobbyIconObj;
    private LobbyIcon[] lobbyIcons;



    private IEnumerator Start() {
        yield return null;

        LoadAllLobby();
    }

    private async void LoadAllLobby() {
        if(lobbyIcons != null && lobbyIcons.Length > 0) {
            for(int i = 0; i < lobbyIcons.Length; i++) {
                UnityObjectPoolManager.Instance.AddObject(lobbyIcons[i]);
            }
        }
        lobbyIcons = null;

        Lobby[] lobbies = await SteamMatchmaking.LobbyList.RequestAsync();
        if(lobbies != null && lobbies.Length > 0) {
            lobbyIcons = new LobbyIcon[lobbies.Length];
            for(int i = 0; i < lobbyIcons.Length; i++) {
                LobbyIcon icon = UnityObjectPoolManager.Instance.GetObject<LobbyIcon>();
                if(icon == null) {
                    icon = Instantiate(lobbyIconObj);
                }

                icon.transform.SetParent(lobbyListRect);
                icon.transform.localScale = Vector3.one;
                icon.gameObject.SetActive(true);

                Lobby lobby = lobbies[i];
                icon.SetIcon(lobby, () => {
                    StringBuilder sb = new StringBuilder();
                    foreach(var item in lobby.Data) {
                        sb.AppendLine($"Key: {item.Key} || Value: {item.Value}");
                    }
                    Debug.Log(sb.Length > 0 ? sb.ToString() : "Data not exist.");
                    Network_Join(lobby);
                });

                lobbyIcons[i] = icon;
            }
        }
    }

    private async void Network_Join(Lobby lobby) {
        RoomEnter enter = await lobby.Join();
        if(enter != RoomEnter.Success) {
            Debug.Log($"Room join failed. Result: {enter}, LobbyID: {lobby.Id}, OwnerName: {lobby.Owner.Name}, OwnerID: {lobby.Owner.Id}");

            return;
        }

        Debug.Log($"Room join success. LobbyID: {lobby.Id}, OwnerName: {lobby.Owner.Name}, OwnerID: {lobby.Owner.Id}");

        //Lobby? l = await SteamMatchmaking.JoinLobbyAsync(lobby.Owner.Id);
        //if(l != null) {
        //    Debug.Log($"Room join success. LobbyID: {l.Value.Id}, OwnerName: {l.Value.Owner.Name}, OwnerID: {l.Value.Owner.Id}");
        //}
    }

    #region Action
    public void Network_StartHost(int maxMembers) {
        GameNetworkManager.Instance.StartHost(maxMembers, (value) => {
            if(value) {
                SceneLoader.Instance.LoadScene(
                    SceneType.Lobby,
                    () => {
                        LoadingPanel.Instance.SetActive(true, 0.5f);
                        LoadingPanel.Instance.UpdateProgress();
                    },
                    () => {
                        LoadingPanel.Instance.SetActive(false, 0.5f);
                        LoadingPanel.Instance.StopProgressUpdate();
                    });
            }
            else {
                Debug.Log("Failed create lobby.");
            }
        });
    }

    public void Network_Server(int maxMembers) {
        GameNetworkManager.Instance.StartServer(maxMembers, (value) => {
            if(value) {
                SceneLoader.Instance.LoadScene(
                    SceneType.Lobby,
                    () => {
                        LoadingPanel.Instance.SetActive(true, 0.5f);
                        LoadingPanel.Instance.UpdateProgress();
                    },
                    () => {
                        LoadingPanel.Instance.SetActive(false, 0.5f);
                        LoadingPanel.Instance.StopProgressUpdate();
                    });
            }
            else {
                Debug.Log("Failed create lobby.");
            }
        });
    }

    public void Refresh() {
        LoadAllLobby();
    }
    #endregion
}