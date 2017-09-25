//################################################################################
//# Script that creates a mesh connecting the vortex blobs, resulting in a 
//# representation of the aircraft's wake as it flies through the air.
//# Due to the way that Unity's mesh renderers work, two meshes need to be 
//# generated in order to produce the desired effect. 
//# Analogically speaking, one mesh will be the "front" of the wake while the other 
//# is the "back", resulting in the wake profile being rendered in all viewing 
//# directions. 
//# 
//# A reading on Unity's implementation of meshes is recommended before trying to 
//# understand the following code. Nevertheless, I will provide a quick initial
//# explanation to help simplify things: 
//# In order to render a mesh in Unity one needs: 
//#  - A mesh filter --> Component that stores the mesh
//#  - A mesh renderer --> Component that renders the mesh in the scene
//#  - The mesh itself
//# Furthermore, in order to generate a mesh from scratch, it is necessary to 
//# have:
//# - Vertices - In this case, the locations of the vortex blobs
//# - Triangles - Trios of vertices carefully selected such that when put together 
//#               form the mesh 
//# - Normals - The direction into which each of the triangles will be rendered
//# 
//# NOTE: This script needs to be attached to the parent object of all the vortex blobs
//# 
//# Joao Vieira, 2016


using UnityEngine;
using System.Collections;

public class MeshGeneration : MonoBehaviour {

	//Declaration of variables

	//Meshfilters - NOTE: gameobjects with mesh filters must also have mesh renderers
	private MeshFilter meshFilter1;     //First mesh filter already present in the probe collector gameobject
	public MeshFilter meshFilter2;		//Second mesh filter that needs to be added from the editor

	//Meshes
	private Mesh mesh1;
	private Mesh mesh2;

	//Vertices - NOTE: both meshes share the same set of vertices
	public Vector3[] vertices;

	//Triangles
	public int[] triangles1;
	public int[] triangles2;

	//Array that will store the vortex blobs' positions
	private Transform[] pointArray;

	//Other 
	private int currentID = 0;
	private int pointsPerRow = 16;      //Size of each batch of blobs. For safety reasons this value should be automatised in a manner similar to that 
	//present in the "CleanCollection" script. As it is now, this value needs to be manually updated after changes
	//in the number of blobs spawned per batch


	void Start()
	{
		meshFilter1 = GetComponent<MeshFilter>();  //Gets the reference to the mesh filter that this script is attached to

	}


	void Update ()
	{
		InitialiseArrays();
		currentID = 0;               //Resets 

		//Both meshes are generated once per frame and then assigned to their respective. The alternative of updating the meshes every frame is more CPU 
		//dependent and should be avoided per CPU reasons
		mesh1 = new Mesh();
		mesh2 = new Mesh();
		meshFilter1.mesh = mesh1;     //Assigns mesh 1 to mesh filter 1 
		meshFilter2.mesh = mesh2;     //Assigns mesh 2 to mesh filter 2 


		foreach(Transform children in transform)  //Equivalent to all the blobs in the scene 
		{
			children.name = currentID.ToString(); //Renames the blobs based on their mesh ID (not necessary, more of a debub feature)
			pointArray[currentID] = children;     //Stores the transforms of the blobs in the pointArray
			currentID++;
		}

		//Defines Vertices and Triangles for both meshes 
		DefineMeshVertices();
		DefineMeshTriangles();
		DefineMeshTriangles2();
		mesh1.vertices = vertices;
		mesh1.triangles = triangles1;
		mesh2.vertices = vertices;
		mesh2.triangles = triangles2;

		//Automatically calculate mesh normals 
		mesh1.RecalculateNormals();
		mesh2.RecalculateNormals();




	}

	//Defines mesh vertices using the position of the vortex blobs
	void DefineMeshVertices ()
	{
		for( int i = 0; i < pointArray.Length; i++)
		{
			vertices[i] = pointArray[i].position;
		}
	}

	//Counts the number of the points that make up the triangles of the meshes (used for array initialisation). Based on DefineMeshTriangles
	int CountTrianglePoints()
	{
		int i = 0;
		int focusPoint = 0;
		while(focusPoint < pointArray.Length-pointsPerRow-1)
		{
			if((focusPoint+1)%pointsPerRow==0)
			{
				focusPoint++;
				continue;
			}
			i = i+6;
			focusPoint++;

		}
		return i;
	}

	//Defines mesh triangles in pairs (quadrilateral method). Some care is taken for the code not to generate impossible triangles
	void DefineMeshTriangles() 
	{
		int i = 0;
		int focusPoint = 0;
		while(focusPoint < pointArray.Length-pointsPerRow-1)
		{
			if((focusPoint+1)%pointsPerRow==0)
			{
				focusPoint++;
				continue;
			}
			triangles1[i] = focusPoint;
			triangles1[i+1] = focusPoint+1;
			triangles1[i+2] = focusPoint+pointsPerRow;
			triangles1[i+3] = focusPoint+pointsPerRow;
			triangles1[i+4] = focusPoint+1;
			triangles1[i+5] = focusPoint+pointsPerRow+1;

			i = i + 6;
			focusPoint++;
		}
	}

	//Same as DefineMeshTriangles but swaps point 1 with point 2 and point 4 with point 5 in order to invert mesh normals
	void DefineMeshTriangles2() 
	{
		int i = 0;
		int focusPoint = 0;
		while(focusPoint < pointArray.Length-pointsPerRow-1)
		{
			if((focusPoint+1)%pointsPerRow==0)
			{
				focusPoint++;
				continue;
			}
			triangles2[i] = focusPoint;
			triangles2[i+1] = focusPoint+pointsPerRow;
			triangles2[i+2] = focusPoint+1;
			triangles2[i+3] = focusPoint+pointsPerRow;
			triangles2[i+4] = focusPoint+pointsPerRow+1;
			triangles2[i+5] = focusPoint+1;
			i = i + 6;
			focusPoint++;
		}
	}




	void InitialiseArrays()
	{
		pointArray = new Transform[transform.childCount];
		vertices = new Vector3[transform.childCount];
		triangles1 = new int[CountTrianglePoints()];
		triangles2 = new int[CountTrianglePoints()];
	}

}
