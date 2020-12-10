using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;
using Valve.VR.InteractionSystem;
using Random = UnityEngine.Random;
using Mirror;

public class ExperimentManager : MonoBehaviour
{
       // Scene object/animation variables.
       public GameObject sortingHat;
       public GameObject leftDoor;
       public GameObject rightDoor;
       public GameObject leftLight;
       public GameObject rightLight;
       
       private NetworkManager _manager;
       private Material _hatColor;
       private Animator _leftDoorAnim;
       private Animator _rightDoorAnim;
       private Animator _leftLightAnim;
       private Animator _rightLightAnim;
       private GameObject _ui;

       // Participant's input variables.
       public SteamVR_Action_Boolean leftHandLeftResponse;
       public SteamVR_Action_Boolean rightHandRightResponse;
       public SteamVR_Action_Boolean rightHandLeftResponse;

       // Data variables.
       private string _experimentID;
       private float _RT;
       private string _compatibility;
       private string _color;
       private string _irrelevantStimulus;
       private string _response;
       private string _correctResponse;
       private bool _correctness;

       // Experiment tool/helper variables.
       private float _trialStartTime;
       private int _trialID;
       private bool _beginExperiment;
       private bool _experimentDone;
       private bool _leftResponseGiven;
       private bool _rightResponseGiven;
       private bool _skipTrial;
       private bool _leftReady;
       private bool _rightReady;

       /*
        General lifecycle:
        - Start Experiment with coroutine Experiment().
        - Get random trial with StartTrial() (+ track time begin).
        - Wait for response in Update().
        - Save response times and other trial-specific data.
        - Wait for animations to end (in-between trials).
        - Next trial starts ...
        - Next Block starts ...
        - Finished experiment.
       */

       IEnumerator Start()
       {
              // Initialize hat MeshRenderer and doors and lights animators.
              _hatColor = sortingHat.GetComponent<MeshRenderer>().material;
              _leftDoorAnim = leftDoor.GetComponent<Animator>();
              _rightDoorAnim = rightDoor.GetComponent<Animator>();
              _leftLightAnim = leftLight.GetComponent<Animator>();
              _rightLightAnim = rightLight.GetComponent<Animator>();
              
              // Save name of experimental condition.
              _experimentID = UIOptions.experimentID;

              // When joining the networked experiment, spawn both participants.
              if (_experimentID == "Joint_GoNoGo")
              {
                     _manager = GameObject.Find("NetworkManager").GetComponent<NetworkManagerDobby>();
                     
                     if (UIOptions.isHost)
                     {
                            _manager.StartHost();
                            Debug.Log("Started as host.");
                     }
                     else
                     {
                            _manager.StartClient();
                            Debug.Log("Started as client.");
                     }
              }

              // Show instructions to the participants, wait for them to begin the experiment via button click and disable instructions.
              _beginExperiment = false;
              StartCoroutine(HandleInstructions());
              yield return new WaitUntil(() => _beginExperiment);

              // Start experiment.
              StartCoroutine(Experiment(3f, 2, 5));
              yield return new WaitUntil(() => _experimentDone);
              _hatColor.SetColor("_Color", Color.white);
              yield return new WaitForSeconds(3f);
              _experimentDone = false;
              
              UIOptions.experimentID = "EntranceHall";
              
              // After finishing the experiment, (stop server and) return to the EntranceHall.
              if (UIOptions.experimentID == "Joint_GoNoGo" && _manager.isNetworkActive)
              {
                     _manager = GameObject.Find("NetworkManager").GetComponent<NetworkManagerDobby>();
                     _manager.StopServer();
              }
              SceneManager.LoadScene("EntranceHall");
       }

       // Each trial, Experiment() coroutine waits for user input in Update().
       // Also the start of the experiment is handled here via user input (left/right ready).
       void Update()
       {
              // "X"/"B" button on left/right Oculus controller.
              if ((leftHandLeftResponse.state && _experimentID == "Individual_TwoChoice") || 
                  (leftHandLeftResponse.state && _experimentID == "Individual_GoNoGo" && Participant.leftSpawned) || 
                  (rightHandLeftResponse.state && NetParticipant.connectionID == NetworkManagerDobby.leftConnection && _experimentID == "Joint_GoNoGo" && (_trialID == 0 || _trialID == 1)))
              {
                     _leftResponseGiven = true;
                     _leftReady = true;
              }
              // "A" button on right Oculus controller.
              else if ((rightHandRightResponse.state && _experimentID == "Individual_TwoChoice") || 
                       (rightHandRightResponse.state && _experimentID == "Individual_GoNoGo" && !Participant.leftSpawned) || 
                       (rightHandRightResponse.state && NetParticipant.connectionID == NetworkManagerDobby.rightConnection && _experimentID == "Joint_GoNoGo" && (_trialID == 2 || _trialID == 3)))
              {
                     _rightResponseGiven = true;
                     _rightReady = true;
              }
       }

