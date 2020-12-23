using Mirror;
using UnityEngine;

// Custom NetworkManager to assign spawnpoints correctly to both participants.
[AddComponentMenu("")]

public class NetworkManagerDobby : NetworkManager
{
    private GameObject _instructionsLeft;
    private GameObject _instructionsRight;
    private NetExperimentManager _expManager;

    public override void Start()
    {
        // When joining the networked experiment, spawn both participants.
        if (UIOptions.experimentID == "Joint_GoNoGo")
        {
            if (UIOptions.isHost)
            {
                StartHost();
                Debug.Log("Started as host.");
            }
            else
            {
                StartClient();
                Debug.Log("Started as client.");
            }
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        // Spawn NetParticipant when client is joining the server (isReady is set to true).
        Vector3 position = numPlayers == 0 ? new Vector3(-1.5f, 0, -5) : new Vector3(1.5f, 0, -5);
        GameObject participant = Instantiate(playerPrefab, position, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, participant);
        
        // Spawn UI and tell ExperimentManager that spawning is done.
        if (numPlayers == 2)
        {
            _instructionsLeft = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "InstructionsUILeft"));
            NetworkServer.Spawn(_instructionsLeft);
            _instructionsRight = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "InstructionsUIRight"));
            NetworkServer.Spawn(_instructionsRight);
            _expManager = GameObject.FindGameObjectWithTag("ExperimentManager").GetComponent<NetExperimentManager>();
            _expManager.RpcSpawningDone();
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        // Destroy instructions when server is shut down.
        if (_instructionsLeft != null)
        {
            NetworkServer.Destroy(_instructionsLeft);
        }
        
        if (_instructionsRight != null)
        {
            NetworkServer.Destroy(_instructionsRight);
        }

        // Call base functionality (actually destroys the player).
        base.OnServerDisconnect(conn);
    }
}
