using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Discretization : MonoBehaviour {

    //Test Variables
    public Vector3 testPosition = new Vector3(5.0f, 5.0f, 5.0f);
    public Vector3 testVelocity = new Vector3(2.0f, 2.0f, 2.0f);
    public float testTimeStep = 2.0f;

    //Shared Variables for all Schemes
    private Vector3[] elementPositions = new Vector3[255];
    private Vector3[] elementVelocities = new Vector3[255];
    public float timeIncrement = 2.0f;

    //Variables Unique to the Quadratic Position Scheme
    private Vector3[] elementPastPositions = new Vector3[255];
    public bool[] eulerMode;

    //Variables Unique to the Quadratic Velocity Scheme 
    private Vector3[,] elementPastVelocities = new Vector3[255, 2];
    public int[] eulerCount;


    //variables for testing purpose
    public int iterationCounter;
    public int currentIterations;
    public int iterationIncrement;

    // Use this for initialization
    void Start () {

        //set initial conditions to allow for testings of schemes
        //None specific conditions
        elementPositions[0] = new Vector3(5.0f, 5.0f, 5.0f);
        elementVelocities[0] = new Vector3(2.0f, 2.0f, 2.0f);
        //QP conditions
        elementPastPositions[0] = new Vector3(3.0f, 3.0f, 3.0f);
        eulerMode = new bool[255];
        eulerMode[0] = false;
        //QV conditions
        eulerCount = new int[255];
        eulerCount[0] = 0;
        elementPastVelocities[0, 0] = new Vector3(2.0f, 2.0f, 2.0f);
        elementPastVelocities[0, 1] = new Vector3(2.0f, 2.0f, 2.0f);
        //other
        iterationCounter = 50;
        currentIterations = 0;
        iterationIncrement = 500000;

        Debug.Log(Time.realtimeSinceStartup);        

    }

    // Update is called once per frame
    void Update () {


        if (iterationCounter > 0)
        {
           // Debug.Log("For "+currentIterations+": "+testIterationTime(2, currentIterations, 0));
            currentIterations = currentIterations + iterationIncrement;
            iterationCounter = iterationCounter - 1;
        }


    }

    //Function to Iterate Eulers Method
    public Vector3 EulerIterate(int elementID)
    {

        //Get Required Information from the Element ID
        Vector3 currentPosition = elementPositions[elementID];
        Vector3 currentVelocity = elementVelocities[elementID];

        //Apply Eulers Methods
        Vector3 newPosition = currentPosition + (timeIncrement * currentVelocity);

        //Return calcualted new position
        return newPosition;
        
    }

    //Function to Iterate using the Quadratic Position Scheme
    public Vector3 QPIterate(int eID)
    {

        Vector3 newPos = new Vector3(0.0f, 0.0f, 0.0f);

        //Determine whether Euler's method should be used
        if (eulerMode[eID] == true)
        {
            newPos = EulerIterate(eID);
            eulerMode[eID] = false;
        }
        else
        {
            newPos = elementPastPositions[eID] + (2f * timeIncrement * elementVelocities[eID]);
        }

        elementPastPositions[eID] = elementPositions[eID];         //This stores the value for the next iteration

       return newPos;    
    }

    //Function to Iterate using the Quadratic Velocity Scheme
    public Vector3 QVIterate(int eID)
    {

        //Declare variable with null value to define local scope in function
        Vector3 newPos = new Vector3(0.0f, 0.0f, 0.0f);

        if (eulerCount[eID] != 0)
        {

            //Call Function to Iterate via Eulers Method
            newPos = EulerIterate(eID);

            //reduce the ammont eulers method needs to be used
            eulerCount[eID] -= 1;

        }
        else
        {
            //Debug.Log("Doin a ting");
            //Use the QV Scheme to find new position
            newPos = elementPositions[eID] + timeIncrement * ((5f / 12f) * elementPastVelocities[eID, 0]-(4f/3f)* elementPastVelocities[eID, 1]+(23f/12f)*elementVelocities[eID]);
        }

        //shift past velocities array
        elementPastVelocities[eID, 0] = elementPastVelocities[eID, 1];
        elementPastVelocities[eID, 1] = elementVelocities[eID];

        return newPos;
  
    }

    //Function to perform
    public float testIterationTime(int scheme, int iterationCount, int eID)
    {


        //create variable to set iterations to
        Vector3 testPosition;
        float iterationTime = 0;

        //series of if loops to determine which scheme to use
        if (scheme == 1)
        {
            //Take initial time reading
            var initialTime = Time.realtimeSinceStartup;

            for (int itNo = 0; itNo < iterationCount; itNo++)
            {

                //Use Eulers Method
                testPosition = EulerIterate(eID);

            }

            //Take end time
            var endTime = Time.realtimeSinceStartup;

            //return time take
            iterationTime = (endTime - initialTime);
        }
        if (scheme == 2)
        {
            //Take initial time reading
            var initialTime = Time.realtimeSinceStartup;

            for (int itNo = 0; itNo < iterationCount; itNo++)
            {

                //Use QP Scheme
                testPosition = QPIterate(eID);

            }

            //Take end time
            var endTime = Time.realtimeSinceStartup;

            //return time take
            iterationTime = (endTime - initialTime);
        }

        if (scheme == 3)
        {
            //Take initial time reading
            var initialTime = Time.realtimeSinceStartup;

            for (int itNo = 0; itNo < iterationCount; itNo++)
            {

                //Use QV scheme
                QVIterate(eID);

            }

            //Take end time
            var endTime = Time.realtimeSinceStartup;

            //return time take
            iterationTime = (endTime - initialTime);
        }

        //return the tie taken
        return iterationTime;

    }

}
