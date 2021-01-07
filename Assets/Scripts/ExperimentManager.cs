using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using Valve.VR;

public class ExperimentManager : MonoBehaviour
{

       // Participant input.
       #pragma warning disable 649
       [SerializeField] private SteamVR_Action_Boolean _leftHandLeftResponse;
       [SerializeField] private SteamVR_Action_Boolean _rightHandRightResponse;
       #pragma warning restore 649

       // Scene object/animation variables.
       private Material _hatColor;
       private Animator _leftDoorAnim;
       private Animator _rightDoorAnim;
       private Animator _leftLightAnim;
       private Animator _rightLightAnim;
       private GameObject _ui;

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
       private bool _beginExperiment;
       private bool _experimentDone;
       private bool _skipTrial;
       private int _trialID = -1;
       private bool _leftResponseGiven;
       private bool _rightResponseGiven;
       private bool _leftReady;
       private bool _rightReady;
       private bool _inTrial = true;

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

              // Save name of experimental condition.
              _experimentID = UIOptions.experimentID;

              // Show instructions to the participants, wait for them to begin the experiment via button click and disable instructions.
              _beginExperiment = false;
              StartCoroutine(HandleInstructions());
              yield return new WaitUntil(() => _beginExperiment);

              // Start experiment.
              StartCoroutine(Experiment(3f, 4, 126));
              yield return new WaitUntil(() => _experimentDone);
              _hatColor.SetColor("_Color", Color.white);
              yield return new WaitForSeconds(3f);
              _experimentDone = false;

