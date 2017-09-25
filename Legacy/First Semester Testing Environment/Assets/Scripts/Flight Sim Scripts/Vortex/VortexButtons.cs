//################################################################################
//# Unused script

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VortexButtons : MonoBehaviour {


	public GameObject CameraSwap; 

	private bool internalSwitch = false;

	void Start()
	{
		CameraSwap.gameObject.SetActive(false);

	}

	public void ToggleButtons()
	{
		internalSwitch = !internalSwitch;
		CameraSwap.gameObject.SetActive(internalSwitch);

	}
}
