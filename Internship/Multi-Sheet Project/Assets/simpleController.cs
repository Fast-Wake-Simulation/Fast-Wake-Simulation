using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simpleController : MonoBehaviour {

    public Rigidbody rb;

	// Use this for initialization
	void Start () {

        //get the component
        Rigidbody rb = GetComponent<Rigidbody>();

    }
	
	// Update is called once per frame
	void Update () {

        //apply forces 
        if (Input.GetKey("up"))
        {
            rb.AddForce(transform.right * 100.0f);
        }
        if (Input.GetKey("down"))
        {
            rb.AddForce(transform.right * -100.0f);
        }
        if (Input.GetKey("left"))
        {
            rb.AddTorque(transform.up * -100.0f);
        }
        if (Input.GetKey("right"))
        {
            rb.AddTorque(transform.up * 100.0f);
        }

    }
}
