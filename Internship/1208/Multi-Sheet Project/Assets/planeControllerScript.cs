using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class planeControllerScript : MonoBehaviour {

    //variables for flight control

    //rigid body reference
    public GameObject parentObject;
    private Rigidbody rb;

	// Use this for initialization
	void Start () {

        //get reference to the rigibbody
        rb = parentObject.GetComponent<Rigidbody>();

    }
	
	// Update is called once per frame
	void Update () {
		
        

    }

    private void FixedUpdate()
    {

        //apply simple force forward
        if (Input.GetKey("w"))
        {
            rb.AddForce(transform.right * 100.0f, ForceMode.Acceleration);
        }
        if (Input.GetKeyUp("w"))
        {
            //rb.AddRelativeForce(transform.right * -000.0f, ForceMode.Force);
        }

        //apply lift
        rb.AddForce(transform.up * transform.InverseTransformDirection(rb.velocity).z * 10);
        Debug.Log(transform.InverseTransformDirection(rb.velocity).z);

    }
}
