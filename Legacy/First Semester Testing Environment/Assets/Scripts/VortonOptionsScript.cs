using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class VortonOptionsScript : MonoBehaviour {

    //Declaring Input Variables
    public InputField rows;
    public InputField columns;
    public Button spawnButton;
    public Button clearButton;
    public GameObject vortexProbeParticle;
    public Transform probePointCollection;

    //Declaring spawn related variables
    public float vortonSpacing;
    public float vortonStrength;
    private Vector3 vortonPosition;
    public Vector3 vortonVelocity;


    //Conversion related vaiables
    public int rowsInt;
    public int columnsInt;

    //Array to hold blobs
    private GameObject[] blobArray;

    // Use this for initialization
    void Start () {

        //Add click events
        spawnButton.onClick.AddListener(SpawnButton);
        clearButton.onClick.AddListener(ClearButton);

    }
	
	// Update is called once per frame
	void Update () {

    }

    void SpawnButton() {

        //Button even indicator


        //Convert InputField strings to Integers
        rowsInt = Convert.ToInt32(rows.text);
        columnsInt = Convert.ToInt32(columns.text);

        //Two for loops to produce vortons in a grid shape
        for (int x = 1; x < rowsInt+1; x++)
        {
            for (int y = 1; y < columnsInt+1; y++)
            {

                //Produce necessary variables for the instantiated vortons
                vortonPosition = new Vector3((x-1) * vortonSpacing*2, 0, (y-1) * vortonSpacing);

                //Produce actual Vortons
                GameObject clone = Instantiate(vortexProbeParticle,vortonPosition, Quaternion.identity) as GameObject;

                //Puts the new blobs in a eay to handle place
                clone.transform.parent = probePointCollection;

                //Sets some random values for vortex parameters so the result math isnt zero
                VortexBlobBehaviour scr = clone.GetComponent<VortexBlobBehaviour>();  //Get the blob behaivour script
                //scr.VortexBlobStrength = vortonStrength;
                vortonStrength = 5;
                scr.VortexBlobStrength = (-1 * (vortonStrength / columnsInt) * (((y)-1) - (columnsInt/2)) / rowsInt) * ((5*x)/(rowsInt)) ;
                scr.initialisationVelocity = vortonVelocity;
                scr.initialVelocity = new Vector3 (0.0f, 0.0f, 0.0f);
                scr.positionIDX = x;
                scr.positionIDY = y;

            }

        }

    }

    void ClearButton()
    {

        //Populate array with current blobs
        blobArray = GameObject.FindGameObjectsWithTag("VortexBlob");

        //Delete all current blobs
        for (int id = 0; id <blobArray.Length; id++)
        {
            Destroy(blobArray[id]);
        }

    }

}
