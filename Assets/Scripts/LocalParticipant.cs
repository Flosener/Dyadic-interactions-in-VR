using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class LocalParticipant : MonoBehaviour
{
    public SteamVR_Action_Vector2 input;
    public float speed = 1f;
    private UnityEngine.CharacterController charController;

    private void Start()
    {
        charController = gameObject.GetComponent<UnityEngine.CharacterController>();
    }

    private void Update()
    {
        if (input.axis.magnitude > 0.1f)
        {
            Vector3 direction = Player.instance.hmdTransform.TransformDirection(new Vector3(input.axis.x, 0, input.axis.y));
            charController.Move(speed * Time.deltaTime * Vector3.ProjectOnPlane(direction, Vector3.up) - new Vector3(0,9.81f,0) * Time.deltaTime);
        }
    }
}
