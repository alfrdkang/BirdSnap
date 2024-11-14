using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public static Timer instance;

    public bool timerRunning = false;
    
    [SerializeField] private TextMeshProUGUI timerText;
    
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

    public IEnumerator StartTimer(float targetTime)
    {
        timerRunning = true;
        while (targetTime > 0)
        {
            targetTime -= 1;
            int minutes = Mathf.FloorToInt(targetTime / 60);
            int seconds = Mathf.FloorToInt(targetTime % 60);

            timerText.text = string.Format("{00:00}:{01:00}", minutes,seconds);
            yield return new WaitForSeconds(1);
        }
        timerRunning = false;
        GameManager.instance.EndGame();
    }
}
