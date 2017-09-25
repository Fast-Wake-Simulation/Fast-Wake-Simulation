//################################################################################
//# Multi-function script that provides support to the vortex blob system.
//# Consists of: 
//# - An IDentification system that assigns IDs to all the blobs on the scene based  
//#   on how long they have existed for;
//# - A Blob destruction system that deletes the oldest batch of blobs. Necessary 
//#   in order to avoid excessive CPU load
//# - A "Clean" routine that deletes all the blobs on the scene, effectively 
//#   resetting the vortex blob system. 
//#
//# Joao Vieira, 2016

using UnityEngine;
using System.Collections;

public class CleanCollection : MonoBehaviour {
	//Declaration all the necessary variables

	public int currentBlobCount;						//Stores the current number of blobs in the scene 

	[Range(1,200)]
	public int maxNumberofBlobs = 100;					//Maximum number of blobs allowed in the scene. How high this number can go will depend on the used CPU and code optimisation
														//Currently, for a decent CPU, 100 vortex blobs should be adequate. Lower that value to 80 if running other systems at the same time

	private GameObject[] blobArray;						//Stores all the vortex blobs gameobjets in an array

	//Variables regarding the ID system
	private int currentID = 0;
	private int lastDeletedID = 0;

	public VortexFilamentParticleSpawner rightWingSCR;  //Stores a reference to the particle spawner script. Used later to find the number of blobs spawned per batch
	private int blobsSpawnedperCycle; 					//See above (1 line)

	void Start()
	{
		//Calculates the number of blobs spawned per cycle. Used later to limit the number of blobs on scene in an organised fashion 
		blobsSpawnedperCycle = rightWingSCR.numberOfRows*2;
	}

	void Update()
	{
		//Stores all the vortex blobs in a gameobject array 
		blobArray = GameObject.FindGameObjectsWithTag("VortexBlob");
		int newBlobCount = blobArray.Length;

		//Checks if new IDs need to be assigned and assigns them if necessary
		if(newBlobCount != currentBlobCount)
		{
			AssingID();
			currentBlobCount = newBlobCount;
		}

		//Checks if the current number of vortex blobs exceeds the predifined maximum and deletes the oldest batch of blobs 
		if(currentBlobCount > maxNumberofBlobs)
		{
			DestroyOlderBlobs();
		}

	}

	//Assigns an ID to each of the vortex blobs in the scene. Due to the way that the "FindGameObjectsWithTag" command works
	//older blobs will be assigned lower IDs, as the blob array will be filled by the order in which the blobs entered the scene 
	void AssingID()
	{
		foreach(GameObject blob in blobArray)
		{
			VortexBlobBehaviour blobScript = blob.GetComponent<VortexBlobBehaviour>(); //Gets the reference to the vortex blob behaviour script present in each vortex blob in the scene
			if(blobScript.ID == -1)
			{
				blobScript.ID = currentID; 											   //If the blob does not have an ID, attributes it an unassigned one 
				blob.name = "ID" + currentID.ToString("0F");						   //Attributes the blob a name based on its ID
				currentID++;
			}
		}
	}

	//Deletes the older batch of vortex blobs, which helps avoiding overloading the system  
	void DestroyOlderBlobs ()
	{
		foreach(GameObject blob in blobArray)
		{
			VortexBlobBehaviour blobScript = blob.GetComponent<VortexBlobBehaviour>(); //Gets the reference to the vortex blob behaviour script present in each vortex blob in the scene

			//Checks whether the blob is part of the oldest batch of vortex blobs based on its ID (deletes lowest), if so deletes it. The highest ID deleted is recorded and utilised in later
			//runs of the DestroyOlderBlobs function 
			if(blobScript.ID - lastDeletedID < blobsSpawnedperCycle) Destroy(blob);
			if(blobScript.ID - lastDeletedID == blobsSpawnedperCycle) 
			{
				lastDeletedID = blobScript.ID; //Stores the last deleted ID 
				break;
			}
		}
	}

	//Public routine that, when called, deletes all the blobs on the scene, effectively resetting the vortex blob system 
	public void Clean () 
	{
		foreach (Transform children in transform)
			Destroy(children.gameObject);         //Destroys all the child gameobjects of the ProbeCollector parent gameobject

		//Resets the ID system variables
		currentID = 0;
		lastDeletedID = 0;


	}


}
