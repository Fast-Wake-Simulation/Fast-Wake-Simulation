using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This Script contains the maths for the simulation element of the effect, it is a basic vortex method with a Barnes-Hutt type optimization algoritm applied to the N-Body Problem.
/// To use the information from the simulatio
/// It is highly reccomended that a non-zero Free Stream Velocity is used as it is makes a huge difference to the aesthetics realism
/// Elements are spawned along the X axis and travel along the Y axis
/// </summary>

public class streamSimulator : MonoBehaviour {

    //Variables that specify the size and number of sheets to be simulated
    public int numberOfSheets;      //Editor accesible control for the number of sheets to be produced
    public int sheetSpanx;
    public int sheetSpany;          //Both these variables specift the element counts of the sheets to be produced

    //Arrays to hold variables for all the elements, arrays are 3-dimensoinal and take the form [x position,y position, sheet number] 
    public Vector3[,,,] elementPositions;
    public Vector3[,,] elementVelocities;
    public Vector2[,,,] elementSheetVorticities;
    public Vector3[,,,] elementActualVorticities;      //The size of the arrays is not initialized here, instead it is done by a funciton called in start as the sizes of sheet are user selectable

    //Arrays to hold sheet spawn location start and end points (in local coordinates) and the spawn locations
    public Vector3[] sheetSpawnStartPoints;
    public Vector3[] sheetSpawnEndPoints;
    public Vector3[,] sheetSpawnLocations;

    //variables to hold timing information (used for spawning new rows
    private float lastTime;
    public float spawnInterval;

    //Editor accesible toggle for the Gizmo Visualization of the simulatio
    public bool displayGizmos;

    //publicly accesible variable for the freestream velocity, public so it can changed by another script easily 
    public Vector3 freeStreamVelocity = new Vector3(4.5f, 0.0f, 0.0f);

    //Variables for the abstraction algorithm
    public int clusterSize;
    public int maxDegreeOfAbstraction;
    public int currentSheetToBeConvected;

    //Variables related to how vorticity is spawned
    public float sineMultiplier;
    public float sineOffset;
    public float xVorticityMagnitude;
    public float yVorticityMagnitude;
    private System.Random rand = new System.Random();
    public float xNoiseMagnitude;
    public float yNoiseMagnitude;
    public float horseshoeEffectMagnitude;
    public float globalEffectMultipler;


    // Use this for initialization
    void Start () {

        //Initialization functions here either declare a data size or populate a variable with initial information (without calculating)
        determineMaximumDegreeOfAbstraction();
        InitializeArraySizes();     //simple function that sets the sizes of the arrays for element information
        initializeSpawnTimer();     //Basically only gets the current time at startup for now


        //////////////////////////////////////////////
        //SPACE TO DECLARE START AND END POINTS         (place holders inserted to provide example)
        //sheetSpawnStartPoints[0] = new Vector3(-0.5f, 0.5f, -0.5f);
        //sheetSpawnEndPoints[0] = new Vector3(-0.5f, 0.5f, 0.5f);
        sheetSpawnStartPoints[0] = new Vector3(-0.5f, -0.5f, -0.5f);
        sheetSpawnEndPoints[0] = new Vector3(-0.5f, -0.5f, 0.5f);
        //sheetSpawnStartPoints[2] = new Vector3(-0.5f, -1.5f, -0.5f);
        //sheetSpawnEndPoints[2] = new Vector3(-0.5f, -1.5f, 0.5f);
        //////////////////////////////////////////////


        //This next block of function performe calculations required before the first iteration of the simulation
        calculateSpawnLocations();
        setInitialConditions();             //This is an initialization function but goes here rather than the first block as it requires information from calculateSpawnLocations()
        

    }
	
	// Update is called once per frame
	void Update () {
		

        //the following executes when the spawn interval timer activates (time betwean executiions = spawnInterval in seconds)
        if (onSpawnTimer(Time.time))
        {
            createNewElements();
        }


	}

