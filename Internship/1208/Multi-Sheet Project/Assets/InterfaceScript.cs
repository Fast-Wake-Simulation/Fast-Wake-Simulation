using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceScript : MonoBehaviour {

	//Acces to the DVM is aquired through this reference
	public GameObject parentObject;
	public DVM dvmReference;

	//Public references for velocity controls and indicators
	public Slider xComponentSlider;
	public Text xIndicator;
	public Slider yComponentSlider;
	public Text yIndicator;
	public Slider zComponentSlider;
	public Text zIndicator;
	private float maxMagnitude = 5.0f;			//This is not a reference, but sets the magnitude that can be achieved with the slider
	
	//Public References for Vorticity controls and indicators
	public Slider xMagnitudeSlider;
	public Text xVortMagIndicator;
	public Slider yMagnitudeSlider;
	public Text yVortMagIndicator;
	public Slider sineMultiplierSlider;
	public Text sineMultiplierIndicator;
	public Slider noiseMultiplierSlider;
	public Text noiseMultiplierIndicator;
	private float maxVortMagnitude = 5.0f; 		//Same idea as the previous maxMagnitude but for vorticity
	private float maxSineMultiplier = 2.5f;
	private float maxNoiseMultiplier = 0.5f;


    //public references for the base particle system pane
    public Toggle positionParticlesEnabled;
    public GameObject positionParticlesOptions;
    public GameObject positionParticlesContainer;
    public positionParticleScript positionParticleReference;


    // Use this for initialization
    void Start () {
		
		//refence to the required scripts
		DVM dvmReference = parentObject.GetComponent<DVM>();
        positionParticleScript positionParticleReference = positionParticlesContainer.GetComponent<positionParticleScript>();



    }
	
	// Update is called once per frame
	void Update () {
		
		//Series of function calls for the seperate panes
		handleVelocityPane();
		handleVorticityPane();
        handleParticlesPane();

	}
	
	/////////////////////////////////////////////////////////////////////////////////////
	//To keep the code organised every options "Pane" is handled by a seperate function
	/////////////////////////////////////////////////////////////////////////////////////
	
	//Function to handle all components in the velocity pane
	void handleVelocityPane(){
		
		//first calculate the velocity magnitudes
		float xMagnitude = xComponentSlider.value * maxMagnitude;
		float yMagnitude = yComponentSlider.value * maxMagnitude;
		float zMagnitude = zComponentSlider.value * maxMagnitude;

		//Set the indicator values
		xIndicator.text = xMagnitude.ToString("F2");
		yIndicator.text = yMagnitude.ToString("F2");
		zIndicator.text = zMagnitude.ToString("F2");
		
		//now write to the actual DVM script
		dvmReference.freeStreamVelocity = new Vector3(xMagnitude, yMagnitude, zMagnitude);
		
	}
	
	//Function to handle the Vorticity Controls
	void handleVorticityPane(){
		
		//calculate the relative magnitude compared to the maximums
		float xMagnitude = xMagnitudeSlider.value * maxVortMagnitude;
		float yMagnitude = yMagnitudeSlider.value * maxVortMagnitude;
		float sineMultiplier = sineMultiplierSlider.value * maxSineMultiplier;
		float noiseMultiplier = noiseMultiplierSlider.value * maxNoiseMultiplier;
		
		//update the indicators
		xVortMagIndicator.text = xMagnitude.ToString("F2");
		yVortMagIndicator.text = yMagnitude.ToString("F2");
		sineMultiplierIndicator.text = sineMultiplier.ToString("F2");
		noiseMultiplierIndicator.text = noiseMultiplier.ToString("F2");
		
		//write the values to the DVM script
		dvmReference.maxVorticityX = xMagnitude;
		dvmReference.maxVorticityY = yMagnitude;
		dvmReference.maxSineMultiplier = sineMultiplier;
		dvmReference.noiseGenerator = noiseMultiplier;
		
	}

    //Function to handle the Particles pane
    void handleParticlesPane()
    {

        //this function is slightly different from the other pane handling functions as every option displays a new pane, so dispatcher functions are used
        //handle whether the options pane is visible, and if  the dispatcher needs to be called
        if (positionParticlesEnabled.isOn == true)
        {
            positionParticlesOptions.SetActive(true);
            
        }
        else
        {
            positionParticlesOptions.SetActive(false);
        }
        positionParticlesDispatcher();

    }
	
    //dispatch function for position particles
    void positionParticlesDispatcher()
    {

        //set variables accordingly
        positionParticleReference.particlesActive = positionParticlesEnabled.isOn;

    }

}
