using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkCharacterPropertyUpdateHelper : NetworkBehaviour {
    [SerializeField] private NetworkCharacterController player;

    public NetworkVariable<Vector3> Net_Pos = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Quaternion> Net_Rot = new NetworkVariable<Quaternion>(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Vector3> Net_Scale = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> Net_MoveBlend = new NetworkVariable<float>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);



    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();

        if(!IsOwner) {
            Net_Pos.OnValueChanged += Net_PosChanged;
            Net_Rot.OnValueChanged += Net_RotChanged;
            Net_Scale.OnValueChanged += Net_ScaleChanged;
            Net_MoveBlend.OnValueChanged += Net_MoveBlendChanged;
        }
    }

    public override void OnNetworkDespawn() {
        base.OnNetworkDespawn();

        if(!IsOwner) {
            Net_Pos.OnValueChanged -= Net_PosChanged;
            Net_Rot.OnValueChanged -= Net_RotChanged;
            Net_Scale.OnValueChanged -= Net_ScaleChanged;
            Net_MoveBlend.OnValueChanged -= Net_MoveBlendChanged;
        }
    }

    private void Awake() {
        if(player == null) player = GetComponent<NetworkCharacterController>();
    }

    //private void Start() {
    //    if(!IsOwner) {
    //        Net_Pos.OnValueChanged += Net_PosChanged;
    //        Net_Rot.OnValueChanged += Net_RotChanged;
    //        Net_Scale.OnValueChanged += Net_ScaleChanged;
    //        Net_MoveBlend.OnValueChanged += Net_MoveBlendChanged;
    //    }
    //}

    //public override void OnDestroy() {
    //    if(!IsOwner) {
    //        Net_Pos.OnValueChanged -= Net_PosChanged;
    //        Net_Rot.OnValueChanged -= Net_RotChanged;
    //        Net_Scale.OnValueChanged -= Net_ScaleChanged;
    //        Net_MoveBlend.OnValueChanged -= Net_MoveBlendChanged;
    //    }

    //    base.OnDestroy();
    //}

    #region Utility
    public void UpdatePropertyAll() {
        Net_Pos.Value = player.Pos;
        Net_Rot.Value = player.Rot;
        Net_Scale.Value = player.transform.localScale;
        Net_MoveBlend.Value = player.MoveBlend;
    }
    #endregion

    #region Action
    private void Net_PosChanged(Vector3 oldValue, Vector3 newValue) {
        player.Pos = newValue;
    }

    private void Net_RotChanged(Quaternion oldValue, Quaternion newValue) {
        player.Rot = newValue;
    }

    private void Net_ScaleChanged(Vector3 oldValue, Vector3 newValue) {
        player.transform.localScale = newValue;
    }

    private void Net_MoveBlendChanged(float oldValue, float newValue) {
        player.MoveBlend = newValue;
    }
    #endregion
}
