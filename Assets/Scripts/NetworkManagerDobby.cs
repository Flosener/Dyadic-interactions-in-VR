using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

// Custom networkmanager to assign spawnpoints correctly to both participants.
[AddComponentMenu("")]

public class NetworkManagerDobby : NetworkManager
{
    public Transform leftParticipantSpawn;
    public Transform rightParticipantSpawn;
    public GameObject menu;

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        // add participant at correct spawn position
        Transform start = numPlayers % 2 != 0 ? leftParticipantSpawn : rightParticipantSpawn;
        GameObject participant = Instantiate(playerPrefab, start.position, start.rotation);
        NetworkServer.AddPlayerForConnection(conn, participant);
        Debug.Log("Player spawned");

        // spawn menu if two participants join, for them to be able to enter the experiment room
        if (numPlayers == 2)
        {
            menu = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "UI"));
            NetworkServer.Spawn(menu);
        }
    }
    
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        // destroy menu
        if (menu != null)
            NetworkServer.Destroy(menu);

        // call base functionality (actually destroys the player)
        base.OnServerDisconnect(conn);
    }
}
