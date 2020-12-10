using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR.InteractionSystem;

/*
    This class is used for the UI in the EntranceHall to join the different experiments.
*/

public class UIOptions : MonoBehaviour
{
    // Declare variables.
    private GameObject _networkManager;
    private NetworkManager _manager;
    public static string experimentID;
    public static bool isHost;

    // Join experiment 1: Individual two choice task.
    public void JoinIndividual2Choice()
    {
        experimentID = "Individual_TwoChoice";
        SceneManager.LoadScene("ExperimentRoomIndividual");
    }
    
    // Join experiment 2: Individual Go-NoGo task.
    public void JoinIndividualGoNoGo()
    {
        experimentID = "Individual_GoNoGo";
        SceneManager.LoadScene("ExperimentRoomIndividual");
    }
    
    // Join experiment 3: Joint Go-NoGo task as host.
    public void JoinJointGoNoGoAsHost()
    {
        isHost = true;
        experimentID = "Joint_GoNoGo";
        SceneManager.LoadScene("ExperimentRoomJoint");
    }
    
    // Join experiment 3: Joint Go-NoGo task as client.
    public void JoinJointGoNoGoAsClient()
    {
        isHost = false;
        experimentID = "Joint_GoNoGo";
        SceneManager.LoadScene("ExperimentRoomJoint");
    }
}
