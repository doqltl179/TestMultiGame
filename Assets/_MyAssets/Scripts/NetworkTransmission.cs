using Mu3Library.Utility;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//-------------------------------------------------------------------------------
// ServerRpc => Client���� ȣ���ϸ� Server���� ����.
// ClientRpc => Server���� ȣ���ϸ� Client���� ����.
//-------------------------------------------------------------------------------
public class NetworkTransmission : NetworkBehaviour {
    public Action<ulong, bool> OnClickReady;



    [ServerRpc(RequireOwnership = false)]
    public void Ready_ServerRpc(ulong from, bool value) {
        Debug.Log($"Ready_ServerRpc. from: {from}, value: {value}");

        Ready_ClientRpc(from, value);
    }

    [ClientRpc]
    public void Ready_ClientRpc(ulong from, bool value) {
        Debug.Log($"Ready_ClientRpc. from: {from}, value: {value}");

        OnClickReady?.Invoke(from, value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendChatToServer_ServerRpc(ulong from, string message) {

    }

    [ClientRpc]
    public void SendChatToClient_ClientRpc(ulong from, string message) {

    }
}
