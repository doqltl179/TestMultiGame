using Steamworks;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//-------------------------------------------------------------------------------
// ServerRpc => Client���� ȣ���ϸ� Server���� ����.
// ClientRpc => Server���� ȣ���ϸ� Client���� ����.
//-------------------------------------------------------------------------------
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



    [ServerRpc(RequireOwnership = false)]
    public void Ready_ServerRpc(ulong from, bool value) {
        Debug.Log($"Ready_ServerRpc. from: {from}, value: {value}");

        Ready_ClientRpc(from, value);
    }

    [ClientRpc]
    public void Ready_ClientRpc(ulong from, bool value) {
        Debug.Log($"Ready_ClientRpc. from: {from}, value: {value}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendChatToServer_ServerRpc(ulong from, string message) {

    }

    [ClientRpc]
    public void SendChatToClient_ClientRpc(ulong from, string message) {

    }
}
