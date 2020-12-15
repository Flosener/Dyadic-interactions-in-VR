using Mirror;
using UnityEngine;

// Custom NetworkManager to assign spawnpoints correctly to both participants.
[AddComponentMenu("")]

public class NetworkManagerDobby : NetworkManager
{
    private GameObject _experimentManager;
    private GameObject _instructionsLeft;
    private GameObject _instructionsRight;
    private NetworkConnection _leftConnection;
    private NetworkConnection _rightConnection;

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        // Spawn NetParticipant when client is joining the server (isReady is set to true).
        Vector3 position = numPlayers == 0 ? new Vector3(-1.5f, 0, -5) : new Vector3(1.5f, 0, -5);
        GameObject participant = Instantiate(playerPrefab, position, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, participant);
        
        // Save network connections of both participants for UI auth, spawn UI and tell ExperimentManager that spawning is done.
        switch (numPlayers)
        {
            case 1:
                _leftConnection = conn;
                break;
            case 2:
                _rightConnection = conn;
                _experimentManager = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "NetExperimentManager"));
                NetworkServer.Spawn(_experimentManager);
                _instructionsLeft = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "InstructionsUILeft"));
                NetworkServer.Spawn(_instructionsLeft, _leftConnection);
                _instructionsRight = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "InstructionsUIRight"));
                NetworkServer.Spawn(_instructionsRight, _rightConnection);
                NetExperimentManager.spawningDone = true;
                break;
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        // destroy instructions
        if (_instructionsLeft != null)
        {
            NetworkServer.Destroy(_instructionsLeft);
        }
        
        if (_instructionsRight != null)
        {
            NetworkServer.Destroy(_instructionsRight);
        }

        // call base functionality (actually destroys the player)
        base.OnServerDisconnect(conn);
    }
}
