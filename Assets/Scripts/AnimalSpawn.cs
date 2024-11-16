using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script to Spawn Birds when Game Started
/// </summary>
public class AnimalSpawn : MonoBehaviour
{
    /// <summary>
    /// List of birds, varying quantities depending on rarity of birds
    /// </summary>
    [SerializeField] private GameObject[] birds;
    /// <summary>
    /// Game Object to hold birds spawned, allows for easier tracking and destroying after
    /// </summary>
    [SerializeField] private GameObject spawnedBirds;
    
    /// <summary>
    /// interval between birds spawned
    /// </summary>
    private float spawnInterval;
    /// <summary>
    /// float to store seconds left in timer
    /// </summary>
    private float timer;

    private void Start()
    {
        spawnInterval = Random.Range(1f, 5f);
    }

    private void Update()
    {
        if (GameManager.instance.gameStarted)
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                SpawnObject();
                timer = 0;
                spawnInterval = Random.Range(1f, 5f); // randomise how often birds are spawned, min 1 sec, max 5 sec
            }
        }
    }

    /// <summary>
    /// spawn bird
    /// </summary>
    public void SpawnObject()
    {
        float randomY = Random.Range(-2f, 3f);
        Vector3 spawnPosition = new Vector3(10, randomY, 0);
        GameObject bird = Instantiate(birds[Random.Range(0,birds.Length)], spawnPosition, Quaternion.identity, spawnedBirds.transform);
        Destroy(bird, 7.0f);
        bird.GetComponent<SpriteRenderer>().sortingOrder = Random.Range(2, 4); // randomise bird sprite sorting layer, allowing for birds to spawn behind some trees
    }
}
