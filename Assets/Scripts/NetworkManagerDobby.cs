using Mirror;
using UnityEngine;

// Custom NetworkManager to assign spawnpoints correctly to both participants.
[AddComponentMenu("")]

public class NetworkManagerDobby : NetworkManager
{
    public static bool spawningDone;
    
    // Initialize connections with -1 (for input authority in NetParticipant).
    public static int leftConnection = -1;
    public static int rightConnection = -1;
    
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        // Spawn NetParticipant when client is joining the server (isReady is set to true).
        GameObject participant = Instantiate(playerPrefab);
        NetworkServer.AddPlayerForConnection(conn, participant);
        Debug.Log("Spawned client with connectionID: " + conn.connectionId);

        // Save network connections of both participants for participant-specific input authority and tell ExperimentManager that spawning is done.
        switch (numPlayers)
        {
            case 1:
                leftConnection = conn.connectionId;
                break;
            case 2:
                rightConnection = conn.connectionId;
                spawningDone = true;
                break;
        }
    }
}
