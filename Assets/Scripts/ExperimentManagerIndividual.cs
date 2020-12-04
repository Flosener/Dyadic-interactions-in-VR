using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ExperimentManagerIndividual : MonoBehaviour
{
       // Scene object/animation variables.
       public GameObject sortingHat;
       public GameObject leftDoor;
       public GameObject rightDoor;
       public GameObject leftLight;
       public GameObject rightLight;
       
       private Material _hatColor;
       private Animator _leftDoorAnim;
       private Animator _rightDoorAnim;
       private Animator _leftLightAnim;
       private Animator _rightLightAnim;

       // Participants input variables.
       public SteamVR_Action_Boolean leftResponse;
       public SteamVR_Action_Boolean rightResponse;

       // Data variables.
       private float _RT;
       private string _compatibility;
       private string _color;
       private string _irrelevantStimulus;
       private string _response;
       private string _correctResponse;
       private bool _correctness;
       private readonly List<float> _RTList = new List<float>();
       private readonly List<string> _compatibilityList = new List<string>();
       private readonly List<string> _colorList = new List<string>();
       private readonly List<string> _irrelevantStimulusList = new List<string>();
       private readonly List<string> _responseList = new List<string>();
       private readonly List<string> _correctResponseList = new List<string>();
       private readonly List<bool> _correctnessList = new List<bool>();
       
       // Experiment tool/helper variables.
       private float _trialStartTime;
       private bool _experimentDone;
       private int _trialID;
       private bool _leftResponseGiven;
       private bool _rightResponseGiven;

       /*
        General lifecycle:
        - Start Experiment with coroutine TwoChoice().
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
              
              // To-do: Add "if" for UI interaction.
              // Start corresponding coroutine for experiment room.
              if (SceneManager.GetActiveScene().name == "ExperimentRoom_Individual2Choice")
              {
                     StartCoroutine(TwoChoice(3f, 2, 5));
                     yield return new WaitUntil(() => _experimentDone);
              }
              else if (SceneManager.GetActiveScene().name == "ExperimentRoom_Individual_GoNogo")
              {
                     // StartCoroutine(GoNoGo(3f, 4, 10));
                     yield return new WaitUntil(() => _experimentDone);
              }
              
              Debug.Log("Experiment done!");
              
              // To-do: Bind data together and export as .csv file.
              // To-do: Stop current scene.
              // To-do: ExperimentID (?)
              // After finishing the experiment, return to the EntranceHall.
              // SceneManager.LoadScene("EntranceHall_Individual");
       }

       // Each trial, TwoChoice() coroutine waits for user input in Update().
       void Update()
       {
              // "B" button on right Oculus controller.
              if (leftResponse.state)
              {
                     _leftResponseGiven = true;
              }
              // "A" button on right Oculus controller.
              else if (rightResponse.state)
              {
                     _rightResponseGiven = true;
              }
       }

       // Coroutine for individual two choice task experiment.
       private IEnumerator TwoChoice(float seconds, int blockCount, int trialCount)
       {
              for (int i = 0; i < blockCount; i++)
              {
                     for (int j = 0; j < trialCount; j++)
                     {
                            // Set hat to base color and irrelevant stimuli off at the start of each trial and wait.
                            _hatColor.SetColor("_Color", Color.white);
                            yield return new WaitForSeconds(seconds);
                            
                            // Save the trial start time and select random trial.
                            _trialStartTime = Time.time;
                            
                            // Reset response check to false.
                            _leftResponseGiven = false;
                            _rightResponseGiven = false;
                            
                            StartTrial();
                            
                            // Wait for participant's response in Update().
                            yield return new WaitUntil(() => leftResponse.state || rightResponse.state);
                            
                            // Check given response and play corresponding open-door animation.
                            if (_leftResponseGiven)
                            {
                                   _leftDoorAnim.Play("doorAnim");
                                   _response = "left";
                            }
                            else if (_rightResponseGiven)
                            {
                                   _rightDoorAnim.Play("doorAnim");
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
                            
                            // Get reaction time of the trial.
                            _RT = _trialStartTime - Time.time;

                            // Get correctness of given response.
                            if (_response == _correctResponse)
                            {
                                   _correctness = true;
                            }
                            else _correctness = false;
              
                            // Add all values to lists.
                            _RTList.Add(_RT);
                            _compatibilityList.Add(_compatibility);
                            _colorList.Add(_color);
                            _irrelevantStimulusList.Add(_irrelevantStimulus);
                            _responseList.Add(_response);
                            _correctResponseList.Add(_correctResponse);
                            _correctnessList.Add(_correctness);
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
}
