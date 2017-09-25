using UnityEngine;
using System.Collections;

public class ComplexAdvection : MonoBehaviour {

    //public stuff
    public int iterationCount = 0;
    public float[] clusterStrength;     //Hold the strength of a vorton cluster
    public float[] clusterPosition;     //Holds the position of a cluster
    public GameObject[,,,] clusterObjects;  //Holds references to all individua vortons in a cluster
    public GameObject[] currentBlobs;   //Hold a reference to all current blobs
    public bool calculateGroupings;
    public int clusterSpacing = 3;
    public int maxX;                    //Maximum X grid dimension
    public int maxY;                    //same as above but for t
    public int posX;                        //Just to be used in a loop
    public int posY;
    private int curX;
    private int curY;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

        //calculate groupings if necessary
        if (calculateGroupings == true)
        {

            //Start by finding all curent blob objects
            currentBlobs = GameObject.FindGameObjectsWithTag("VortexBlob");

            //Find maximum dimensions
            maxX = 0;           //Reseting values
            maxY = 0;
            for (int i = 0; i < currentBlobs.Length; i++)
            {
                VortexBlobBehaviour scriptRef = currentBlobs[i].GetComponent<VortexBlobBehaviour>();
                //Hande x length
                    if (scriptRef.positionIDX > maxX)
                {
                    maxX = scriptRef.positionIDX;
                }
                //Handle y length
                if (scriptRef.positionIDY > maxY)
                {
                    maxY = scriptRef.positionIDY;
                }
            }


            //cycle through a square of blobs and give references to them all in an array
            for (int clusterX = 1; clusterX < (maxX/clusterSpacing)+1; clusterX++)
            {
                //Calculate position of clusters in unclustered grid in x dimension
                posX = (clusterX * clusterSpacing)-2;       //this line gives
                
                //Add cyclic routing for y-dimension is unclustered grid
                for (int clusterY = 1; clusterY < (maxY/clusterSpacing)+1; clusterY++)
                {

                    //Calculate position of clusters in unclustered grid in y dimension
                    posY = (clusterY * clusterSpacing) - 2;

                    //Cycle through the blobs in the unclustered grid that need to be added to the cluster element
                    for (int x = 0; x < clusterSpacing; x++)
                    {
                        for (int y = 0; y < clusterSpacing; y++)
                        {

                            //calculate positon in unclustered grid to put in clusters
                            curX = posX + x;
                            curY = posY + y;

                            //Enter GameObject references into clustered grid
                            foreach (GameObject unclustered in currentBlobs)
                            {
                                VortexBlobBehaviour unclusteredScriptReference = unclustered.GetComponent<VortexBlobBehaviour>();
                                if (unclusteredScriptReference.positionIDX == curX && unclusteredScriptReference.positionIDY == curY)
                                {
                                    clusterObjects[posX,posY,x,y] = unclustered;
                                    Debug.Log(posX);
                                }
                            }

                        }
                    }

                }

            }




            //reset flip flop counter
            calculateGroupings = false;
        }

    }

    //simulation should be called in fixed update to maintain a fixed timestep
    void FixedUpdate()
    {
        //check if iterations should run
        if (iterationCount > 0)
        {

            //Get reference to all blobs
            currentBlobs = GameObject.FindGameObjectsWithTag("VortexBlob");

            //Calculate cluster coefficients
            for (int i = 0; i < 4; i++)
            {

            }


            //Reduce the iteration count every loop
            iterationCount = iterationCount - 1;

            //Calculate the Blobs again


        }
    }
}
