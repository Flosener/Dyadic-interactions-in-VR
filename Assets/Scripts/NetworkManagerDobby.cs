using Mirror;
using UnityEngine;
using System.Net;
using UnityEngine.UI;

// Custom NetworkManager to assign spawnpoints correctly to both participants.
[AddComponentMenu("")]

public class NetworkManagerDobby : NetworkManager
{
    private GameObject _instructionsLeft;
    private GameObject _instructionsRight;
    private NetExperimentManager _expManager;
    [SerializeField] private GameObject _getIPAddress;
    [SerializeField] private GameObject _inputFieldText;

    public override void Start()
    {
        // When joining the networked experiment, spawn both participants.
        if (UIOptions.experimentID == "Joint_GoNoGo")
        {
            // If host, get the IP address and start the host.
            if (UIOptions.isHost)
            {
                networkAddress = "localhost";
                StartHost();
                Debug.Log("Started as host.");
            }
            else
            {
                _getIPAddress.SetActive(true);
                // On UI Button click, client gets started via GetIPAddress() function.
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

    // UI: IP4 input request -> button click "Enter" -> network address is now the input, client is started, UI set to false afterwards.
    public void GetIPAddress()
    {
        networkAddress = _inputFieldText.GetComponent<Text>().text;
        StartClient();
        Debug.Log("Started as client.");
        _getIPAddress.SetActive(false);
    }
}
