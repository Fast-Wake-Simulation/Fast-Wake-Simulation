using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryScript : MonoBehaviour {


    //Variables I might need
    Vector2 freeStreamVelocity = new Vector2(0.5f, 0.0f);
    Vector2[] elementPositions = new Vector2[5];
    Vector2[] elementVelocity = new Vector2[5];
    float[] elementVorticities = new float[5];
    

	// Use this for initialization
	void Start () {
		
        //seed initial conditions
        for (int y = 0; y < 5; y++)
        {
            elementPositions[y] = new Vector2(0, y);
            elementVorticities[y] = 0.0f;
        }

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //Fixed Update for physics related maths
    private void FixedUpdate()
    {

        //update positions
        updatePositions();

        if (collisionCheck() == true)
        {
            Debug.Log("Collided!");
        }

    }

    private void updatePositions()
    {
        
        for (int iteration = 0; iteration < 5; iteration++)
        {
            elementPositions[iteration] += Time.deltaTime * (elementVelocity[iteration] + freeStreamVelocity);
        }

    }

    private bool collisionCheck()
    {
        bool collided = false;

        //check for collisions
        for (int iteration = 0; iteration < 5; iteration++)
        {
            if (elementPositions[iteration].x > 2 && elementPositions[iteration].x < 5)
            {
                if (elementPositions[iteration].y > 0 && elementPositions[iteration].y < 5)
                {

                    collided = true;
                }
            }
        }

        return collided;
    }

    private void OnDrawGizmos()
    {

        for (int iteration = 0; iteration < 5; iteration++)
        {
            Gizmos.DrawSphere(new Vector3(elementPositions[iteration].x, elementPositions[iteration].y, 0.05f), 0.1f);
        }

    }
}
