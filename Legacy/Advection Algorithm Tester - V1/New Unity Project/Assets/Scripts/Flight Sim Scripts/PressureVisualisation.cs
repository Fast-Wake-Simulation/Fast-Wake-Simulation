//################################################################################
//# Script that generates arrows on the wing surface representing the local 
//# pressure coefficients. 
//# The arrows are evenly spaced on each wing's surface. The number of arrows 
//# depends on the values attributed to "numberOfRows" and "numberOfArrowsPerRow". 
//# After instantiating the arrows, this script updates their scale to reflect the
//#	pressure coefficients. 
//# Pressure coefficient equation source:
//# "The Calculation of the Pressure Distribution over the Surface of Two-dimensional 
//#  and Swept Wings with Symmetrical Aerofoil Sections" by J.Weber (1956)
//#
//# Joao Vieira, 2016 


using UnityEngine;
using System.Collections;

public class PressureVisualisation : MonoBehaviour
{	
	//This section initialiases the required variables

	//Enumerator indicating the part of the aircraft that the script is attached to. This is necessary as each of the surfaces is using a different set of localaxis. 
	public enum Part
	{
		rightWing, leftWing, tail
	}
	public Part part;

	public GameObject pressureArrow; //Prefab containing the arrow geometry and reference axis
	public GameObject wing;			 //The Gameobject that this script is attached to (note: this is not necessary, but helps clarifying the code) 
	public GameObject arrowStorage;  //Parent object that will store the arrows, there needs to be one per wing as a child object

	//The next two variables  define the number of arrows that will be created when this script is run. Please do not exceed predefined range limits. 
	[Range(1,10)]
	public int numberOfRows = 4; 
	[Range(1, 10)]
	public int numberOfArrowsPerRow = 5; 

	//Boolean variable that if true will spawn the pressure coefficient arrows
	//IMPORTANT - This boolean controls whether the script is in operation. Be carefull when changing its value from external scripts 
	public bool instantiateArrows = true;

	//Variables that contain the wing properties
	private Aerodynamics3 wingProperties; //Script that contains flying condations relative the focus object. 
	private Vector3 size;
	private float halfWingSpan;
	private float chord;
	private float alpha; 

	private float thicknessOverChord;     //Ratio of thickness to chord


	//Arrays
	public Vector3[,] relativeSpawnLocations;   //2D Vector3 array that stores the spawn positions of the arrows
	private float[] relativeArrowPosAlongChord; //Float array that stores the position of the arrows along the chord of the wing/surface
	private GameObject[,] arrowArray;			//Gameobject array containing all the spawned arrows

	//This variable will store part of the coefficient of pressure calculation. Increases performance and readability
	private float firstSquare; 

	void Start ()
	{
		InitialiaseArrays();

		//Defines properties related to wing size
		size = wing.GetComponent<BoxCollider>().size;
		halfWingSpan = size.x;
		chord = size.z;

		//Stores part of the coefficient of pressure calculation. As the wing geometry does not change during flight, this value only needs to be
		//calculated once
		firstSquare = Mathf.Pow((1 + size.y/chord),2);

		//Stores a reference to the script that governs the wing aerodynamics. Used later to obtain lift and alpha at a given frame.
		wingProperties = wing.GetComponent<Aerodynamics3>();

		//Defines the position of the arrows along the chord
		for(int i = 0; i < numberOfArrowsPerRow; i++)
		{
			relativeArrowPosAlongChord[i] = 0.1f + 0.2f*i; 
		}

		//Sets spawn locations of the arrows
		SetSpawnLocations();
	}

