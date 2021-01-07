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
    
    // UI to get IP from host and type in IP in client app.
    #pragma warning disable 649
    [SerializeField] private GameObject _inputField;
    [SerializeField] private GameObject _joinButton;
    [SerializeField] private GameObject _ipField;
    #pragma warning restore 649

    public override void Start()
    {
        // When joining the networked experiment, spawn both participants.
        if (UIOptions.experimentID == "Joint_GoNoGo")
        {
            if (UIOptions.isHost)
            {
                // Get the IP from local network and display it.
                _ipField.SetActive(true);
                _ipField.GetComponent<Text>().text = DisplayIPAddress();
                
                // Start the host.
                networkAddress = "localhost";
                StartHost();
                Debug.Log("Started as host.");
            }
            else
            {
                _inputField.SetActive(true);
                _joinButton.SetActive(true);
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
        // Set IP UI false.
        _ipField.SetActive(false);
        _inputField.SetActive(false);
        _joinButton.SetActive(false);

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

    // UI: IP4 input request -> button click "Join" -> network address is now the input, client is started, UI set to false afterwards.
    public void GetIPAddress()
    {
        networkAddress = _inputField.transform.Find("Text").GetComponent<Text>().text;
        StartClient();
        Debug.Log("Started as client.");
        _inputField.SetActive(false);
        _joinButton.SetActive(false);
    }
    
    // Origin: pawan Sharma, https://www.codeproject.com/questions/443193/how-to-get-local-ip-address-in-csharp
    private string DisplayIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        var ip4 = "";

        // GetHostEntry() also searches for IPv6 addresses which we do not need.
        foreach (IPAddress ip in host.AddressList)
        {
            // InterNetwork is the address family of IPv4.
            if (ip.AddressFamily.ToString() == "InterNetwork")
            {
                ip4 = ip.ToString();
            }
        }
        
        return ip4;
    }
}
