using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class positionParticleScript : MonoBehaviour {


    //Variables to handle when to destroy and create particle system 
    public bool particlesActive = false;
    private bool particlesInitiated = false;


    //Varaibales for the actual particle effect
    public GameObject baseParticle;
    private GameObject[,] positionBasedParticles = new GameObject[10, 10];
    private ParticleSystem[,] positionBasedParticlesReference = new ParticleSystem[10, 10];
    private Renderer[,] positionBasedParticlesRenderer = new Renderer[10,10];

    //references to get simulation information from the DVM
    public DVM referenceDVM;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		

        if (particlesActive == true)
        {
            if (particlesInitiated == true)
            {
                updateParticles();
            }
            else
            {
                initiateParticles();
            }
        }
        else
        {
            if (particlesInitiated == true)
            {
                deleteParticles();
            }
        }
        

	}

    //Function to initialize the particle system
    void initiateParticles()
    {
        
        //create grid of particles
        for (int x = 0; x < 10; x=x+5)
        {
            for (int y = 0; y < 10; y++)
            {
                //positionBasedParticles[x,y] = Instantiate(baseParticle, referenceDVM.elementPositions[x,y,0], Quaternion.identity);
                //positionBasedParticles[x, y] = Instantiate(baseParticle, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
                positionBasedParticlesReference[x, y] = positionBasedParticles[x, y].GetComponent<ParticleSystem>();
                positionBasedParticlesRenderer[x, y] = positionBasedParticles[x,y].GetComponent<Renderer>();
            }
        }

        //get references
        for (int x = 0; x < 10; x = x + 5)
        {
            for (int y = 0; y < 10; y++)
            {
                //positionBasedParticles[x, y] = Instantiate(baseParticle, referenceDVM.elementPositions[x, y, 0], Quaternion.identity);
                //positionBasedParticles[x, y] = Instantiate(baseParticle, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);

            }
        }

        particlesInitiated = true;
    }

    //Function to update the particles
    void updateParticles()
    {

        //Just a simple or loop to change their positions
        for (int x = 0; x < 10; x=x + 5)
        {
            for (int y = 0; y < 10; y++)
            {
                positionBasedParticles[x, y].transform.position = referenceDVM.elementPositions[x, y, 0];
                //positionBasedParticlesReference[x, y].renderer. = new Color32(0, 0, 0, 0);
                //positionBasedParticlesReference[x, y].shape.radius = 4.0f;
            }
        }

        //set the particles opacity
        //Just a simple or loop to change their positions
        for (int x = 0; x < 10; x = x + 5)
        {
            for (int y = 0; y < 10; y++)
            {
                //positionBasedParticlesReference[x, y]. 
            }
        }

    }

    //function to clear out the particle system
    void deleteParticles()
    {
        Debug.Log("Deinitated");
        particlesInitiated = false;
    }

}
