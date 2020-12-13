using Mirror;
using UnityEngine;
using Valve.VR;

public class NetParticipant : NetworkBehaviour
{
    // Helper variable for coordinating spawns and input.
    public static int connectionID;
    public SteamVR_Action_Boolean rightHandRightResponse;
    public SteamVR_Action_Boolean rightHandLeftResponse;

    private void Start()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        // Get the connection ID of this participant.
        connectionID = NetworkClient.connection.connectionId;
        Debug.Log("connectionID: " + connectionID);
        
        // Set position.
        transform.position = connectionID == NetworkManagerDobby.leftConnection
            ? new Vector3(-2, 0, -5)
            : new Vector3(2, 0, -5);
    }

    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        
        // "B" button on right Oculus controller.
        if ((rightHandLeftResponse.state && connectionID == NetworkManagerDobby.leftConnection && (ExperimentManager.trialID == 0 || ExperimentManager.trialID == 1)))
        {
            CmdLeftResponse();
        }
        // "A" button on right Oculus controller.
        else if ((rightHandRightResponse.state && connectionID == NetworkManagerDobby.rightConnection && (ExperimentManager.trialID == 2 || ExperimentManager.trialID == 3)))
        {
            CmdRightResponse();
        }
        
    }

    [Command]
    private void CmdLeftResponse()
    {
        ExperimentManager.leftResponseGiven = true;
        ExperimentManager.leftReady = true;
    }

    [Command]
    private void CmdRightResponse()
    {
        ExperimentManager.rightResponseGiven = true;
        ExperimentManager.rightReady = true;
    }
}
