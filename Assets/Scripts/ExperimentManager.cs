using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ExperimentManager : MonoBehaviour
{
       public GameObject sortingHat;
       public GameObject door1;
       public GameObject door2;
       public GameObject light1;
       public GameObject light2;

       private bool _doorClosed = true;
       
       

       void Start()
       {
              sortingHat.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.white);
       }

       void Update()
       {
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
