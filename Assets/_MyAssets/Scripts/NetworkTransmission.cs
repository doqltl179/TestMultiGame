using Steamworks;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkTransmission : NetworkBehaviour {
    public static NetworkTransmission Instance {
        get {
            if(instance == null) {
                instance = FindObjectOfType<NetworkTransmission>();
            }

            return instance;
        }
    }
    private static NetworkTransmission instance;

    [SerializeField] private ChatController chatController;



    [ServerRpc(RequireOwnership = false)]
    public void IWishToSendAChatServerRPC(string message, ulong fromWho) {
        ChatFromServerClientRPC(message, fromWho);
    }

    [ClientRpc]
    private void ChatFromServerClientRPC(string message, ulong fromWho) {
        chatController.SendChat(message, fromWho, false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddMeToDictionaryServerRPC(ulong steamId, string steamName, ulong clientId) {
        Debug.Log($"AddMeToDictionaryServerRPC. steamId: {steamId}, name: {steamName}, clientId: {clientId}");
        chatController.SendChat($"[{steamName}] has joined", clientId, true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveMeFromDictionaryServerRPC(ulong steamId) {
        Debug.Log($"RemoveMeFromDictionaryServerRPC. steamId: {steamId}");
        RemovePlayerFromDictionaryClientRPC(steamId);
    }

    [ClientRpc]
    private void RemovePlayerFromDictionaryClientRPC(ulong steamId) {
        Debug.Log("Removing client");
    }

    [ClientRpc]
    public void UpdateClientsPlayerInfoClientRPC(ulong steamId, string steamName, ulong clientId) {
        Debug.Log($"UpdateClientsPlayerInfoClientRPC. steamId: {steamId}, name: {steamName}, clientId: {clientId}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void IsTheClientReadyServerRPC(bool ready, ulong clientId) {
        Debug.Log($"IsTheClientReadyServerRPC. ready: {ready}, clientId: {clientId}");
        AClientMightBeReadyClientRPC(ready, clientId);
    }

    [ClientRpc]
    private void AClientMightBeReadyClientRPC(bool ready, ulong clientId) {
        Debug.Log($"AClientMightBeReadyClientRPC. ready: {ready}, clientId: {clientId}");
    }
}
