using Mirror;
using UnityEngine;
using Valve.VR;

public class NetParticipant : NetworkBehaviour
{
    // Helper variable for coordinating spawns and input.
    [SerializeField] private SteamVR_Action_Boolean _rightHandRightResponse;
    [SerializeField] private SteamVR_Action_Boolean _rightHandLeftResponse;
    private NetExperimentManager _expManager;

    // Source GameObjects head + hands of local player
    private GameObject _localPlayer;
    private GameObject _headlessPlayer;
    private GameObject _localHead;
    private GameObject _localLeftHand;
    private GameObject _localRightHand;
 
    // Player parts viewable to others and hidden from the local player
    #pragma warning disable 649
    [SerializeField] private GameObject _netHeadObj;
    [SerializeField] private GameObject _netLeftCtrl;
    [SerializeField] private GameObject _netRightCtrl;
    #pragma warning restore 649
 
    // Objects tracked for server synchronization
    // private SteamVR_TrackedObject trackedObjHead;
    // private SteamVR_TrackedObject trackedObjRight;
    // private SteamVR_TrackedObject trackedObjLeft;
    
    public override void OnStartLocalPlayer()
    {
        // find the gaming rig in the scene and link to it
        if (_localPlayer == null)
        {
            _localPlayer = GameObject.Find("SteamVRObjects");

            if (_localPlayer == null)
            {
                // prevent headless version of app from crashing
                // depends on SteamVR version if HMD is null or simply won't move
                _headlessPlayer = GameObject.Find("NoSteamVRFallbackObjects");
 
                // when running as headless, provide default non-moving fallback objects instead
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
 
            //trackedObjRight = localRightHand.GetComponent<SteamVR_TrackedObject>();
            //trackedObjLeft = localLeftHand.GetComponent<SteamVR_TrackedObject>(); 
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
        
        // Get ExperimentManager
        if (_expManager == null)
        {
            _expManager = GameObject.FindGameObjectWithTag("ExperimentManager").GetComponent<NetExperimentManager>();
            Debug.LogWarning(_expManager);
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
        // "B" button on right Oculus controller.
        if (Input.GetKeyDown(KeyCode.O) && UIOptions.isHost && (_expManager.trialID == -1 || _expManager.trialID == 0 || _expManager.trialID == 1))
        {
            _expManager.CmdLeftResponse();
            Debug.LogWarning("Left response given");
        }
        // "A" button on right Oculus controller.
        else if (Input.GetKeyDown(KeyCode.K) && !UIOptions.isHost && (_expManager.trialID == -1 || _expManager.trialID == 2 || _expManager.trialID == 3))
        {
            _expManager.CmdRightResponse();
            Debug.LogWarning("Right response given");
        }
    }
    
    private void UpdateHeadAndHands()
    {
        if (!isLocalPlayer)
        {
            // do nothing, net transform does all the work for us
        }
        else
        {
            // we are the local player.
            // Copy the values from the Rig's parts so they can be used for positioning the online presence
            _netHeadObj.transform.position = _localHead.transform.position;
            _netHeadObj.transform.rotation = _localHead.transform.rotation;
 
            if (_localLeftHand)
            {
                // we need to check in case player left the hand unconnected
                _netLeftCtrl.transform.position = _localLeftHand.transform.position;
                _netLeftCtrl.transform.rotation = _localLeftHand.transform.rotation;
            }
 
            if (_localRightHand)
            {
                // only if right hand is connected
                _netRightCtrl.transform.position = _localRightHand.transform.position;
                _netRightCtrl.transform.rotation = _localRightHand.transform.rotation;
            }
        }
    }
}
