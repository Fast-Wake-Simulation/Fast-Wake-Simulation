//################################################################################
//# Script that spawns bound vortex blobs along the quarter chord of the wings. 
//# The number of bound blobs depends on the variable "numberOfRows". The vortex
//# intensity of each blob is updated each frame depending on the lift generated 
//# by the wing. Likewise, the effective direction of the vortex is also updated 
//# depending on the airplane orientation. 
//#
//# Joao Vieira, 2016 


using UnityEngine;
using System.Collections;

public class BoundVortexBlobs : MonoBehaviour {

	//Initialises the required variables 

	public GameObject vortexBlobBound;			//Prefab Gameobject consisting of a predefined bound vortex blob containing the script "BoundBlobsProperties"
	public Transform wingProbePointCollection;  //Parent object that will store the bound vortex blobs, there needs to be one per wing.
	public bool isRightWing;					//Boolean that should be set to true if script is attached to the right wing or false if it is instead attached to the left wing. Important due to the different relative axis
	public bool spawnBool = true;				//Controls whether the bound vortex blobs should be spawned or not. 


	private int numberOfRows = 10;				//Number of bound blobs present in each wing

	//Wing properties

	private Aerodynamics3 wingProperties; 		//Stores the aerodynamics script in order to easely access the incidence angle and lift values      
	private Vector3 size;						//Stores the size of the wing based on its transform.localScale value
	private float halfWingSpan;
	private float currentVelocity;				//Current velocity of the wing to which this script is attached to

	//Bound Vortex Blobs properties

	public GameObject[] boundVortexArray;		//Array containing the bound vortex blobs instantiated in this script
	public BoundBlobsProperties[] strengthSCR;	//Stores the reference to all the bound blobs properties script in an array (script array)  
	private float[] vortexStrenth;				//Stores the values of the vortex strength of each bound blob. Used to update the vortex strength value in "BoundBlobProperties"
	private float maximumVortexStrength;		//Maximum vortex strenth as obtained in a elliptical lift distribution over the full wingspan


	private Vector3 localPosOffset;				//Distance between each bound blob along the wingspan 


	void Start () 
	{
		//Assigns script references and initialises arrays

		size = gameObject.GetComponent<BoxCollider>().size;
		wingProperties = gameObject.GetComponent<Aerodynamics3>();
		vortexStrenth = new float[numberOfRows + 1];
		boundVortexArray = new GameObject[numberOfRows + 1];
		strengthSCR = new BoundBlobsProperties[numberOfRows + 1];

		halfWingSpan = size.x;
	}


	void Update () {

		currentVelocity = Mathf.Sqrt(wingProperties.vSquared); //Obtains the current wing velocity

		//If spawn bool is set to true, this section calculates the positions and spawns the bound vortex blobs along the wingspan 
		if(spawnBool)
		{
			SetVortexIntensity3();  //Calculates the vortex intensity values of each blob based on the current lift and blob position along the wingspan

			//Spawns vortex blobs in equal number to "numberOfRows"
			for ( int i = 0; i < numberOfRows; i++ )
			{
				//Calculates the distance between bound vortex blobs such that the blobs can simultaneously be evenly spaced and all fit inside the wing
				float incrementalLength = halfWingSpan/numberOfRows;

				//Defines the position of each blob. If the script is attached to the left wing the blobs will be spawned from the center and procedurally move left. The inverse occurs for the right wing
				if(!isRightWing) localPosOffset = -incrementalLength*transform.right*i + size.z/4f * transform.forward;
				else localPosOffset = incrementalLength*transform.right*i + size.z/4f * transform.forward;

				//Instantiates the vortex blobs
				boundVortexArray[i] = Instantiate(vortexBlobBound, transform.position + localPosOffset   , Quaternion.identity) as GameObject;

				//Parents the bound vortex blob to the predefined Gameobject, effectively attaching the blobs to the wing
				boundVortexArray[i].transform.parent =  wingProbePointCollection;

				//Sets the strength and actuation direction of each blob
				strengthSCR[i] = boundVortexArray[i].GetComponent<BoundBlobsProperties>() ;
				strengthSCR[i].VortexStrength = vortexStrenth[i];
				if(!isRightWing) strengthSCR[i].actuationDir = -transform.TransformDirection(transform.right);
				else strengthSCR[i].actuationDir = transform.TransformDirection(transform.right);

			}
			spawnBool = false;
		}

		//###################################################################################################################################################

		//This section checks if there are bound vortex blobs in the scene. If that is the case, it updates the strength and direction of the blobs 
		if(boundVortexArray.Length>1)
		{
			SetVortexIntensity3(); //Recalculates vortex intensities
			for ( int i = 0; i < numberOfRows; i++ )
			{
				strengthSCR[i].VortexStrength = vortexStrenth[i]; //Updates vortex blob strength 
				if(!isRightWing) strengthSCR[i].actuationDir = -transform.TransformDirection(transform.right); //Updates actuation direction if on left wing
				else strengthSCR[i].actuationDir = transform.TransformDirection(transform.right);			   //Updates actuation direction if on right wing
			}
		}

	}

	//Calculates the vortex intensity values of each blob based on the current lift and blob position along the wingspan
	void  SetVortexIntensity3 ()
	{
		float lift = wingProperties.normalForce;
		// The next line calculates the vortex strength and the tips of the wing (point of maximum vorticity) assuming an elliptical lift distribution 
		// over the full wingspan. The value is exaggerated for ease of visualisation 
		maximumVortexStrength = 2 * lift / (1.225f * currentVelocity * Mathf.PI*halfWingSpan*2.0f) * 8; //For real values remove the *8

		//This line calculates the change in vorticity between each of the bound blobs depending on the number of blobs and assuming a linear vorticity
		//distribution with the absolute maximums occuring at the tips. Note that one wing tip will have negative vorticity while the other has positive
		float individualVortexStrength = -maximumVortexStrength/numberOfRows;

		//Section that implements the value calculated above in order to find the vortex strength value of each of the individual bound blobs 
		//Note that the vorticity value at a given point on each of the wings is equal and opposite (for the same lift generation)
		if(isRightWing)
		{
			for ( int i = 0; i < numberOfRows + 1; i++ )
			{
				vortexStrenth[i] = individualVortexStrength*i; 
			}
		}
		else
		{
			for ( int i = 0; i < numberOfRows + 1; i++ )
			{
				vortexStrenth[i] = - individualVortexStrength*i; 
			}
		}
	}

	//This public functions allows external elements to request the spawn of bound vortex blobs 
	public void SpawnToggle ()
	{
		spawnBool = !spawnBool;

	}
}
