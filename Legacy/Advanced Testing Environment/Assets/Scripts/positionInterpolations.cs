using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class positionInterpolations : MonoBehaviour {

    //This script is for different interpolating scheme for the velocity time integration

    //Variable for simple linear interpolation
    public Vector3[,] newPositions;

    //Variables for quadratic interpolation
    public int quadraticIterations = 3;
    public Vector3[,,] oldVelocities;

    //test variables
    public Vector3[,] testPositions = new Vector3[5,5];
    public Vector3[,] testVelocities = new Vector3[5, 5];

    // Use this for initialization
    void Start () {
		
        //Set test variables up
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                testPositions[x, y] = new Vector3(x, y, 0.0f);
            }
        }
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                testVelocities[x, y] = new Vector3(x, y, 1.0f);
            }
        }

        //Create buffers for quadratic interpolation
        oldVelocities = new Vector3[50, 50, 3];
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    //Fixed update, most interpolating scheme should go in here to be time controlled with the blob method
    void FixedUpdate()
    {
        testPositions = quadraticInterpolation(testPositions, testVelocities);
    }

    //Simple Linear Interpolation
    public Vector3[,] linearInterpolation(Vector3[,] position, Vector3[,] velocity)
    {

        //Define initial variables
        int xLength = position.GetLength(0);
        int yLength = position.GetLength(1);
        newPositions = new Vector3[xLength,yLength];

        //Simple linear interpolation for position
        for (int x = 0; x < xLength; x++)
        {
            for (int y = 0; y < yLength; y++)
            {
                newPositions[x, y] = position[x, y] + velocity[x,y]*Time.deltaTime;
            }
        }


        //return new position
        return newPositions;

    }

    //Quadratic Interpolation
    public Vector3[,] quadraticInterpolation(Vector3[,] position, Vector3[,] velocity)
    {

        //Define initial variables
        int xLength = position.GetLength(0);
        int yLength = position.GetLength(1);
        newPositions = new Vector3[xLength, yLength];

        //Check to see if a first order scheme is required to start the interpolation (requried to get 3 data points for the polynomial
        if (quadraticIterations != 0)
        {
            //reduce iteration counter for linear interpolation and active the linear interpolation function
            //quadraticIterations = quadraticIterations - 1;
            newPositions = linearInterpolation(position, velocity);

            //shift the old values matrix (programmed in a modular way so higher order schemes can be implemented
            for (int t = 0; t < oldVelocities.GetLength(2)-2; t++)
            {
                for (int x = 0; x < xLength; x++)
                {
                    for (int y = 0; y < yLength; y++)
                    {
                        oldVelocities[x, y, t + 1] = oldVelocities[x, y, t];
                    }
                }
            }
            //Creat new values for the current time
            for (int x = 0; x < xLength; x++)
            {
                for (int y = 0; y < yLength; y++)
                {
                    oldVelocities[x, y, 0] = velocity[x, y];
                }                
            }

        }
        else     //This is where the actual interpolation goes
        {

        }

        //return the new positions
        return newPositions;

    }

    //This function draws gizmos so that each blob can be visualised 
    private void OnDrawGizmos()
    {

        Gizmos.color = Color.red;
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                Gizmos.DrawSphere(testPositions[x, y], 0.1f);     // this shows control points as red spheres in the edit window
            }
        }

    }
}
