using System.Collections;
using Mirror;
using UnityEngine;
using Valve.VR;

public class NetParticipant : NetworkBehaviour
{
    // Helper variable for coordinating spawns and input.
    public SteamVR_Action_Boolean rightHandRightResponse;
    public SteamVR_Action_Boolean rightHandLeftResponse;
    
    //source gameobjects head, left and right controller object of local player
    private GameObject theLocalPlayer;
    private GameObject headlessPlayer;
    private GameObject localHead;
    private GameObject localLeftHand;
    private GameObject localRightHand;
 
    //Player parts viewable to others and hidden from the local player
    #pragma warning disable 649
    [SerializeField] private GameObject netHeadObj;
    [SerializeField] private GameObject netLeftCtrl;
    [SerializeField] private GameObject netRightCtrl;
    #pragma warning restore 649
 
    //Objects tracked for server synchronization
    private SteamVR_TrackedObject trackedObjHead;
    private SteamVR_TrackedObject trackedObjRight;
    private SteamVR_TrackedObject trackedObjLeft;
    
 
    void Start()
    {
 
        Debug.Log("Start of the vr player");
 
        if (isLocalPlayer)
        {
            //disabled conroller meshes at VR player side so it cannont be viewed by local player
            netHeadObj.transform.Find("SortingHat").GetComponent<MeshRenderer>().enabled = false;
            netLeftCtrl.transform.Find("vr_glove_left_model_slim").transform.Find("slim_l").transform.Find("vr_glove_right_slim").GetComponent<SkinnedMeshRenderer>().enabled = false;
            netRightCtrl.transform.Find("vr_wand").transform.Find("slim_r").transform.Find("vr_glove_right_slim").GetComponent<SkinnedMeshRenderer>().enabled = false;
            transform.position = GameObject.Find("Player").transform.position;
        }
    }
 
    void Update()
    {
 
        if (!isLocalPlayer)
        {
            return;
        }
        
        // "B" button on right Oculus controller.
        if ((Input.GetKeyDown(KeyCode.O) && UIOptions.isHost && (NetExperimentManager.trialID == 0 || NetExperimentManager.trialID == 1 || NetExperimentManager.trialID == -1)))
        {
            CmdLeftResponse();
            Debug.LogWarning("Left response given");
        }
        // "A" button on right Oculus controller.
        else if ((Input.GetKeyDown(KeyCode.K) && !UIOptions.isHost && (NetExperimentManager.trialID == 2 || NetExperimentManager.trialID == 3 || NetExperimentManager.trialID == -1)))
        {
            CmdRightResponse();
            Debug.LogWarning("Right response given");
        }
 
        //sync pos on network
       OnStartLocalPlayer();
       UpdateHeadAndHands();
    }
 
    //instantiate networkPlayer prefab and connect to Local Player Rig
    public override void OnStartLocalPlayer()
    {
        // this is ONLY called on local player
 
        //Debug.Log(gameObject.name + "Entered local start player, locating rig objects");
        //isLinkedToVR = true;
 
        // find the gaming rig in the scene and link to it
        if (theLocalPlayer == null)
        {
            theLocalPlayer = GameObject.Find("SteamVRObjects");// find the rig in the scene
        }
 
        // now link localHMD, localHands to the Rig so that they are
        // automatically filled when the rig moves
        localHead = theLocalPlayer.transform.Find("VRCamera").gameObject;
        localLeftHand = theLocalPlayer.transform.Find("LeftHand").gameObject;
        localRightHand = theLocalPlayer.transform.Find("RightHand").gameObject;
 
        trackedObjRight = localRightHand.GetComponent<SteamVR_TrackedObject>();
        trackedObjLeft = localLeftHand.GetComponent<SteamVR_TrackedObject>();
    }
 
    void UpdateHeadAndHands()
    {
        if (!isLocalPlayer)
        {
            // do nothing, net transform does all the work for us
        }
        else
        {
            // we are the local player.
            // Copy the values from the Rig's parts so they can be used for positioning the online presence
 
            // prevent headless version of app from crashing
            // depends on SteamVR version if HMD is null or simply won't move
            if (localHead == null)
            {
                headlessPlayer = GameObject.Find("NoSteamVRFallbackObjects");
 
                // when running as headless, provide default non-moving objects instead
                localHead = headlessPlayer.transform.Find("FallbackObjects").gameObject;
                localLeftHand = headlessPlayer.transform.Find("FallbackHand").gameObject;
                localRightHand = headlessPlayer.transform.Find("FallbackHand").gameObject;
                Debug.Log("HEADLESS detected");
            }
            netHeadObj.transform.position = localHead.transform.position;
            netHeadObj.transform.rotation = localHead.transform.rotation;
 
            if (localLeftHand)
            {
                // we need to check in case player left the hand unconnected
                netLeftCtrl.transform.position = localLeftHand.transform.position;
                netLeftCtrl.transform.rotation = localLeftHand.transform.rotation;
            }
 
            if (localRightHand)
            {
                // only if right hand is connected
                netRightCtrl.transform.position = localRightHand.transform.position;
                netRightCtrl.transform.rotation = localRightHand.transform.rotation;
            }
        }
    }

    [Command]
    private void CmdLeftResponse()
    {
        NetExperimentManager.leftResponseGiven = true;
        NetExperimentManager.leftReady = true;
    }

    [Command]
    private void CmdRightResponse()
    {
        NetExperimentManager.rightResponseGiven = true;
        NetExperimentManager.rightReady = true;
    }
}
