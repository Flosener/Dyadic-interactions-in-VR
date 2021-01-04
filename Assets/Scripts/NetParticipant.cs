using Mirror;
using UnityEngine;
using Valve.VR;

/*
 * A lot of this code is used from a Unity forum thread: https://forum.unity.com/threads/multiplayer-with-steamvr.535321/
 * (conversation between "csofranz" and "Giantbean").
 */

public class NetParticipant : NetworkBehaviour
{
    // Helper variable for coordinating spawns and input.
    [SerializeField] private SteamVR_Action_Boolean _rightHandRightResponse;
    [SerializeField] private SteamVR_Action_Boolean _rightHandLeftResponse;
    private NetExperimentManager _expManager;
 
    // Source GameObjects head + hands of local player.
    private GameObject _localPlayer;
    private GameObject _headlessPlayer;
    private GameObject _localHead;
    private GameObject _localLeftHand;
    private GameObject _localRightHand;
 
    // Player parts viewable to others and hidden from the local player.
    #pragma warning disable 649
    [SerializeField] private GameObject _netHeadObj;
    [SerializeField] private GameObject _netLeftCtrl;
    [SerializeField] private GameObject _netRightCtrl;
    #pragma warning restore 649
 
    // Objects tracked for server synchronization
    private SteamVR_TrackedObject _trackedObjHead;
    private SteamVR_TrackedObject _trackedObjRight;
    private SteamVR_TrackedObject _trackedObjLeft;
    
    public override void OnStartLocalPlayer()
    {
        // Find the gaming rig in the scene and link to it.
        if (_localPlayer == null)
        {
            _localPlayer = GameObject.Find("SteamVRObjects");

            if (_localPlayer == null)
            {
                // Prevent headless version of app from crashing.
                _headlessPlayer = GameObject.Find("NoSteamVRFallbackObjects");
 
                // When running as headless, provide default non-moving fallback objects instead.
                _localHead = _headlessPlayer.transform.Find("FallbackObjects").gameObject;
                _localLeftHand = _headlessPlayer.transform.Find("FallbackHand").gameObject;
                _localRightHand = _headlessPlayer.transform.Find("FallbackHand").gameObject;
                Debug.Log("HEADLESS detected");
            }
        }

        // Get position and rotation from local player head + hands.
        if (_localPlayer != null)
        {
            _localHead = _localPlayer.transform.Find("VRCamera").gameObject;
            _localLeftHand = _localPlayer.transform.Find("LeftHand").gameObject;
            _localRightHand = _localPlayer.transform.Find("RightHand").gameObject;
 
            _trackedObjRight = _localRightHand.GetComponent<SteamVR_TrackedObject>();
            _trackedObjLeft = _localLeftHand.GetComponent<SteamVR_TrackedObject>(); 
        }
    }
    
 
    private void Start()
    {
        // Disable mesh renderers for head + hands of netObject for local player to prevent overlap with localPlayer.
        if (isLocalPlayer)
        {
            _netHeadObj.transform.Find("SortingHat").GetComponent<MeshRenderer>().enabled = false;
            _netLeftCtrl.transform.Find("vr_glove_left_model_slim").transform.Find("slim_l").transform.Find("vr_glove_right_slim").GetComponent<SkinnedMeshRenderer>().enabled = false;
            _netRightCtrl.transform.Find("vr_wand").transform.Find("slim_r").transform.Find("vr_glove_right_slim").GetComponent<SkinnedMeshRenderer>().enabled = false;
            _netRightCtrl.transform.Find("vr_wand").transform.Find("Wand4").gameObject.SetActive(false);
        }
        
        // Get ExperimentManager.
        if (_expManager == null)
        {
            _expManager = GameObject.FindGameObjectWithTag("ExperimentManager").GetComponent<NetExperimentManager>();
        }
    }
 
    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        // Get response from participant.
        GetResponse();
        
        // Synchronize position on network.
        UpdateHeadAndHands();
    }

    private void GetResponse()
    {
        // DEBUG: Change back input source.
        
        // "B" button on right Oculus controller.
        // If host gives left response on green trial get RT and send response to server.
        if (Input.GetKeyDown(KeyCode.F) && UIOptions.isHost && (_expManager.trialID == -1 || _expManager.trialID == 0 || _expManager.trialID == 1 || _expManager.trialID == 4))
        {
            _expManager.CmdReactionTime();
            _expManager.CmdLeftResponse();
            Debug.Log("Left response given");
        }
        // "A" button on right Oculus controller.
        // If client gives right response on red trial get RT and send response to server.
        else if (Input.GetKeyDown(KeyCode.J) && !UIOptions.isHost && (_expManager.trialID == -1 || _expManager.trialID == 2 || _expManager.trialID == 3 || _expManager.trialID == 5))
        {
            _expManager.CmdReactionTime();
            _expManager.CmdRightResponse();
            Debug.Log("Right response given");
        }
    }
    
    private void UpdateHeadAndHands()
    {
        if (!isLocalPlayer)
        {
            // Do nothing, net transform does all the work for us.
        }
        else
        {
            // We are the local player.
            // Copy the values from the Rig's parts so they can be used for positioning the online presence.
            _netHeadObj.transform.position = _localHead.transform.position;
            _netHeadObj.transform.rotation = _localHead.transform.rotation;
 
            if (_localLeftHand)
            {
                // We need to check in case player left the hand unconnected.
                _netLeftCtrl.transform.position = _localLeftHand.transform.position;
                _netLeftCtrl.transform.rotation = _localLeftHand.transform.rotation;
            }
 
            if (_localRightHand)
            {
                // Only if right hand is connected.
                _netRightCtrl.transform.position = _localRightHand.transform.position;
                _netRightCtrl.transform.rotation = _localRightHand.transform.rotation;
            }
        }
    }
}
