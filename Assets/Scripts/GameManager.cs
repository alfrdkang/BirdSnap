using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Game Manager Script
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int score;
    public int birdsSnapped;
    public int snapCount;
    
    public bool gameStarted = false;
    public float gameDuration = 45f;
    
    // Game Over Screens
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject gameOverScore;
    [SerializeField] private GameObject gameOverBirdsSnap;
    [SerializeField] private GameObject gameOverAccuracy;
    
    /// <summary>
    /// Game Object parent of spawned birds
    /// </summary>
    [SerializeField] private GameObject spawnedBirds;
    
    // HUD
    [SerializeField] private GameObject hud;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI timerText;
    
    // SFX
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip gameEndClip;
    [SerializeField] private AudioClip buttonClickClip;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void StartGame()
    {
        // if a previous game is running
        StopAllCoroutines();
        foreach (Transform child in spawnedBirds.transform)
        {
            Destroy(child.gameObject);
        }

        scoreText.text = "Score: 0";
        score = 0;
        birdsSnapped = 0;
        snapCount = 0;
        
        hud.SetActive(true);
        gameStarted = true;
        
        // restart bgm
        BGMController.instance.PlayAmbience();
        
        StartCoroutine(Timer.instance.StartTimer(gameDuration));
    }

    /// <summary>
    /// Plays an SFX when UI buttons are clicked
    /// </summary>
    public void ButtonSFX()
    {
        sfxSource.PlayOneShot(buttonClickClip);
    }

    public void EndGame()
    {
        BGMController.instance.PlayMenuBGM();
        sfxSource.PlayOneShot(gameEndClip);
        gameStarted = false;
        hud.SetActive(false);
        
        // Display Game Over Screen
        gameOverScreen.SetActive(true);
        gameOverScore.GetComponent<TextMeshProUGUI>().text = "Score: " + score;
        gameOverBirdsSnap.GetComponent<TextMeshProUGUI>().text = "Birds Snapped: " + birdsSnapped;
        gameOverAccuracy.GetComponent<TextMeshProUGUI>().text = "Accuracy: " + CalculateAccuracy(birdsSnapped, snapCount) + "%";

        DatabaseManager.instance.UpdatePlayData(score, birdsSnapped, snapCount);
    }

    public void BirdSnapped(Bird bird)
    {
        score += bird.birdScore;
        birdsSnapped++;
        scoreText.text = "Score: " + score.ToString();
    }

    /// <summary>
    /// calculates user's snap accuracy
    /// </summary>
    /// <param name="birdsSnapped"></param>
    /// <param name="snapCount"></param>
    /// <returns></returns>
    public float CalculateAccuracy(float birdsSnapped, float snapCount)
    {
        return Mathf.Round(birdsSnapped / snapCount * 100);
    }
}
