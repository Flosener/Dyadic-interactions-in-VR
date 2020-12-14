using Mirror;
using UnityEngine;

// Custom NetworkManager to assign spawnpoints correctly to both participants.
[AddComponentMenu("")]

public class NetworkManagerDobby : NetworkManager
{
    private GameObject _instructions;
    private NetworkConnection _leftConnection;
    private NetworkConnection _rightConnection;

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        // Spawn NetParticipant when client is joining the server (isReady is set to true).
        Vector3 position = numPlayers == 0 ? new Vector3(-1.5f, 0, -5) : new Vector3(1.5f, 0, -5);
        GameObject participant = Instantiate(playerPrefab, position, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, participant);
        
        Debug.LogWarning(participant.transform.position);
        
        // Save network connections of both participants for participant-specific input authority and tell ExperimentManager that spawning is done.
        switch (numPlayers)
        {
            case 1:
                _leftConnection = conn;
                break;
            case 2:
                _rightConnection = conn;
                _instructions = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "InstructionsUI"));
                NetworkServer.Spawn(_instructions);
                _instructions.GetComponent<NetworkIdentity>().AssignClientAuthority(_leftConnection);
                _instructions.GetComponent<NetworkIdentity>().AssignClientAuthority(_rightConnection);
                NetExperimentManager.spawningDone = true;
                break;
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        // destroy instructions
        if (_instructions != null)
        {
            NetworkServer.Destroy(_instructions);
        }

        // call base functionality (actually destroys the player)
        base.OnServerDisconnect(conn);
    }
}
