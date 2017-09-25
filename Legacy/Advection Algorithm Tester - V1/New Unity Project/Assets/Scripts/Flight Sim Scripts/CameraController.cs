using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    //This script is used within the wind tunnel scene, and is used to control which camera is active

    //All the cameras within the scene
    public GameObject sideCamera;
    public GameObject frontCamera;
    public GameObject angleCamera;

    Vector3 offsetCam; //The distance of the cameras relative to the aircraft
    Vector3 offsetBack; //The distance of the background elements relative to the aircraft

    void Start ()
    {
        //Sets the default active camera
        sideCamera.SetActive(true);
        frontCamera.SetActive(false);
        angleCamera.SetActive(false);
    }
	
    void Update ()
    {
        //Activates and deactivates the cameras depending on the user input
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            sideCamera.SetActive(true);
            frontCamera.SetActive(false);
            angleCamera.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            sideCamera.SetActive(false);
            frontCamera.SetActive(true);
            angleCamera.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            sideCamera.SetActive(false);
            frontCamera.SetActive(false);
            angleCamera.SetActive(true);
        }
    }


}