    //All physics based math goes in here to keep in sync with unitys own physics engine
    private void FixedUpdate()
    {

        //This function is a container for all the maths for the convective routine 
        handleConvectiveRoutine(0);

    }

    ///////////////////////////////////////////////////
    //PRIMARY FUNCTIONS
    ///////////////////////////////////////////////////
    //(Primary functions are functions that are called
    //in either Start() or Update(), secondary functions
    //are ones which are only ever called by other 
    //functions)


    //This function initializes the arrays sizes according to the user entered dimensions, this is done as an optimization step so that an oversized buffer does not need to be used
    void InitializeArraySizes()
    {


        //relevant declarations for element arrays
        elementPositions = new Vector3[sheetSpanx, sheetSpany, numberOfSheets, maxDegreeOfAbstraction+1];
        elementVelocities = new Vector3[sheetSpanx, sheetSpany, numberOfSheets];
        elementSheetVorticities = new Vector2[sheetSpanx, sheetSpany, numberOfSheets, maxDegreeOfAbstraction+1];
        elementActualVorticities = new Vector3[sheetSpanx, sheetSpany, numberOfSheets, maxDegreeOfAbstraction+1];


        //Exactly the same but for the element start and end locations and element locations arrays
        sheetSpawnStartPoints = new Vector3[numberOfSheets];
        sheetSpawnEndPoints = new Vector3[numberOfSheets];
        sheetSpawnLocations = new Vector3[numberOfSheets, sheetSpanx];


    }

    //initialization for everything spawn related
    void initializeSpawnTimer()
    {

        //for now all the functionality needed is to get the time at startup
        lastTime = Time.time;

    }

    //this functions populates the array of element spawn locations 
    void calculateSpawnLocations()
    {


        //spawn locations must be calcualted for every sheet, hence every sheet is cycled through first
        for (int sheetNo = 0; sheetNo < numberOfSheets; sheetNo++)
        {

            //A new local variables is declared that stores the vector betwean the two points
            Vector3 completeVector = sheetSpawnEndPoints[sheetNo] - sheetSpawnStartPoints[sheetNo];

            //Now the total distance vector is known, a unit length vector can be made from this
            Vector3 unitVector = completeVector / sheetSpanx;

            //Now the unit vector is found this fully defines the spawn positions, however each individual spawn location is stored, this increases memory usage, however it avoids
            //the spawn locations being needed to be found via unitvector * X-position along every time it is needed and as such is done as an optimization consideration
            //Hence a for loop is used to cycle through all spawn potions
            for (int locationNo = 0; locationNo < sheetSpanx; locationNo++)
            {

                sheetSpawnLocations[sheetNo, locationNo] = sheetSpawnStartPoints[sheetNo] + (locationNo * unitVector);

            }


        }

    }

    //This function sets the initial conditions, this is required as the arrays are initialised with no value and so the convection math results in NaN if no values are passed to it, which causes the particle system to render a stream from the current location to (0, 0, 0)
    void setInitialConditions()
    {


        //The positions need to be set for all elements or NaN results from the simulation section, however positions of elements are not known, to get around this all the element positions are set to their spawn points, so this doesnt affect the simulation their vorticities are set to zero
        for (int sheetNo = 0; sheetNo < numberOfSheets; sheetNo++)
        {
            for (int x = 0; x < sheetSpanx; x++)
            {
                for (int y = 0; y < sheetSpany; y++)
                {

                    //set their positions to the spawn locations for the column spanning the y direction
                    elementPositions[x, y, sheetNo, 0] = sheetSpawnLocations[sheetNo, x];

                    //and the vorticities to zero
                    elementSheetVorticities[x, y, sheetNo, 0] = new Vector2( 0.0f, 0.0f);

                }
            }
        }

    }

    void determineMaximumDegreeOfAbstraction()
    {


        //to determine maximum degreee, find the last degree where 
        bool shouldContinue = true;
        int cureDegree = 0;

        //this while loop incremements through degrees ob abstraction until a degree that is impossible is found
        while (shouldContinue == true)
        {

            if (sheetSpanx % Mathf.Pow(clusterSize, cureDegree) == 0 && sheetSpany % Mathf.Pow(clusterSize, cureDegree) == 0)
            {
                cureDegree++;
            }
            else
            {
                shouldContinue = false;
            }

        }

        maxDegreeOfAbstraction = cureDegree - 1;
    }

