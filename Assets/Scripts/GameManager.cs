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

    private void Start()
    {
        
    }

    public void StartGame(int duration)
    {
        gameStarted = true;
        StartCoroutine(Timer.instance.StartTimer(duration));
    }

    public void EndGame()
    {
        gameStarted = false;
        gameOverScreen.SetActive(true);
        gameOverScore.GetComponent<TextMeshProUGUI>().text = "Score: " + score;
        gameOverBirdsSnap.GetComponent<TextMeshProUGUI>().text = "Birds Snapped: " + birdsSnapped;
        gameOverAccuracy.GetComponent<TextMeshProUGUI>().text = "Accuracy: " + (Mathf.Round(CalculateAccuracy(birdsSnapped, snapCount) * 100)) / 100.0 + "%";
    }

    public void BirdSnapped(Bird bird)
    {
        score += bird.birdScore;
        birdsSnapped++;
        snapCount++;
        scoreText.text = "Score: " + score.ToString();
    }

    public float CalculateAccuracy(float birdsSnapped, float snapCount)
    {
        return(snapCount / birdsSnapped * 100);
    }
}
