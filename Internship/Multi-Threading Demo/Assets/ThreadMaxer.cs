using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadMaxer : MonoBehaviour {

    public float testVal;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        testVal = Mathf.Pow(9, 99);

        for (int i = 0; i < 1000; i++)
        {
            Debug.Log(testVal*i);
        }
        
	}
}
