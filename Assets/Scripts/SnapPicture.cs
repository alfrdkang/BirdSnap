using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SnapPicture : MonoBehaviour
{
    public TextMeshProUGUI snapText;
    public Camera cam;
    public Vector2 originalCamPos;

    void Start()
    {
        originalCamPos = new Vector3(cam.transform.position.x, cam.transform.position.y, -10);
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                Vector3 clickPoint = new Vector3(hit.collider.transform.position.x, hit.collider.transform.position.y-0.2f, -10);
                cam.transform.position = clickPoint;
                cam.orthographicSize = 1;
                Debug.Log(hit.collider.name);
                snapText.text = hit.collider.gameObject.name+" Caught!";
                StartCoroutine(ZoomOut());
            }
            else
            {
                cam.transform.position = cam.ScreenToWorldPoint(Input.mousePosition);
                cam.orthographicSize = 1;
                Debug.Log("Miss!");
                snapText.text = "Miss!";
                StartCoroutine(ZoomOut());
            }
        }
    }
    
    IEnumerator ZoomOut()
    {
        yield return new WaitForSeconds(1f);
        cam.transform.position = new Vector3(0, 0, -10);
        cam.orthographicSize = 5;
        snapText.text = "";
    }
}
