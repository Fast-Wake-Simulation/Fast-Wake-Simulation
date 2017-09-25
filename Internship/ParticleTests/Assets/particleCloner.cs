using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particleCloner : MonoBehaviour {

    //public references
    public ParticleSystem baseParticles;

    //array to hold the particle systems
    private ParticleSystem[,] effectParticles = new ParticleSystem[10, 10];

	// Use this for initialization
	void Start () {
		
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                effectParticles[x, y] = Instantiate(baseParticles, new Vector3(x,0,y), Quaternion.identity);
            }
        }

	}
	
	// Update is called once per frame
	void Update () {
		
        //sine wave coolness
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                effectParticles[x, y].transform.position = new Vector3(x,1.5f * Mathf.Sin(1.4f * (x + Time.time)), y);
            }
        }

	}
}