              UIOptions.experimentID = "EntranceHall";
              SceneManager.LoadScene("EntranceHall");
       }

       private void Update()
       {
              GetResponse();
       }

       // Method for getting participant's response.
       private void GetResponse()
       {
              // "X" button on left Oculus controller.
              if ((_leftHandLeftResponse.state && _experimentID == "Individual_TwoChoice" && _inTrial) || 
                  (_leftHandLeftResponse.state && _experimentID == "Individual_GoNoGo" && Participant.leftSpawned && _inTrial))
              {
                     // Take reaction time directly after response.
                     _RT = Time.time - _trialStartTime;
                     _response = "left";
                     _leftResponseGiven = true;
                     _leftReady = true;
                     _inTrial = false;

                     if (_trialID != -1)
                     {
                            // Door opening animation on response.
                            _leftDoorAnim.Play("doorAnim");
                            SoundManager.PlaySound("doorOpen");
                     }
              }
              
              // "A" button on right Oculus controller.
              else if ((_rightHandRightResponse.state && _experimentID == "Individual_TwoChoice" && _inTrial) || 
                       (_rightHandRightResponse.state && _experimentID == "Individual_GoNoGo" && !Participant.leftSpawned && _inTrial))
              {
                     _RT = Time.time - _trialStartTime;
                     _response = "right";
                     _rightResponseGiven = true;
                     _rightReady = true;
                     _inTrial = false;

                     if (_trialID != -1)
                     {
                            _rightDoorAnim.Play("doorAnim");
                            SoundManager.PlaySound("doorOpen");
                     }
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
                            
                            // Select random trial.
                            StartTrial();

                            // If we are in the individual Go-NoGo task, skip unnecessary trials after two seconds.
                            if (UIOptions.experimentID == "Individual_GoNoGo" &&
                                ((Participant.leftSpawned && (_trialID == 2 || _trialID == 3 || _trialID == 5)) ||
                                 (!Participant.leftSpawned && (_trialID == 0 || _trialID == 1 || _trialID == 4))))
                            {
                                   yield return new WaitForSeconds(2f);
                                   _correctResponse = "NONE";

                                   // If the participant did not respond during NoGo trial (for 2s), response is none.
                                   if ((_leftResponseGiven || _rightResponseGiven) == false)
                                   {
                                          _skipTrial = true;
                                          _RT = Time.time - _trialStartTime;
                                          _response = "NONE";

                                          // Animation for automatic response.
                                          switch (_trialID)
                                          {
                                                 case 2 : case 3: case 5:
                                                        _rightDoorAnim.Play("doorAnim");
                                                        SoundManager.PlaySound("doorOpen");
                                                        break;
                                                 case 0: case 1: case 4:
                                                        _leftDoorAnim.Play("doorAnim");
                                                        SoundManager.PlaySound("doorOpen");
                                                        break;
                                          }
                                   }
                            }
                            
                            // Wait for participant's response in Update().
                            yield return new WaitUntil(() => _leftResponseGiven || _rightResponseGiven || _skipTrial);

                            // Check for trial type and play corresponding ending-light animation.
                            switch (_trialID)
                            {
                                   case 0 : case 3:
                                          _leftLightAnim.Play("doorGloom");
                                          break;
                                   case 1: case 2:
                                          _rightLightAnim.Play("doorGloom");
                                          break;
                                   case 4: case 5:
                                          _leftLightAnim.Play("doorGloom");
                                          _rightLightAnim.Play("doorGloom");
                                          break;
                            }

                            // Get correctness of given response.
                            _correctness = _response == _correctResponse;
                            
                            // Add all values to results.
                            AddRecord(0, i+1, j+1, _experimentID, _RT, _compatibility, _color, _irrelevantStimulus, _response, _correctResponse, _correctness, "Assets/Results/results.txt");
                     }
              }
              // Back to Start() coroutine if all trials in all blocks have been completed.
              _experimentDone = true;
       }

       // StartTrial() randomly selects a trial.
       private void StartTrial()
       {
              // Get a random integer, identifying the different trial cases.
              _trialID = Random.Range(0, 6);
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
       }
       
       // There are six possible trials (2 compatible, 2 incompatible, 2 neutral).
       // Correct responses: Green -> leftButton; Red -> rightButton
       void GreenCompatible()
       {
              _inTrial = true;
              _trialStartTime = Time.time;
              _hatColor.SetColor("_Color",Color.green);
              _leftLightAnim.Play("lightOn");
       }

       void GreenIncompatible()
       {
              _inTrial = true;
              _trialStartTime = Time.time;
              _hatColor.SetColor("_Color",Color.green);
              _rightLightAnim.Play("lightOn");
       }
       
       void GreenNeutral()
       {
              _inTrial = true;
              _trialStartTime = Time.time;
              _hatColor.SetColor("_Color",Color.green);
              _leftLightAnim.Play("lightOn");
              _rightLightAnim.Play("lightOn");
       }
       
       void RedCompatible()
       {
              _inTrial = true;
              _trialStartTime = Time.time;
              _hatColor.SetColor("_Color",Color.red);
              _rightLightAnim.Play("lightOn");
       }

       void RedIncompatible()
       {
              _inTrial = true;
              _trialStartTime = Time.time;
              _hatColor.SetColor("_Color",Color.red);
              _leftLightAnim.Play("lightOn");
       }
       
       void RedNeutral()
       {
              _inTrial = true;
              _trialStartTime = Time.time;
              _hatColor.SetColor("_Color",Color.red);
              _leftLightAnim.Play("lightOn");
              _rightLightAnim.Play("lightOn");
       }
       
       // Method for binding and writing data to .csv file.
       // Origin: Max O'Didily, https://www.youtube.com/watch?v=vDpww7HsdnM&ab_channel=MaxO%27Didily.
       private void AddRecord(int participantID, int blockCount, int trialCount, string _experimentID, float _RT, string _compatibility, string _color, string _irrelevantStimulus, string _response, string _correctResponse, bool _correctness, string filepath)
       {
              try
              {
                     // Instantiate StreamWriter object and write line to file including all recorded variables.
                     using (StreamWriter file = new StreamWriter(@filepath, true))
                     {
                            file.WriteLine(participantID + "," + blockCount + "," + trialCount + "," + _experimentID + "," + _RT + "," + _compatibility + "," + _color + "," + _irrelevantStimulus + "," + _response + "," + _correctResponse + "," + _correctness);
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
              // Get UI from scene.
              _ui = GameObject.Find("InstructionsUI");

              // Set participants' readiness to false at the beginning.
              _leftReady = false;
              _rightReady = false;
              
              // Show instructions depending on the room and spawn position.
              switch (_experimentID)
              {
                     case "Individual_TwoChoice":
                            GameObject instructionsUI = _ui.transform.Find("IndividualTwoChoice").gameObject;
                            instructionsUI.SetActive(true);
                            yield return new WaitUntil(() => _leftReady || _rightReady);
                            instructionsUI.SetActive(false);
                            break;
                     
                     case "Individual_GoNoGo":
                            GameObject instructionsUILeft = _ui.transform.Find("IndividualGoNoGoLeft").gameObject;
                            GameObject instructionsUIRight = _ui.transform.Find("IndividualGoNoGoRight").gameObject;
                            
                            // If participant is on the left side, only consider left UI.
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
              }
              
              // After the instructions are handled, the experiment can begin.
              _beginExperiment = true;
       }
}
