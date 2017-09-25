using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DVM : MonoBehaviour {

    //Componenets to store gameobject and rigidbody component
    public GameObject parentObject;
    public Rigidbody rb;

    //variables for the editor for the user
    public int ElementSpanCountX;
    public int ElementSpanCountY;
    public float spawnCounter;
    public int numberOfSheets;

    //Arrays for vortex sheets
    private Vector3[,,] elementPositions = new Vector3[555, 555, 2];
    private Vector3[,,] elementVelocities = new Vector3[555, 555, 2];
    private Vector2[,,] elementVorticities = new Vector2[555, 555, 2];
    private Vector3[,,] elementSheetwiseVorticity = new Vector3[555, 555, 2];
    private int rowsActive = 0;

    //arrays for vortex sheet positions
    private Vector3[] initialPoint = new Vector3[555];
    private Vector3[] endPoint = new Vector3[555];
    private Vector3[] spawnUnitVector = new Vector3[555];

    //Random Generator Variables
    private System.Random rand = new System.Random();

    //testing variables (THIS SHOULD BE ClEARED A LOT)
    private float lastTime;
    private Vector3 testPosition = Vector3.zero;
    private bool flipFlop = false;

	// Use this for initialization
	void Start () {

        //testing variables (THIS SHOULD BE ClEARED A LOT)
        initialPoint[0] = new Vector3(0.0f, -1.5f, -0.4f);
        endPoint[0] = new Vector3(0.0f, 1.5f, -0.4f);
        initialPoint[1] = new Vector3(0.6f, -0.7f, -2.4f);
        endPoint[1] = new Vector3(0.6f, 0.7f, -2.4f);


        setupDispatcher();

	}
	
	// Update is called once per frame
	void Update () {

        if (timerFunction(0.25f))
        {
            spawnNewRow();
        }

        //general loop
            convectionDispatcher();

        

    }


    //function to manage the timer (use this function to check if a certain routine should be called
    bool timerFunction(float timeInterval)
    {

        //this is the bool used as the return for the function
        bool returnBool = false;

        //simple compare the disparity
        if (Time.time - lastTime > timeInterval)
        {

            lastTime = Time.time;       //resets the timer
            returnBool = true;          //returns a true value

        }

        return returnBool;

    }

    //function to create a new row of elements
    void spawnNewRow()
    {



        //shift all elements

        for (int x = 0; x < ElementSpanCountX; x++)
        {
            for (int y = ElementSpanCountY; y > -1; y--)
            {
                for (int sheetNo = 0; sheetNo < numberOfSheets; sheetNo++)
                {
                    elementPositions[x, y + 1, sheetNo] = elementPositions[x, y, sheetNo];
                    elementVorticities[x, y + 1, sheetNo] = elementVorticities[x, y, sheetNo];

                }
                
            }
        }

        //This creates a new row of elements
        for (int x = 0; x < ElementSpanCountX; x++)
        {

            for (int sheetNo = 0; sheetNo < numberOfSheets; sheetNo++)
            {
                elementPositions[x, 0, sheetNo] = transform.TransformPoint(initialPoint[sheetNo] + spawnUnitVector[sheetNo] * x);
                elementVorticities[x, 0, sheetNo] = new Vector2(0.0f * Mathf.Sin(rand.Next()) * rb.velocity.magnitude, 0.5f * Mathf.Sin(rand.Next()) * Mathf.Pow(rb.velocity.magnitude, 2));
            }

        }

    }

    //initialization function (grabs references basically)
    void SetupInitialization()
    {

        //testing variables (THIS SHOULD BE CLEARED A LOT)
        lastTime = Time.time;

        //get required information
        Rigidbody rb = GetComponent<Rigidbody>();

    }

    //This function is responsible for calling all setup related function
    void setupDispatcher()
    {

        SetupInitialization();       //This is the old setup function - replaced by the more segregated dispatcher function now, should be redone at some point
        calculateLocalSpawnLocations();

    }

    //this function find the spawn locations in local space
    void calculateLocalSpawnLocations()
    {

        //cycle through all sheets
        for (int iteration = 0; iteration < numberOfSheets; iteration++)
        {

            //first find the vector from the two points
            Vector3 completeVector = endPoint[iteration] - initialPoint[iteration];

            //now divide this by the number of points per sheet and set it to the unit vector
            spawnUnitVector[iteration] = completeVector / ElementSpanCountX;

        }

    }

    //calculate full vorticities
    void calculateSheetwiseVorticity()
    {

        //cycle through all elements
        for (int iteration = 0; iteration < numberOfSheets; iteration++)
        {
            for (int x = 1; x < ElementSpanCountX; x++)
            {
                for (int y = 1; y < ElementSpanCountY; y++)
                {
                    //find vorticity in x and y
                    Vector3 vorX = elementVorticities[x, y, iteration].x * (elementPositions[x + 1, y, iteration] - elementPositions[x - 1, y, iteration]).normalized;
                    Vector3 vorY = elementVorticities[x, y, iteration].y * (elementPositions[x, y + 1, iteration] - elementPositions[x, y - 1, iteration]).normalized;
                    elementSheetwiseVorticity[x, y, iteration] = vorX + vorY;
                }
            }
        }
        

    }

    //function to handle the convection routinge
    void convectionDispatcher()
    {

        //first find the actual vorticities
        calculateSheetwiseVorticity();

        //this function is simple to start with, just a brute force approach for the 10x10 grid
        for (int iteration = 0; iteration < numberOfSheets; iteration++)
        {
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {

                    ///the currrent elements velocity needs to be zeroed
                    elementVelocities[x, y, iteration] = Vector3.zero;
                    

                    //now all elements are cycled through, all other elements need to be cycled through 
                    for (int xi = 0; xi < 10; xi++)
                    {
                        for (int yi = 0; yi < 10; yi++)
                        {

                            //now check we're not convecting with ourselves
                            if (x != xi && y != yi)
                            {

                                //First the radius,
                                Vector3 radius = elementPositions[x, y, iteration] - elementPositions[xi, yi, iteration];

                                //now the cumulative velocity fields can be superimposed
                                elementVelocities[x, y, iteration] += (1f / (4f * Mathf.PI)) * (Vector3.Cross(elementSheetwiseVorticity[xi, yi, iteration], radius) / (Mathf.Pow(radius.magnitude, 2)));
                            }

                        }
                    }

                }
            }
        }

        //now iterate all the positions
        updatePositions();

    }

    //this function is the temporal discretization (eulers method now, should be 3rd order eventually)
    void updatePositions()
    {

        //cycle through all elements (for now 10 to keep overheads low)
        for (int iteration = 0; iteration < numberOfSheets - 0; iteration++)
        {
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    //
                    if (flipFlop == false)
                    {
                        //flipFlop = true;
                    }
                    else
                    {
                        elementPositions[x, y, iteration] += elementVelocities[x, y, iteration] * Time.deltaTime;
                    }


                    if (Input.GetKey("space"))
                    {
                        //Debug.Log(elementVelocities[x,y,iteration]);
                        //elementPositions[x, y, iteration] += elementVelocities[x, y, iteration] * Time.deltaTime;
                        //Debug.Log(elementSheetwiseVorticity[x, y, iteration]);
                        flipFlop = true;
                    }
                }
            }
        }

    }

    //function to render element positions with gizmos
    private void OnDrawGizmos()
    {

        //testing sphere
        //Gizmos.DrawSphere(new Vector3(0f, 0f, 0f), 5.0f);
        //Gizmos.DrawSphere(testPosition, 0.1f);

        //this section just for positions
        for (int columns = 0; columns < ElementSpanCountX - 1; columns++)
        {
            for (int rows = 0; rows < ElementSpanCountY - 1; rows++)
            {
                for (int sheetNo = 0; sheetNo < numberOfSheets; sheetNo++)
                {
                    Gizmos.DrawSphere(elementPositions[columns, rows, sheetNo], 0.05f);
                }
            }
        }

        //draw the lines betwean the elements
        for (int x = 0; x < ElementSpanCountX; x++)
        {
            for (int y = 0; y < ElementSpanCountY - 1; y++)
            {
                for (int sheetNo = 0; sheetNo < numberOfSheets; sheetNo++)
                {
                    Gizmos.DrawLine(elementPositions[x, y, sheetNo], elementPositions[x, y + 1, sheetNo]);
                }
            }
        }
        for (int x = 0; x < ElementSpanCountX - 1; x++)
        {
            for (int y = 0; y < ElementSpanCountY; y++)
            {

                //Gizmos.DrawLine(elementPositions[x, y, 0], elementPositions[x + 1, y, 0]);

            }
        }

    }
}
