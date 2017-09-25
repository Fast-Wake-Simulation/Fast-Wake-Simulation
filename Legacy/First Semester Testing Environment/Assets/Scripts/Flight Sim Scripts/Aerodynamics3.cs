using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class Aerodynamics3 : MonoBehaviour
{
    //This class controls the aerodynamics on the wings and tail

	public GameObject liftArrow;
	public Rigidbody rb;  //The rigidbody of the gameobject this script is attached to
	private Vector3 velocityEarth; //Not used anymore
	public float alpha; //the effective local angle of the gameobject this script is attached to
	public float normalForce;  //The normal force that will be applied
	private Vector3 aerodynamicForceBodyAxes;
	private Vector3 velocityWind; //The effective velocity of the 'wind' relative to the aircraft
	private Vector3 aerodynamicCentre; //The local aerodynamic centre of the aircraft
	private float panelArea; //The area of the gameobject this script is attached to
	private Vector3 size;
	public float vSquared; //Needs to be public for export reasons
	private bool liftArrowSwitch = true;
	private GameObject liftArrowInst;

    //The indivisual parts of the 2D lift visualisation image
    public Image rightWingImage;
    public Image leftWingImage;
    public Image tailImage;

    //The right wing condensation emitter, emitter transform, and original emmiter scale
    public ParticleSystem rightWingEmitter;
    private Transform rightWingEmitterTransform;
    private Vector3 rightWingEmitterSizeOriginal;

    //The left wing condensation emitter, emitter transform, and original emitter scale
    public ParticleSystem leftWingEmitter;
    private Transform leftWingEmitterTransform;
    private Vector3 leftWingEmitterSizeOriginal;

    //The left and right wing mesh condensation particle system
    public ParticleSystem rightWingEmitterMesh;
    public ParticleSystem leftWingEmitterMesh;

    //The position that the trail render for the wing tip will be positioned
    public GameObject rightWingTip;
    public GameObject leftWingTip;

    //The left and right wing graphic children of the physical gameobjects
    public Transform rightWingGraphic;
    public Transform leftWingGraphic;

    //The materials of the wing tip condensation 
    private Material rightWingTipMaterial;
    private Material leftWingTipMaterial;

    public static Vector3 tunnelWind; //the wind velocity within the wind tunnel scene

    public float deltaAlpha;

	void Start()
	{
		liftArrowInst = new GameObject();

        tunnelWind = new Vector3(0, 0, -1); //Sets the default direction that the wind is travelling within the wind tunnel scene

        rightWingEmitterTransform = rightWingEmitter.gameObject.transform; //creates a reference variable of the right wing condensation emitter system's transform
        leftWingEmitterTransform = leftWingEmitter.gameObject.transform; //creates a reference variable of the left wing condensation emitter system's transform

        rightWingEmitterSizeOriginal = rightWingEmitterTransform.localScale; //Creates a referance variable of the right wing condensation emitter system's scale
        leftWingEmitterSizeOriginal = leftWingEmitterTransform.localScale;  //Creates a referance variable of the left wing condensation emitter system's scale

        rightWingTipMaterial = rightWingTip.GetComponent<TrailRenderer>().material; //Creates a referance veriable to the right wing tip condensation trail renderer
        leftWingTipMaterial = leftWingTip.GetComponent<TrailRenderer>().material; //Creates a referance veriable to the left wing tip condensation trail renderer

        rb = GetComponent<Rigidbody> (); //Sets rb to be a reference of the rigidbody component of the gameobject that this script is attached to.

		size = GetComponent<BoxCollider> ().size;// box collider size is set to panel size in setup
		aerodynamicCentre = new Vector3 (0, 0, size.z/4);

        //This checks whether the gameobject this script is attached to is the vertical tail. If it is then the panelArea is calculated in the z y plane rather than the x z plane
        if (rb.CompareTag("TailVertical"))
        {
            panelArea = size.z * size.y / 6; //Sets the panelArea. /6 is used as it can be unstable with a large area. if instability continues, remove the vertTail tag from the vertical tail
        }
        else
        {
            panelArea = size.z * size.x; //Sets the panelArea
        }

        //Adapts the condensation lifetime depending on the size of the wings in the z-direction. 
        if (rb.CompareTag("LeftWing"))
        {
            leftWingEmitter.startLifetime = (leftWingGraphic.localScale.z / 0.3743556f) * 0.8f; //0.3743556f is used as the condensation lifetime was originally created using the default aircraft dimensions as a reference. So by dividing the scale by this, it will increase the start time accordingly
        }
        else if (rb.CompareTag("RightWing"))
        {
            rightWingEmitter.startLifetime = (rightWingGraphic.localScale.z / 0.3743556f) * 0.8f; //0.3743556f is used as the condensation lifetime was originally created using the default aircraft dimensions as a reference. So by dividing the scale by this, it will increase the start time accordingly
        }

    }

    void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().name == "Tunnel") //As the aircraft is motionless within the wind tunnel scene, the wind tunnel velocity is considered instead of the aircraft's
        {
            velocityWind = transform.InverseTransformDirection(-tunnelWind * PlayerControl.currentThrottlePercentage); //Gets the local velocity of the wind relative to the aircrafts reference.
        }
        else
        {
            velocityWind = transform.InverseTransformDirection(rb.velocity); //Gets the local velocity of the aircraft.
        }
                                                                    

        //vSquared is calculated in the z x plane for the vertical tail and in the z y plane for the wings and horizontal tail.
        if (rb.CompareTag("TailVertical"))
        {
            vSquared = velocityWind.z * velocityWind.z + velocityWind.x * velocityWind.x; //used to calculate the area of the vertical tail
        }
        else
        {
            vSquared = velocityWind.z * velocityWind.z + velocityWind.y * velocityWind.y; // exclude wind in panel spanwise direction
        }

        //Checks which aircraft part this script is attached to by checking its tag, and then performs the necessary function
        if (rb.CompareTag("LeftWing"))
        {
            LeftWingControl();
        }
        else if (rb.CompareTag("RightWing"))
        {
            RightWingControl();
        }
        else if (rb.CompareTag("Tail"))
        {
            TailControl();
        }
        else if (rb.CompareTag("TailVertical"))
        {
            TailVerticalControl();
        }

        
    }

    void LeftWingControl() //Controls the left wing aerodynamics
    {
        alpha = Mathf.Atan2(-velocityWind.y, velocityWind.z) + deltaAlpha; //Sets the angle, alpha, that the left wing makes with the aircraft's velocity direction
		normalForce = Mathf.Clamp(1.5f * 3.14f * Mathf.Sin(alpha) * panelArea * vSquared, -200, 200); //Sets the normal force that will be applied to the rb
		rb.AddForceAtPosition(rb.transform.up * normalForce, rb.transform.TransformPoint(rb.GetComponent<BoxCollider>().center) + size.z/4 * rb.transform.forward); //adds the normal force in the local y-direction of the left wing, and at the centre of the left wing's boxcollider


		if (liftArrowSwitch) 
		{
			liftArrowInst = Instantiate (liftArrow, rb.transform.TransformPoint (rb.GetComponent<BoxCollider> ().center) + size.z / 4 * rb.transform.forward, Quaternion.identity) as GameObject;
			liftArrowInst.transform.parent = transform;
			liftArrowInst.transform.localRotation = Quaternion.Euler(90,0,0);
			liftArrowSwitch = false;
		}
		else liftArrowInst.transform.localScale = new Vector3 (liftArrow.transform.localScale.x, liftArrow.transform.localScale.y, normalForce/5);


        //Adjusts the colour of the left wing on the 2D lift visualisation image
        if (normalForce >= 0)
        {
            leftWingImage.color = new Color(1 * normalForce / 50, 0, 0, 0.75f); //Turns the left wing image red as a function of the normal force when the normal force is positive
        }
        else
        {
            leftWingImage.color = new Color(0, 0, 1 * Mathf.Abs(normalForce / 20), 0.75f); //Turns the left wing image blue as a function of the normal force when the normal force is negative
        }

        //Used to control the left wing mesh condensation particle emitter
        var rate = new ParticleSystem.MinMaxCurve(); //creates a variable which references the mesh particle system's Min Max Curve
        var em = leftWingEmitterMesh.emission; //creates a variable which references the mesh particles system's emission component
        rate.constantMax = 900 * Mathf.Pow(Mathf.Clamp(normalForce / 80, 0, 1), 2); //Sets the mesh particle system rate as a function of the normal force squared
        em.rate = rate; //Sets the mesh particle system rate as a function of the normal force squared

        //Used to control the left wing billboard condensation particle emitter
        leftWingEmitter.startColor = new Color(1, 1, 1, 0.6f *  Mathf.Pow(Mathf.Clamp((normalForce + 8) / 80, 0, 1), 2)); //Changes the start colour of the particle emitter as a function of the normal force squared. Increase the power in oder to make the turn on point more sharp. +8 is used so that condensation is present when lift = weight and the aircraft is flying horizontally.
        leftWingEmitterTransform.localScale = new Vector3(-size.x*2, leftWingEmitterSizeOriginal.y * Mathf.Pow(Mathf.Clamp((normalForce + 8) / 80, 0, 1), 2), leftWingEmitterSizeOriginal.z); //Changes the height of the condensation as a function of the normal force. Increase the power in order to make the turn on point more sharp.  +8 is used so that condensation is present when lift = weight and the aircraft is flying horizontally.

        leftWingTipMaterial.SetColor("_TintColor", new Color(1, 1, 1, 0.6f * Mathf.Pow(Mathf.Clamp(normalForce + 8 / 80, 0, 1), 2))); //Chamges the wing tip condensation trail renderer as a function of the normal force squared.  Increase the power in oder to make the turn on point more sharp.  +8 is used so that condensation is present when lift = weight and the aircraft is flying horizontally.
    }

    void RightWingControl() //Controls the right wing aerodynamics
    {
        alpha = Mathf.Atan2(-velocityWind.y, velocityWind.z) - deltaAlpha; //Sets the angle, alpha, that the right wing makes with the aircraft's velocity direction  
		normalForce = Mathf.Clamp(1.5f * 3.14f * Mathf.Sin(alpha) * panelArea * vSquared, -200, 200); //Sets the normal force that will be applied to the rb
		rb.AddForceAtPosition(rb.transform.up * normalForce, rb.transform.TransformPoint(rb.GetComponent<BoxCollider>().center) + size.z/4 * rb.transform.forward);  //adds the normal force in the local y-direction of the right wing, and at the centre of the right wing's boxcollider



		if (liftArrowSwitch) 
		{
			liftArrowInst = Instantiate (liftArrow, rb.transform.TransformPoint (rb.GetComponent<BoxCollider> ().center) + size.z / 4 * rb.transform.forward, Quaternion.identity) as GameObject;
			liftArrowInst.transform.parent = transform;
			liftArrowInst.transform.localRotation = Quaternion.Euler(90,0,0);
			liftArrowSwitch = false;
		}
		else liftArrowInst.transform.localScale = new Vector3 (liftArrow.transform.localScale.x, liftArrow.transform.localScale.y, normalForce/5);




		//Debug.DrawLine(rb.transform.TransformPoint(rb.GetComponent<BoxCollider>().center) + size.z/4 * rb.transform.forward, rb.transform.TransformPoint(rb.GetComponent<BoxCollider>().center) + size.z/4 * rb.transform.forward + rb.transform.up*100);

        //Adjusts the colour of the right wing on the 2D lift visualisation image
        if (normalForce >= 0)
        {
            rightWingImage.color = new Color(1 * normalForce / 50, 0, 0, 0.75f); //Turns the right wing image red as a function of the normal force when the normal force is positive
        }
        else
        {
            rightWingImage.color = new Color(0, 0, 1 * Mathf.Abs(normalForce / 20), 0.75f); //Turns the right wing image blue as a function of the normal force when the normal force is negative
        }

        //Used to control the right wing mesh condensation particle emitter
        var rate = new ParticleSystem.MinMaxCurve(); //creates a variable which references the mesh particle system's Min Max Curve
        var em = rightWingEmitterMesh.emission; //creates a variable which references the mesh particles system's emission component
        rate.constantMax = 900 * Mathf.Pow(Mathf.Clamp(normalForce / 80, 0, 1), 2); //Sets the mesh particle system rate as a function of the normal force squared
        em.rate = rate; //Sets the mesh particle system rate as a function of the normal force squared

        //Used to control the left wing billboard condensation particle emitter
        rightWingEmitter.startColor = new Color(1, 1, 1, 0.6f * Mathf.Pow(Mathf.Clamp((normalForce + 8) / 80, 0, 1), 2)); //Changes the start colour of the particle emitter as a function of the normal force squared. Increase the power in oder to make the turn on point more sharp. +8 is used so that condensation is present when lift = weight and the aircraft is flying horizontally.
        rightWingEmitterTransform.localScale = new Vector3(size.x*2, rightWingEmitterSizeOriginal.y * Mathf.Pow(Mathf.Clamp((normalForce + 8) / 80, 0, 1), 2), rightWingEmitterSizeOriginal.z); //Changes the height of the condensation as a function of the normal force. Increase the power in oder to make the turn on point more sharp. +8 is used so that condensation is present when lift = weight and the aircraft is flying horizontally.

        rightWingTipMaterial.SetColor("_TintColor", new Color(1, 1, 1, 0.6f * Mathf.Pow(Mathf.Clamp(normalForce + 8 / 80, 0, 1), 2))); //Chamges the wing tip condensation trail renderer as a function of the normal force squared.  Increase the power in oder to make the turn on point more sharp. +8 is used so that condensation is present when lift = weight and the aircraft is flying horizontally.
    }

    void TailControl () //Controls the horizonral tail aerodynamics
    {
		alpha = Mathf.Atan2(-velocityWind.y, velocityWind.z) + deltaAlpha * 0.1f; //Sets the angle, alpha, that the horizontal tail makes with the aircraft's velocity direction
		normalForce = Mathf.Clamp(1.5f * 3.14f * Mathf.Sin(alpha) * panelArea * vSquared, -200, 200); //Sets the normal force that will be applied to the rb
        aerodynamicForceBodyAxes = new Vector3(0, normalForce, 0); //The force and direction which will be applied to the tail
		rb.AddForceAtPosition(rb.transform.up * normalForce, rb.transform.TransformPoint(rb.GetComponent<BoxCollider>().center) + size.z/4 * rb.transform.forward);  //Applies the lift force to the tail, relative to the tails coordinate system


		if (liftArrowSwitch) 
		{
			liftArrowInst = Instantiate (liftArrow, rb.transform.TransformPoint (rb.GetComponent<BoxCollider> ().center) + size.z / 4 * rb.transform.forward, Quaternion.identity) as GameObject;
			liftArrowInst.transform.parent = transform;
			liftArrowInst.transform.localRotation = Quaternion.Euler(90,0,0);
			liftArrowSwitch = false;
		}
		else liftArrowInst.transform.localScale = new Vector3 (liftArrow.transform.localScale.x, liftArrow.transform.localScale.y, normalForce/5);




	

        //Adjusts the colour of the tail on the 2D lift visualisation image
        if (normalForce >= 0)
        {
            tailImage.color = new Color(1 * normalForce / 50, 0, 0, 0.75f); //Turns the tail image red as a function of the normal force when the normal force is positive
        }
        else
        {
            tailImage.color = new Color(0, 0, 1 * Mathf.Abs(normalForce / 20), 0.75f); //Turns the tail image blue as a function of the normal force when the normal force is negative
        }
    }

    void TailVerticalControl () //Controls the vertical tail aerodynamics
    {
        alpha = Mathf.Atan2(-velocityWind.x, velocityWind.z) - deltaAlpha*1.35f; //Sets the angle, alpha, that the vertical tail makes with the aircraft's velocity direction
		normalForce = Mathf.Clamp(1.5f * 3.14f * Mathf.Sin(alpha) * panelArea * vSquared*0.2f, -200, 200); //Sets the normal force that will be applied to the rb
        rb.AddForceAtPosition(rb.transform.right * normalForce, rb.transform.InverseTransformPoint(rb.GetComponent<BoxCollider>().center)); //adds the normal force in the local x-direction of the vertical tail, and at the centre of the vertical tail's boxcollider
    }

	public void SetDeltaAlpha(float value) //This sets the value of deltaAlpha as defined in PlayerControl
	{
		deltaAlpha = value;
	}

}