	void Update () 
	{
		//If instantiateArrows is true, this section of the update function will initialise the arrows at the appropriate position and orientation
		if(instantiateArrows) 
		{
			for(int i = 0; i < numberOfRows; i++)
			{
				for (int j = 0; j < numberOfArrowsPerRow; j++)
				{
					GameObject arrow = Instantiate(pressureArrow, Vector3.zero, Quaternion.identity) as GameObject; //Spawns the arrow based on the prefab
					arrow.transform.parent = arrowStorage.transform; 												//Defines the surface as the arrows parent, effectively attaching the arrow to the surface
					arrow.transform.localPosition = relativeSpawnLocations[i,j];									//Positions the arrow in the previously defined location
					arrow.transform.localRotation = Quaternion.Euler(90,0,0);										//Rotates the arrow to an upwards position relative to the surface
					arrowArray[i,j] = arrow;																		//Stores the arrow in a Gameobject array
				}
			}
			//Turns the spawn variable off guaranteeing that the arrows are only instanciated once
			instantiateArrows = false;
		}
		//##############################################################################################################################################
		//Section that updates the arrows' scale based on the incidence angle of the surface they are attached to

		alpha = wingProperties.alpha; //Obtains the incidence angle

		//If there are arrows, updates their scale based on the incidence angle
		if(GameObject.FindGameObjectWithTag("PressureArrow") != null)
		{
			UpdateArrows(alpha);
		}
	}

	void InitialiaseArrays ()
	{
		relativeSpawnLocations = new Vector3[numberOfRows,numberOfArrowsPerRow];
		relativeArrowPosAlongChord = new float[numberOfArrowsPerRow];
		arrowArray = new GameObject[numberOfRows,numberOfArrowsPerRow];
	}

	//Sets the arrow positions along the surface based on which part the script is attached to. 
	//The spawn locations will adapt automatically to the shape of the surface, as long as the correct type of reference axis is employed
	void SetSpawnLocations()
	{
		float incrementalLength = halfWingSpan/(numberOfRows+1); //Calculates the distance between rows such that the arrows can simultaneously be evenly spaced and all fit inside the wing

		switch(part)
		{
		case Part.rightWing:
			//For the right wing, the origin of the reference axis is on leftmost center point of the surface, resulting in positive increments in the x direction. 
			for (int i = 0; i < numberOfRows; i++)
			{
				for (int j = 0; j < numberOfArrowsPerRow; j++)
				{
					relativeSpawnLocations[i, j] = new Vector3(incrementalLength * (i + 1), size.y / 2, -size.z / 2 + relativeArrowPosAlongChord[j] * chord);
				}
			}
			break;
		case Part.leftWing:
			//For the right wing, the origin of the reference axis is on rightmost center point of the surface, resulting in negative increments in the x direction.
			for (int i = 0; i < numberOfRows; i++)
			{
				for (int j = 0; j < numberOfArrowsPerRow; j++)
				{
					relativeSpawnLocations[i, j] = new Vector3(-incrementalLength * (i + 1), size.y / 2, -size.z / 2 + relativeArrowPosAlongChord[j] * chord);
				}
			}
			break;
		case Part.tail:
			//For the elevator, the origin of the reference axis is center of the leading edge.
			for (int i = 0; i < numberOfRows; i++)
			{
				for (int j = 0; j < numberOfArrowsPerRow; j++)
				{
					relativeSpawnLocations[i, j] = new Vector3((size.x / (numberOfRows+1)) * (i + 1) - size.x / 2, size.y / 2, -size.z / 2 + relativeArrowPosAlongChord[j] * chord);
				}
			}
			break;
		}

	}

	//Updates the size of the arrows based on the pressure coefficient. Pressure coefficient calculation is as seen in "The Calculation of the Pressure Distribution over the Surface of Two-dimensional and Swept Wings
	//with Symmetrical Aerofoil Sections" by J.Weber (1956) - Page 20
	void UpdateArrows(float alpha)
	{
		for(int i = 0; i < numberOfRows; i++)
		{
			for (int j = 0; j < numberOfArrowsPerRow; j++)
			{

				float pressureCoeff = 4 * Mathf.Cos(alpha)*Mathf.Sin(alpha)*firstSquare*Mathf.Sqrt(  (1 - relativeArrowPosAlongChord[numberOfArrowsPerRow-j-1] )/relativeArrowPosAlongChord[numberOfArrowsPerRow-j-1] );
				arrowArray[i,j].transform.localScale = new Vector3 ( arrowArray[i,j].transform.localScale.x, arrowArray[i,j].transform.localScale.y, pressureCoeff*12);
			}

		}
	}
}
