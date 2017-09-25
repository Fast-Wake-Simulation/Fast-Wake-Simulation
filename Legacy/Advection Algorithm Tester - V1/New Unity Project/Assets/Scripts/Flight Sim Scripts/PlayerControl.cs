using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerControl : MonoBehaviour
{
    //This class contols a number of UI elements which report on the aircraft's height, velocity, angle, etc. As well as allowing the user to control the aircraft's thrust and aileron angles.

    //The physical gameobjects of the wings, tail and fuselage
	public Rigidbody fuselageRb;
    public GameObject rightWingPhysical;
    public GameObject leftWingPhysical;
	public GameObject tailPhysical;
    public GameObject tailVerticalPhysical;
    public GameObject fuselagePhysical;

    //The graphic child gameobjects of the wings and tail
    public GameObject tailVerticalGraphic;
    public GameObject tailGraphic;
    public GameObject rightWingGraphic;
    public GameObject leftWingGraphic;

    //The main UI elements
	public Slider throttleSlider;
    public Text throttleText;
    public Slider elevatorSlider;
    public Text elevatorSliderText;
	public Slider rollSlider;
	public Text rollSliderText;
    public Slider yawSlider;
    public Text yawSliderText;
    public Text velocityText;
    public Text heightText;
    public Slider climbAngleSlider;
    public Text climbAngleSliderText;
    public Slider rollAngleSlider;
    public Text rollAngleSliderText;

    //Sets the default values of throttle, horizontal tail aileron angle, vertical tail aileron angle, and wings aileron angle.
    public float maxThrust = 50.0f;
	public static float currentThrottlePercentage = 50.0f;
	public float currentElevatorPercentage = 50.0f;
	public float currentRollPercentage;
    public float currentYawPercentage = 50.0f;
    
    //Used as references to the Aerodynamics3 script so that functions of that variable values within that script may be set from here
	private Aerodynamics3 tailProperties;
    private Aerodynamics3 tailVerticalProperties;
    private Aerodynamics3 leftWingProperties;
	private Aerodynamics3 rightWingProperties;

    
    float climbAngle; //Actual climb angle of the fuselage around the x-axis
    float climbAnlgeDisplay; //Converted climb angle of the fuselage that is used by the onscreen slider
    float rollAngle; //Actual roll angle of the fuselage around the x-axis
    float rollAngleDisplay; //Converted roll angle of the fuselage that is used by the onscreen slider

    //The roll and yaw input values
    float rollInput;
    float yawInput;

    //The ground gameobject
    public GameObject ground;

	void Start()
	{
        UpdateUI(); //Udates the UI 

        currentThrottlePercentage = 50.0f; //Defaults the throttle percentage everytime the scene is loaded

        //Sets the references of the Aerodynamics3 script to their respective instance of the script
        tailProperties = tailPhysical.GetComponent<Aerodynamics3>();
        tailVerticalProperties = tailVerticalPhysical.GetComponent<Aerodynamics3>();
		leftWingProperties = leftWingPhysical.GetComponent<Aerodynamics3>();
		rightWingProperties = rightWingPhysical.GetComponent<Aerodynamics3>();
	}

    void Update ()
    {
        UpdateUI(); //Updates the UI every frame

        //Adjuists the audio depending on the trottle value
        fuselageRb.gameObject.GetComponent<AudioSource>().pitch = 1 + ((currentThrottlePercentage / 100) - 0.5f) * 0.2f; //changes the audio pitch depending on the throttle value
        fuselageRb.gameObject.GetComponent<AudioSource>().volume = 0.2f + currentThrottlePercentage / 100 / Mathf.Sqrt(1 + Mathf.Pow(currentThrottlePercentage / 100, 2)); //changes the audio volume depending on the throttle value
    }

	void FixedUpdate () 
	{
        //Contols the trottle with the mouse scrollwheel
		float throttleInput = Input.GetAxis("Mouse ScrollWheel");
		currentThrottlePercentage += throttleInput * 4f; //increase or decrease the throttle percentage depending on the mouse scrollwheel input
        currentThrottlePercentage = Mathf.Clamp(currentThrottlePercentage, 0, 100); //limits the throttle percentage between a value of 0 and 100

        //Checks whether the wind tunnel scene is loaded. If it is, roll input is locked to 0, as only elevation is allowed in that scene
        if (SceneManager.GetActiveScene().name == "Tunnel") 
        {
            rollInput = 0;
        }
        else
        {
            rollInput = Input.GetAxis("Horizontal");
        }

		currentRollPercentage = rollInput;

        //Controlls the elevators with the W and S keys
		float elevatorInput = Input.GetAxis("Vertical");
		//currentElevatorPercentage += elevatorInput * 0.2f; //increase or decrease the elevator percentage depending on the W or S keys input
        //currentElevatorPercentage = Mathf.Clamp(currentElevatorPercentage, 0, 100); //limits the elevator percentage between a value of 0 and 100
		currentElevatorPercentage = elevatorInput;

        //Checks whether the wind tunnel scene is loaded. If it is, yaw input is locked to 0, as only elevation is allowed in that scene
        if (SceneManager.GetActiveScene().name == "Tunnel")
        {
            yawInput = 0;
        }
        else
        {
            yawInput = Input.GetAxis("Yaw");
        }

        currentYawPercentage =  yawInput * 50;

        //Sets the thrust to a percentage of the max thrust
        Vector3 relativeThrustVector = new Vector3(0.0f, 0.0f, currentThrottlePercentage / 100.0f * maxThrust);

        //Sets the effective wing, and tail angles in radians
        float deltaAlphaWings = rollInput * 0.002f; 
		//float deltaAlphaTail = (currentElevatorPercentage / 100.0f - 0.5f) * 20.0f * Mathf.PI / 180.0f*0.5f;
		float deltaAlphaTail = elevatorInput*0.5f;
        float deltaAlphaTailVertical = (currentYawPercentage / 100.0f ) * 20.0f * Mathf.PI / 180.0f;

        //rotates the ailerons to the input
        leftWingGraphic.transform.localEulerAngles = new Vector3(-rollInput * 30, 0, 0);
        rightWingGraphic.transform.localEulerAngles = new Vector3(rollInput * 30, 0, 0);
        tailGraphic.transform.localEulerAngles = new Vector3(-((currentElevatorPercentage - 50) / 100 * 20) * 2, 0, 0); //multiplied by 2 in order for the rotation to be more visible
        tailVerticalGraphic.transform.localEulerAngles = new Vector3(0, -((currentYawPercentage) / 100 * 20) * 2, 0); //multiplied by 2 in order for the rotation to be more visible
        
        //Employ all the user changes on the control surfaces/motors
        leftWingProperties.SetDeltaAlpha(deltaAlphaWings);
		rightWingProperties.SetDeltaAlpha(deltaAlphaWings);
		tailProperties.SetDeltaAlpha(deltaAlphaTail);
        tailVerticalProperties.SetDeltaAlpha(deltaAlphaTailVertical);

        //Adds the thrust force to the fuselage
        fuselageRb.AddRelativeForce(relativeThrustVector);
	}

	void UpdateUI () //Updates the UI elements
    {
        //Sets the trottle text and value to the current throttle percentage
		throttleText.text = currentThrottlePercentage.ToString("F0"); //Rounds the current throttle percentage to a whole number
		throttleSlider.value = currentThrottlePercentage;

        //The elevate slider and text to the angle that the fuselage makes with the horizon around the local x-axis
        elevatorSlider.value = ((currentElevatorPercentage - 50) / 100 * 20);
		elevatorSliderText.text = "Elevator: " + elevatorSlider.value.ToString("F0") + "°";

        //Sets the velocity text to the current velocity of the fuselage in its local z-direction
        if (SceneManager.GetActiveScene().name == "Tunnel") //Used when in the tunnel scene as the velocity of the aircraft is zero so the wind velocity is used
        {
            velocityText.text = (Aerodynamics3.tunnelWind.magnitude * currentThrottlePercentage).ToString("F1") + "m/s";
        }
        else
        {
            velocityText.text = fuselageRb.velocity.magnitude.ToString("F1") + "m/s";
        }

        //Checks whether the current scene is the wind tunnel scene. If it is, then the height text is ignored
        if (SceneManager.GetActiveScene().name != "Tunnel")
        {
            heightText.text = (fuselageRb.position.y - ground.transform.position.y).ToString("F1") + "m"; //Sets the height text to the distance between the fuslage and the ground in the global y-direction  
        }
       
        //Sets the roll slider value and text to the current roll percentage
        rollSlider.value = currentRollPercentage;
        rollSliderText.text = "Roll: " + (currentRollPercentage * 10).ToString("F0") + "°";

        //Sets the yaw slider and text to the current yaw percentage
        yawSlider.value = ((currentYawPercentage) / 100 * 20);
        yawSliderText.text = "Yaw: " + yawSlider.value.ToString("F0") + "°";

        //Climb Angle Slider
        climbAngle = fuselagePhysical.transform.eulerAngles.x;
        if (climbAngle > 90) //This is used due to the way that unity measures angles. 
        {
            climbAnlgeDisplay = 360 - climbAngle;
        }
        else
        {
            climbAnlgeDisplay = -climbAngle;
        }

        //Sets the climb angle slider value and text to the corrected value of climb angle
        climbAngleSlider.value = climbAnlgeDisplay;
        climbAngleSliderText.text = climbAngleSlider.value.ToString("F0") + "°";

        //Roll Angle Slider
        rollAngle = fuselagePhysical.transform.eulerAngles.z;
        if (rollAngle > 180) //This is used due to the way that Unity measures angles
        {
            rollAngleDisplay = 360 - rollAngle;
        }

        else
        {
            rollAngleDisplay = -rollAngle;
        }

        //Sets the roll angle slider and text to the corrected value of roll angle
        rollAngleSlider.value = rollAngleDisplay;
        rollAngleSliderText.text = rollAngleSlider.value.ToString("F0") + "°";
    }

	public void RecieveThrottleSliderData (Slider sliderValue) //allows the use of the slider to change the throttle value
	{
		currentThrottlePercentage = sliderValue.value; //Sets the current throttle percentage to the value of the throttle slider
	}
}