       // Coroutine for all experimental conditions.
       private IEnumerator Experiment(float seconds, int blockCount, int trialCount)
       {
              for (int i = 0; i < blockCount; i++)
              {
                     for (int j = 0; j < trialCount; j++)
                     {
                            // Set hat to base color and irrelevant stimuli off at the start of each trial and wait.
                            _hatColor.SetColor("_Color", Color.white);
                            yield return new WaitForSeconds(seconds);

                            // Reset response check to false.
                            _leftResponseGiven = false;
                            _rightResponseGiven = false;
                            _skipTrial = false;
                            
                            // Save the trial start time and select random trial.
                            _trialStartTime = Time.time;
                            StartTrial();

                            // If we are in the individual Go-NoGo task, skip unnecessary trials.
                            if (UIOptions.experimentID == "Individual_GoNoGo" &&
                                ((Participant.leftSpawned && (_trialID == 2 || _trialID == 3)) ||
                                 (!Participant.leftSpawned && (_trialID == 0 || _trialID == 1))))
                            {
                                   yield return new WaitForSeconds(1f);
                                   _response = "none";
                                   _skipTrial = true;
                            }
                            
                            // Wait for participant's response in Update().
                            yield return new WaitUntil(() => _leftResponseGiven || _rightResponseGiven || _skipTrial);
                            
                            // Get reaction time of the trial.
                            _RT = Time.time - _trialStartTime;
                            
                            // Check given response and play corresponding open-door animation.
                            if (_leftResponseGiven && !_skipTrial)
                            {
                                   _leftDoorAnim.Play("doorAnim");
                                   SoundManager.PlaySound("doorOpen");
                                   _response = "left";
                            }
                            else if (_rightResponseGiven && !_skipTrial)
                            {
                                   _rightDoorAnim.Play("doorAnim");
                                   SoundManager.PlaySound("doorOpen");
                                   _response = "right";
                            }

                            // Check for trial type and play corresponding ending-light animation.
                            switch (_trialID)
                            {
                                   case 0:
                                          _leftLightAnim.Play("doorGloom");
                                          break;
                                   case 1:
                                          _rightLightAnim.Play("doorGloom");
                                          break;
                                   case 2:
                                          _rightLightAnim.Play("doorGloom");
                                          break;
                                   case 3:
                                          _leftLightAnim.Play("doorGloom");
                                          break;
                            }

                            // Get correctness of given response.
                            if (_response == _correctResponse)
                            {
                                   _correctness = true;
                            }
                            else _correctness = false;
                            
                            // Add all values to results.
                            AddRecord(_experimentID, _RT, _compatibility, _color, _irrelevantStimulus, _response, _correctResponse, _correctness, "D:/Studium/Unity/Dyadic-interactions-in-VR/Assets/Results/results.txt");
                     }
              }
              // Back to Start() coroutine if all trials in all blocks have been completed.
              _experimentDone = true;
       }

       // StartTrial() randomly selects a trial.
       private void StartTrial()
       {
              // Get a random integer, identifying the different trial cases.
              _trialID = Random.Range(0, 4);
              switch (_trialID)
              {
                     case 0:
                            GreenCompatible();
                            _compatibility = "compatible";
                            _color = "green";
                            _irrelevantStimulus = "left";
                            _correctResponse = "left";
                            break;
                     case 1:
                            GreenIncompatible();
                            _compatibility = "incompatible";
                            _color = "green";
                            _irrelevantStimulus = "right";
                            _correctResponse = "left";
                            break;
                     case 2:
                            RedCompatible();
                            _compatibility = "compatible";
                            _color = "red";
                            _irrelevantStimulus = "right";
                            _correctResponse = "right";
                            break;
                     case 3:
                            RedIncompatible();
                            _compatibility = "incompatible";
                            _color = "red";
                            _irrelevantStimulus = "left";
                            _correctResponse = "right";
                            break;
              }
       }
       
