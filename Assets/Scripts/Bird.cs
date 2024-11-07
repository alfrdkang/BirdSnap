using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    public float speed;
    public float originalSpeed;

    public bool snapped = false;
    
    public string birdName;
    public int birdScore;

    private void Awake()
    {
        originalSpeed = speed;
    }

    private void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;
    }
}
