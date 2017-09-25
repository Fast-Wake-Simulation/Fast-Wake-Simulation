using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CameraDropdownController : MonoBehaviour
{
    //This class allows the user to select which camera to use in the flying arena scene

    public Dropdown cameraDropdown; //The camera selector dropdown within the options menu

    //The camera choices within the dropdown menu
    public GameObject lockedCam;
    public GameObject chaseCam;

	private int internalCameraSwitch = 0;

	void Start ()
    {
        CameraCheck(); //Checks which camera to activate when the scene is loaed
	}

    public void CameraCheck () //This is used by the camera dropdown. When the value is changed in the dropdown, this function is run.
    {
        if (cameraDropdown.value == 0)
        {
            ChaseCamera();
        }
        else if (cameraDropdown.value == 1)
        {
            LockedCamera();
        }
    }

	void ChaseCamera () //Disables all other cameras but the chase camera
	{
		chaseCam.SetActive(true);
		lockedCam.SetActive(false);
	}

	void LockedCamera () //Disables all other cameras but the locked camera
	{
		chaseCam.SetActive(false);
		lockedCam.SetActive(true);
	}
	public void SwitchCamera()
	{
		if(internalCameraSwitch == 0) internalCameraSwitch =1;
		else internalCameraSwitch = 0;

		if(internalCameraSwitch == 0) ChaseCamera();
		else if(internalCameraSwitch == 1) LockedCamera();
	}

}
