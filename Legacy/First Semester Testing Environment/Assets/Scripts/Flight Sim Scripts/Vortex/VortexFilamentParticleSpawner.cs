//################################################################################
//# Script responsible for spawning vortex blobs in the wake of the aircraft.
//# 
//# Number of vortex blobs spawned per batch is equal to numberOfRows*2, while   
//# the regularity in which blob are spawned depends on spawnInterval. Their overall
//# number is controlled externally by the "CleanCollection" script
//#
//# This script also defines the vortex strength that the vortex blobs have when 
//# they are spawned. This depends on the lift of the wing, for which an elliptical
//# lift distribution is assumed.
//#
//# Batches of vortex blobs start to be spawned when the user clicks the "Spawn 
//# Particles" interface button which toggles the internal variables of this script
//# through the public function "SpawnToggle". This initiates the coroutine 
//# spawnParticles, which will spawn batches of vortex blobs until the "Spawn
//# Particles" button is clicked again, turning the internal bool variables off and
//# stopping the referred coroutine
//# 
//# Note that this script was initially used to spawn vortex filaments and some 
//# artefacts of that period still exist in the current version of the script
//#
//# 


using UnityEngine;
using System.Collections;

public class VortexFilamentParticleSpawner : MonoBehaviour {

	//Declaration of variables


	public GameObject vortexProbeParticle;    //Prefab Gameobject that will be spawned by the script. Currently assigned to the vortex blob prefab blob containing the script "VortexBlobBehaviour"
	public Transform probePointCollection;	  //Parent object that will store the vortex blobs.
	public bool isRightWing;                  //Boolean that should be set to true if script is attached to the right wing or false if it is instead attached to the left wing. Important due to the different relative axis
	public bool spawnBool = false;			  //Controls whether the script should spawn vortex blobs. Externally controlled 
	private bool spawnisReady = false;        //Switch that denotes whether a batch of vortex blobs should be spawned. Internally controlled by the coroutine "spawnParticles"


	//Batch properties

	public int numberOfRows = 10;			  //Number of blobs spawned per batch per wing. Number of blobs per batch equal to numberOfRows*2
	public float spawnInterval = 0.3f;		  //Interval between batches of vortex blobs. Values lower than 0.2 seconds are not recommeneded


	//Wing properties

	public GameObject wing;					  //Public reference to the wing. Since the script must be attached to the wing this is unecessary, but helps making the code more readable
	private Rigidbody RB;
	private Aerodynamics3 wingProperties;	  //Stores the aerodynamics script in order to easely access the incidence angle and lift values 
	private Vector3 size;					  //Stores the size of the wing based on its transform.localScale value
	private float halfWingSpan;
	private float currentVelocity;			  //Current velocity of the wing to which this script is attached to

	//Vortex blob spawning locations and strenth 

	private Vector3[] spawnLocations;        //Stores the spawn position of the vortex blobs. Used in the instantiation process.
	private float[] vortexStrenth; 		     //Stores the values of the vortex strength of each blob, which are later assigned to the blobs immediately after they are spawned.
	public float maximumVortexStrength;      //Maximum vortex strenth as obtained in a elliptical lift distribution over the full wingspan
	private Vector3 localPosOffset;          //Distance between each blob along the wingspan 

	//Variables required for spawning a trailling camera behind the aircraft when the blobs are released. Functional, but currently not implemented in lieu of a fixed camera looking at the leading edges
	//public GameObject cameraHolder;
	//private Rigidbody traillingCameraRB;


	void Start ()
	{

		//Initialiases arrays
		spawnLocations = new Vector3[numberOfRows + 1];
		vortexStrenth = new float[numberOfRows + 1];

		//Stores the rigidbody component of the wings in an easy to access variable
		RB = GetComponent<Rigidbody>();

		//Finds wing geometry
		size = wing.GetComponent<BoxCollider>().size;
		halfWingSpan = size.x;

		//Stores a reference to the script that governs the wing aerodynamics. Used later to obtain lift and alpha at a given frame.
		wingProperties = wing.GetComponent<Aerodynamics3>();

	}


	void FixedUpdate ()
	{
		currentVelocity = Mathf.Sqrt(wingProperties.vSquared); //Obtains the current wing velocity

		//If spawnBool (allowed to spawn) is true and spawnisReady (ready to spawn) is false, this if clause calculates
		//the vortex strengths and initiates the coroutine "SpawnParticles" in order to initiate the spawn of vortex blobs
		if(spawnBool&&!spawnisReady)
		{
			SetVortexIntensity3 ();
			StartCoroutine(spawnParticles());

			/* OLD CODE
			Spawns the trailling camera (Hidden for now) 
			GameObject[] alternateCamerasArray = GameObject.FindGameObjectsWithTag("Camera");
			if(isRightWing)
			{
				Vector3 flightDir = wingProperties.rb.velocity.normalized;
				GameObject traillingCamera = Instantiate(cameraHolder, wing.transform.position - flightDir*3, Quaternion.identity) as GameObject; 
				traillingCameraRB = traillingCamera.GetComponent<Rigidbody>();
				traillingCameraRB.velocity = -wingProperties.rb.velocity * 0.2f;
				traillingCamera.transform.LookAt(wing.transform);
				SetCameras();
			}
			  END OF OLD CODE
			*/
			spawnisReady = true;

		}

	}