       // There are four possible trials (2 compatible, 2 incompatible).
       // Correct responses: Green -> leftButton; Red -> rightButton
       void GreenCompatible()
       {
              _hatColor.SetColor("_Color",Color.green);
              _leftLightAnim.Play("lightOn");
       }

       void GreenIncompatible()
       {
              _hatColor.SetColor("_Color",Color.green);
              _rightLightAnim.Play("lightOn");
       }
       
       void RedCompatible()
       {
              _hatColor.SetColor("_Color",Color.red);
              _rightLightAnim.Play("lightOn");
       }

       void RedIncompatible()
       {
              _hatColor.SetColor("_Color",Color.red);
              _leftLightAnim.Play("lightOn");
       }
       
       // Method for binding and writing data to .csv file.
       private void AddRecord(string _experimentID, float _RT, string _compatibility, string _color, string _irrelevantStimulus, string _response, string _correctResponse, bool _correctness, string filepath)
       {
              try
              {
                     // Instantiate StreamWriter object and write line to file including all recorded variables.
                     using (StreamWriter file = new StreamWriter(@filepath, true))
                     {
                            file.WriteLine(_experimentID + "," + _RT + "," + _compatibility + "," + _color + "," + _irrelevantStimulus + "," + _response + "," + _correctResponse + "," + _correctness);
                     }
              }
              catch (Exception ex)
              {
                     throw new ApplicationException("An error occured: " + ex);
              }
       }

       // Show instructions to the participants, wait for them to begin the experiment via button click and disable instructions.
       private IEnumerator HandleInstructions()
       {
              // UI has to be spawned by NetworkManagerDobby in the networked experiment, for individual experiments the UI is already in the scene.
              if (_experimentID != "Joint_GoNoGo")
              { 
                     _ui = GameObject.Find("InstructionsUI");
              }
              
              // Set participants' readiness to false at the beginning.
              _leftReady = false;
              _rightReady = false;
              
              // Show instructions depending on the room and spawn position.
              switch (_experimentID)
              {
                     case "Individual_TwoChoice":
                            var instructionsUI = _ui.transform.Find("IndividualTwoChoice").gameObject;
                            instructionsUI.SetActive(true);
                            yield return new WaitUntil(() => _leftReady || _rightReady);
                            instructionsUI.SetActive(false);
                            break;
                     
                     case "Individual_GoNoGo":
                            var instructionsUILeft = _ui.transform.Find("IndividualGoNoGoLeft").gameObject;
                            var instructionsUIRight = _ui.transform.Find("IndividualGoNoGoRight").gameObject;
                            
                            if (Participant.leftSpawned)
                            {
                                   instructionsUILeft.SetActive(true);
                                   yield return new WaitUntil(() => _leftReady);
                                   instructionsUILeft.SetActive(false);
                            }
                            else
                            {
                                   instructionsUIRight.SetActive(true);
                                   yield return new WaitUntil(() => _rightReady);
                                   instructionsUIRight.SetActive(false);
                            }
                            break;
                     
                     case "Joint_GoNoGo":
                            // Wait until both participants connected to the network and UI got spawned.
                            // after Lab check: (NetworkManagerDobby.leftConnection != -1 && NetworkManagerDobby.rightConnection != -1 && spawningDone)
                            yield return new WaitUntil(() => NetworkManagerDobby.spawningDone);
                            
                            var ui = GameObject.FindGameObjectWithTag("InstructionsUI");
                            var instructionsUILeftJoint = ui.transform.Find("JointGoNoGoLeft").gameObject;
                            var instructionsUIRightJoint = ui.transform.Find("JointGoNoGoRight").gameObject;
                            
                            // After spawning, the UI is active, so set them false first and decide by if-else.
                            instructionsUILeftJoint.SetActive(false);
                            instructionsUIRightJoint.SetActive(false);

                            if (NetParticipant.connectionID == NetworkManagerDobby.leftConnection)
                            {
                                   instructionsUILeftJoint.SetActive(true);
                                   // after Lab check: _leftReady && _rightReady
                                   yield return new WaitUntil(() => _leftReady);
                                   instructionsUILeftJoint.SetActive(false);
                            }
                            else if (NetParticipant.connectionID == NetworkManagerDobby.rightConnection)
                            {
                                   instructionsUIRightJoint.SetActive(true);
                                   yield return new WaitUntil(() => _leftReady && _rightReady);
                                   instructionsUIRightJoint.SetActive(false);
                            }
                            break;
              }
              
              // After the instructions are handled, the experiment can begin.
              _beginExperiment = true;
       }
}
