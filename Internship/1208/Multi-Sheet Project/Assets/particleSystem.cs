using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particlesSystem : MonoBehaviour {

	//public references for the dvm script
	public DVM dvmReference;
	public ParticleSystem[,] positionParticles;
	public ParticleSystem baseParticle;

	// Use this for initialization
	void Start () {
		
		//function calls to each systems initialiszation routine
		initialisePositionsBasedSystem();
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Each seperatre particle system has a Start() and Update() call to keep the system self contained and tidy
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	
	//function to initialise the position based system
	void initialisePositionsBasedSystem(){
		
		//first particles need to be created
		positionParticles = new ParticleSystem[10,10];
		for (int x = 0; x < 10; x++){
			for (int y = 0; y < 10; y++){
				positionParticles[x,y] = Instantiate(baseParticle);
			}
		}
		
	}
}
