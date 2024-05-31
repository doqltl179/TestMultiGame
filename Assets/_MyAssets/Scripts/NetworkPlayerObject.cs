using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class NetworkPlayerObject : NetworkBehaviour {
    private Friend? owner = null;

    public ulong OwnerId { get; private set; }
    private NetworkVariable<ulong> ownerId = new NetworkVariable<ulong>(9999, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public string OwnerName { get; private set; }
    private NetworkVariable<FixedString128Bytes> ownerName = new NetworkVariable<FixedString128Bytes>(new FixedString128Bytes(""), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public bool IsReady { get; private set; }
    private NetworkVariable<bool> isReady = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public Action<ulong, bool> OnReadyChanged;



    public override void OnNetworkSpawn() {
        ownerId.OnValueChanged += OnOwnerIdChangedFunc;
        ownerName.OnValueChanged += OnOwnerNameChangedFunc;
        isReady.OnValueChanged += OnReadyChangedFunc;
    }

    public override void OnNetworkDespawn() {
        if(IsOwner) {
            owner = null;

            ownerId.Value = 9999;
            ownerName.Value = new FixedString128Bytes("");

            isReady.Value = false;
        }

        ownerId.OnValueChanged -= OnOwnerIdChangedFunc;
        ownerName.OnValueChanged -= OnOwnerNameChangedFunc;
        isReady.OnValueChanged -= OnReadyChangedFunc;
    }

    #region Utility
    public void Init(Friend? id) {
        if(id != null) {
            ownerId.Value = id.Value.Id;
            ownerName.Value = new FixedString128Bytes(id.Value.Name);
        }
        isReady.Value = false;

        owner = id;
    }
    #endregion

    #region Action
    private void OnOwnerIdChangedFunc(ulong oldValue, ulong newValue) {
        OwnerId = newValue;
    }

    private void OnOwnerNameChangedFunc(FixedString128Bytes oldValue, FixedString128Bytes newValue) {
        OwnerName = newValue.ToString();
    }

    private void OnReadyChangedFunc(bool oldValue, bool newValue) {
        IsReady = newValue;

        OnReadyChanged?.Invoke(OwnerId, newValue);
    }
    #endregion
}
