using Mu3Library.Character;
using Mu3Library.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardMovement : CharacterState {
    private Vector3 moveDir;
    private float boost;
    private float boostMax;

    private Vector3 cameraLeft;
    private Vector3 cameraRight;
    private Vector3 cameraForward;
    private Vector3 cameraBack;



    public override void Enter() {
        boost = 0.0f;
    }

    public override void Exit() {

    }

    public override void Update() {
        moveDir = Vector3.zero;

        if(KeyCodeInputCollector.Instance.GetKey(character.KeyCode_MoveL)) moveDir += cameraLeft;
        if(KeyCodeInputCollector.Instance.GetKey(character.KeyCode_MoveR)) moveDir += cameraRight;
        if(KeyCodeInputCollector.Instance.GetKey(character.KeyCode_MoveF)) moveDir += cameraForward;
        if(KeyCodeInputCollector.Instance.GetKey(character.KeyCode_MoveB)) moveDir += cameraBack;

        boostMax = KeyCodeInputCollector.Instance.GetKey(character.KeyCode_Run) ? 1.0f : 0.5f;

        if(moveDir.magnitude > 0) {
            boost = Mathf.Lerp(boost, boostMax, Time.deltaTime * character.MoveBoost);
        }
        else {
            boost = Mathf.Lerp(boost, 0.0f, Time.deltaTime * character.MoveBoost);

            moveDir = character.Forward;
        }

        character.Rot = Quaternion.Lerp(character.Rot, Quaternion.LookRotation(moveDir.normalized, Vector3.up), Time.deltaTime * character.RotateSpeed);

        cameraForward = UtilFunc.GetDirectionXZ(CameraManager.Instance.CamPos, character.Pos).normalized;
        cameraBack = cameraForward * -1;
        cameraRight = Quaternion.AngleAxis(90, Vector3.up) * cameraForward;
        cameraLeft = cameraRight * -1;

        character.Move(character.Forward, Time.deltaTime * character.MoveSpeed * boost);

        character.MoveBlend = boost;
    }
}
