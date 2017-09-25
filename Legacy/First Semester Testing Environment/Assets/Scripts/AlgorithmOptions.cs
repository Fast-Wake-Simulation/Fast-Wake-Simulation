using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class AlgorithmOptions : MonoBehaviour {

    //UI controls
    public Button runButton;
    public InputField iterationCount;
    public Dropdown algorithmSelection;
    public int iterationCountInt = 0;
    public int algorithm = 2;

    //Time recording variables
    public float startTime;
    public float endTime;

    private GameObject[] currentBlobs;
    public GameObject basicAdvectionReference;
    public GameObject complexAdvectionReference;
    public GameObject basicAdvectionPanelReference;
    public GameObject complexAdvectionPanelReference;
    public Button basicAdvectionInitializeReferences;
    public Button complexAdvectionCalculateGroups;

	// Use this for initialization
	void Start () {

        //Handle button stuff
        runButton.onClick.AddListener(runButtonFunction);
        complexAdvectionCalculateGroups.onClick.AddListener(calculateComplexFunction);
        basicAdvectionInitializeReferences.onClick.AddListener(initializeBasicFunction);

    }
	
	// Update is called once per frame
	void Update () {

        //Handle whether to use the physics engine
        if (iterationCountInt == 0)    //this sets the physics timestep
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }

        //Handle extra options
        if (algorithmSelection.value == 2)      //Extra options for complex advection
        {
            complexAdvectionPanelReference.SetActive(true);
        }
        else
        {
            complexAdvectionPanelReference.SetActive(false);
        }
        //Same for Basic Advection
        if (algorithmSelection.value == 1)      //Extra options for complex advection
        {
            basicAdvectionPanelReference.SetActive(true);
        }
        else
        {
            basicAdvectionPanelReference.SetActive(false);
        }


    }

    void FixedUpdate()
    {
        //Reduce iteration count
        if (iterationCountInt > 0)
        {
            iterationCountInt = iterationCountInt - 1;

            if (iterationCountInt == 1)
            {
                endTime = Time.realtimeSinceStartup;
                Debug.Log(string.Concat("Time for Iterations: ", endTime - startTime));
            }
        }

    }

    //Function called when button pressed
    void runButtonFunction()
    {
        
        //Convert the input field to a useable variable
        iterationCountInt = Convert.ToInt32(iterationCount.text);
  
        //If VortexBlobBehaivour.cs should be run
        if (algorithmSelection.value == 0)
        {

            //find all current blobs
            currentBlobs = GameObject.FindGameObjectsWithTag("VortexBlob");

            for (int i = 0; i < currentBlobs.Length; i++)
            {
                //Get reference to the behaivour script
                VortexBlobBehaviour scriptref = currentBlobs[i].GetComponent<VortexBlobBehaviour>();

                //set the iteration count
                scriptref.iterationCount = iterationCountInt;

            }

        }

        //If BasicAdvection.cs should be run
        if (algorithmSelection.value == 1)
        {

            //Get reference to the script
            BasicAdvection scriptref2 = basicAdvectionReference.GetComponent<BasicAdvection>();
            scriptref2.iterations = iterationCountInt;

        }

        //If ComplexAdvection.cs should be run
        if (algorithmSelection.value == 2)
        {

            //Get referene to the script
            ComplexAdvection scriptref3 = complexAdvectionReference.GetComponent<ComplexAdvection>();
            scriptref3.iterationCount = iterationCountInt;
    
        }

        //Record time at the start
        startTime = Time.realtimeSinceStartup;

    }

    //Button to initialize references for Basic Advection
    void initializeBasicFunction()
    {

        BasicAdvection sriptref5 = basicAdvectionReference.GetComponent<BasicAdvection>();
        sriptref5.findBlobObjects = true;

    }

    //Button to initialize complex advection to calculate groups
    void calculateComplexFunction()
    {

        //Get reference to the script
        ComplexAdvection scriptref4 = complexAdvectionReference.GetComponent<ComplexAdvection>();
        scriptref4.calculateGroupings = true;

    }
}
