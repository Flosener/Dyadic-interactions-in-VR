using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;
using Valve.VR.InteractionSystem;
using Random = UnityEngine.Random;

public class NetParticipant : NetworkBehaviour
{
    // Helper variable for coordinating spawns and input with the experiment manager.
    public static int connectionID;

    private void Start()
    {
        // Get the connection ID of this participant.
        connectionID = NetworkClient.connection.connectionId;
        Debug.Log("connectionID: " + connectionID);
    }
    
    // Remark: As the NetParticipant only exists in the Joint experiment room,
    // we do not have to implement any further movement and spawnpositions are handled by the NetworkManager.
}
