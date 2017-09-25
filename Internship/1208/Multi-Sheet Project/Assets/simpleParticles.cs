using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simpleParticles : MonoBehaviour {

    //initialization variables
    private bool scriptActive = false;

    //reference to base sprite
    public GameObject baseSprite;

    //particle storage
    private GameObject[,] effectSprites = new GameObject[10, 100];
    private SpriteRenderer[,] effectReference = new SpriteRenderer[10,100];

    //reference to the dvm to get the positions from
    public DVM referenceDVM;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		

        if (Input.GetKeyDown("e"))
        {
    
            if (scriptActive == false)
            {
                initialize();
                scriptActive = true;
            }


        }

        if (scriptActive == true)
        {
            updatePartciles();
        }

	}

    //
    void initialize()
    {
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 100; y++)
            {
                effectSprites[x, y] = Instantiate(baseSprite, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
                effectReference[x, y] = effectSprites[x, y].GetComponent<SpriteRenderer>();
            }
        }

        Debug.Log("I work");

    }
    
    //
    void updatePartciles()
    {
        
        //cycle through all particles
        for (int x = 0; x < 10; x++)
        {


            for (int y = 0; y < 100; y++)
            {

                //reposition all particles
                effectSprites[x, y].transform.position = referenceDVM.elementPositions[x, y, 0];

                //set their scale
                //effectSprites[x, y].transform.localScale = ((50/100) * y + (50 / 100) * ((Time.time - referenceDVM.lastTime) / (referenceDVM.spawnCounter * 2))) * new Vector3(51.0f, 51.0f, 51.0f); 
                effectSprites[x, y].transform.localScale = ((50F / 100F) * (y + ((Time.time - referenceDVM.lastTime) / (referenceDVM.spawnCounter))) ) * new Vector3(0.4f, 0.4f, 0.4f);

                //set their transparency
                if (y == 0)
                {
                    //effectReference[x, y].color = new Color(1.0f, 1.0f, 1.0f, (Time.time - referenceDVM.lastTime) / referenceDVM.spawnCounter);

                    for ( int yi = 0; yi < 2; yi++)
                    {
                        effectReference[x, y+yi].color = new Color(1.0f, 1.0f, 1.0f, (yi*0.5f)* (Time.time - referenceDVM.lastTime) / (referenceDVM.spawnCounter *2));
                    }

                }

            }
        }

    }

}