	/* OLD CODE - Old methods to set the vortex intensity, precursors to SetVortexIntensity3
	//Defines the spawn locations of the vortex filaments
	void SetSpawnLocations()
	{
		

		if(isRightWing)
		{
			
			for ( int i = 0; i < numberOfRows+ 1; i++ )
			{
				spawnLocations[i] =  ;

			}

		}

		else
		{
			for ( int i = 0; i < numberOfRows+ 1; i++ )
			{
				spawnLocations[i] = - incrementalLength*wing.transform.right*i;
			}
		}
	}

	  void SetVortexIntensity1 ()
		{
		float individualVortexStrength = totalVortexStrength/numberOfRows;
		for ( int i = 0; i < numberOfRows; i++ )
		{
			if(numberOfRows%2==0)
			{
				if(i<(numberOfRows/2))
					vortexStrenth[i] = -individualVortexStrength;
				else
					vortexStrenth[i] = individualVortexStrength;
			}
			else
			{
				if( i < (numberOfRows/2 - 1) )
					vortexStrenth[i] = -individualVortexStrength;
				else if( i > (numberOfRows/2 + 1) )
					vortexStrenth[i] = -individualVortexStrength;
				else
					vortexStrenth[i] = 0;
			}
		}
	}

	void SetVortexIntensity2 ()
	{
		float individualVortexStrength = totalVortexStrength/numberOfRows;
		for ( int i = 0; i < numberOfRows; i++ )
		{
			vortexStrenth[i] = -individualVortexStrength + 2.0f*individualVortexStrength/(numberOfRows-1)*i;
		}
	}
	END OF OLD CODE
	*/


	//Calculates the vortex intensity of each vortex filament based on the lift distribution of the wing. Assumes an elliptic lift distribution
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
				vortexStrenth[i] = -individualVortexStrength*i; 
			}
		}
		else
		{
			for ( int i = 0; i < numberOfRows + 1; i++ )
			{
				vortexStrenth[i] =  individualVortexStrength*i; 
			}
		}
	}

	/* OLD CODE - Part of the script that controlled camera priorities when the trailling camera was spawn 
	void SetCameras()
	{
		GameObject mainC = GameObject.FindGameObjectWithTag("MainCamera");
		mainC.SetActive(false);
	}
	  END OF OLD CODE 
	*/




	IEnumerator spawnParticles()
	{
		
		while(spawnBool) //Once the 
		{
			for ( int i = 0; i < numberOfRows; i++ )
			{
				//Calculates the vortex intensity values of each blob based on the current lift and blob position along the wingspan
				SetVortexIntensity3 ();

				//Calculates the distance between vortex blobs such that the blobs can simultaneously be evenly spaced and all fit inside the profile of the wing
				float incrementalLength = halfWingSpan/numberOfRows;

				//Defines the position of each blob. If the script is attached to the left wing the blobs will be spawned from the center and procedurally move left. The inverse occurs for the right wing
				if(!isRightWing) localPosOffset = -incrementalLength*wing.transform.right*i - size.z/1.2f * transform.forward;
				else localPosOffset = incrementalLength*wing.transform.right*(numberOfRows-i) - size.z/1.2f * transform.forward;

				//Instantiates the vortex blobs
				GameObject clone = Instantiate(vortexProbeParticle, wing.transform.position + localPosOffset   , Quaternion.identity) as GameObject;

				//Parents the bound vortex blob to the predefined Gameobject. Implemented in order to keep the instantiated gameobjects out of the front page of the editor
				clone.transform.parent =  probePointCollection;

				//Sets the strength of each filament
				VortexBlobBehaviour scr = clone.GetComponent<VortexBlobBehaviour>();

				//Sets the strength and velocity direction of each blob
				if(!isRightWing) scr.VortexBlobStrength = vortexStrenth[i];
				else  scr.VortexBlobStrength = vortexStrenth[numberOfRows-i];
				scr.initialisationVelocity = RB.velocity.normalized;
				scr.initialVelocity = Vector3.zero;


			}

			yield return new WaitForSeconds(spawnInterval);
		}
	}

	//This public functions allows external elements to request the spawn of batches of vortex blobs 
	public void SpawnToggle ()
	{
		spawnBool = !spawnBool;
		if(!spawnBool) spawnisReady = false;
	}

}