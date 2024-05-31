using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerObject : NetworkBehaviour {
    public Friend Owner { get; private set; }

    public NetworkVariable<bool> IsReady = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public Action<bool> OnReadyChanged;



    public override void OnNetworkSpawn() {
        IsReady.OnValueChanged += ReadyChanged;
    }

    public override void OnNetworkDespawn() {
        IsReady.OnValueChanged -= ReadyChanged;
    }

    #region Utility
    public void Init(Friend owner) {
        Owner = owner;
    }

    public void SetReady(bool value) {
        if(!IsOwner) return;

        IsReady.Value = value;
    }
    #endregion

    #region Action
    private void ReadyChanged(bool oldValue, bool newValue) {
        OnReadyChanged?.Invoke(newValue);
    }
    #endregion
}
