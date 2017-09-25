using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class planeScript2 : MonoBehaviour {

    //reference to parent object to get the rigidbody componenent from (Remember to set in editor)
    public GameObject parentObject;
    public Rigidbody parentRigidBody;           //To store the reference to control the physics of the plane

    //Public variables for flight variables
    public float maxVelocity;

	// Use this for initialization
	void Start () {

        initiatizeAll();        //Function used to load everything relevant, function used to keep the start and update functions more organised

	}
	
	// Update is called once per frame
	void Update () {
		

	}

    //all physics based updates go in this loop
    private void FixedUpdate()
    {

        //call all relevant functions (functions should be designed so that the order of execution does not matter)
        physicsControls();

    }

    //Function to load anything required 
    void initiatizeAll()
    {

        parentRigidBody = parentObject.GetComponent<Rigidbody>();       //This gets the reference to the rigidbody on start up

    }

    //this function controls the velocity of the plane
    void physicsControls()                                      //This version of the script controls the velocity, for force based control use the other plane control script                                
    {

        //First set the forward velocity
        if (Input.GetKey(KeyCode.LeftShift))
        {

            //check if the max speed has been reached
            if (parentRigidBody.velocity.magnitude < maxVelocity)
            {
                parentRigidBody.velocity = parentRigidBody.velocity + parentRigidBody.transform.right * 1.0f;
            }

        }
        //and now the equivalent for throttle down
        //First set the forward velocity
        if (Input.GetKey(KeyCode.LeftControl))
        {

            //check if the max speed has been reached
            if (parentRigidBody.velocity.magnitude > 0)
            {
                parentRigidBody.velocity = parentRigidBody.velocity - parentRigidBody.transform.right * 1.0f;
            }

        }

        //now set the rolling moments
        if (Input.GetKey("a"))
        {
            parentRigidBody.transform.localEulerAngles = parentRigidBody.transform.localEulerAngles + new Vector3(1.5f, 0.0f, 0.0f);
        }
        if (Input.GetKey("d"))
        {
            parentRigidBody.transform.localEulerAngles = parentRigidBody.transform.localEulerAngles + new Vector3(-1.5f, 0.0f, 0.0f);
        }

        //now the lifting moments
        if (Input.GetKey("w"))
        {
            parentRigidBody.transform.localEulerAngles = parentRigidBody.transform.localEulerAngles + new Vector3(0.0f, 0.0f, -1.5f);
        }
        if (Input.GetKey("s"))
        {
            parentRigidBody.transform.localEulerAngles = parentRigidBody.transform.localEulerAngles + new Vector3(-0.0f, 0.0f, 1.5f);
        }

        //now handle lift

        //now we direct the plane to its new trajectory
        parentRigidBody.velocity = parentRigidBody.velocity.magnitude * parentRigidBody.transform.right;
    }

}
