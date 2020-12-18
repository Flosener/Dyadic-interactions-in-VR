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
using Valve.VR.Extras;

public class NetExperimentManager : NetworkBehaviour
{
       // Scene object/animation variables.
       private NetworkManager _manager;
       private Material _hatColor;
       private Animator _leftDoorAnim;
       private Animator _rightDoorAnim;
       private Animator _leftLightAnim;
       private Animator _rightLightAnim;
       private GameObject _leftUI;
       private GameObject _rightUI;

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
       private bool _experimentDone;
       [SerializeField] [SyncVar] public int trialID = -1;
       [SerializeField] [SyncVar] private bool _leftResponseGiven;
       [SerializeField] [SyncVar] private bool _rightResponseGiven; 
       [SerializeField] [SyncVar] private bool _leftReady;
       [SerializeField] [SyncVar] private bool _rightReady;
       [SerializeField] [SyncVar] private bool _spawningDone;

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
              
              yield return new WaitUntil(() => _spawningDone);
              
              // Initialize hat MeshRenderer and doors and lights animators.
              _hatColor = GameObject.Find("SortingHat").GetComponent<MeshRenderer>().material;
              _leftDoorAnim = GameObject.Find("LeftDoorAnim").GetComponent<Animator>();
              _rightDoorAnim = GameObject.Find("RightDoorAnim") .GetComponent<Animator>();
              _leftLightAnim = GameObject.Find("LeftGloomAnim").GetComponent<Animator>();
              _rightLightAnim = GameObject.Find("RightGloomAnim").GetComponent<Animator>();
              _manager = GameObject.Find("NetworkManager").GetComponent<NetworkManagerDobby>();
              _leftUI = GameObject.FindGameObjectWithTag("InstructionsUILeft");
              _rightUI = GameObject.FindGameObjectWithTag("InstructionsUIRight");

              // Save name of experimental condition.
              // _experimentID = UIOptions.experimentID;

              // for debugging only, later: join exp3
              UIOptions.isHost = false;
              _experimentID = "Joint_GoNoGo";
              
              // Show instructions to the participants, wait for them to begin the experiment via button click and disable instructions.
              yield return new WaitUntil(() => _leftReady && _rightReady);
              
              // Start experiment.
              StartCoroutine(Experiment(3f, 2, 5));
              yield return new WaitUntil(() => _experimentDone);
              _hatColor.SetColor("_Color", Color.white);
              yield return new WaitForSeconds(3f);
              _experimentDone = true;

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
                            _leftResponseGiven = false;
                            _rightResponseGiven = false;
                            
                            // Save the trial start time and select random trial.
                            _trialStartTime = Time.time;
                            StartTrial();
                            
                            // Wait for participant's response in Update().
                            yield return new WaitUntil(() => _leftResponseGiven || _rightResponseGiven);
                            
                            // Get reaction time of the trial.
                            _RT = Time.time - _trialStartTime;
                            
                            // Check given response and play corresponding open-door animation.
                            if (_leftResponseGiven)
                            {
                                   _leftDoorAnim.Play("doorAnim");
                                   SoundManager.PlaySound("doorOpen");
                                   _response = "left";
                            }
                            else if (_rightResponseGiven)
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
                            AddRecord(_experimentID, _RT, _compatibility, _color, _irrelevantStimulus, _response, _correctResponse, _correctness, "Assets/Results/results.txt");
                     }
              }
              // Back to Start() coroutine if all trials in all blocks have been completed.
              _experimentDone = true;
       }

       // StartTrial() randomly selects a trial.
       private void StartTrial()
       {
              // Get a random integer, identifying the different trial cases.
              if (UIOptions.isHost)
              {
                     // Send command to server from host-client; server will synchronize trialID back to all clients.
                     CmdRandomTrial();
              }
              Debug.LogWarning($"Synchronized trialID: {trialID}");
              
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
       private void AddRecord(string experimentID, float RT, string compatibility, string color, string irrelevantStimulus, string response, string correctResponse, bool correctness, string filepath)
       {
              try
              {
                     // Instantiate StreamWriter object and write line to file including all recorded variables.
                     using (StreamWriter file = new StreamWriter(@filepath, true))
                     {
                            file.WriteLine(experimentID + "," + RT + "," + compatibility + "," + color + "," + irrelevantStimulus + "," + response + "," + correctResponse + "," + correctness);
                     }
              }
              catch (Exception ex)
              {
                     throw new ApplicationException("An error occured: " + ex);
              }
       }

       // Get random trial.
       [Command(ignoreAuthority = true)]
       private void CmdRandomTrial()
       {
              trialID = Random.Range(0, 4);
              Debug.LogWarning($"TrialID on host client: {trialID}");
       }
       
       // Get left response.
       [Command(ignoreAuthority = true)]
       public void CmdLeftResponse()
       {
              // Destroy left instructions, when participant gives input.
              if (_leftUI != null)
              {
                     NetworkServer.Destroy(_leftUI);
                     _leftReady = true;
              }
              _leftResponseGiven = true;
       }

       // Get right response.
       [Command(ignoreAuthority = true)]
       public void CmdRightResponse()
       {
              // Destroy right instructions, when participant gives input.
              if (_rightUI != null)
              {
                     NetworkServer.Destroy(_rightUI);
                     _rightReady = true;
              }
              _rightResponseGiven = true;
       }

       // Called on server (NetworkManagerDobby): Set spawning done flag to true.
       [ClientRpc]
       public void RpcSpawningDone()
       {
              _spawningDone = true;
       }
}