    //this function returns a true value when a new row of elements should be spawn
    bool onSpawnTimer(float currentTime)            //The time is passed to the function rather than just using Time.time in the function itself so that the simulation can be puased without affecting Time.time
    {

        //This is the bool for the return value, declared here so all paths lead to a value
        bool shouldSpawn = false;

        //check if enough time has elapsed
        if (currentTime > lastTime + spawnInterval){

            shouldSpawn = true;
            lastTime = currentTime;
        }

        //and finally return the value
        return shouldSpawn;
    }

    //This function creates a new row of elements
    void createNewElements()
    {


        ///This takes a dispatcher form, the main functions for simplicity are segregated to other functions
        shiftAllElementsByOne();
        positionNewRowOfElements();
        applyInitialVorticityOfElements();

    }

    //this function contains all the simulation math to be executed during run time
    void handleConvectiveRoutine(int optimizationMode)
    {


        //The following functions calculate information required before the induced velocities can be calculated
        calculateActualElementVorticities();
        calculateCoefficientsForAllAbstractions();
        resetAllVelocities();


        //Cycling through all sheets, this is an odd method but explained below
        for (int sheetNo = 0; sheetNo < numberOfSheets; sheetNo++)
        {

            //Set which sheet to be convected
            currentSheetToBeConvected = sheetNo;

            //using a global variable changed by this function and then read in the recursive function is an odd way to program this, however it's programmed like this so that a variable
            //does not need to be sent through all the cycles of the recursive function, so whilst odd this is for optimization

            //Now we cycle through all elements and run the convection routine
            for (int x = 0; x < sheetSpanx; x++)
            {
                for (int y = 0; y < sheetSpany; y++)
                {
                    ascendDataStructure(x, y, maxDegreeOfAbstraction, 0, sheetSpanx, 0, sheetSpany, 0, 0);
                }
            }


        }

        //this function updates the positions array according to the newly calculated velocities
        updatePositions();


    }


    ///////////////////////////////////////////////////
    //SECONDARY FUNCTIONS
    ///////////////////////////////////////////////////
    //(Primary functions are functions that are called
    //in either Start() or Update(), secondary functions
    //are ones which are only ever called by other 
    //functions)


    //This function finds the appropriate elements and abstracted clusters for the given element 
    void ascendDataStructure(int xg, int yg, int degree, int xs, int xe, int ys, int ye, int xBias, int yBias)
    {

        //The Debug.Log calls used to test whether this function gave the right values are left in, uncomment them to help understand how this funciton works

        //determine grid spacing 
        int spacing = (int)Mathf.Pow(clusterSize, degree);
        //Debug.Log(spacing);
        //Debug.Log("Current Degree: " + degree);

        //first step is to determine the current cluster size
        //first initialize variables
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
            if (xg >= (xs + x * spacing) && xg < (xs + (x + 1) * spacing))
            {
                xgp = x;
            }
        }

        //now determine the y coordinate
        for (int y = 0; y < yGridSize; y++)
        {
            if (yg >= (ys + y * spacing) && yg < (ys + (y + 1) * spacing))
            {
                ygp = y;
            }
        }

