using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int score;
    public float accuracy;
    public int birdsSnapped;
    public int snapCount;
    
    public bool gameStarted = false;
    
    [SerializeField] private GameObject hud;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject gameOverScore;
    [SerializeField] private GameObject gameOverBirdsSnap;
    [SerializeField] private GameObject gameOverAccuracy;
    
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI timerText;

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

    public void StartGame(int duration)
    {
        gameStarted = true;
        StartCoroutine(Timer.instance.StartTimer(duration));
    }

    public void EndGame()
    {
        gameStarted = false;
        hud.SetActive(false);
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

    public float CalculateAccuracy(float birdsSnapped, float snapCount)
    {
        return Mathf.Round(birdsSnapped / snapCount * 100);
    }
}
