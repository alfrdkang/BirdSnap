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

    public bool isPaused = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && GameManager.instance.gameStarted)
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        hud.SetActive(false);
        pauseMenu.SetActive(true);
        
        GameManager.instance.GetComponent<SnapPicture>().canSnap = false;
        Time.timeScale = 0;
    }

    public void Resume()
    {
        hud.SetActive(true);
        pauseMenu.SetActive(false);
        
        Time.timeScale = 1;
        Invoke("ResumeSnap", 1f);
    }

    private void ResumeSnap()
    {
        GameManager.instance.GetComponent<SnapPicture>().canSnap = true;
    }
    
    public void Play()
    {
        hud.gameObject.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        Application.Quit();
        EditorApplication.isPlaying = false;
    }
}
