using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : NetworkBehaviour
{
    private bool _gameIsPaused = false;
    public GameObject pauseMenuUI;
    public GameObject mainMenuUI;

    // Update is called once per frame
    private void Update()
    {
        // change to vr controller input
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            // if PauseMenu on in ExpRoom
            if (_gameIsPaused && SceneManager.GetActiveScene().name == "ExperimentRoom")
            {
                Resume(pauseMenuUI);
            }
            // if MainMenu on in EntranceHall
            else if (_gameIsPaused && SceneManager.GetActiveScene().name == "EntranceHall")
            {
                Resume(mainMenuUI);
            }
            // if PauseMenu off in ExpRoom
            else if (!_gameIsPaused && SceneManager.GetActiveScene().name == "ExperimentRoom")
            {
                Pause(pauseMenuUI);
            }
            // if MainMenu off in EntranceHall
            else
            {
                Pause(mainMenuUI);
            }
        }
    }

    private void Resume(GameObject menuUI)
    {
        menuUI.SetActive(false);
        Time.timeScale = 1f;
        _gameIsPaused = false;
    }
    private void Pause(GameObject menuUI)
    {
        menuUI.SetActive(true);
        Time.timeScale = 0f;
        _gameIsPaused = true;
    }

    private void HandleClick(GameObject menuUI)
    {
        
    }

    private void LoadMenu()
    {
        SceneManager.LoadScene("EntranceHall");
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
