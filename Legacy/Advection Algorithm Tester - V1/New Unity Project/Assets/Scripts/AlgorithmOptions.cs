using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AlgorithmOptions : MonoBehaviour {

    //UI controls
    public Button runButton;

	// Use this for initialization
	void Start () {

        //Handle button stuff
        runButton.onClick.AddListener(runButtonFunction);

    }
	
	// Update is called once per frame
	void Update () {
	
	}

    //Function called when button pressed
    void runButtonFunction()
    {
        Debug.Log("Lol imma thing");

        //If statemets 
    }
}
