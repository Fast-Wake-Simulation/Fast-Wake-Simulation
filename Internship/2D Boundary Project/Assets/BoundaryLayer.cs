using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryLayer : MonoBehaviour {

    //Standard variables for elements
    Vector2[] elementPositions = new Vector2[255];
    Vector2[] elementVelocities = new Vector2[255];
    float[] elementVorticity = new float[255];
    int[] elementID = new int[225];
    int[] elementState = new int[255];
    float[] elementAge = new float[255];

    //pseudo element variables
    Vector2[] pseudoElementPositions = new Vector2[255];
    float[] pseudoElementVorticity = new float[255];
    bool[] pseudoElementExist = new bool[255];

    //Global variables
    Vector2 freeStreamVelocity = new Vector2( 0.5f, 0.0f);

    // Use this for initialization
    void Start () {
		
        //create initial elements
        for (int iteration = 0; iteration < 10; iteration++)
        {
            elementPositions[iteration]= new Vector2(0.0f, iteration * 1.0f);
            elementVelocities[iteration] = new Vector2(0.0f, 0.0f);
            elementState[iteration] = 1;
        }

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //physics update
    private void FixedUpdate()
    {

        //convect elements
        convectElements();

        //update positions
        updatePositions();

        //check for collision
        checkCollision();

    }

    //function to iterate positions
    void updatePositions()
    {
           
        //cycle through elements
        for (int iteration = 0; iteration < 10; iteration++)
        {
            elementPositions[iteration] += (elementVelocities[iteration] + freeStreamVelocity) * Time.deltaTime;
        }

    }

    //Function to chec for collisions
    void checkCollision()
    {

        //cycle through elements
        for (int iteration = 0; iteration < 10; iteration++)
        {

            //calculate radius of seperation
            Vector2 radius = elementPositions[iteration] - new Vector2(6.0f, 4.0f);

            //check if the radius is under
            if (radius.magnitude < 4.0f)
            {
                //elementVelocities[iteration] = -freeStreamVelocity;
                createPseudoElement(iteration);
                //Debug.Log("Collided");
            }
        }

    }

    //function to create pseudo element
    void createPseudoElement(int elementID)
    {

        //set its state to disabled
        elementState[elementID] = 1;

        //first calculate the total velocity
        Vector2 totalVelocity = elementVelocities[elementID] + freeStreamVelocity;

        //now find the unit vector where this acts
        Vector2 velocityNormal = (new Vector2(totalVelocity.y, -totalVelocity.x)) / totalVelocity.magnitude;
        float vorticity = - totalVelocity.magnitude;
        //Debug.Log(elementID + " Has Pseudoelement at: " + velocityNormal.x + "," + velocityNormal.y+" With vorticity: "+vorticity);

        //now set the values to the array
        pseudoElementExist[elementID] = true;
        pseudoElementPositions[elementID] = elementPositions[elementID] + velocityNormal;
        pseudoElementVorticity[elementID] = vorticity;

    }

    //convect elements
    void convectElements()
    {

        //first cycle through the pseudo elements
        for (int i = 0; i <255; i++)
        {

            //check if this particular pseudo element exists
            if (pseudoElementExist[i])
            {
                
                //now cycle through all real elements
                for (int ii = 0; ii<255; ii++)
                {

                    if (elementState[ii] == 0)
                    {

                        //Debug.Log("I work");
                        Vector2 radius = elementPositions[ii] - pseudoElementPositions[i];
                        elementVelocities[ii] += new Vector2(- pseudoElementVorticity[i] * radius.y, pseudoElementVorticity[i] * radius.x);


                    }

                }
            }

        }

    }

    //visualize the elements
    private void OnDrawGizmos()
    {

        //cycle through elements
        for (int iteration = 0; iteration < 10; iteration++)
        {
            Gizmos.DrawSphere(elementPositions[iteration], 0.1f);
        }
    }

}
