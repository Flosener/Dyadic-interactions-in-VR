using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

// Custom NetworkManager to assign spawnpoints correctly to both participants.
[AddComponentMenu("")]

public class NetworkManagerDobby : NetworkManager
{
    // Declare spawn-related variables.
    private Transform _leftParticipantSpawn;
    private Transform _rightParticipantSpawn;
    private GameObject _instructionsUI;
    public static bool spawningDone;
    
    // Initialize connections with -1 (for input authority in NetParticipant).
    public static int leftConnection = -1;
    public static int rightConnection = -1;

    public override void Start()
    {
        // Initialize SpawnPositions.
        _leftParticipantSpawn = new GameObject().transform;
        _rightParticipantSpawn = new GameObject().transform;
        _leftParticipantSpawn.position = new Vector3(-2,0,-5);
        _leftParticipantSpawn.rotation = new Quaternion(0,0,0,1);
        _rightParticipantSpawn.position = new Vector3(2,0,-5);
        _rightParticipantSpawn.rotation = new Quaternion(0,0,0,1);
    }

    // Spawn NetParticipant when client is joining the server.
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        // Add participant at correct spawn position.
        Transform start = numPlayers == 0 ? _leftParticipantSpawn : _rightParticipantSpawn;
        GameObject participant = Instantiate(playerPrefab, start.position, start.rotation);
        NetworkServer.AddPlayerForConnection(conn, participant);

        // Save network connections of both participants for participant-specific input authority.
        if (numPlayers == 1)
        {
            leftConnection = conn.connectionId;
        }
        else
        {
            rightConnection = conn.connectionId;
        }

        // Spawn menu if two participants join to read instructions in joint Go-NoGo room.
        // after Lab check: numPlayers == 2
        if (numPlayers == 1)
        {
            _instructionsUI = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "InstructionsUI"));
            NetworkServer.Spawn(_instructionsUI);
            Debug.Log("instructions spawned");
        }
        
        // Tell ExperimentManager that spawning of all objects is done.
        spawningDone = true;
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        // Destroy UI when server disconnects.
        if (_instructionsUI != null)
            NetworkServer.Destroy(_instructionsUI);

        // Call base functionality (actually destroys the player).
        base.OnServerDisconnect(conn);
    }
}
