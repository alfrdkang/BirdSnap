using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SnapPicture : MonoBehaviour
{
    public TextMeshProUGUI snapText;
    public Camera cam;
    public Vector2 originalCamPos;
    
    public bool canSnap = true;

    void Start()
    {
        originalCamPos = new Vector3(cam.transform.position.x, cam.transform.position.y, -10);
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && GameManager.instance.gameStarted && canSnap)
        {
            canSnap = false;
            
            RaycastHit2D hit = Physics2D.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            
            if (hit.collider.CompareTag("Animal"))
            {
                if (hit.collider.gameObject.GetComponent<Bird>().snapped == false)
                {
                    GameManager.instance.BirdSnapped(hit.collider.gameObject.GetComponent<Bird>());

                    Vector3 clickPoint = new Vector3(hit.collider.transform.position.x, hit.collider.transform.position.y-0.2f, -10);
                    cam.transform.position= clickPoint;
                    cam.orthographicSize = 1;
                    hit.collider.gameObject.GetComponent<Bird>().speed = 0;
                    hit.collider.gameObject.GetComponent<Bird>().snapped = true;
                    snapText.text = hit.collider.GetComponent<Bird>().birdName + " Caught!";
                    StartCoroutine(ZoomOut(hit.collider.gameObject));
                }
            }
            else
            {
                GameManager.instance.birdsSnapped++;
                cam.transform.position = cam.ScreenToWorldPoint(Input.mousePosition);
                cam.orthographicSize = 1;
                snapText.text = "Miss!";
                StartCoroutine(ZoomOut(null));
            }
        }
    }
    
    IEnumerator ZoomOut(GameObject bird)
    {
        yield return new WaitForSeconds(1);
        cam.transform.position = new Vector3(0, 0, -10);
        cam.orthographicSize = 5;
        snapText.text = "";
        if (bird != null)
        {
            bird.GetComponent<Bird>().speed = bird.GetComponent<Bird>().originalSpeed;
        }
        
        canSnap = true;
    }
}
