using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Contexts;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;
using Valve.VR.InteractionSystem;
using Random = UnityEngine.Random;
using Mirror;

public class NetExperimentManager : NetworkBehaviour
{
       // Scene object/animation variables.
       private NetworkManager _manager;
       private Material _hatColor;
       private Animator _leftDoorAnim;
       private Animator _rightDoorAnim;
       private Animator _leftLightAnim;
       private Animator _rightLightAnim;
       // private GameObject _ui;

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
       // private bool _beginExperiment;
       private bool _experimentDone;
       private bool _skipTrial;
       public static int trialID = -1;
       [SyncVar] public bool leftResponseGiven;
       [SyncVar] public bool rightResponseGiven; 
       [SyncVar] public bool leftReady;
       [SyncVar] public bool rightReady;
       public static bool spawningDone;

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
              _hatColor = GameObject.Find("SortingHat").GetComponent<MeshRenderer>().material;
              _leftDoorAnim = GameObject.Find("LeftDoorAnim").GetComponent<Animator>();
              _rightDoorAnim = GameObject.Find("RightDoorAnim") .GetComponent<Animator>();
              _leftLightAnim = GameObject.Find("LeftGloomAnim").GetComponent<Animator>();
              _rightLightAnim = GameObject.Find("RightGloomAnim").GetComponent<Animator>();
              _manager = GameObject.Find("NetworkManager").GetComponent<NetworkManagerDobby>();

              // Save name of experimental condition.
              // _experimentID = UIOptions.experimentID;

              // When joining the networked experiment, spawn both participants.
              if (_experimentID == "Joint_GoNoGo")
              {
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

              // for debugging only, later: join exp3
              UIOptions.isHost = false;
              _experimentID = "Joint_GoNoGo";
              Debug.LogWarning("before instructions");
              
              // Show instructions to the participants, wait for them to begin the experiment via button click and disable instructions.
              // _beginExperiment = false;
              // StartCoroutine(HandleInstructions());
              //leftReady = false;
              //rightReady = false;
              yield return new WaitUntil(() => leftReady && rightReady);
              Debug.LogWarning("after instructions");
              
              // Start experiment.
              StartCoroutine(Experiment(3f, 2, 5));
              yield return new WaitUntil(() => _experimentDone);
              _hatColor.SetColor("_Color", Color.white);
              yield return new WaitForSeconds(3f);
              _experimentDone = false;

              // After finishing the experiment, (stop server and) return to the EntranceHall.
              if (_experimentID == "Joint_GoNoGo" && _manager.isNetworkActive)
              {
                     _manager.StopServer();
              }

              UIOptions.experimentID = "EntranceHall";
              SceneManager.LoadScene("EntranceHall");
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
                            leftResponseGiven = false;
                            rightResponseGiven = false;
                            _skipTrial = false;
                            
                            // Save the trial start time and select random trial.
                            _trialStartTime = Time.time;
                            StartTrial();

                            // If we are in the individual Go-NoGo task, skip unnecessary trials.
                            if (UIOptions.experimentID == "Individual_GoNoGo" &&
                                ((Participant.leftSpawned && (trialID == 2 || trialID == 3)) ||
                                 (!Participant.leftSpawned && (trialID == 0 || trialID == 1))))
                            {
                                   yield return new WaitForSeconds(1f);
                                   _response = "none";
                                   _skipTrial = true;
                            }
                            
                            // Wait for participant's response in Update().
                            yield return new WaitUntil(() => leftResponseGiven || rightResponseGiven || _skipTrial);
                            
                            // Get reaction time of the trial.
                            _RT = Time.time - _trialStartTime;
                            
                            // Check given response and play corresponding open-door animation.
                            if (leftResponseGiven && !_skipTrial)
                            {
                                   _leftDoorAnim.Play("doorAnim");
                                   SoundManager.PlaySound("doorOpen");
                                   _response = "left";
                            }
                            else if (rightResponseGiven && !_skipTrial)
                            {
                                   _rightDoorAnim.Play("doorAnim");
                                   SoundManager.PlaySound("doorOpen");
                                   _response = "right";
                            }

                            // Check for trial type and play corresponding ending-light animation.
                            switch (trialID)
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
                            _correctness = _response == _correctResponse;
                            
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
              trialID = Random.Range(0, 4);
              switch (trialID)
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
       /*private IEnumerator HandleInstructions()
       {
              yield return new WaitUntil(() => spawningDone);
              
              Debug.LogWarning("in instructions after spawning done");

              GameObject leftUI = GameObject.FindGameObjectWithTag("InstructionsUI").transform.Find("JointGoNoGoLeft").gameObject;
              GameObject rightUI = GameObject.FindGameObjectWithTag("InstructionsUI").transform.Find("JointGoNoGoRight").gameObject;

              Debug.LogWarning("found UI gameobject");

              if (UIOptions.isHost)
              {
                     Debug.LogWarning("in left instructions");
                     yield return new WaitUntil(() => leftReady);
                     leftUI.SetActive(false);
              }
              else
              {
                     Debug.LogWarning("in right instructions");
                     yield return new WaitUntil(() => rightReady);
                     rightUI.SetActive(false);
              }

              // After the instructions are handled, the experiment can begin.
              yield return new WaitUntil(() => leftReady && rightReady);
              _beginExperiment = true;
       }*/
}
