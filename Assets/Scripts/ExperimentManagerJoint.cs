using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ExperimentManagerJoint : NetworkBehaviour
{
       public GameObject sortingHat;
       public GameObject door1;
       public GameObject door2;
       public GameObject light1;
       public GameObject light2;

       public GameObject spawnLeft;
       public GameObject spawnRight;
       private int spawn;

       private bool _doorClosed = true;
       
       

       void Start()
       {
              // if inital spawn is left assign spawn1, else spawn2
              spawn = transform.position == spawnLeft.transform.position ? 1 : 2;
              sortingHat.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.white);
       }

       void Update()
       {
              // if local player and first participant, you can only control left door
              //if (isLocalPlayer && spawn == 1)
              {
                     //Debug.Log(light1.transform.Find("DoorGloomEffect").GetComponent<Light>().intensity);
                     if (Input.GetKeyDown(KeyCode.I))
                     {
                            //light1.transform.Find("DoorGloomClose1").GetComponent<Light>().intensity = 0f;
                            door1.GetComponent<Animator>().Play("doorAnim");
                            light1.GetComponent<Animator>().Play("doorGloom");
                            _doorClosed = false;
                     }
                     else if(_doorClosed)
                     {
                            sortingHat.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
                            light1.transform.Find("DoorGloomEffect").GetComponent<Light>().intensity = 100f;
                            //Debug.Log( light1.transform.Find("DoorGloomEffect").GetComponent<Light>().intensity);
                     }
              }
              //else if (isLocalPlayer && spawn == 2)
              {
                     
              }

       }

       void leftcongruent()
       {
              sortingHat.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
              light1.transform.Find("DoorGloomClose1").GetComponent<Light>().intensity = 100f;

       }

       void leftIncongruent()
       {
              
       }
       
       void rightcongruent()
       {
              
       }

       void rightIncongruent()
       {
              
       }
}