        //Now cycle through clsuters
        for (int x = 0; x < xGridSize; x++)
        {
            for (int y = 0; y < yGridSize; y++)
            {
                if (x == xgp && y == ygp)
                {

                    //Check if we're at the base level abstraction
                    if (degree > 0)
                    {

                        ascendDataStructure(xg, yg, degree - 1, xs + (xgp * spacing), xs + (xgp + 1) * spacing, ys + (ygp * spacing), ys + (ygp + 1) * spacing, (xBias + x) * clusterSize, (yBias + y) * clusterSize);

                    }
                }
                else
                {

                    //Now the end of the current path is reached the element can be convected
                    cumulateElementInfluence(xg, yg, x + xBias, y + yBias, degree);

                }
            }
        }

    }

    //This function implements the biot-savart law maybe
    void cumulateElementInfluence(int ex, int ey, int cx, int cy, int degree)
    {

        //This calculate the influence
        //First the radius,
        Vector3 radius = elementPositions[ex, ey, currentSheetToBeConvected, 0] - elementPositions[cx, cy, currentSheetToBeConvected, degree];

        //Now the Biot-Savart law itself
        if (radius.magnitude > 0.05f)
        {

            //the following commented line is how the Biot-Savart law should be applied, this creates a jerky motion however as new elements are spawned, if a low spawn timer is used
            //the smoothing section should be commented out and the following line used
            //elementVelocities[ex, ey, currentSheetToBeConvected] += (1f / (4f * Mathf.PI)) * (Vector3.Cross(elementActualVorticities[cx, cy, currentSheetToBeConvected, degree], radius) / (Mathf.Pow(radius.magnitude, 2)));

            //this next section "smooths" the influence of newly created elements to avoid a jerky motion in the simulation as they suddenly come into existance
            if (ey == 0)
            {
                elementVelocities[ex, ey, currentSheetToBeConvected] += ((Time.time - lastTime) / spawnInterval) * (1f / (4f * Mathf.PI)) * (Vector3.Cross(elementActualVorticities[cx, cy, currentSheetToBeConvected, degree], radius) / (Mathf.Pow(radius.magnitude, 2)));
            }
            else
            {
                elementVelocities[ex, ey, currentSheetToBeConvected] += (1f / (4f * Mathf.PI)) * (Vector3.Cross(elementActualVorticities[cx, cy, currentSheetToBeConvected, degree], radius) / (Mathf.Pow(radius.magnitude, 2)));
            }

        }

    }

    //update positions (FROM SIMPLE CONVECTION - IS EULERS METHOD)
    void updatePositions()
    {

        //for loops to cycle through all sheets
        for (int sheetNo = 0; sheetNo < numberOfSheets; sheetNo++)
        {
            for (int x = 0; x < sheetSpanx; x++)
            {
                for (int y = 0; y < sheetSpany; y++)
                {

                        elementPositions[x, y, sheetNo, 0] = elementPositions[x, y, sheetNo, 0] + 0.02f * (elementVelocities[x, y, sheetNo] + freeStreamVelocity);

                }
            }
        }

    }

    //function to find actual vorticities from sheetwise vorticities
    void calculateActualElementVorticities()
    {

        //Cycle through all elements on all sheets
        for (int sheetNo = 0; sheetNo < numberOfSheets; sheetNo++)
        {
            for (int x = 1; x < sheetSpanx - 1; x++)
            {
                for (int y = 1; y < sheetSpany - 1; y++)
                {

                    //Find vorticity in global space from sheetwise vorticities
                    Vector3 vorX = elementSheetVorticities[x, y, sheetNo, 0].x * (elementPositions[x + 1, y, sheetNo, 0] - elementPositions[x - 1, y, sheetNo, 0]).normalized;
                    Vector3 vorY = elementSheetVorticities[x, y, sheetNo, 0].y * (elementPositions[x, y + 1, sheetNo, 0] - elementPositions[x, y - 1, sheetNo, 0]).normalized;
                    elementActualVorticities[x, y, sheetNo, 0] = vorX + vorY;

                }
            }
        }

    }

    //This function deterines cluster coefficients (THIS FUNCTION WORKS - NO MESSING)
    void calculateCoefficientsForAllAbstractions()
    {

        //cycle through all sheets
        for (int sheetNo = 0; sheetNo < numberOfSheets; sheetNo++)
        {

            //cycle through levels of "clustering"
            for (int degree = 1; degree < maxDegreeOfAbstraction + 1; degree++)
            {

                //Determine step size and 
                int stepSize = (int)Mathf.Pow(clusterSize, degree);
                int xSteps = sheetSpanx / stepSize;
                int ySteps = sheetSpany / stepSize;

                //cycle through the points
                for (int x = 0; x < xSteps; x++)
                {
                    for (int y = 0; y < ySteps; y++)
                    {

                        //First reset the values we're interested in
                        elementPositions[x, y, sheetNo, degree] = Vector3.zero;          //this line being debuged (is degree causing the problem)
                        elementActualVorticities[x, y, sheetNo, degree] = Vector3.zero;

                        //now that every index is cycled through for the given degree, we need to determine their values 
                        for (int xIn = 0; xIn < clusterSize; xIn++)
                        {
                            for (int yIn = 0; yIn < clusterSize; yIn++)
                            {

                                //perform sumations
                                elementActualVorticities[x, y, sheetNo, degree] += elementActualVorticities[x * clusterSize + xIn, y * clusterSize + yIn, sheetNo, degree - 1];
                                elementPositions[x, y, sheetNo, degree] += elementPositions[x * clusterSize + xIn, y * clusterSize + yIn, sheetNo, degree - 1];

                            }
                        }

                        //Now the average needs to be taken for the position
                        elementPositions[x, y, sheetNo, degree] = elementPositions[x, y, sheetNo, degree] / Mathf.Pow(clusterSize, 2);


                    }
                }


            }

        }

    }

    //Function to reset velocities
    void resetAllVelocities()
    {

        //Reset all velocities to zero
        for (int sheetNo = 0; sheetNo < numberOfSheets; sheetNo++)
        {
            for (int x = 0; x < sheetSpanx; x++)
            {
                for (int y = 0; y < sheetSpany; y++)
                {

                    //this cycles through all element, now reset their velocities
                    elementVelocities[x, y, sheetNo] = Vector3.zero;

                }
            }
        }

    }

    //This function simply shifts all elements on all sheets in  the positive y direct
    void shiftAllElementsByOne()
    {

        //All sheets neee to be considered so first they are cycled through
        for (int sheetNo = 0; sheetNo < numberOfSheets; sheetNo++)
        {

            //all elements need to be shifted one element to the positive Y direction
            for (int x = 0; x < sheetSpanx; x++)
            {
                for (int y = sheetSpany - 1; y > 0; y--)
                {

                    elementPositions[x, y, sheetNo, 0] = elementPositions[x, y - 1, sheetNo, 0];
                    elementVelocities[x, y, sheetNo] = elementVelocities[x, y - 1, sheetNo];
                    elementSheetVorticities[x, y, sheetNo, 0] = elementSheetVorticities[x, y - 1, sheetNo, 0];

                }
            }

        }

    }

    //this function assigns the location of new elements in global space
    void positionNewRowOfElements()
    {

        //firstly all sheets are cycled through
        for (int sheetNo = 0; sheetNo < numberOfSheets; sheetNo++)
        {
            
            //now the initial row of the sheet needs to be cycled through
            for (int x = 0; x < sheetSpanx; x++)
            {

                //now the new position is calculated and converted to global space
                elementPositions[x, 0, sheetNo, 0] = transform.TransformPoint(sheetSpawnLocations[sheetNo, x]);
                //elementSheetVorticities[x, 0, sheetNo, 0] = new Vector2(0.0f, 0.1f);
                //Debug.Log(transform.TransformPoint(sheetSpawnLocations[sheetNo, x]));

            }

        }

    }

    //this function calcualtes teh initial vorticities of the newly spawned elements (see accompanying documentation)
    void applyInitialVorticityOfElements()
    {

        //firstly all sheets are cycled through
        for (int sheetNo = 0; sheetNo < numberOfSheets; sheetNo++)
        {

            //now the initial row of the sheet needs to be cycled through
            for (int x = 0; x < sheetSpanx; x++)
            {

                //now the new vorticity can be calculated and applied
                //first the sinusoidal contribution
                float xSine = xVorticityMagnitude * Mathf.Pow(Mathf.Sin((Time.time * sineMultiplier) + Mathf.PI/2),2);
                float ySine = yVorticityMagnitude * Mathf.Pow(Mathf.Sin((Time.time * sineMultiplier) + sineOffset),2);

                //Now the noise contributions are calculated
                float xNoise = (rand.Next(-100, 100) / 100.0f) * xNoiseMagnitude;
                float yNoise = (rand.Next(-100, 100) / 100.0f) * yNoiseMagnitude;

                //now the horseshoe vortex contributions are calculated
                float horseshoeEffect = (x - (sheetSpanx / 2)) * horseshoeEffectMagnitude;

                //now all the additions are accumulated
                Vector2 newVorticity = new Vector2(xSine + xNoise, ySine + yNoise + horseshoeEffect);

                //now the vorticity can be applied
                elementSheetVorticities[x, 0, sheetNo, 0] = newVorticity * globalEffectMultipler;

            }

        }

    }

    //This function if called by unity when gizmos are drawn, to keep the code clean only function calls are made here
    private void OnDrawGizmos()
    {

        visualiseElements(0.05f);     //This calls the function to display the element positions and sheet orientation
        visualiseGrid();
        //visualiseHigherAbstractionPositions(0.05f);

    }

    //This functions displays the simulation via using gizmos
    void visualiseElements(float elementSize)
    {

        //The visualization should only be performed if the used wants to see the information, hence the toggle is checked for
        if (displayGizmos == true)
        {

            //Firstly the positons of every element is cycled through so it can be displayed
            for (int x = 0; x < sheetSpanx; x++)
            {
                for (int y = 0; y < sheetSpany; y++)
                {

                    //now all elements positions are cycled through, however there are numerous sheets, so these need to be cycled through
                    for (int sheetNo = 0; sheetNo < numberOfSheets; sheetNo++)
                    {

                        //Now the element can be represented with a gizmo
                        Gizmos.DrawSphere(elementPositions[x, y, sheetNo, 0], elementSize);

                    }

                }
            }
            

        }

    }

    //This function draws the lines betwean the elements to represent the sheet (again segregated from the element funtion to keep the code clean)
    void visualiseGrid()
    {

        //as always this needs to be performed for all sheets
        for (int sheetNo = 0; sheetNo < numberOfSheets; sheetNo++)
        {

            //firstly the grid lines spanning the x direction is draw, this requires cycling through all x positions, and y-1 positions (as there are y-1 grid lines)
            for (int x = 0; x < sheetSpanx; x++)
            {
                for (int y = 0; y < sheetSpany - 1; y++)
                {

                    //now the lines can be drawn
                    Gizmos.DrawLine(elementPositions[x, y, sheetNo, 0], elementPositions[x, y + 1, sheetNo, 0]);

                }
            }

            //similiarly the grid lines in the y direction are draw, but the cycle is inversed (x-1 positions and all y)
            for (int y = 0; y < sheetSpany; y++)
            {
                for (int x = 0; x < sheetSpanx - 1; x++)
                {

                    //now the lines can be drawn
                    Gizmos.DrawLine(elementPositions[x, y, sheetNo, 0], elementPositions[x + 1, y, sheetNo, 0]);

                }
            }

        }

    }

    //this function is for displaying the cluster positions, it is primerily for debugging purposes if effects look ifffy due to the optimization algorithm
    void visualiseHigherAbstractionPositions(float elementSize)
    {

        //This visualization is "dumb" in that it will check every position in the array and not just 

        //The visualization should only be performed if the used wants to see the information, hence the toggle is checked for
        if (displayGizmos == true)
        {

            //Firstly the positons of every element is cycled through so it can be displayed
            for (int x = 0; x < sheetSpanx; x++)
            {
                for (int y = 0; y < sheetSpany; y++)
                {

                    //now all elements positions are cycled through, however there are numerous sheets, so these need to be cycled through
                    for (int sheetNo = 0; sheetNo < numberOfSheets; sheetNo++)
                    {

                        //Now the element can be represented with a gizmo
                        Gizmos.DrawSphere(elementPositions[x, y, sheetNo, 1] + new Vector3(0.0f, 0.5f, 0.0f), elementSize);

                    }

                }
            }


        }

    }


}
