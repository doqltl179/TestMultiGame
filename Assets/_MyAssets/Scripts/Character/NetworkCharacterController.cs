using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

using CharacterController = Mu3Library.Character.CharacterController;

[RequireComponent(typeof(NetworkObject))]
public class NetworkCharacterController : CharacterController {
    [Space(20)]
    [SerializeField] private NetworkObject networkObject;
    public bool IsOwner => networkObject.IsOwner;

    public NetworkVariable<Vector3> Net_Pos;
    public NetworkVariable<Quaternion> Net_Rot;
    public NetworkVariable<Vector3> Net_Scale;
    public NetworkVariable<float> Net_MoveBlend;



    protected override void Awake() {
        if(networkObject == null) networkObject = GetComponent<NetworkObject>();

        base.Awake();

        if(!IsOwner) {
            Net_Pos.OnValueChanged += Net_PosChanged;
            Net_Rot.OnValueChanged += Net_RotChanged;
            Net_Scale.OnValueChanged += Net_ScaleChanged;
            Net_MoveBlend.OnValueChanged += Net_MoveBlendChanged;
        }
    }

    protected override void OnDestroy() {
        base.OnDestroy();

        if(!IsOwner) {
            Net_Pos.OnValueChanged -= Net_PosChanged;
            Net_Rot.OnValueChanged -= Net_RotChanged;
            Net_Scale.OnValueChanged -= Net_ScaleChanged;
            Net_MoveBlend.OnValueChanged -= Net_MoveBlendChanged;
        }
    }

    protected override void Update() {
        if(IsOwner) {
            base.Update();

            Net_Pos.Value = transform.position;
            Net_Rot.Value = transform.rotation;
            Net_Scale.Value = transform.localScale;
            Net_MoveBlend.Value = MoveBlend;
        }
    }

    #region Action
    private void Net_PosChanged(Vector3 oldValue, Vector3 newValue) {
        transform.position = newValue;
    }

    private void Net_RotChanged(Quaternion oldValue, Quaternion newValue) {
        transform.rotation = newValue;
    }

    private void Net_ScaleChanged(Vector3 oldValue, Vector3 newValue) {
        transform.localScale = newValue;
    }

    private void Net_MoveBlendChanged(float oldValue, float newValue) {
        MoveBlend = newValue;
    }
    #endregion
}
