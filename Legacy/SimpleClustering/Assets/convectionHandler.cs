using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class convectionHandler : MonoBehaviour {

    //Variables for simulation purpose
    private int xSize;
    private int ySize;
    private float gridSpacing;
    private Vector3[,] elementPosition;
    private Vector3[,] elementVelocity;
    private Vector2[,] elementVorticity;
    private int clusterSize;
    private Vector3[,] vorT;

    //cluster related
    public Vector3[,] clusterPosition;
    private Vector3[,] clusterVorticity;

    //for recording/analysis purposes
    private float[] timesTaken;
    private float[] gridSizeUsed;

    // Use this for initialization
    void Start () {

        //Initialize Arrays
        elementPosition = new Vector3[255, 255];
        elementVelocity = new Vector3[255, 255];
        elementVorticity = new Vector2[255, 255];
        vorT = new Vector3[255, 255];
        clusterSize = 3;

        //for clusters
        clusterPosition = new Vector3[255, 255];
        clusterVorticity = new Vector3[255, 255];

        //specify testing variables
        xSize = 15;
        ySize = 21;
        gridSpacing = 0.5f;
        setInitialConditions();
        timesTaken = new float[255];
        gridSizeUsed = new float[255];
        setInitialConditions();


    }
	
	// Update is called once per frame
	void Update () {
		
	}

    //Physics related math
    void FixedUpdate()
    {

        //Uncomment these to just run the simulation in real time
        //findVorticities();
        //calculateCoefficients();
        //ClusteredConvection();
        //updatePositions();

        //Just a button press to activate testing
        if (Input.GetKeyDown("space"))
        {
            testComputationalTime(2);

        }

    }

    void testComputationalTime(int mode)
    {

        if (mode == 1)
        {


        //Index counter is
        int indexCounter = 0;

        //first for loop sets the cluster size
        for (int gridSize = 5; gridSize < 26; gridSize += 5, indexCounter++)
        {

            //set initial conditions
            xSize = 15;
            ySize = gridSize;
            setInitialConditions();

            //take initial time
            float initialTime = Time.realtimeSinceStartup;

            //now different clustersizes are cycled through, the iterations need to be done
            for (int iterations = 0; iterations < 500; iterations++)
            {
                findVorticities();
                calculateCoefficients();
                ClusteredConvection();
                updatePositions();
            }

            //take end time
            float endTime = Time.realtimeSinceStartup;

            //record values for writing later
            timesTaken[indexCounter] = endTime - initialTime;
            gridSizeUsed[indexCounter] = gridSize;
        }

        //Now we need to write the data to an excel files
        saveTimeCSV();


        }

        //Second mode is to find errors
        if (mode == 2)
        {

            //Run for the set ammount of iteration
            for (int iterations = 0; iterations < 500; iterations++)
            {
                findVorticities();
                calculateCoefficients();
                ClusteredConvection();
                updatePositions();
            }

            //Now write the results
            savePositionCSV();
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
            writetext.WriteLine("Grid Size Used" + "," + "Computational Time");

            //cycle through the array and write the data value
            for (int val = 0; val < timesTaken.Length; val++)
            {
                writetext.WriteLine(gridSizeUsed[val] + "," + timesTaken[val]);
            }
        }

    }

    //This function calculates the coefficients for the clusters
    void calculateCoefficients()
    {

        //Reinitialize arrays
        clusterVorticity = new Vector3[255, 255];
        clusterPosition = new Vector3[255, 255];

        //For loop to cycle through cluster positions
        for (int xClust = 0; xClust < (xSize/clusterSize); xClust++)
        {
            for (int yClust = 0; yClust < (ySize/clusterSize); yClust++)
            {

                //Now each cluster is cycled through, every element in the cluster needs to be cycled through
                for (int xIn = 0; xIn < clusterSize; xIn++)
                {
                    for (int yIn = 0; yIn < clusterSize; yIn++)
                    {

                        // Now we can add up their vorticity
                        clusterVorticity[xClust, yClust] += vorT[(xClust*clusterSize) + xIn, (yClust*clusterSize) + yIn];
                        clusterPosition[xClust, yClust] += elementPosition[(xClust*clusterSize) + xIn, (yClust * clusterSize) + yIn] * (1.0f/Mathf.Pow(clusterSize,2));

                    }
                }

            }
        }
    }

    void calculateClusterCoefficients()
    {

        //First set the values back to zero
        for (int x = 0; x < (xSize / clusterSize); x++)
        {
            for (int y = 0; y < (ySize/clusterSize); y++)
            {
                clusterVorticity[x, y] = new Vector3(0.0f, 0.0f, 0.0f);
                clusterPosition[x, y] = new Vector3(0.0f, 0.0f, 0.0f);
            }
        }

        //Now we cycle through the clusters
        for (int xClusterPos = 0; xClusterPos < xSize; xClusterPos += clusterSize)
        {
            for (int yClusterPos = 0; yClusterPos < xSize; yClusterPos += clusterSize)
            {

                //Now we need to cycle through all the elements
                for (int xInCluster = 0; xInCluster < clusterSize; xInCluster++)
                {
                    for (int yInCluster = 0; yInCluster < clusterSize; yInCluster++)
                    {

                        //Now all elements are cycled through, we need to add their values to the cluster
                        clusterVorticity[(xClusterPos / clusterSize), (yClusterPos / clusterSize)] += vorT[xClusterPos + xInCluster, yClusterPos + yInCluster];
                        clusterPosition[(xClusterPos / clusterSize), (yClusterPos / clusterSize)] += elementPosition[xClusterPos + xInCluster, yClusterPos + yInCluster];
                    }
                }
            }
        }

        //Now take the average position
        for (int xCluster = 0; xCluster < xSize/clusterSize; xCluster++)
        {
            for (int yCluster = 0; yCluster < ySize/clusterSize; yCluster++)
            {
                clusterPosition[xCluster, yCluster] = clusterPosition[xCluster, yCluster] * (1 / (clusterSize * clusterSize));
            }
        }
    

    }

    //update positions
    void updatePositions()
    {

        //for loops to cycle through grid
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                elementPosition[x, y] = elementPosition[x, y] + 0.02f * elementVelocity[x, y];
            }
        }

    }

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

    //Function to convect elements
    private void convectElements()
    {

        //First set of for loops cycle through the clusters
        for (int xClust = 0; xClust < (xSize/clusterSize); xClust++)
        {
            for (int yClust = 0; yClust < (ySize/clusterSize); yClust++)
            {

                //Now every blob in the clsuter needs to be cycled through
                for (int xInC = 0; xInC < clusterSize; xInC++)
                {
                    for (int yInC = 0; yInC < clusterSize; yInC++)
                    {


                        //this for loop series convects the given element with the elements in its cluster
                        for (int xConv = 0; xConv < clusterSize; xConv++)
                        {
                            for (int yConv = 0; yConv < clusterSize; yConv++)
                            {

                                //Check that the convection element isnt the element to be convected
                                if ( xInC != xConv && yInC != yConv)
                                {
                                    //Debug.Log("x: "+(xClust * clusterSize + xInC)+" Y: "+(yClust * clusterSize + yInC));
                                    //Convect the element
                                    Vector3 r = elementPosition[xClust * clusterSize + xInC, yClust * clusterSize + yInC] - elementPosition[xClust * clusterSize + xConv, yClust * clusterSize + yConv];

                                    elementVelocity[xClust*clusterSize+xInC,yClust*clusterSize+yInC] += (1f / (4f * Mathf.PI)) * (Vector3.Cross(vorT[xClust * clusterSize + xConv, yClust * clusterSize + yConv], r) / (Mathf.Pow(r.magnitude, 2)));
                                    
                                    //elementVelocity[xN, yN] = elementVelocity[xN, yN] + (1f / (4f * Mathf.PI)) * (Vector3.Cross(vorT[xC, yC], r) / (Mathf.Pow(r.magnitude, 2)));

                                }

                            }
                        }


                        //This next for loop convects the element int he cluster with other clsuters


                    }
                }

            }
        }

    }

    void clusterConvectElements()
    {

    //First cycle through all cluster
    for (int xClusterPos = 0; xClusterPos < xSize; xClusterPos += clusterSize)
        {
            for (int yClusterPos = 0; yClusterPos < ySize; yClusterPos += clusterSize)
            {
                //Debug.Log("X :" + xClusterPos + "   Y: " + yClusterPos);

                //Now that all clusters are cycled through, the elements in them need to be cycled
                for (int xInCluster = 0; xInCluster < clusterSize; xInCluster++)
                {
                    for (int yInCluster = 0; yInCluster < clusterSize; yInCluster++)
                    {
                        //Debug.Log("X :" + (xClusterPos+xInCluster) + "   Y: " + (yClusterPos+yInCluster));

                        //The velocity of the element to be convected needs to be zeroed
                        elementVelocity[xClusterPos + xInCluster, yClusterPos + yInCluster] = Vector3.zero;

                        //Now that all elements are cycled through, the elements in their respective cluster need to be cycled through
                        for (int xConvecting = 0; xConvecting < clusterSize; xConvecting++)
                        {
                            for (int yConvecting = 0; yConvecting < clusterSize; yConvecting++)
                            {
                                //Debug.Log("X :" + (xClusterPos + xInCluster) + "   Y: " + (yClusterPos + yInCluster));

                                //Now we need to check that we're not convecting the element with itself
                                if (xInCluster != xConvecting && yInCluster != yConvecting)
                                {

                                    //Now we can implement the biot savart law
                                    //First the radius need to be calculated
                                    Vector3 radius = elementPosition[xClusterPos + xInCluster, yClusterPos + yInCluster] - elementPosition[xClusterPos + xConvecting, yClusterPos + yConvecting];
                                    //Now we can implement the biot-savart law itself
                                    elementVelocity[xClusterPos + xInCluster, yClusterPos + yInCluster] += (1.0f / (4.0f * Mathf.PI)) * (Vector3.Cross(vorT[xClusterPos + xConvecting, yClusterPos + yConvecting], radius) / (Mathf.Pow(radius.magnitude,2)));

                                }
                            }
                        }

                       //Now we need to cycle through clusters
                       for (int xClusterID = 0; xClusterID < (xSize/clusterSize); xClusterID++)
                        {
                            for (int yClusterID = 0; yClusterID < (ySize/clusterSize); yClusterID++)
                            {

                                //Check we're not going to consider our own cluster
                                if (xClusterID != (xClusterPos*clusterSize) && yClusterID != (yClusterPos * clusterSize))
                                {

                                    elementVelocity[xClusterPos + xInCluster, yClusterPos + yInCluster] += new Vector3(0.0f, 0.5f, 0.5f);
                                }
                            }
                        }

                    }
                }

            }
        }

    }

    //Third attempt at this function!
    void ClusteredConvection()
    {

        //First Cycle through all clusters
        for (int xC = 0; xC < (xSize/clusterSize); xC++)
        {
            for (int yC = 0; yC < (ySize/clusterSize); yC++)
            {

                //Now we need to consider every element in the clsuter
                for (int xI = 0; xI < clusterSize; xI++)
                {
                    for (int yI = 0; yI < clusterSize; yI++)
                    {

                        //We need to reset their velocities so they dont add up
                        elementVelocity[(xC * clusterSize) + xI, (yC * clusterSize) + yI] = Vector3.zero;

                        //Now we need to cycle through the individual elements in that cluster
                        for (int xIC = 0; xIC < clusterSize; xIC++)
                        {
                            for (int yIC = 0; yIC < clusterSize; yIC++)
                            {
                                
                                //Now we need to check that we're not convecting the element with itself
                                if (xI != xIC && yI != yIC)
                                {

                                    //now we need to find the radius
                                    Vector3 Radius = elementPosition[(xC * clusterSize) + xI, (yC * clusterSize) + yI] - elementPosition[(xC * clusterSize) + xIC, (yC * clusterSize) + yIC];
                                    //Now we can implement the biot-savart law
                                    elementVelocity[(xC * clusterSize) + xI, (yC * clusterSize) + yI] += (1.0f / (4.0f * Mathf.PI)) * (Vector3.Cross(vorT[(xC * clusterSize) + xIC, (yC * clusterSize) + yIC], Radius) / Mathf.Pow(Radius.magnitude, 2));

                                }
                            }
                        }

                        //Now we need to convect the given element with the clusters
                        //First we cycle through all the clusters
                        for (int xCC = 0; xCC < (xSize/clusterSize); xCC++)
                        {
                            for (int yCC = 0; yCC < (ySize/gridSpacing); yCC++)
                            {

                                //Now we need to check that we're not going to convect with the clsuter the element is inside of
                                if (xC != xCC && yC != yCC)
                                {

                                    //Now we need to calculate the radius
                                    Vector3 Radius = elementPosition[(xC * clusterSize) + xI, (yC * clusterSize) + yI] - clusterPosition[xCC, yCC];
                                    //Now we can implement the biot-savart law
                                    if (Radius.magnitude != 0)
                                    {
                                        elementVelocity[(xC * clusterSize) + xI, (yC * clusterSize) + yI] += (1.0f / (4.0f * Mathf.PI)) * (Vector3.Cross(clusterVorticity[xCC, yCC], Radius) / Mathf.Pow(Radius.magnitude, 2));
                                    }
                                   


                                }
                            }
                        }
                    }
                }

            }
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
