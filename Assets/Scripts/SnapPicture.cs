using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Script for bird snapping function
/// </summary>
public class SnapPicture : MonoBehaviour
{
    public TextMeshProUGUI snapText;
    public Camera cam;
    public Vector2 originalCamPos;
    
    public bool canSnap = true;
    
    // sfx
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip snapClip;
    [SerializeField] private AudioClip snapSuccessClip;

    void Start()
    {
        originalCamPos = new Vector3(cam.transform.position.x, cam.transform.position.y, -10);
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && GameManager.instance.gameStarted && canSnap)
        {
            sfxSource.PlayOneShot(snapClip);
            canSnap = false;
            GameManager.instance.snapCount++;
            
            RaycastHit2D hit = Physics2D.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            
            if (hit && hit.collider.CompareTag("Animal")) // Hit Bird
            {
                if (hit.collider.gameObject.GetComponent<Bird>().snapped == false)
                {
                    sfxSource.PlayOneShot(snapSuccessClip);
                    GameManager.instance.BirdSnapped(hit.collider.gameObject.GetComponent<Bird>());

                    // zooms in camera
                    Vector3 clickPoint = new Vector3(hit.collider.transform.position.x, hit.collider.transform.position.y-0.2f, -10);
                    cam.transform.position= clickPoint;
                    cam.orthographicSize = 1;
                    
                    hit.collider.gameObject.GetComponent<Bird>().speed = 0;
                    hit.collider.gameObject.GetComponent<Bird>().snapped = true;
                    hit.collider.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 6;
                    snapText.text = hit.collider.GetComponent<Bird>().birdName + " Caught!";
                    StartCoroutine(ZoomOut(hit.collider.gameObject));
                }
            }
            else // Missed Bird
            {
                // zooms in camera
                cam.transform.position = cam.ScreenToWorldPoint(Input.mousePosition);
                cam.orthographicSize = 1;
                snapText.text = "Miss!";
                StartCoroutine(ZoomOut(null));
            }
        }
    }
    
    /// <summary>
    /// Zooms the camera back to original position
    /// </summary>
    /// <param name="bird"></param>
    /// <returns></returns>
    IEnumerator ZoomOut(GameObject bird)
    {
        yield return new WaitForSeconds(1);
        cam.transform.position = new Vector3(0, 0, -10);
        cam.orthographicSize = 5;
        snapText.text = "";
        if (bird != null)
        {
            bird.GetComponent<Bird>().speed = 20;
        }
        
        canSnap = true;
    }
}
