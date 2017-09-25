using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TestingScripts : MonoBehaviour {

    //Variables related to elements
    private Vector3[,,] elementPosition = new Vector3[255,255,9];
    private Vector3[,,] elementVorticity = new Vector3[255, 255, 9];
    private Vector3[,] elementVelocity = new Vector3[255, 255];
    private Vector3[,,] vorT = new Vector3[255, 255,9];
    private int xSize = 16;
    private int ySize = 16;
    private float gridSpacing = 0.5f;
    private Vector3 freeStreamVelocity = new Vector3(0.0f, 0.0f, 3.5f);

    //clustering scheme variables
    private int clusterSize = 2;
    private int maxDegree = 4;

    //data gathering variables
    private float[] timesTaken = new float[255];
    private float[] gridSizeUsed = new float[255];

    //Variables for visualization
    private Mesh gridMesh;
    public Vector3[] meshVertices;
    public int removeLines = 2;




    //variables for wake mechanics
    private float spawnInterval = 3.0f;
    private float lastTime = Time.time;
    System.Random rnd = new System.Random();

    // Use this for initialization
    void Start () {

        //Set initial conditions
        setInitialConditions();
        createGrid();
        findVorticities();
        determineCoefficients();
        determineMaximumDegree();


        //Convect(5, 5, new Vector3(0.0f, 0.0f, 0.0f), maxDegree, 0, xSize-1, 0, xSize-1);

    }
	
	// Update is called once per frame
	void Update () {

        
        //updatePositions();

        if (Input.GetKeyDown("space") == true)
        {
            //Convect(7, 0, 2, 0, 7, 0, 7, 0, 0);
            //convectionDispatcher();
            //gatherData(1,0,0);
            //gatherData(2, 4, 4);
            //gatherData(2, 8, 4);
            //gatherData(2, 12, 4);
            //gatherData(2, 16, 4);
            //gatherData(2, 16, 8);
            //gatherData(2, 16, 12);
            //gatherData(2, 16, 16);
            //gatherData(2, 32, 16);
            //gatherData(2, 32, 32);
            gatherData(2, 16, 20);


        }
		
	}

    private void FixedUpdate()
    {
        convectionDispatcher();
        updateGrid();
    }

    //this function is just a nice container to store the conditions 
    void gatherData(int mode, int xVal, int yVal)
    {

        //first mode, simple times taken
        if (mode == 1)
        {

            //lets get some for loops to cycle through some values, actually, Only y is needed really...
            //for (int x = clusterSize)
            xSize = 32;
            int testCount = 0;
            for (int y = 1; y < 7; y++, testCount++)
            {

                //preamble necessary for the iterations to work
                ySize = (int) Mathf.Pow(clusterSize,y);
                xSize = (int)Mathf.Pow(clusterSize, y);
                setInitialConditions();
                findVorticities();
                determineMaximumDegree();

                float initialTime = Time.realtimeSinceStartup;

                //now the for loop to run through 20 seconds of time
                for (int iterations = 0; iterations < 500; iterations++)
                {
                    convectionDispatcher();
                }

                float endTime = Time.realtimeSinceStartup;

                timesTaken[testCount] = endTime - initialTime;
                gridSizeUsed[testCount] = xSize * ySize;
            }

            saveTimeCSV();
        }
        
        //second mode
        if (mode == 2)
        {
            //preamble necessary for the iterations to work
            xSize = xVal;
            ySize = yVal;
            setInitialConditions();
            findVorticities();
            determineMaximumDegree();

            float initialTime = Time.realtimeSinceStartup;

            //now the for loop to run through 20 seconds of time
            for (int iterations = 0; iterations < 500; iterations++)
            {
                convectionDispatcher();
            }

            float endTime = Time.realtimeSinceStartup;
            savePositionCSV();
            Debug.Log("For x: " + xVal + " y: " + yVal + " Time was: " + (endTime - initialTime) );

        }


        Debug.Log("Done");

    }

    void determineMaximumDegree()
    {

        //to determine maximum degreee, find the last degree where 
        bool shouldContinue = true;
        int cureDegree = 0;
        while (shouldContinue == true)
        {
            if (xSize % Mathf.Pow(clusterSize,cureDegree) == 0 && ySize % Mathf.Pow(clusterSize,cureDegree) == 0)
            {
                cureDegree++;
            }
            else
            {
                shouldContinue = false;
            }
            
        }

        maxDegree = cureDegree - 1;
    }

    //function to set initial conditions (STRAIGHT FROM SIMPLE CONVECTION)
    void setInitialConditions()
    {
        //set initial positiions and velocities
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                elementPosition[x, y, 0] = new Vector3(x * gridSpacing, 0, y * gridSpacing);
                elementVelocity[x, y] = new Vector3(0.0f, 0.0f, 0.0f);
                elementVorticity[x, y, 0] = new Vector2(0.0f, 0.0f);

                //complete horseshoe
                if (y == 1)
                {
                    elementVorticity[x, y, 0] = new Vector2(0.0f, -0.0f);
                }

                //complete horseshoe
                if (y == 31)
                {
                    elementVorticity[x, y, 0] = new Vector2(-0.0f, -0.0f);
                }

                if (x == 1)
                {
                    elementVorticity[x, y, 0] = new Vector2(0.0f, -0.0f);

                }

                if (x == xSize - 2)
                {
                    elementVorticity[x, y, 0] = new Vector2(0.0f, 0.0f);
                }

                

            }
        }
    }

    //function to find vorticities (STRAIGHT FROM SIMPLE CONVECTION)
    void findVorticities()
    {

        //find initial vorticity vectors
        for (int x = 1; x < xSize - 1; x++)
        {
            for (int y = 1; y < ySize - 1; y++)
            {

                //find vorticity in x and y
                Vector3 vorX = elementVorticity[x, y, 0].x * (elementPosition[x + 1, y, 0] - elementPosition[x - 1, y, 0]).normalized;
                Vector3 vorY = elementVorticity[x, y, 0].y * (elementPosition[x, y + 1, 0] - elementPosition[x, y - 1, 0]).normalized;
                vorT[x, y, 0] = vorX + vorY;

            }
        }

    }

    //This function deterines cluster coefficients (THIS FUNCTION WORKS - NO MESSING)
    void determineCoefficients()
    {
        
        //cycle through levels of "clustering"
        for ( int degree = 1; degree < maxDegree + 1; degree++)
        {

            //Determine step size and 
            int stepSize = (int) Mathf.Pow(clusterSize, degree);
            int xSteps = xSize / stepSize;
            int ySteps = ySize / stepSize;

            //cycle through the points
            for ( int x = 0; x < xSteps; x++)   
            {
                for ( int y = 0; y < ySteps; y++)
                {

                    //First reset the values we're interested in
                    elementPosition[x, y, degree] = Vector3.zero;
                    vorT[x, y, degree] = Vector3.zero;

                    //now that every index is cycled through for the given degree, we need to determine their values 
                    for (int xIn = 0; xIn < clusterSize; xIn++)
                    {
                        for (int yIn = 0; yIn < clusterSize; yIn++)
                        {

                            //perform sumations
                            vorT[x, y, degree] += vorT[x * clusterSize + xIn, y * clusterSize + yIn, degree - 1];
                            elementPosition[x, y, degree] += elementPosition[x * clusterSize + xIn, y * clusterSize + yIn, degree - 1];

                        }
                    }

                    //Now the average needs to be taken for the position
                    elementPosition[x, y, degree] = elementPosition[x, y, degree] / Mathf.Pow(clusterSize, 2);


                }
            }
            

        }

    }

    //Function that ties all the parts together
    void convectionDispatcher()
    {

        //Perform Subroutines to find necessary values
        findVorticities();
        determineCoefficients();
        resetAllVelocities();


        //Now we cycle through all elements and run the convection routine
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                Convect(x, y, maxDegree, 0, xSize, 0, ySize, 0, 0);
            }
        }

        //Now implement the discretizaton scheme
        updatePositions();
        updateWake();

    }

    //Function to reset velocities
    void resetAllVelocities()
    {

        //Reset all velocities to zero
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                //this cycles through all element, now reset their velocities
                elementVelocity[x, y] = Vector3.zero;
            }
        }

    }

    //This function finds the appropriate elements and abstracted clusters for the given element 
    void Convect(int xg, int yg, int degree, int xs, int xe, int ys, int ye, int xBias, int yBias)
    {

        //determine grid spacing 
        int spacing = (int) Mathf.Pow(clusterSize, degree);
        //Debug.Log(spacing);
        //Debug.Log("Current Degree: " + degree);

        //first step is to determine the current cluster size
        int xGridSize = 0;
        int yGridSize = 0;
        if (degree != 0)
        {
            xGridSize = (xe - xs + 1) / spacing;
            yGridSize = (ye - ys + 1) / spacing;
        }
        else
        {
            xGridSize = 2;
            yGridSize = 2;
        }
        //Debug.Log(xGridSize);
        //Debug.Log(yGridSize);


        //determine current position on the grid
        int xgp = 0;    //start by declaring variables for our position on the "local" grid
        int ygp = 0;    //And againt for the y position
        //for for loops to determine their values
        for (int x = 0; x < xGridSize; x++)
        {
            if ( xg >= (xs + x*spacing) && xg < (xs + (x + 1) * spacing))
            {
                xgp = x;
            }
        }
        //now determine the y coordinate
        for (int y = 0; y < yGridSize; y++)
        {
            if ( yg >= (ys + y * spacing) && yg < (ys + (y + 1) * spacing))
            {
                ygp = y;
            }
        }

        //Now cycle through clsuters
        for ( int x = 0; x <  xGridSize; x++)
        {
            for ( int y = 0; y < yGridSize; y++)
            {
                if ( x == xgp && y == ygp)
                {
                    //Check if we're at the base level abstraction
                    if (degree > 0)
                    {
                        Convect(xg, yg, degree - 1, xs + (xgp * spacing), xs + (xgp+1)*spacing, ys + (ygp * spacing), ys + (ygp + 1) * spacing, (xBias + x) * clusterSize, (yBias + y) * clusterSize);
                    }                
                }
                else
                {
                    biotSavart(xg, yg, x + xBias, y + yBias, degree);
                }
            }
        }

    }

    //This function implements the biot-savart law maybe
    void biotSavart(int ex, int ey, int cx, int cy, int degree)
    {

        //This calculate the influence
        //First the radius,
        Vector3 radius = elementPosition[ex, ey, 0] - elementPosition[cx, cy, degree];
        //Now the Biot-Savart law itself
        elementVelocity[ex, ey] += (1f / (4f * Mathf.PI)) * (Vector3.Cross(vorT[cx, cy, degree], radius) / (Mathf.Pow(radius.magnitude, 2)));

    }

    //update positions (FROM SIMPLE CONVECTION - IS EULERS METHOD)
    void updatePositions()
    {

        //for loops to cycle through grid
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                if (y != 0)
                {
                    elementPosition[x, y, 0] = elementPosition[x, y, 0] + 0.02f * (elementVelocity[x, y] + freeStreamVelocity);
                }
                
            }
        }

    }

    //Function to write a csv file containing the times taken (STRAIGHT FROM SIMPLE CONVECTION)
    void saveTimeCSV()
    {

        using (StreamWriter writetext = new StreamWriter("TimesTaken.csv"))
        {

            //Write Initial headings
            writetext.WriteLine("Number of Elements" + "," + "Computational Time");

            //cycle through the array and write the data value
            for (int val = 0; val < timesTaken.Length; val++)
            {
                writetext.WriteLine(gridSizeUsed[val] + "," + timesTaken[val]);
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
                    writetext.WriteLine(x + "," + y + "," + elementPosition[x, y, 0].x + "," + elementPosition[x, y, 0].y + "," + elementPosition[x, y, 0].z);
                }
            }
        }


    }

    //This function creates the inital grid
    void createGrid()
    {

        //Get required references via searches
        GetComponent<MeshFilter>().mesh = gridMesh = new Mesh(); gridMesh.name = "Grid Mesh";

        //start by clearing the grid (so this function can be called numerous times in the progra)
        gridMesh.Clear();

        //now buffer the grid to the correct size
        meshVertices = new Vector3[xSize * ySize];

        //set initial values of the vertices
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                meshVertices[y * xSize + x] = elementPosition[x, y, 0];
            }
        }

        //set the newly found verices to the mesh vertices
        gridMesh.vertices = meshVertices;

        //now create the triangles array
        int[] triangles = new int[xSize * (ySize - 1) * 3 * 2];
        int gridRowsInt = xSize;
        int gridColumnsInt = ySize;

        //now populate the array
        for (int nc = 0, n = 3; nc < (gridColumnsInt - 1); nc++)
        {
            for (int nr = 0; nr < gridRowsInt - 1; nr++, n = n + 3)
            {

                //These tri's are the lower quarter of a quad           (this could be done far more compactly, combining upper and lower quarters, but this is far easier to read)
                triangles[n] = gridRowsInt * nc + nr;
                triangles[n + 1] = gridRowsInt * (nc + 1) + nr + 1;
                triangles[n + 2] = gridRowsInt * nc + nr + 1;

                //These tri's are the upper quarter of a quad
                triangles[n + gridRowsInt * (gridColumnsInt - 1) * 3] = gridRowsInt * nc + nr;
                triangles[n + 1 + gridRowsInt * (gridColumnsInt - 1) * 3] = gridRowsInt * (nc + 1) + nr;
                triangles[n + 2 + gridRowsInt * (gridColumnsInt - 1) * 3] = gridRowsInt * (nc + 1) + nr + 1;

            }
        }

        //Create the grid described by triangles and vertices arrays
        gridMesh.triangles = triangles.Concat(triangles.Reverse().ToArray()).ToArray(); ;
        //gridMesh.uv = uv;
        gridMesh.RecalculateNormals();

    }

    //function to update grid 
    void updateGrid()
    {

        //This functions just manages the the vertex-vortonpositon relationship
        //Seed the vorton position array with coordinates for the vortons
        for (int nc = 0, n = 0; nc < ySize; nc++)
        {
            for (int nr = 0; nr < xSize; nr++, n++)
            {
                meshVertices[n] = elementPosition[nc, nr, 0];
            }
        }

        //create the mesh for the grids
        gridMesh.vertices = meshVertices;
        gridMesh.RecalculateNormals();

    }

    //function to handle wake mechanics
    void updateWake()
    {

        //this checks if its time to shift the cells
        if (Time.time - lastTime > spawnInterval)
        {
            lastTime = Time.time;
            //Debug.Log("doin a work");

            for (int y = ySize - 2; y > 0; y--)
            {
                //Debug.Log(y);
                
                for (int x = 0; x < xSize; x++)
                {
                    elementPosition[x, y + 1, 0] = elementPosition[x, y, 0];
                    elementVorticity[x, y + 1, 0] = elementVorticity[x, y, 0];
                    //elementVorticity[x, y + 1, 0] = elementPosition[x, y, 0];
                    //elementVelocity[x, y + 1] = elementVelocity[x, y];
                }
            }

            //create new elements
            for (int x = 0; x < xSize; x++)
            {
                elementPosition[x, 1, 0] = new Vector3(x * gridSpacing, 0, gridSpacing);
                elementVorticity[x, 1, 0] = new Vector2(1.5f * Mathf.Sin(rnd.Next()), 1.5f * Mathf.Sin(rnd.Next()));
            }

        }


    }

    //Draw the elements so they're visible
    private void OnDrawGizmos() // this shows control points as red spheres in the edit window (MODIFIED FROM SIMPLE CONVECTION)
    {

        Gizmos.color = Color.red;
        //cycle through all elements
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {

                //draw the gizmo
                Gizmos.DrawSphere(elementPosition[x, y,0], 0.1f);
            }
        }

        Gizmos.color = Color.blue;
        //cycle through all elements
        for (int x = 0; x < xSize / 2; x++)
        {
            for (int y = 0; y < ySize / 2; y++)
            {

                //draw the gizmo
                //Gizmos.DrawSphere(elementPosition[x, y, 1] + new Vector3(0.0f, 0.5f, 0.0f), 0.1f);
            }
        }

        Gizmos.color = Color.green;
        //cycle through all elements
        for (int x = 0; x < xSize / 4; x++)
        {
            for (int y = 0; y < ySize / 4; y++)
            {

                //draw the gizmo
                //Gizmos.DrawSphere(elementPosition[x, y, 2] + new Vector3(0.0f, 1.0f, 0.0f), 0.1f);
            }
        }

    }

}
