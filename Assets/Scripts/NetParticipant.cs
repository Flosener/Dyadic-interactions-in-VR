using System.Collections;
using Mirror;
using UnityEngine;
using Valve.VR;

public class NetParticipant : NetworkBehaviour
{
    // Helper variable for coordinating spawns and input.
    public SteamVR_Action_Boolean rightHandRightResponse;
    public SteamVR_Action_Boolean rightHandLeftResponse;
    private NetExperimentManager expManager;

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
    
    public override void OnStartLocalPlayer()
    {
 
        // find the gaming rig in the scene and link to it
        if (theLocalPlayer == null)
        {
            theLocalPlayer = GameObject.Find("SteamVRObjects");

            if (theLocalPlayer == null)
            {
                // prevent headless version of app from crashing
                // depends on SteamVR version if HMD is null or simply won't move
                headlessPlayer = GameObject.Find("NoSteamVRFallbackObjects");
 
                // when running as headless, provide default non-moving fallback objects instead
                localHead = headlessPlayer.transform.Find("FallbackObjects").gameObject;
                localLeftHand = headlessPlayer.transform.Find("FallbackHand").gameObject;
                localRightHand = headlessPlayer.transform.Find("FallbackHand").gameObject;
                Debug.Log("HEADLESS detected");
            }
        }

        // Get position and rotation from local player head + hands.
        if (theLocalPlayer != null)
        {
            localHead = theLocalPlayer.transform.Find("VRCamera").gameObject;
            localLeftHand = theLocalPlayer.transform.Find("LeftHand").gameObject;
            localRightHand = theLocalPlayer.transform.Find("RightHand").gameObject;
 
            trackedObjRight = localRightHand.GetComponent<SteamVR_TrackedObject>();
            trackedObjLeft = localLeftHand.GetComponent<SteamVR_TrackedObject>(); 
        }
    }
    
 
    private void Start()
    {
        // Disable mesh renderers for head + hands of netObject for local player to prevent overlap with localPlayer.
        if (isLocalPlayer)
        {
            netHeadObj.transform.Find("SortingHat").GetComponent<MeshRenderer>().enabled = false;
            netLeftCtrl.transform.Find("vr_glove_left_model_slim").transform.Find("slim_l").transform.Find("vr_glove_right_slim").GetComponent<SkinnedMeshRenderer>().enabled = false;
            netRightCtrl.transform.Find("vr_wand").transform.Find("slim_r").transform.Find("vr_glove_right_slim").GetComponent<SkinnedMeshRenderer>().enabled = false;
            netRightCtrl.transform.Find("vr_wand").transform.Find("Wand4").gameObject.SetActive(false);

            expManager = GameObject.Find("NetExperimentManager").GetComponent<NetExperimentManager>();
        }
    }
 
    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        
        // "B" button on right Oculus controller.
        if (Input.GetKeyDown(KeyCode.O) && UIOptions.isHost && (NetExperimentManager.trialID == -1 || NetExperimentManager.trialID == 0 || NetExperimentManager.trialID == 1))
        {
            CmdLeftResponse();
            Debug.LogWarning("Left response given");
        }
        // "A" button on right Oculus controller.
        else if (Input.GetKeyDown(KeyCode.K) && !UIOptions.isHost && (NetExperimentManager.trialID == -1 || NetExperimentManager.trialID == 2 || NetExperimentManager.trialID == 3))
        {
            CmdRightResponse();
            Debug.LogWarning("Right response given");
        } 
        
        // Synchronize position on network.
        UpdateHeadAndHands();
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
    public void CmdLeftResponse()
    {
        NetworkServer.Destroy(GameObject.FindGameObjectWithTag("InstructionsUILeft"));
        expManager.leftResponseGiven = true;
        expManager.leftReady = true;
    }

    [Command]
    public void CmdRightResponse()
    {
        NetworkServer.Destroy(GameObject.FindGameObjectWithTag("InstructionsUIRight"));
        expManager.rightResponseGiven = true;
        expManager.rightReady = true;
    }
}
