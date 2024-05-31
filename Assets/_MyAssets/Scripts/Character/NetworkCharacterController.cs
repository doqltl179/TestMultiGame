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



    protected override void Awake() {
        if(networkObject == null) networkObject = GetComponent<NetworkObject>();

        base.Awake();
    }

    protected override void Update() {
        if(IsOwner) {
            base.Update();


        }
    }
}
