using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Valve.VR;

public class NetParticipant : NetworkBehaviour
{
    // source gameobjects head, left and right controller object of local player
    private GameObject theLocalPlayer;
    private GameObject headlessPlayer;
    
    [SerializeField] private GameObject localHead;
    [SerializeField] private GameObject localLeftHand;
    [SerializeField] private GameObject localRightHand;
 
    //Player parts viewable to others and hidden from the local player
    #pragma warning disable 0649
    [SerializeField] private GameObject netHeadObj;
    [SerializeField] private GameObject netLeftCtrl;
    [SerializeField] private GameObject netRightCtrl;
    #pragma warning restore 0649
 
    //Objects tracked for server synchronization
    private SteamVR_TrackedObject trackedObjHead;
    private SteamVR_TrackedObject trackedObjRight;
    private SteamVR_TrackedObject trackedObjLeft;
 
    void Start()
    {
 
        Debug.Log("Start of the vr player");
 
        if (isLocalPlayer)
        {
            // disabled controller meshes at VR player side so it cannot be viewed by local player
            // netHeadObj.GetComponent<MeshRenderer>().enabled = false; // commented out for testing
            // netLeftCtrl.GetComponent<MeshRenderer>().enabled = false;
            // netRightCtrl.GetComponent<MeshRenderer>().enabled = false;
        }
    }
 
    void Update()
    {
 
        if (!isLocalPlayer)
        {
            return;
        }
 
        //sync pos on network
       OnStartLocalPlayer();
       UpdateHeadAndHands();
    }
 
    //instantiate networkPlayer prefab and connect to Local Player Rig
    public override void OnStartLocalPlayer()
    {
        // this is ONLY called on local player
 
        // find the gaming rig in the scene and link to it
        if (theLocalPlayer == null)
        {
            theLocalPlayer = GameObject.Find("SteamVRObjects"); // find the rig in the scene
            Debug.Log("Found local player");
        }
 
        // now link localHMD, localHands to the Rig so that they are automatically filled when the rig moves
        // localHead = Camera.main.gameObject; // get HMD with Camera.main wasn't working for me so a lazy load of the game object name was used.
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
}
