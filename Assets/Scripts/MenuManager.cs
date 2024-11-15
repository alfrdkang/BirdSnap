using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject hud;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject homeMenu;
    
    [SerializeField] private GameObject spawnedBirds;

    public bool isPaused = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && GameManager.instance.gameStarted)
        {
            if (isPaused)
            {
                Resume();
                isPaused = false;
            }
            else
            {
                Pause();
                isPaused = true;
            }
        }
    }

    public void Pause()
    {
        BGMController.instance.PauseMusic();
        hud.SetActive(false);
        pauseMenu.SetActive(true);
        
        GameManager.instance.GetComponent<SnapPicture>().canSnap = false;
        Time.timeScale = 0;
    }

    public void Resume()
    {
        BGMController.instance.PlayMusic();
        hud.SetActive(true);
        pauseMenu.SetActive(false);
        
        Time.timeScale = 1;
        Invoke("ResumeSnap", 1f);
    }

    private void ResumeSnap()
    {
        GameManager.instance.GetComponent<SnapPicture>().canSnap = true;
    }

    public void Restart()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        
        Time.timeScale = 1;
        ResumeSnap();
        
        GameManager.instance.StartGame();
    }

    public void Home()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        homeMenu.SetActive(true);
        
        Time.timeScale = 1;
        
        //if a previous game is running
        StopAllCoroutines();
        foreach (Transform child in spawnedBirds.transform)
        {
            Destroy(child.gameObject);
        }
        
        GameManager.instance.gameStarted = false;
        BGMController.instance.PlayMenuBGM();
    }

    public void Quit()
    {
        Application.Quit();
        // EditorApplication.isPlaying = false;
    }
}
