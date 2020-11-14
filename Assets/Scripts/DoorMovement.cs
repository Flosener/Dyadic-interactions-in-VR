using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class DoorMovement : MonoBehaviour
{
    public Hand hand;
    public Animator anim;
    private bool _isOpen;
    private bool _isHoveredOver;
    
    void Start()
    {
        // If there is no hand yet, get the hand component from the player.
        if (hand == null)
        {
            hand = GameObject.FindGameObjectWithTag("Player").GetComponent<Hand>();
        }
    }
    
    void Update()
    {
        //if (SteamVR_Input.GetState("GrabGrip", hand.handType) && !_isOpen && _isHoveredOver)
        if (Input.GetMouseButtonDown(0) && !_isOpen && _isHoveredOver)
        {
            anim.Play("openDoor");
            _isOpen = true;
            SoundManager.PlaySound("doorOpen");
        }
        // Later: Door not closed by participant but at the end of the trial.
        //else if (SteamVR_Input.GetState("GrabGrip", hand.handType) && _isOpen && _isHoveredOver)
        else if (Input.GetMouseButtonDown(0) && _isOpen && _isHoveredOver)
        {
            anim.Play("closeDoor");
            _isOpen = false;
            SoundManager.PlaySound("doorClose");
        }
    }

    // Event functions for hovering over the door. Necessary since hovering is a condition for opening/closing the door.
    private void OnHandHoverBegin()
    {
        _isHoveredOver = true;
    }
    
    private void OnHandHoverEnd()
    {
        _isHoveredOver = false;
    }
}
