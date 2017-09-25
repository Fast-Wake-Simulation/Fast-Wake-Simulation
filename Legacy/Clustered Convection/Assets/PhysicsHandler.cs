﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//This script manages whether or not the physics engine should run

public class PhysicsHandler : MonoBehaviour {

    //References to necessary UI components 
    public Toggle contToggle;                   //This determines whether the physics engine should run continously or for a set number of iteration
    public InputField iterationCountInput;           //If the physics engine should run a certain ammount of iteration, this is the number of iterations
    public Button pauseButton;                  //Button to stop iterations
    public Button startButton;                  //Button to start iterations

    //Varaibes for iteration counter
    public int iterationCount;
    private bool toggleBool = true;


	// Use this for initialization
	void Start () {

        //Add listener function so button presses active required methods
        pauseButton.onClick.AddListener(pausePhysics);
        startButton.onClick.AddListener(startPhysics);

        //Add toggle listener to flip flop the bool
        contToggle.onValueChanged.AddListener(flipFlopToggle);

        //Set the initial time step to zero so the physics engine doesnt run at startup
        Time.timeScale = 0;

	}
	
	// Update is called once per frame
	void Update () {
		
        //Determine whether of not physics engine should be active
        if (iterationCount == 0)
        {
            Time.timeScale = 0;             //These are doen in Update as FixedUpdate() may not exectute under some conditions
        }
        else
        {
            Time.timeScale = 1;
        }
	}

    //Update called once per update of physics engine
    void FixedUpdate()
    {
        if (iterationCount > 0)
        {

            if (toggleBool == false)
            {
                iterationCount = iterationCount - 1;
            }
            
        } 

        

    }

    //Function to flip flop the toggle boolean
    void flipFlopToggle(bool flipVar)
    {

        //In unity you cant access the togges value directly, only when its changed, this keeps track of changes (toggleBool must be private or editor interations can override its initial value)
        if (toggleBool == true)
        {
            toggleBool = false;
        }
        else
        {
            toggleBool = true;
        }


    }

    //Function for when the start button is pressed
    void startPhysics()
    {
        if (toggleBool == true)
        {
            iterationCount = 1;                                             //Simple way to use the iteration counter for inifinite iterations
        }
        else
        {
            iterationCount = Convert.ToInt32(iterationCountInput.text);     //Sets iteration count to required number
        }
    }

    //Function for if the pause button is pressed
    void pausePhysics()
    {
        iterationCount = 0;         //Ensure physics engine is paused
    }
}