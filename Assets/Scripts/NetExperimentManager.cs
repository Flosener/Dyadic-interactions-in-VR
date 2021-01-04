using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
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
       private GameObject _leftUI;
       private GameObject _rightUI;

       // Data variables.
       private string _experimentID;
       [SyncVar] private float _RT;
       private string _compatibility;
       private string _color;
       private string _irrelevantStimulus;
       private string _response;
       private string _correctResponse;
       private bool _correctness;

       // Experiment tool/helper variables.
       private float _trialStartTime;
       private bool _experimentDone;
       private bool _trialSynchronized;
       private int _oldTrialID;
       [SyncVar] private bool _sameTrial;
       [SerializeField] [SyncVar(hook = nameof(OnTrialChange))] public int trialID = -1;
       [SerializeField] [SyncVar] private bool _leftResponseGiven;
       [SerializeField] [SyncVar] private bool _rightResponseGiven; 
       [SyncVar] private bool _leftReady;
       [SyncVar] private bool _rightReady;
       [SyncVar] private bool _spawningDone;

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
              // Wait for spawning process to finish (in NetworkManagerDobby).
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
              _experimentID = UIOptions.experimentID;
              
              // Show instructions to the participants, wait for them to begin the experiment via button click and disable instructions.
              yield return new WaitUntil(() => _leftReady && _rightReady);
              
              // Start experiment.
              StartCoroutine(Experiment(3f, 4, 4));
              yield return new WaitUntil(() => _experimentDone);
              _hatColor.SetColor("_Color", Color.white);
              yield return new WaitForSeconds(3f);
              _experimentDone = false;

              // After finishing the experiment, (stop server and) return to the EntranceHall.
              if (_manager.isNetworkActive)
              {
                     _manager.StopServer();
                     Debug.LogWarning("Server stopped.");
              }

              UIOptions.experimentID = "EntranceHall";
              SceneManager.LoadScene("EntranceHall", LoadSceneMode.Single);
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
                            StartCoroutine(StartTrial());
                            
                            // Wait for participant's response in Update().
                            yield return new WaitUntil(() => _leftResponseGiven || _rightResponseGiven);

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
       private IEnumerator StartTrial()
       {
              // Get a random integer, identifying the different trial cases.
              if (UIOptions.isHost)
              {
                     // Send command to server from host-client; server will synchronize trialID back to all clients.
                     CmdRandomTrial();
              }
              
              // Wait for synchronization process to finish.
              yield return new WaitUntil(() => _trialSynchronized || _sameTrial);

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
                     case 4:
                            GreenNeutral();
                            _compatibility = "neutral";
                            _color = "green";
                            _irrelevantStimulus = "NONE";
                            _correctResponse = "left";
                            break;
                     case 5:
                            RedNeutral();
                            _compatibility = "neutral";
                            _color = "red";
                            _irrelevantStimulus = "NONE";
                            _correctResponse = "right";
                            break;
              }
              
              // After each trial, set condition flags to false again.
              _trialSynchronized = false;
              _sameTrial = false;
       }
       
       // There are six possible trials (2 compatible, 2 incompatible, 2 neutral).
       // Correct responses: Green -> leftButton; Red -> rightButton
       void GreenCompatible()
       {
              _trialStartTime = Time.time;
              _hatColor.SetColor("_Color",Color.green);
              _leftLightAnim.Play("lightOn");
       }

       void GreenIncompatible()
       {
              _trialStartTime = Time.time;
              _hatColor.SetColor("_Color",Color.green);
              _rightLightAnim.Play("lightOn");
       }
       
       void GreenNeutral()
       {
              _trialStartTime = Time.time;
              _hatColor.SetColor("_Color",Color.green);
       }
       
       void RedCompatible()
       {
              _trialStartTime = Time.time;
              _hatColor.SetColor("_Color",Color.red);
              _rightLightAnim.Play("lightOn");
       }

       void RedIncompatible()
       {
              _trialStartTime = Time.time;
              _hatColor.SetColor("_Color",Color.red);
              _leftLightAnim.Play("lightOn");
       }
       
       void RedNeutral()
       {
              _trialStartTime = Time.time;
              _hatColor.SetColor("_Color",Color.red);
       }
       
       // Method for binding and writing data to .csv file.
       /*
        * AddRecord function is mostly copied from Max O'Didily's YouTube video: https://www.youtube.com/watch?v=vDpww7HsdnM&ab_channel=MaxO%27Didily.
        */
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

       // Hook into trial change as condition for trial begin (see TrialStart method).
       private void OnTrialChange(int oldID, int newID)
       {
              trialID = newID;
              _trialSynchronized = true;
       }
       
       // Get random trial.
       [Command(ignoreAuthority = true)]
       private void CmdRandomTrial()
       {
              _oldTrialID = trialID;
              trialID = Random.Range(0, 6);
              
              // Synchronized variable does not update when old and new trial are the same, such that animation is not played on client.
              if (trialID == _oldTrialID)
              {
                     _sameTrial = true;
              }
       }

       // Get reaction time right after input and send to server to sync with net player.
       [Command(ignoreAuthority = true)]
       public void CmdReactionTime()
       {
              _RT = Time.time - _trialStartTime;
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
