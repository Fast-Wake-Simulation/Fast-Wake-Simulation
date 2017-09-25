﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

public class SimpleConvection : MonoBehaviour {

    //variables for simulation
    private int xSize;
    private int ySize;
    private float gridSpacing;
    private Vector3[,] elementPosition;
    private Vector3[,] elementVelocity;
    private Vector2[,] elementVorticity;
    public Vector3[,] vorT;
    public Vector3 freeStreamVelocity = new Vector3(0.1f, 0.0f, 0.0f);
    //private Vector2[,] elementVorticityVector;

    //For testing and data collection purposes
    private bool testFlipFLop;
    private bool biasFlipFlop;
    private float[] timesTaken;
    private float initialTime;
    private float endTime;
    private float vorticityBias;
    private float radiusBias;
    private float[] biasTimes;
    private float[] biasParameters;
    private int iterationCount;
    private bool runFlipFlop;

	// Use this for initialization
	void Start () {

        //define grid size and spacing
        xSize = 16;
        ySize = 20;
        gridSpacing = 0.5f;
        elementPosition = new Vector3[255,255];
        elementVelocity = new Vector3[255, 255];
        elementVorticity = new Vector2[255, 255];
        vorT = new Vector3[255, 255];

        //for testing and data collection purposes
        testFlipFLop = false;
        biasFlipFlop = false;
        runFlipFlop = true;
        timesTaken = new float[255];
        biasTimes = new float[255];
        biasParameters = new float[255];
        iterationCount = 0;
        

        setInitialConditions();

        savePositionCSV();

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    //physics sensitive things
    private void FixedUpdate()
    {


        //testing routing
        if (testFlipFLop == true)
        {

            for (int gridSize = 1; gridSize < 21; gridSize++)
            {

                //Counter to show user progress

                //Make sure initial conditions are kept constant
                xSize = 15;
                ySize = gridSize;
                setInitialConditions();

                //take initial time
                initialTime = Time.realtimeSinceStartup;

                //this for loop replicates the fixed update loop, but doesnt require rendering
                for (int loopcount = 0; loopcount < 500; loopcount++)
                {

                    updatePositions();
                    findVorticities();
                    convectElements();

                }

                //take initial time
                endTime = Time.realtimeSinceStartup;

                //reporttime
                timesTaken[gridSize] = endTime - initialTime;
                saveTimeCSV();
                //savePositionCSV();

            }
            testFlipFLop = false;

        }

        //biasing factors test
        if (biasFlipFlop == true)
        {


            for (float biasFactor = 0; biasFactor < 0.6; biasFactor = biasFactor + 0.03f, iterationCount++)
            {

                //Make sure initial conditions are kept constant
                xSize = 15;
                ySize = 20;
                setInitialConditions();

                //take initial time
                initialTime = Time.realtimeSinceStartup;

                //set bias
                radiusBias = biasFactor;

                //this for loop replicates the fixed update loop, but doesnt require rendering
                for (int loopcount = 0; loopcount < 500; loopcount++)
                {

                    updatePositions();
                    findVorticities();
                    convectElements();

                }
            

            //take initial time
            endTime = Time.realtimeSinceStartup;

            //record values
            biasTimes[iterationCount] = (endTime - initialTime);
            biasParameters[iterationCount] = biasFactor;
                //saveBiasPositionCSV(biasFactor);
                //savePositionCSV();


            }

            //savePositionCSV();
            saveBiasCSV();
            Debug.Log("Done!");

            //reset so it doesnt run in the loop
            biasFlipFlop = false;
        }

        //this is just for normal running
        if (runFlipFlop == true)
        {

            for (int iterations = 0; iterations < 500; iterations++)
            {
                updatePositions();
                findVorticities();
                convectElements();
            }
            savePositionCSV();
            runFlipFlop = false;
        }


    }

    //function to set initial conditions
    void setInitialConditions()
    {
        //set initial positiions and velocities
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                elementPosition[x, y] = new Vector3(x * gridSpacing, 0, y * gridSpacing);
                elementVelocity[x, y] = new Vector3(0.0f, 0.0f, 0.0f);
                elementVorticity[x, y] = new Vector2(0.0f, 0.0f);

                if (x == 1)
                {
                    elementVorticity[x, y] = new Vector2(0.0f, -0.5f);

                }

                if (x == xSize - 2)
                {
                    elementVorticity[x, y] = new Vector2(0.0f, 0.5f);
                }
            }
        }
    }

    //update positions
    void updatePositions()
    {
        
        //for loops to cycle through grid
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y< ySize; y++)
            {
                elementPosition[x, y] = elementPosition[x, y] + 0.02f * (elementVelocity[x, y] + freeStreamVelocity);
            }
        }

    }

    //This function is written awfuly but it works, if you're reading this, redo this!
    //function to find vorticities
    void findVorticities()
    {

        //find initial vorticity vectors
        for (int x = 1; x < xSize - 1; x++)
        {
            for (int y = 1; y < ySize - 1; y++)
            {

                //find vorticity in x and y
                Vector3 vorX = elementVorticity[x, y].x * (elementPosition[x + 1, y] - elementPosition[x - 1, y]).normalized;
                Vector3 vorY = elementVorticity[x, y].y * (elementPosition[x, y + 1] - elementPosition[x, y - 1]).normalized;
                vorT[x, y] = vorX + vorY;

            }
        }

    }

    //This function is written awfuly but it works, if you're reading this, redo this!
    //function to find vorticities
    void findVorticities2()
    {

        //find initial vorticity vectors
        for (int x = 1; x < xSize - 2; x++)
        {
            for (int y = 1; y < ySize - 2; y++)
            {

                //find vorticity in x and y
                Vector3 vorX = elementVorticity[x, y].x * (elementPosition[x + 1, y] - elementPosition[x - 1, y]).normalized;
                Vector3 vorY = elementVorticity[x, y].y * (elementPosition[x, y + 1] - elementPosition[x, y - 1]).normalized;
                vorT[x, y] = vorX + vorY;

            }
        }

        //determine vorticities at grid extremes
        //At 0,0
        Vector3 vortX = elementVorticity[0, 0].x * (elementPosition[1, 0] - elementPosition[0, 0]).normalized;
        Vector3 vortY = elementVorticity[0, 0].y * (elementPosition[0,0] - elementPosition[0, 1]).normalized;
        vorT[0, 0] = vortX + vortY;
        //at xMax,0
        vortX = elementVorticity[xSize - 1, 0].x * (elementPosition[xSize - 1, 0] - elementPosition[xSize - 2, 0]).normalized;
        vortY = elementVorticity[xSize - 1, 0].y * (elementPosition[xSize - 1, 0] - elementPosition[xSize - 1, 1]).normalized;
        vorT[xSize-1, 0] = vortX + vortY;
        //at 0,yMax
        vortX = elementVorticity[0, ySize-1].x * (elementPosition[1, ySize - 1] - elementPosition[0, ySize - 1]).normalized;
        vortY = elementVorticity[0, ySize - 1].y * (elementPosition[0, ySize - 2] - elementPosition[0, ySize - 1]).normalized;
        vorT[0, ySize - 1] = vortX + vortY;
        //at xMax,yMax
        vortX = elementVorticity[xSize - 1, ySize - 1].x * (elementPosition[xSize - 2, ySize - 1] - elementPosition[xSize - 1, ySize - 1]).normalized;
        vortY = elementVorticity[xSize - 1, ySize - 1].y * (elementPosition[xSize - 1, ySize - 2] - elementPosition[xSize - 1, ySize - 1]).normalized;
        vorT[xSize - 1, ySize - 1] = vortX + vortY;

        //now lets figure them out for the rows along the sides! (this is very inefficient but time constraints)
        //first the row at x=0
        for (int y = 1; y< ySize -2; y++)
        {
            vortX = elementVorticity[0, y].x * (elementPosition[1, y] - elementPosition[0, y]).normalized;
            vortY = elementVorticity[0, y].y * (elementPosition[0, y] - elementPosition[0, y + 1]).normalized;
            vorT[0, 0] = vortX + vortY;
        }
        //and the row at y=0
        for (int x = 1; x < xSize - 2; x++)
        {

        }

    }

    //function to convect all elements
    void convectElements()
    {



        //for loop to cycle through all elements
        for (int xN = 0; xN < xSize; xN++)
        {
            for (int yN = 0; yN < ySize; yN++)
            {
                //Reset the velocity field
                elementVelocity[xN, yN] = Vector3.zero;

                //Now that every element is going to be cycled through, the elements need to be cycled through again
                for (int xC = 0; xC < xSize; xC++)
                {
                    for (int yC = 0; yC < ySize; yC++)
                    {

                        if (xN != xC && yN != yC)
                        {
                 
                            

                            if (elementVorticity[xC,yC].magnitude >= radiusBias)
                            {

                                //Calcuate Influence
                                Vector3 r = elementPosition[xN, yN] - elementPosition[xC, yC];
                                elementVelocity[xN, yN] = elementVelocity[xN, yN] + (1f / (4f * Mathf.PI)) * (Vector3.Cross(vorT[xC, yC], r) / (Mathf.Pow(r.magnitude, 2)));

                            }
                        }
                    }
                }

            }
        }

    }

    //function to convect all elements
    void convectClusteredElements()
    {



        //for loop to cycle through all elements
        for (int xN = 0; xN < xSize; xN++)
        {
            for (int yN = 0; yN < ySize; yN++)
            {
                //Reset the velocity field
                elementVelocity[xN, yN] = Vector3.zero;

                //Now that every element is going to be cycled through, the elements need to be cycled through again
                for (int xC = 1; xC < xSize - 1; xC++)
                {
                    for (int yC = 1; yC < ySize - 1; yC++)
                    {

                        if (xN != xC && yN != yC)
                        {



                            if (elementVorticity[xC, yC].magnitude >= radiusBias)
                            {

                                //Calcuate Influence
                                Vector3 r = elementPosition[xN, yN] - elementPosition[xC, yC];
                                elementVelocity[xN, yN] = elementVelocity[xN, yN] + (1f / (4f * Mathf.PI)) * (Vector3.Cross(vorT[xC, yC], r) / (Mathf.Pow(r.magnitude, 2)));

                            }
                        }
                    }
                }

            }
        }

    }

    //Function to write positions to  csv file
    void savePositionCSV()
    {


        //file to write data into
        using (StreamWriter writetext = new StreamWriter("Output.csv"))
        {

            //Write Initial headings
            writetext.WriteLine("X Grid Position, Y  Grid Position, VecX, VecY, VecZ");

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    writetext.WriteLine(x + "," + y + "," + elementPosition[x,y].x + "," + elementPosition[x, y].y + "," + elementPosition[x, y].z);
                }
            }
        }


    }

    //Function to write positions to  csv file
    void saveBiasPositionCSV(float BiasFactor)
    {


        //file to write data into
        using (StreamWriter writetext = new StreamWriter("Positions"+BiasFactor+".csv"))
        {

            //Write Initial headings
            writetext.WriteLine("X Grid Position, Y  Grid Position, VecX, VecY, VecZ");

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    writetext.WriteLine(x + "," + y + "," + elementPosition[x, y].x + "," + elementPosition[x, y].y + "," + elementPosition[x, y].z);
                }
            }
        }


    }

    //Function to write a csv file containing the times taken
    void saveTimeCSV()
    {

        using (StreamWriter writetext = new StreamWriter("TimesTaken.csv"))
        {

            //Write Initial headings
            writetext.WriteLine("Grid Size Y"+","+"Computational Time");

            //cycle through the array and write the data value
            for (int val = 1; val < timesTaken.Length; val++)
            {   
                    writetext.WriteLine(val+","+timesTaken[val]);
            }
        }

    }

    //Function to write a csv file containing the times taken
    void saveBiasCSV()
    {

        using (StreamWriter writetext = new StreamWriter("BiasTimesTaken.csv"))
        {

            //Write Initial headings
            writetext.WriteLine("Bias Factor" + "," + "Computational Time");

            //cycle through the array and write the data value
            for (int val = 0; val < biasTimes.Length; val++)
            {
                writetext.WriteLine(biasParameters[val] + "," + biasTimes[val]);
            }
        }

    }

    //Draw the elements so they're visible
    private void OnDrawGizmos() // this shows control points as red spheres in the edit window
    {

        //cycle through all elements
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {

                //draw the gizmo
                Gizmos.DrawSphere(elementPosition[x, y], 0.1f);
            }
        }

    }

}
