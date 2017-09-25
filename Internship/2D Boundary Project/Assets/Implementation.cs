using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Implementation : MonoBehaviour {


    //free particle varaiables
    private Vector3[,,] elementPosition = new Vector3[10,10,10];
    private Vector3[,,] elementVelocities = new Vector3[10, 10, 10];
    private Vector3[,,] elementVorticities = new Vector3[10,10,10];
    private float[,,] timeAtCreation = new float[10,10,10];
    private bool[,,] elementActive = new bool[10,10,10];

    //pseudo particle properties
    private float pseudoElementRadius = 1.0f;

    //global simulation variables
    private Vector3 freeStreamVelocity = new Vector3(1.0f, 0.0f, 0.0f);

    //public variables (interface options)
    public bool shouldRun;

	// Use this for initialization
	void Start () {

        //call to the function to set up initial conditions
        setInitialConditions();

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //physics based updatess
    private void FixedUpdate()
    {

        //this if condition allows the either the 2d or 3d implementation to be selected from the interface
        if (shouldRun)
        {

            updatePositions();

            applyBoundarySphere(15.0f, 5.0f, 5.0f, 2.0f);

        }


    }

    //function to set initial conditions
    void setInitialConditions()
    {

        //first setup an equally spaced grid 
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                for (int z = 0; z < 10; z++)
                {
                    elementPosition[x, y, z] = new Vector3(x, y, z); 
                }
            }
        }

    }

    //this function is responsible for the freestream and convectve velocities
    void updatePositions()
    {

        //update positions
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                for (int z = 0; z < 10; z++)
                {

                    //now all positions are cycled through apply the change in position
                    elementPosition[x, y, z] += (elementVelocities[x, y, z] + freeStreamVelocity) * Time.deltaTime;

                }
            }
        }

    }

    //function to create new elements and remove old ones
    void createNewGridSegment()
    {

    }

    //function to handle the boundary layer, this function calls other subsidiary functions
    void applyBoundarySphere(float x, float y, float z, float radius)
    {

        //first a collision check needs to take place
        for (int xi = 0; xi < 10; xi++)
        {
            for (int yi = 0; yi < 10; yi++)
            {
                for (int zi = 0; zi < 10; zi++)
                {

                    //now all elements are cycled through their radius from the sphere needs to be found
                    Vector3 radiusToSphere = elementPosition[xi, yi, zi] - new Vector3(x, y, z);

                    //now we need to check if the element has/is near colliding (depending on the implementaiton of the boundary condition)
                    if (radiusToSphere.magnitude < radius)
                    {

                        //now we know that we need to create a pseudo element, so the function is called
                        createPseudoElement(xi, yi, zi, x, y, z, radius, radiusToSphere);

                    }

                }
            }
        }

    }

    //this function is used to enforce the boundary condition by creation of a pseudo element
    void createPseudoElement(int idx, int idy, int idz, float x, float y, float z, float radius, Vector3 radiusToSphere)
    {

        //firstly the total velocity vector of the element in question must be found
        Vector3 totalVelocity = elementVelocities[idx, idy, idz] + freeStreamVelocity;

        //now we calculate the direction of the pseudo elements offset
        Vector3 surfaceNormal = -radiusToSphere.normalized;
        Vector3 offsetDirection = Vector3.Cross(totalVelocity, Vector3.Cross(surfaceNormal, totalVelocity));

    }

    //create function to render the elements
    private void OnDrawGizmos()
    {

        if (shouldRun)
        {
            //cycle through all elements
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    for (int z = 0; z < 10; z++)
                    {
                        Gizmos.DrawSphere(elementPosition[x, y, z], 0.1f);
                    }
                }
            }

            Gizmos.DrawSphere(new Vector3(15.0f, 5.0f, 5.0f), 2.0f);
        }

        

    }

}
