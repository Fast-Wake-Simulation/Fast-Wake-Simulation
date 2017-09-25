using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testParticles : MonoBehaviour {


	//reference to particle emmiter
	public ParticleSystem baseObject;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
		if (Input.GetKeyDown("space")){
			baseObject.emissionRate = 500.0f;
		}
		
	}
}
