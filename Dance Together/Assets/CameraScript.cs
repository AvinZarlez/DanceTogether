using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour
{

    public float screenWidth = 8.0f;  // TO DO: Correct size? 

    public Camera cam;

    void Start()
    {

        screenWidth = screenWidth / Screen.height * Screen.width;
        cam.orthographicSize = screenWidth * Screen.height / Screen.width;
    }
}
