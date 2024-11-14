using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalSpawn : MonoBehaviour
{
    [SerializeField] private GameObject[] birds;
    [SerializeField] private GameObject spawnedBirds;
    
    private float spawnInterval;
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
                spawnInterval = Random.Range(1f, 5f);
            }
        }
    }

    public void SpawnObject()
    {
        float randomY = Random.Range(-2f, 3f);
        Vector3 spawnPosition = new Vector3(10, randomY, 0);
        GameObject bird = Instantiate(birds[Random.Range(0,birds.Length)], spawnPosition, Quaternion.identity, spawnedBirds.transform);
        Destroy(bird, 7.0f);
        bird.GetComponent<SpriteRenderer>().sortingOrder = Random.Range(2, 4);
    }
}
