using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class ExperimentManager : MonoBehaviour
{
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
       private int _trialID;
       public static bool leftResponseGiven;
       public static bool rightResponseGiven;
       public static bool leftReady;
       public static bool rightReady;

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
              StartCoroutine(Experiment(3f, 2, 5));
              yield return new WaitUntil(() => _experimentDone);
              _hatColor.SetColor("_Color", Color.white);
              yield return new WaitForSeconds(3f);
              _experimentDone = false;

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
                            
                            // Select random trial.
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
              // Get UI from scene.
              _ui = GameObject.Find("InstructionsUI");

              // Set participants' readiness to false at the beginning.
              leftReady = false;
              rightReady = false;
              
              // Show instructions depending on the room and spawn position.
              switch (_experimentID)
              {
                     case "Individual_TwoChoice":
                            GameObject instructionsUI = _ui.transform.Find("IndividualTwoChoice").gameObject;
                            instructionsUI.SetActive(true);
                            yield return new WaitUntil(() => leftReady || rightReady);
                            instructionsUI.SetActive(false);
                            break;
                     
                     case "Individual_GoNoGo":
                            GameObject instructionsUILeft = _ui.transform.Find("IndividualGoNoGoLeft").gameObject;
                            GameObject instructionsUIRight = _ui.transform.Find("IndividualGoNoGoRight").gameObject;
                            
                            // If participant is on the left side, only consider left UI.
                            if (Participant.leftSpawned)
                            {
                                   instructionsUILeft.SetActive(true);
                                   yield return new WaitUntil(() => leftReady);
                                   instructionsUILeft.SetActive(false);
                            }
                            else
                            {
                                   instructionsUIRight.SetActive(true);
                                   yield return new WaitUntil(() => rightReady);
                                   instructionsUIRight.SetActive(false);
                            }
                            break;
              }
              
              // After the instructions are handled, the experiment can begin.
              _beginExperiment = true;
       }
}
