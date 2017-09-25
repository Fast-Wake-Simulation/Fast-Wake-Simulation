//################################################################################
//# Script that controls how gameobjects react to the vortex blobs present in the 
//#	flow by directly altering their velocity.
//#	
//# Currently the script is only attached to vortex blobs so that one vortex 
//# blob will react to all the other vortex blobs in the flow. It is also possible
//# to attach this script to probe particles that do not have any effect on the 
//# shape of the flow, allowing for a more detailed mapping of the wake. 
//#
//# The equation employed is the same as for a vortex point in 2D flow:
//# 				   vortexStrength/(2*PI*distance)
//# 
//#	NOTE: Currently, even with a good CPU, the simulation struggles with having
//#	more than 90 vortex blobs in the scene. I have done some profiling and found that
//#	95% of the vortex blob simulation load comes from this script. Hence, if someone
//#	works on this project at a later point, one of his/her focus points should be 
//#	optimising this script such that more particles can be used. 


using UnityEngine;
using System.Collections;

public class VortexBlobBehaviour : MonoBehaviour {

	//Declaration of variables

	//public
	public Vector3 initialVelocity = new Vector3 (1.0f, 0.0f,0.0f);  //Important for wind tunnel test code
	public Vector3 initialisationVelocity = Vector3.zero; 			 //Initial velocity of the vortex blobs when spawned 
	public float testValue;											 //Test value for the exclusion optimisation method, more on this later
	public float VortexBlobStrength;								 //Vortex strenth of the vortex blob
	public int ID = -1;												 //ID of the vortex blob this script is attached to. This is set externally by the "CleanCollection" script
    public int iterationCount;
    public int positionIDX;
    public int positionIDY;

    public Vector3 distanceV;										 //Distance vector between blobs	
	private float velocityMagn;										 //Magnitude of the velocity differential caused by a blob on the blob which this script is being run on
	private Vector3 velocityDir;									 //Direction of the velocity differential caused by a vortex blob
	private Rigidbody RB;											 //Stores the rigidbody component of the blob this script is attached to
	private Vector3 filamentVelocityDifferential = Vector3.zero;     //Velocity difference caused by the blobs on the scene. Effectively consists of the sum of all the velocityMagn*velocityDir contributions
									
	private GameObject[] vortexBlobArray;							 //Array storing all the vortex blobs
	private GameObject[] vortexBlobBoundArray;						 //Array storing all the wing bound vortex blobs
	private float distance;											 //Magnitude of the distance vector
	public Vector3 probe;											 //DEBUG FEATURE 	

	void Start () 
	{
		RB = GetComponent<Rigidbody>();								 //Assigns the rigidbody of the blob to a private variable for ease of use

	}

    void Update()
    {
        
    }

    void FixedUpdate()
    {

        //If statement controls whether the script should run
        if (iterationCount > 0)
        {


            //Stores all the vortex blobs and bound vortex blobs in gameobject arrays
            vortexBlobArray = GameObject.FindGameObjectsWithTag("VortexBlob");
            //vortexBlobBoundArray = GameObject.FindGameObjectsWithTag("VortexBlobBound");

            //Determine whether to iterate or not

            //For loop that evaluates the velocity differential caused by the vortex blobs in the scene
            for (int i = 0; i < vortexBlobArray.Length; i++)
            {
                //Obtains the vortex strength of the vortex blob 
                VortexBlobBehaviour scr = vortexBlobArray[i].GetComponent<VortexBlobBehaviour>();

                //Obtains the distance to the vortex blob
                distance = Vector3.Distance(vortexBlobArray[i].transform.position, transform.position);

                //The following if clause serves two purposes:
                // - First: the second condition "distance > 0.02f" makes it so that, when the script is attached to a vortex blob (as is the case), the vortex blob will not interact with itself, which is desired 
                // - Second: the first condition tries to only take into account blobs whose influence is not neglegible, which is done by forcing said influence to be bigger than a certain value
                //           For now, testValue is set to zero as this seemed to have low influence on the CPU load. More testing may be required to find a suitable value of testValue
                if (Mathf.Abs(scr.VortexBlobStrength) / distance > testValue && distance > 0.00f)
                {
                    //Computes the magnitude and direction of the velocity diferential caused by the blob
                    velocityDir = Vector3.Cross((vortexBlobArray[i].transform.position - transform.position).normalized, initialisationVelocity);
                    velocityMagn = scr.VortexBlobStrength / (2.0f * Mathf.PI * distance);

                    //Updates the velocity differential with the value of velocity difference evaluated for the blob 
                    filamentVelocityDifferential += velocityMagn * velocityDir;

                }


            }

            //For loop that evaluates the velocity differential caused by the bound vortex blobs in the scene
            //for( int i = 0; i<vortexBlobBoundArray.Length; i++)
            //{
            //Obtains the vortex strength of the bound vortex blob 
            //BoundBlobsProperties scr = vortexBlobBoundArray[i].GetComponent<BoundBlobsProperties>();

            //Obtains the distance to the vortex blob
            //distance = Vector3.Distance(vortexBlobBoundArray[i].transform.position,transform.position);

            //Computes the magnitude and direction of the velocity diferential caused by bound blob
            //velocityDir = Vector3.Cross((vortexBlobBoundArray[i].transform.position- transform.position).normalized, scr.actuationDir);
            //velocityMagn = scr.VortexStrength/(2.0f*Mathf.PI*distance);

            //Updates the velocity differential with the value of velocity difference evaluated for the bound blob 
            //filamentVelocityDifferential += velocityMagn*velocityDir;


            //}


            //Updates the velocity of the rigidbody with the calculated velocity differential
            RB.velocity = initialVelocity + filamentVelocityDifferential;
            //Debug.Log(filamentVelocityDifferential);

            //Resets the velocity differential so that it is at zero the following loop iteration
            filamentVelocityDifferential = Vector3.zero;
            

            //probe = initialVelocity + filamentVelocityDifferential; //DEBUG FEATURE

        //this is the close for the iterationcount if statement
            iterationCount = iterationCount - 1;
        }
    }



}
