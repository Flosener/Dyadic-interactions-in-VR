using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class LocalParticipant : MonoBehaviour
{
    public SteamVR_Action_Vector2 input;
    public float speed = 1f;
    private UnityEngine.CharacterController _charController;

    private void Start()
    {
        // Get CharacterController for movement.
        _charController = GetComponent<UnityEngine.CharacterController>();
        // Set position back in the hall/room.
        transform.position = new Vector3(0,0,-5);
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        // Checks whether there is actual input to get teleporting working with joystick movement.
        if (input.axis.magnitude > 0.1f)
        {
            // Move according to direction player is looking at.
            Vector3 direction = Player.instance.hmdTransform.TransformDirection(new Vector3(input.axis.x, 0, input.axis.y));
            _charController.Move(speed * Time.deltaTime * Vector3.ProjectOnPlane(direction, Vector3.up) - new Vector3(0,9.81f,0 * Time.deltaTime));
        }
    }
}
