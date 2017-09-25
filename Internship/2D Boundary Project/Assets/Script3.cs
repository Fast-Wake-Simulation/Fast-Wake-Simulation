using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script3 : MonoBehaviour {

    //Real Element Properties
    private Vector3[] elementPositions = new Vector3[2550];
    private Vector3[] elementVorticity = new Vector3[2550];
    private Vector3[] elementVelocity = new Vector3[2550];
    private bool[] elementExist = new bool[2550];
    private bool[] elementCoupled = new bool[2550];


    //Pseudo Element Properties
    private Vector3[] pseudoElementPositions = new Vector3[2550];
    private Vector3[] pseudoElementVorticity = new Vector3[2550];
    private Vector3[] pseudoElementVelocity = new Vector3[2550];
    private bool[] pseudoElementExist = new bool[2550];

    //global variabeles
    private Vector3 freeStreamVelocity = new Vector3(2.9f, 0.0f, 0.0f);
    private bool tickTock = true;
    public float cumulativeVorticity = 0.0f;

    //flip flop for running
    public bool shouldRun;

    // Use this for initialization
    void Start () {
		
        //Set initial conditions (10x10 grid of equally spaced elements)
        for (int i = 0, x = 0; x < 28; x++)
        {
            for (int y = 0; y < 15; y++, i++)
            {

                //place the element
                elementPositions[i] = new Vector3(x, y, 0.0f);
                elementVorticity[i] = Vector3.zero;
                elementVelocity[i] = Vector3.zero;
                elementExist[i] = true;
                elementCoupled[i] = false;

            }
        }


	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //physics based maths
    void FixedUpdate()
    {

        if (shouldRun)
        {
            updatePositions();

            checkCollisions();
        }

        

    }


    //Check for collisions
    void checkCollisions()
    {

        //cycle through all elements
        for (int i = 0; i < elementPositions.Length; i++)
        {

            if (elementExist[i] == true && elementCoupled[i] == false)
            {

                //this next line defined centre of the sphere
                Vector3 center = new Vector3(37.0f, 5.0f, 0.0f);

                //Now find the radius to the element
                Vector3 radius = center - elementPositions[i];

                //Debug.Log("I WORK");

                //check if its inside
                if (radius.magnitude < 2.0f)
                {
                    //elementVelocity[i] = new Vector3(-0.5f, 0.0f, 0.0f);
                    //Debug.Log("Collided");
                    coupleElement(i, 37f, 5f);
                }

                //A second sphere
                Vector3 center2 = new Vector3(44f, 10f, 0.0f);
                Vector3 radius2 = center2 - elementPositions[i];
                if (radius2.magnitude < 2.0f)
                {
                    coupleElement(i, 44f, 10f);
                }

                //A third sphere
                Vector3 center3 = new Vector3(55f, 2f, 0.0f);
                Vector3 radius3 = center2 - elementPositions[i];
                if (radius3.magnitude < 2.0f)
                {
                    //coupleElement(i, 45f, 2f);
                }

            }

        }

    }

    //function to couple elements with a pseudo paid
    void coupleElement(int ID, float centerx, float centery)
    {

        //first the normal to the velocity vector must be found
        //Vector3 totalVelocity = elementVelocity[ID] + freeStreamVelocity;
        Vector3 totalVelocity = freeStreamVelocity;

        //now the nomral to this vorticity vector is found
        Vector3 velocityNormal = (new Vector3(totalVelocity.y, -totalVelocity.x, 0.0f)) / (totalVelocity.magnitude * 5.1f);

        //now calculate the vorticity magnitude required
        float vorticityMagnitude = totalVelocity.magnitude * velocityNormal.magnitude;


        if (elementPositions[ID].x <= centerx)
        {
            if (elementPositions[ID].y - centery > 0)
            {
                pseudoElementPositions[ID] = elementPositions[ID]; //- velocityNormal;
                pseudoElementVorticity[ID] = new Vector3(0.0f, 0.0f, -vorticityMagnitude);
                pseudoElementExist[ID] = true;
                cumulativeVorticity += -vorticityMagnitude;
            }
            if (elementPositions[ID].y - centery < 0)
            {
                pseudoElementPositions[ID] = elementPositions[ID]; //+ velocityNormal;
                pseudoElementVorticity[ID] = new Vector3(0.0f, 0.0f, + vorticityMagnitude);
                pseudoElementExist[ID] = true;
                cumulativeVorticity += +vorticityMagnitude;
            }
        }
        else
        {
            if (elementPositions[ID].y - centery > 0)
            {
                pseudoElementPositions[ID] = elementPositions[ID] + velocityNormal;
                pseudoElementVorticity[ID] = new Vector3(0.0f, 0.0f, - vorticityMagnitude);
                pseudoElementExist[ID] = true;
                cumulativeVorticity += -vorticityMagnitude;
            }
            else
            {
                pseudoElementPositions[ID] = elementPositions[ID] - velocityNormal;
                pseudoElementVorticity[ID] = new Vector3(0.0f, 0.0f, + vorticityMagnitude);
                pseudoElementExist[ID] = true;
                cumulativeVorticity += +vorticityMagnitude;
            }
        }
        

        

        //Lastly the element must be set to couples
        elementCoupled[ID] = true;

        //Debug.Log("Pseudo particle created at: " + pseudoElementPositions[ID].x + "," + pseudoElementPositions[ID].y + " and with vorticity of: " + vorticityMagnitude+" with a vector magnitude: "+velocityNormal.magnitude);

    }

    //Update positions
    void updatePositions()
    {

        //This is the start of the convection routine
        //first cycle through all elements, provided they exist and are not couples
        for (int i = 0; i < elementPositions.Length; i++)
        {

            //this statement excludes the "Stuck" elements
            if (elementExist[i] == true && elementCoupled[i] == false)
            {

                //now in this loop only the elements that are allowed to move are considered
                elementVelocity[i] = Vector3.zero;       //Its velocity is reset to zero before the summation

                    //now cycle through the pseudo elements (the only elements with vorticity at this time
                    for (int n = 0; n < pseudoElementPositions.Length; n++)
                        {
                            
                            //check the pseudo element in question exists
                            if (pseudoElementExist[n] == true)
                                 {

                                    if (i != n)
                                         {
                                             //find the radius
                                              Vector3 influenceRadius = elementPositions[i] - pseudoElementPositions[n];

                                              //now sumate the induced velocity
                                              elementVelocity[i] += (Vector3.Cross(pseudoElementVorticity[n], influenceRadius)) / Mathf.Pow(influenceRadius.magnitude, 2);
                                         }
                                     

                                 }

                        }

            }

        }

        //cycle through all elements
        for (int i = 0; i < elementPositions.Length; i++)
        {
            if (elementExist[i] == true && elementCoupled[i] == false)
            {

                elementPositions[i] += (elementVelocity[i] + freeStreamVelocity) * Time.deltaTime;
         
            }
        }

    }

    //function to reuse elements
    void spawnNew()
    {



    }

    //draw the elements in gizmos
    private void OnDrawGizmos()
    {

        if (shouldRun)
        {
            //cycle through all values
            for (int i = 0; i < elementPositions.Length; i++)
            {
                if (elementExist[i] == true)
                {
                    Gizmos.DrawSphere(elementPositions[i], 0.1f);
                }
            }

            //draw the solid body
            Gizmos.DrawSphere(new Vector3(37.0f, 5.0f, 0.0f), 2.0f);
            Gizmos.DrawSphere(new Vector3(44.0f, 10.0f, 0.0f), 2.0f);
            //Gizmos.DrawSphere(new Vector3(55.0f, 2.0f, 0.0f), 3.0f);
        }

        
    }

}
