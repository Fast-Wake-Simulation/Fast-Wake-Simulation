using System;
using System.Linq;
//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridHandler : MonoBehaviour {

    //Section for all necessary interface references
    public Button createGrid;               //This is the button that create the grid, this will probable be changed at a later date to real time production
    public InputField  gridRows;                  //variable to store the inpit field for number of rows
    public InputField gridColumns;               //variable to store input field for number of columns
    public InputField gridSpacing;               //variables to store input field for the the grid spacing (this could be changed later for non square spacing)
    private int gridRowsInt;
    private int gridColumnsInt;
    private int gridSpacingInt;

    //Section for arrays that govern the vortex blob position and strength
    private Vector3[,] vortonPositions;
    private Vector3[] meshVertices;

    //Section for the grid mesh arrays+variables+objects
    private Mesh gridMesh;
    private bool gridSpawned = false;
    private bool meshSpawned = false;
    private float spawnTimer = 5f;
    private float lastTime;
    private int initialRows = 0;

    //Convection related variables
    private float blobDistance;
    private Vector3 blobVelocityDirection;
    private float blobVelocityMagnitude;
    private float[,] blobVorticity;
    private Vector3 blobFilamentDifferential;
    private Vector3[,] blobInitialVelocity;
    private Vector3[,] blobVelocity;

    //Clustered convection variables and arrays (the above are shared betwean the simple and clustered algorithms)
    private Vector3[,] clusterPositionCoef;
    public float[,] clusterVorticityCoef;
    private Vector3[,] clusterInitialVelocityCoef;
    private float[,] clusterVelocityCoef;
    private int clusterSize = 3;
    private float vorticityCount;
    private int clusterXIndex;
    private int clusterYIndex;


	// Use this for initialization
	void Start () {

        //Assign all UI buttons their respective listeners (functions/methods called per click
        createGrid.onClick.AddListener(CreateGridCall);

        //Get required references via searches
        GetComponent<MeshFilter>().mesh = gridMesh = new Mesh();    gridMesh.name = "Grid Mesh";


    }
	
	// Update is called once per frame
	void Update () {


    }

    //Update is called once per physics engine update (use for convection math if the rigid body technique is used, however may be replaced with a second order accurate scheme for the integration)
    void FixedUpdate()
    {

        //call functions required
        RenderGridMesh();                //This function is responsible for creating the mesh object on the vortex plane
                                         //sampleAnimation();            //Use this to test the vertex/position arrayy relation (or rendering stuff if convection cant be used)
        handleGrid();
        //simpleConvection();
        clusteredConvection();
        

    }

    //This is the function to create the grid, it is attahed as a listener to a button component above
    void CreateGridCall()
    {

        //Necessary variable conversions
        gridSpacingInt = Convert.ToInt32(gridSpacing.text);                                                 //This is converted here as its required for every grid spacing (as opposed to the other conversions that are required only once)
        gridRowsInt = Convert.ToInt32(gridRows.text);                                                       //Same as above
        gridColumnsInt = Convert.ToInt32(gridColumns.text);                                                 //Same as above
        vortonPositions = new Vector3[Convert.ToInt32(gridRows.text), Convert.ToInt32(gridColumns.text)];   //2D Array to hole vorton position (public and declared earlier, however size specified here)
        blobInitialVelocity = new Vector3[Convert.ToInt32(gridRows.text), Convert.ToInt32(gridColumns.text)];
        blobVelocity = new Vector3[Convert.ToInt32(gridRows.text), Convert.ToInt32(gridColumns.text)];
        blobVorticity = new float[Convert.ToInt32(gridRows.text), Convert.ToInt32(gridColumns.text)];
        meshVertices = new Vector3[(gridRowsInt * gridColumnsInt)];                                         //1D Array of vorton points, used for vertices on the 
        gridMesh.Clear();                                                                                   //Ensures the mesh is blank, helps with retroactively creating new grids in enviroment but considerations to all buffers is not yet given

        //The following viarbes/arrays are for the clustered convection algorithm
        clusterVorticityCoef = new float[gridRowsInt, gridColumnsInt];
        clusterVelocityCoef = new float[gridRowsInt, gridColumnsInt];
        clusterPositionCoef = new Vector3[gridRowsInt, gridColumnsInt];
        clusterInitialVelocityCoef = new Vector3[gridRowsInt, gridColumnsInt];


        //Seed the vorton position array with coordinates for the vortons
        for (int nc = 0, n = 0; nc <gridColumnsInt; nc++)
        {
            for (int nr = 0; nr < gridRowsInt; nr++, n++)
            {
                //vortonPositions[nr, nc] = new Vector3(nr * gridSpacingInt, 0.1f, nc * gridSpacingInt);
                vortonPositions[nr, nc] = new Vector3(0, 0.1f, nc * gridSpacingInt);
                blobVorticity[nr, nc] = 0;                                          //-0.1f*Mathf.Pow((nc-(gridRowsInt/2)),3);
                blobInitialVelocity[nr,nc] = new Vector3(2.5f, 0, 0.0f);
                blobVelocity[nr,nc] = new Vector3(0.0f, 0, 0);
                meshVertices[n] = new Vector3(nr * gridSpacingInt, 0.1f, nc * gridSpacingInt);
            }
        }

        //create the mesh for the grids
        gridMesh.vertices = meshVertices;

        //Create triangle array of appropriate size
        int[] triangles = new int[gridRowsInt*(gridColumnsInt-1)*3*2];

        //Populate the triangle array with vertices
        for (int nc = 0, n = 3; nc < (gridColumnsInt-1); nc++)
        {
            for (int nr = 0; nr < gridRowsInt-1; nr++, n = n + 3)
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
        //gridMesh.Optimize();

        //Set values for all variables other functions rely on
        gridSpawned = true;                                         //This is used when new grids are made after an itial grid and the old grid needs to be removed
        lastTime = Time.realtimeSinceStartup;                       //this is used by the grid handler to determine when to spawn new rows
        initialRows = gridRowsInt;
        

    }

    void RenderGridMesh()
    {

        //This functions just manages the the vertex-vortonpositon relationship
        //Seed the vorton position array with coordinates for the vortons
        for (int nc = 0, n = 0; nc < gridColumnsInt; nc++)
        {
            for (int nr = 0; nr < gridRowsInt; nr++, n++)
            {
                meshVertices[n] = vortonPositions[nc,nr];
            }
        }

        //create the mesh for the grids
        gridMesh.vertices = meshVertices;
        gridMesh.RecalculateNormals();

    }

    void sampleAnimation()
    {

        //simple sine wave animation to check the grids moves properly
        if (gridSpawned == true)
        {
            for (int x = 0; x < gridRowsInt; x++)
            {
                for (int y = 0; y < gridRowsInt; y++)
                {
                    vortonPositions[x, y] = new Vector3(gridSpacingInt * x,Mathf.Sin(Time.time+x*y),gridSpacingInt*y);
                }
            }
        }

    }

    //Function to handle simple un optimized convection
    void simpleConvection()
    {

        //Clear Required Variables

        //Loop to cycle through all vortex blobs
        for (int x = 1; x < gridRowsInt; x++)
        {
            for (int y =0; y < gridColumnsInt; y++)
            {
                //Reset filament differential
                blobFilamentDifferential = Vector3.zero;

                //Now this will cycle through all blobs, but then all other blobs must be cycled through
                for (int nr = 0; nr < gridRowsInt; nr++)
                {
                    for(int nc = 0; nc < gridColumnsInt; nc++)
                    {
                        if ( x != nr && y != nc)
                        {

                            //Convect the blobs effect
                            blobDistance = Vector3.Distance(vortonPositions[x,y],vortonPositions[nr,nc]);
                            blobVelocityDirection = Vector3.Cross((vortonPositions[nr, nc] - vortonPositions[x, y]).normalized, blobInitialVelocity[nr, nc]);
                            blobVelocityMagnitude = blobVorticity[nr,nc] / (2.0f * Mathf.PI * blobDistance);
                            blobFilamentDifferential += blobVelocityMagnitude * blobVelocityDirection;

                        }
                    }
                }

                //set new velocity value
                blobVelocity[x, y] = blobInitialVelocity[x,y] + blobFilamentDifferential;
                //Debug.Log(blobVelocity[x, y]);
                blobFilamentDifferential = Vector3.zero;
                vortonPositions[x,y] += blobVelocity[x,y]*Time.deltaTime;
            }
        }
    }

    //This is the clustered convection agorithm, this should run much faster
    void clusteredConvection()
    {

        //First step in the method is to create a matrix of cluster vorticity and location coficients
        for (int x = 0, xindex = 0; x < (gridRowsInt); x = x + clusterSize, xindex++)
        {
            for (int y = 0, yindex = 0; y < (gridColumnsInt); y = y + clusterSize, yindex++)
            {

                //Reset the vorticity count 
                vorticityCount = 0.0f;

                //inside this loop will give cycle through the corner points of eah custer
                //This next loop cycles through the vortons that should be in the current cluster at corner point x,y
                for (int nx = 0; nx < clusterSize; nx++)
                {
                    for (int ny = 0; ny < clusterSize; ny++)
                    {

                        //summate all the vorticities 
                        vorticityCount = vorticityCount + blobVorticity[x+nx,y+ny];

                    }
                }

                //set the vorticity coefficient
                clusterVorticityCoef[xindex, yindex] = vorticityCount;

                //Set the clusters equivalent position
                clusterPositionCoef[xindex, yindex] = vortonPositions[x+1,y+1];             //This is a very crude midpoint aproximation, however as a first order scheme it will suffice    
                clusterInitialVelocityCoef[xindex, yindex] = blobInitialVelocity[x + 1, y + 1];

            }
        }

        //The blobs are now clustered into clusters of size clusterSize, now the convection maths needs to be performed
        //This loop cycles through the clusters and convects each blob in a given cluster
        for (int cx = 0; cx < (gridRowsInt/clusterSize); cx++)
        {
            for (int cy = 0; cy < (gridColumnsInt/clusterSize); cy++)
            {
                
                // This loop cycles through the individual blobs in a cluster
                for (int bx = 0; bx < clusterSize; bx++)
                {
                    for (int by = 0; by < clusterSize; by++)
                    {

                        //variables to get indices of blobs
                        clusterXIndex = cx * clusterSize;
                        clusterYIndex = cy * clusterSize;

                        //this loop cycles through the clusters individual blobs
                        blobFilamentDifferential = Vector3.zero;

                        //First the blobs should be convected with the blobs inside their own cluster
                        for (int ix = 0; ix < clusterSize; ix++)
                        {
                            for (int iy = 0; iy < clusterSize; iy++)
                            {
                                
                                //This ensures the convected blob doesnt convect itself
                                if ( ix != bx && iy != by)
                                {

                                    //Convect the blob!
                                    blobDistance = Vector3.Distance(vortonPositions[clusterXIndex + bx, clusterYIndex + by], vortonPositions[clusterXIndex + ix, clusterYIndex + iy]);
                                    blobVelocityDirection = Vector3.Cross((vortonPositions[clusterXIndex + ix, clusterYIndex + iy] - vortonPositions[clusterXIndex + bx, clusterYIndex + by]).normalized, blobInitialVelocity[clusterXIndex + ix, clusterYIndex + iy]);
                                    blobVelocityMagnitude = blobVorticity[clusterXIndex + ix, clusterYIndex + iy] / (2.0f * Mathf.PI * blobDistance);
                                    blobFilamentDifferential += blobVelocityMagnitude * blobVelocityDirection;

                                }

                            }
                        }

                        //Now its been convected with the blobs in its cluster, it needs to be convected by the clusters them selves
                        for ( int ccx = 0; ccx <(gridRowsInt/clusterSize); ccx++)
                        {
                            for ( int ccy = 0; ccy <(gridColumnsInt/clusterSize); ccy++)
                            {
                                if ( cx != ccx && cy != ccy)
                                {

                                    //this loop cycles through the other clusters
                                    blobDistance = Vector3.Distance(vortonPositions[clusterXIndex + bx, clusterYIndex + by],clusterPositionCoef[ccx,ccy]);
                                    blobVelocityDirection = Vector3.Cross((clusterPositionCoef[ccx, ccy] - vortonPositions[clusterXIndex + bx, clusterYIndex + by]).normalized, clusterInitialVelocityCoef[ccx,ccy]);
                                    blobVelocityMagnitude = clusterVorticityCoef[ccx,ccy]/ (2.0f * Mathf.PI * blobDistance);
                                    blobFilamentDifferential += blobVelocityMagnitude * blobVelocityDirection;

                                }
                            }
                        }

                        //This math solves discretized and solves the velocity time relationship, but checks for the first row to keep it bound
                        if ((clusterXIndex + bx) > 0)
                        {

                            blobVelocity[clusterXIndex + bx, clusterYIndex + by] = blobInitialVelocity[clusterXIndex + bx, clusterYIndex + by] + blobFilamentDifferential;
                            vortonPositions[clusterXIndex + bx, clusterYIndex + by] += blobVelocity[clusterXIndex + bx, clusterYIndex + by] * Time.deltaTime;

                        }
                    }
                }

            }
        }


    }

    //This function handles the removal and creation of new blobs
    void handleGrid()
    {

        //check whether grid needs to be updates
        if (Time.realtimeSinceStartup > (lastTime + spawnTimer))
        {

            //Reset te timer
            lastTime = Time.realtimeSinceStartup;
            //Debug.Log("I should spawn now");
            //Debug.Log(clusterPositionCoef[2, 2]);
            //Debug.Log(clusterVorticityCoef[2, 2]);

            //Remove blobs that shouldnt be alive yet
            //for ( int i = 1; i <)
           
            //Sub routine to shift rows, this automatically deletes the last rows
            for (int x = gridRowsInt-2; x > 0; x--)
            {
                for (int z = 0; z < gridColumnsInt; z++)
                {

                    vortonPositions[x + 1, z] = vortonPositions[x, z];
                    blobVorticity[x + 1, z] = blobVorticity[x, z];
                    blobVelocity[x + 1, z] = blobVelocity[x, z];
                    blobInitialVelocity[x + 1, z] = blobInitialVelocity[x, z];
                    //vortonPositions[x + 1, z] = vortonPositions[x, z];
                    //vortonPositions[x + 1, z] = vortonPositions[x, z];

                }
            }

            //creat new blobs
            for (int y =  0; y < gridRowsInt; y++)
            {
                vortonPositions[1, y] = new Vector3(0.0f , 0.1f, y * gridSpacingInt);
                blobVorticity[1, y] = -0.04f * Mathf.Pow((y - (gridRowsInt / 2)), 3);
               
                //biasedly creat vorticity
                

                    blobInitialVelocity[1, y] = new Vector3(18.5f, 0, 0.0f);
                blobVelocity[1, y] = new Vector3(18.5f, 0.0f, 0.0f);
            }

        }

    }

    //This function draws gizmos so that each blob can be visualised 
    private void OnDrawGizmos() 
    {

        Gizmos.color = Color.red;
        for (int x = 0; x < gridRowsInt; x++)
        {
            for (int y = 0; y < gridColumnsInt; y++)
            {
                Gizmos.DrawSphere(vortonPositions[x, y], 0.1f);     // this shows control points as red spheres in the edit window
            }
        }

    }

}
