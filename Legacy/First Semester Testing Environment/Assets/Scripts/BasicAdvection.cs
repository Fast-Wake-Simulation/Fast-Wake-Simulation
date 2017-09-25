using UnityEngine;
using System.Collections;

public class BasicAdvection : MonoBehaviour {

    //Simulation based variables
    public GameObject[] currentBlobs;   //Array to hold blob game objects
    public Vector3 currentPosition;
    public Vector3 velocityDirection;
    public float velocityMagnitude;
    public float distance;
    public Vector3 initialVelocity = new Vector3(5.0f, 0.0f, 0.0f);
    public Vector3 initialisationVelocity = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 filamentVelocityDifferential = Vector3.zero;
    public bool shouldExecute = false;
    public bool findBlobObjects = false;
    public VortexBlobBehaviour[] advectingBlob;

    //Monitor based variables
    public int iterations = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        //Get all the current blobs on the scene, this is a slow method, but what can you do?
        if (findBlobObjects == true)
        {
           
            //Get all current blob game objects
            currentBlobs = GameObject.FindGameObjectsWithTag("VortexBlob");
            Debug.Log(currentBlobs.Length);

            //store references to each script
            for (int i = 0; i < currentBlobs.Length - 1; i++)
            {
                //Debug.Log(advectingBlob.Length);
                //advectingBlob[1] = currentBlobs[1].GetComponent<VortexBlobBehaviour>();
                //VortexBlobBehaviour advectingBlob[i] = currentBlobs[i].GetComponent<VortexBlobBehaviour>();
            }
            //Reset flip flop bool and output text to console
            //VortexBlobBehaviour advectingBlob = currentBlobs.GetComponent<VortexBlobBehaviour>();
            findBlobObjects = false;
            Debug.Log("References Initialized!");
        }

    }

    //Advection routing should run in fixed update to keep in time with the physics engine
    void FixedUpdate()
    {



        //Debug.Log("Im alive");
        if (iterations > 0)
        {
            

            //For loop to start iterating through the vortons
            for (int i = 0; i < currentBlobs.Length; i++)
            {

                //Store variables so they dont need to be called in the subsequent for loops
                currentPosition = currentBlobs[i].transform.position;
                VortexBlobBehaviour advectingBlob = currentBlobs[i].GetComponent<VortexBlobBehaviour>();

                for (int j = 0; j < currentBlobs.Length; j++)
                {
                    
                    //Checkif the blob in question if the current blob, if so disregard it
                    if (i != j)
                    {
                        //Get distance to the blob
                        VortexBlobBehaviour advectedBlob = currentBlobs[j].GetComponent<VortexBlobBehaviour>();
                        distance = Vector3.Distance(currentPosition, currentBlobs[j].transform.position);
                            if (advectedBlob.VortexBlobStrength/distance > 1.0)
                        {

                        //Calculate direction and magnitude of the blobe
                        velocityDirection = Vector3.Cross((currentBlobs[j].transform.position - currentPosition).normalized, advectedBlob.initialisationVelocity);
                        velocityMagnitude = advectedBlob.VortexBlobStrength / (2.0f * Mathf.PI * distance);
                        filamentVelocityDifferential += velocityMagnitude * velocityDirection;          //Figure out the velocity differential

                        }
                    }

                    
                }

                //Advect the blob
                Rigidbody velocitySetup = currentBlobs[i].GetComponent<Rigidbody>();
                velocitySetup.velocity = advectingBlob.initialVelocity + filamentVelocityDifferential;
                filamentVelocityDifferential = Vector3.zero;

            }

            //Handle iteration count
            iterations = iterations - 1;


        }
            
    }
}
