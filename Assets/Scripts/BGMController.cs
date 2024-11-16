using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script to control background music
/// </summary>
public class BGMController : MonoBehaviour
{
    public static BGMController instance;
    
    /// <summary>
    /// background music audio source
    /// </summary>
    private AudioSource audioSource;
    
    /// <summary>
    /// main menu background music clip
    /// </summary>
    [SerializeField] private AudioClip menuBGM;
    /// <summary>
    /// game background music clip
    /// </summary>
    [SerializeField] private AudioClip ambienceBGM;
    
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
        audioSource = GetComponent<AudioSource>();
        PlayMusic();
    }
    
    public void PlayMusic()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
    
    public void StopMusic()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    public void PauseMusic()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    /// <summary>
    /// start playing ambience (game) music clip
    /// </summary>
    public void PlayAmbience()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        audioSource.clip = ambienceBGM;
        audioSource.Play();
    }
    
    /// <summary>
    /// start playing menu music clip
    /// </summary>
    public void PlayMenuBGM()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        audioSource.clip = menuBGM;
        audioSource.Play();
    }
}
