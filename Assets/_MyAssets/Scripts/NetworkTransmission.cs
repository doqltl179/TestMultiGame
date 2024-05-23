using Mu3Library.Utility;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//-------------------------------------------------------------------------------
// ServerRpc => Client에서 호출하며 Server에서 실행.
// ClientRpc => Server에서 호출하며 Client에서 실행.
//-------------------------------------------------------------------------------
public class NetworkTransmission : NetworkBehaviour {
    public static NetworkTransmission Instance {
        get {
            if(instance == null) {
                NetworkTransmission pm = FindObjectOfType<NetworkTransmission>();
                if(pm == null) {
                    GameObject resource = ResourceLoader.GetResource<GameObject>($"Network/{nameof(NetworkTransmission)}");
                    if(resource != null) {
                        GameObject go = Instantiate(resource);
                        DontDestroyOnLoad(go);

                        pm = go.GetComponent<NetworkTransmission>();
                    }
                }

                instance = pm;
            }

            return instance;
        }
    }
    private static NetworkTransmission instance;

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
