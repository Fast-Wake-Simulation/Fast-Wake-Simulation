using UnityEngine;
using System.Collections;

public class DesignSheet : MonoBehaviour
{
	// public  Vector3  velocityEarth =  new Vector3 (0, 0, 1); UNUSED
	public float allUpMass = 2.0f; // mass of whole aircraft in Kg
    public float wingloading; // = 20; // N/m^2

	//public float thrustToWeight; // overall thrust to weight ratio

	public float staticMargin = 0.1f; // (xac-xcg)/cbar dimensionless
    public float wingAspectRatio; //7.0f;
    public float wingDihedral; //0f; //wing dihedral angle (degrees)
    public float tailAspectRatio; //5.0f;
    public float wingTailAreaRatio; // = 0.2f; // ratio of tail area to wing area. normally around 5.
    public float tailVolumeRatio; //= 0.4f; // normally around 0.5

	public Vector3 Xcg; // centre of gravity location from forward reference (m)
	public Vector3 Xact; // tail aerodynamic cenetre from forward reference (m)
	public  Vector3 Xacw; // wing aerodynamic centre from forward reference (m)
	public float wingChord; //wing mean chord (m)
	public float wingArea; // m^2
	public float tailArea; // m^2

	public Vector3 wingPosition;
	public Vector3 tailPosition;
    public Vector3 fuselagePosition;

	public Vector3 wingScale;
	public Vector3 tailScale;
	public Vector3 fuselageScale;
	public Vector3 ballastScale;

	public float massFuselage;
	public float massTail;
	public float massWing;
	public Vector3 fuselageCGPosition;
	public Vector3 aircraftCGPosition;
	public Vector3 aircraftACPosition;

	private Vector3 xnose; // aircraft nose from forward reference, m
	private Vector3 fuselageLength; // fuselage length for visual purposes, m
	private ConfigurableJoint wingConfigurableJoint;

	public GameObject leftWingPhysical;
	public GameObject leftWingGraphic;
    public GameObject leftWingAileronPhysical;
    public GameObject leftWingAileronGraphic;
    public GameObject rightWingPhysical;
	public GameObject rightWingGraphic;
    public GameObject rightWingAileronPhysical;
    public GameObject rightWingAileronGraphic;
    public GameObject TailPhysical;
	public GameObject TailGraphic;
    public GameObject TailAileronPhysical;
    public GameObject TailAileronGraphic;
    public GameObject TailVerticalPhysical;
    public GameObject TailVerticalGraphic;
    public GameObject TailVerticalAileronPhysical;
    public GameObject TailVerticalAileronGraphic;
    public GameObject FuselagePhysical;
	public GameObject FuselageGraphic;
	public GameObject CGObject;
	public GameObject ACObject;
	public Vector3 startingVelocity = new Vector3 ( 0, 0, 0);

	void Awake ()
    {
        //loads the custom settings from the setup screen
        wingAspectRatio = SetupController.wingAspectRatio; //7.0f;
        wingloading = SetupController.wingloading; //0f; //wing dihedral angle (degrees)
        tailAspectRatio = SetupController.tailAspectRatio;
        wingTailAreaRatio = SetupController.wingTailAreaRatio;
        tailVolumeRatio = SetupController.tailVolumeRatio;
		staticMargin = SetupController.staticMargin;

        // set up default values in inspector and calculate derived quantites
        //coord system is based on unity rather than aircraft. z is forward, x is right and y is up
        float g = 9.81f; //acceleartion due gravity m/s^2
		float pi = 3.14f;

		// JOAO - Wing Properties
		wingArea = allUpMass * g / wingloading;
		float wingSpan = Mathf.Sqrt (wingArea * wingAspectRatio);
		float wingChord = wingSpan / wingAspectRatio;
		float aWing = 2 * pi / (1 + 2 * pi / (pi * 1 * wingAspectRatio)); // tail lift curve slope
		float zacw = 0.25f*wingChord; // using wing centre point as longitudinal reference point

		// JOAO - Tail Properties
		tailArea = wingArea * wingTailAreaRatio;
		float tailSpan = Mathf.Sqrt (tailArea * tailAspectRatio);
		float tailChord = tailSpan / tailAspectRatio;
		float zTail =-tailVolumeRatio*wingChord * wingArea / tailArea; //length from centre of wing to tail ac.
		float aTail = 2 * pi / (1 + 2 * pi / (pi * 1 * tailAspectRatio)); // tail lift curve slope

		// JOAO - General airplane properties
		float zac = (wingArea * aWing * zacw + tailArea * aTail * zTail) / (wingArea * aWing + tailArea * aTail);// overal ac position
		float zcg = zac + staticMargin * wingChord;
		aircraftCGPosition = new Vector3 (0, 0, zcg);
		aircraftACPosition = new Vector3 (0, 0, zac);

		//JOAO - General fuselage properties
		float noseLength = 2 * wingChord; // distance nose of fuselage ahead of wing ac.
		float fuselageLength = Mathf.Abs(zTail) + noseLength;
		float fuselageDiameter = 0.2f * wingChord;

		// work out tail setting angle eeta for trim
		float alphaTrim = 10;// degrees
		float eetat = alphaTrim * (aWing * wingArea * (-zcg - zacw) / (aTail * tailArea * (-zTail + zcg)) + 1);
		//print (eetat);

		// create size vectors for objects
		Vector3 wingSize = new Vector3 (wingSpan , wingChord / 10, wingChord);
		Vector3 tailSize = new Vector3 (tailSpan , tailChord / 10, tailChord);
		Vector3 fuselageSize = new Vector3 (fuselageDiameter, fuselageLength, fuselageDiameter);

		// create transforms for wing, tail and fuselage
		float zFuselage = zTail + fuselageLength / 2;
		wingPosition = new Vector3 (0, 0, 0);
		tailPosition = new Vector3 (0, 0, zTail );
		fuselagePosition = new Vector3 (0,0,zFuselage);

		// calculate masses
		//float structureDensity = 30f; // 30Kg/m^3. Blue foam has value of around 30.
		float massWing = 0.2f * allUpMass; // guess for now. structureDensity * wingArea * 0.1f * wingChord; // assume wing is 10%c thick
		float massTail = 0.05f * allUpMass; //structureDensity * tailArea * 0.1f * tailChord; // 
		float massFuselage = allUpMass - massWing - massTail;

		//work out ballast position to get cg correct
		float massfuselage = allUpMass - massTail - massWing;
		float zcgFuselage = (allUpMass*zcg-zTail*massTail)/massFuselage;

		fuselageCGPosition = new Vector3 (0, 0, zcgFuselage);

		//Left Wing
		Rigidbody leftWingRB = leftWingPhysical.GetComponent<Rigidbody>();
		BoxCollider leftWingBoxCollider = leftWingPhysical.GetComponent<BoxCollider>();

		leftWingBoxCollider.size = new Vector3 (wingSize.x/2.0f, wingSize.y, wingSize.z);
		leftWingBoxCollider.center = new Vector3 (-0.67f, 0, 0);

		leftWingRB.mass = massWing/2.0f; 
		leftWingRB.centerOfMass = new Vector3 (0, 0, 0); //this should probably be new Vector3 (wingSpan/4.0f, 0, 0) 

        leftWingPhysical.transform.position=new Vector3 (0, 0, 0);
		leftWingPhysical.transform.rotation = Quaternion.Euler(new Vector3 (0.0f, 0.0f, -wingDihedral));

		leftWingGraphic.transform.position = new Vector3 (-wingSpan/4.0f, 0, 0);
		leftWingGraphic.transform.localScale = new Vector3 (wingSize.x/2.0f, wingSize.y, wingSize.z);
		leftWingGraphic.transform.rotation = Quaternion.Euler(new Vector3 (0.0f, 0.0f, -wingDihedral));

        //Left Wing Aileron
        Rigidbody leftWingAileronRB = leftWingAileronPhysical.GetComponent<Rigidbody>();
        BoxCollider leftWingAileronBoxCollider = leftWingAileronPhysical.GetComponent<BoxCollider>();

        leftWingAileronBoxCollider.size = new Vector3(wingSize.x / 2.0f, wingSize.y, wingSize.z / 4);
        leftWingAileronBoxCollider.center = new Vector3(-0.67f, 0, -wingSize.z * 5 / 8);

        leftWingAileronRB.mass = 0; //this is zero as its presence is purley graphical.
        leftWingAileronRB.centerOfMass = new Vector3(0, 0, 0); 

        leftWingAileronPhysical.transform.position = new Vector3(0, 0, 0);
        leftWingAileronPhysical.transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, -wingDihedral));

        leftWingAileronGraphic.transform.position = new Vector3(-wingSpan / 4.0f, 0, -wingSize.z * 5 / 8);
        leftWingAileronGraphic.transform.localScale = new Vector3(wingSize.x / 2.0f, wingSize.y, wingSize.z / 4);
        leftWingAileronGraphic.transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, -wingDihedral));

        //Right Wing
        Rigidbody rightWingRB = rightWingPhysical.GetComponent<Rigidbody>();
		BoxCollider rightWingBoxCollider = rightWingPhysical.GetComponent<BoxCollider>();

		rightWingBoxCollider.size = new Vector3 (wingSize.x/2.0f, wingSize.y, wingSize.z);
		rightWingBoxCollider.center = new Vector3 (0.67f, 0, 0);

		rightWingRB.mass = massWing/2.0f;
        rightWingRB.centerOfMass = new Vector3(0, 0, 0); //this should probably be new Vector3 (wingSpan/4.0f, 0, 0) 

        rightWingPhysical.transform.position=new Vector3 (0, 0, 0);
		rightWingPhysical.transform.rotation = Quaternion.Euler(new Vector3 (0.0f, 0.0f, wingDihedral));

		rightWingGraphic.transform.position = new Vector3 (wingSpan/4.0f, 0, 0);
		rightWingGraphic.transform.localScale = new Vector3 (wingSize.x/2.0f, wingSize.y, wingSize.z);
		rightWingGraphic.transform.rotation = Quaternion.Euler(new Vector3 (0.0f, 0.0f, wingDihedral));

        //Right Wing Aileron
        Rigidbody rightWingAileronRB = rightWingAileronPhysical.GetComponent<Rigidbody>();
        BoxCollider rightWingAileronBoxCollider = rightWingAileronPhysical.GetComponent<BoxCollider>();

        rightWingAileronBoxCollider.size = new Vector3(wingSize.x / 2.0f, wingSize.y, wingSize.z / 4);
        rightWingAileronBoxCollider.center = new Vector3(0.67f, 0, -wingSize.z * 5 / 8);

        rightWingAileronRB.mass = 0; //this is zero as its presence is purley graphical.
        rightWingAileronRB.centerOfMass = new Vector3(0, 0, 0);

        rightWingAileronPhysical.transform.position = new Vector3(0, 0, 0);
        rightWingAileronPhysical.transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, wingDihedral));

        rightWingAileronGraphic.transform.position = new Vector3(wingSpan / 4.0f, 0, -wingSize.z * 5 / 8);
        rightWingAileronGraphic.transform.localScale = new Vector3(wingSize.x / 2.0f, wingSize.y, wingSize.z / 4);
        rightWingAileronGraphic.transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, wingDihedral));

        //Debug.Break();
        //Tail
        Rigidbody TailRB = TailPhysical.GetComponent<Rigidbody>();
		BoxCollider TailBoxCollider = TailPhysical.GetComponent<BoxCollider>();

		TailBoxCollider.size = tailSize;
		TailBoxCollider.center = new Vector3 (0, 0, 0);

		TailRB.mass = massTail;
		TailRB.centerOfMass = new Vector3 (0, 0, 0);

		TailPhysical.transform.position=tailPosition;
		TailPhysical.transform.rotation = Quaternion.Euler (0.5f, 0, 0);

		TailGraphic.transform.position = tailPosition;
		TailGraphic.transform.localScale = tailSize;

        //Tail Aileron
        Rigidbody TailAileronRB = TailAileronPhysical.GetComponent<Rigidbody>();
        BoxCollider TailAileronBoxCollider = TailAileronPhysical.GetComponent<BoxCollider>();

        TailAileronBoxCollider.size = new Vector3(tailSize.x, tailSize.y, tailSize.z / 4); //tailSize;
        TailAileronBoxCollider.center = new Vector3(0, 0, -tailSize.z * 5 / 8);

        TailAileronRB.mass = 0; //this is zero as its presence is purley graphical.
        TailAileronRB.centerOfMass = new Vector3(0, 0, 0);

        TailAileronPhysical.transform.position = tailPosition;
        TailAileronPhysical.transform.rotation = Quaternion.Euler(0.5f, 0, 0);

        TailAileronGraphic.transform.position = new Vector3(0, 0, zTail - tailSize.z * 5 / 8); //tailPosition;
        TailAileronGraphic.transform.localScale = new Vector3(tailSize.x, tailSize.y, tailSize.z / 4);

        //Tail Vertical
        Rigidbody TailVerticalRB = TailVerticalPhysical.GetComponent<Rigidbody>();
        BoxCollider TailVerticalBoxCollider = TailVerticalPhysical.GetComponent<BoxCollider>();

        TailVerticalBoxCollider.size = new Vector3(tailChord / 10, tailSpan / 4, tailChord);//tailSize;
        TailVerticalBoxCollider.center = new Vector3(0, tailSpan / 4, 0);

        TailVerticalRB.mass = massTail / 2;
        TailVerticalRB.centerOfMass = new Vector3(0, 0, 0); //this might need to be raised to new Vector3(0, tailSpan / 4, 0) as there is no counterweight like the wings or tail

        TailVerticalPhysical.transform.position = tailPosition;
        TailVerticalPhysical.transform.rotation = Quaternion.Euler(0, 0, 0);

        TailVerticalGraphic.transform.position = new Vector3(0, tailSpan / 8, zTail); //tailPosition;
        TailVerticalGraphic.transform.localScale = new Vector3(tailChord / 10, tailSpan / 4, tailChord); //tailSize;
        
        //Tail Vertical Aileron
        Rigidbody TailVerticalAileronRB = TailVerticalAileronPhysical.GetComponent<Rigidbody>();
        BoxCollider TailVerticalAileronBoxCollider = TailVerticalAileronPhysical.GetComponent<BoxCollider>();

        TailVerticalAileronBoxCollider.size = new Vector3(tailChord / 10, tailSpan / 4, tailChord / 4);//tailSize;
        TailVerticalAileronBoxCollider.center = new Vector3(0, tailSpan / 4, - tailChord * 5 / 8);

        TailVerticalAileronRB.mass = 0; //this is zero as its presence is purley graphical.
        TailVerticalAileronRB.centerOfMass = new Vector3(0, 0, 0);

        TailVerticalAileronPhysical.transform.position = tailPosition;
        TailVerticalAileronPhysical.transform.rotation = Quaternion.Euler(0, 0, 0);

        TailVerticalAileronGraphic.transform.position = new Vector3(0, tailSpan / 8, zTail - tailChord * 5 / 8); //tailPosition;
        TailVerticalAileronGraphic.transform.localScale = new Vector3(tailChord / 10, tailSpan / 4, tailChord / 4); //tailSize;
        

        //Fuselage
        Rigidbody FuselageRB = FuselagePhysical.GetComponent<Rigidbody>();
		CapsuleCollider FuselageCapsuleCollider = FuselagePhysical.GetComponent<CapsuleCollider>();

		FuselageCapsuleCollider.radius = fuselageDiameter/2;
		FuselageCapsuleCollider.height = fuselageLength;
		FuselageCapsuleCollider.center = new Vector3 (0, 0, 0);

		FuselageRB.mass = massFuselage;
		FuselageRB.centerOfMass = fuselageCGPosition;

		FuselagePhysical.transform.position=fuselagePosition;
		FuselageGraphic.transform.position = fuselagePosition;
		FuselageGraphic.transform.localScale = fuselageSize/2;

		//CG
		CGObject.transform.localPosition = aircraftCGPosition;
		float CGScale = 4*wingSize.y;
		CGObject.transform.localScale = new Vector3(CGScale,CGScale,CGScale);
	
		//AC
		ACObject.transform.localPosition = aircraftACPosition;
		float ACScale = 4*wingSize.y;
		ACObject.transform.localScale = new Vector3(ACScale,ACScale,ACScale);

		//Left Wing Joint
		ConfigurableJoint leftWingConfigurableJoint = leftWingPhysical.AddComponent<ConfigurableJoint>();
		leftWingConfigurableJoint.anchor = Vector3.zero;

		leftWingConfigurableJoint.xMotion = ConfigurableJointMotion.Locked;
		leftWingConfigurableJoint.yMotion = ConfigurableJointMotion.Locked;
		leftWingConfigurableJoint.zMotion = ConfigurableJointMotion.Locked;

		leftWingConfigurableJoint.angularXMotion=ConfigurableJointMotion.Locked;
		leftWingConfigurableJoint.angularYMotion=ConfigurableJointMotion.Locked;
		leftWingConfigurableJoint.angularZMotion=ConfigurableJointMotion.Locked;

		leftWingConfigurableJoint.connectedBody = FuselageRB;
		leftWingConfigurableJoint.connectedAnchor = wingPosition;
		leftWingConfigurableJoint.autoConfigureConnectedAnchor = false;

        //Left Wing Aileron Joint
        ConfigurableJoint leftWingAileronConfigurableJoint = leftWingAileronPhysical.AddComponent<ConfigurableJoint>();
        leftWingAileronConfigurableJoint.anchor = Vector3.zero;

        leftWingAileronConfigurableJoint.xMotion = ConfigurableJointMotion.Locked;
        leftWingAileronConfigurableJoint.yMotion = ConfigurableJointMotion.Locked;
        leftWingAileronConfigurableJoint.zMotion = ConfigurableJointMotion.Locked;

        leftWingAileronConfigurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
        leftWingAileronConfigurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
        leftWingAileronConfigurableJoint.angularZMotion = ConfigurableJointMotion.Locked;

        leftWingAileronConfigurableJoint.connectedBody = leftWingRB;
        leftWingAileronConfigurableJoint.connectedAnchor = wingPosition;
        leftWingAileronConfigurableJoint.autoConfigureConnectedAnchor = false;

        //Right Wing Joint
        ConfigurableJoint rightWingConfigurableJoint = rightWingPhysical.AddComponent<ConfigurableJoint>();
		rightWingConfigurableJoint.anchor = Vector3.zero;

        rightWingConfigurableJoint.xMotion = ConfigurableJointMotion.Locked;
        rightWingConfigurableJoint.yMotion = ConfigurableJointMotion.Locked;
        rightWingConfigurableJoint.zMotion = ConfigurableJointMotion.Locked;

        rightWingConfigurableJoint.angularXMotion=ConfigurableJointMotion.Locked;
        rightWingConfigurableJoint.angularYMotion=ConfigurableJointMotion.Locked;
        rightWingConfigurableJoint.angularZMotion=ConfigurableJointMotion.Locked;

        rightWingConfigurableJoint.connectedBody = FuselageRB;
        rightWingConfigurableJoint.connectedAnchor = wingPosition;
        rightWingConfigurableJoint.autoConfigureConnectedAnchor = false;

        //Right Wing Aileron Joint
        ConfigurableJoint rightWingAileronConfigurableJoint = rightWingAileronPhysical.AddComponent<ConfigurableJoint>();
        rightWingAileronConfigurableJoint.anchor = Vector3.zero; //new Vector3(0.67f, 0, -wingSize.z / 2); 

        rightWingAileronConfigurableJoint.xMotion = ConfigurableJointMotion.Locked;
        rightWingAileronConfigurableJoint.yMotion = ConfigurableJointMotion.Locked;
        rightWingAileronConfigurableJoint.zMotion = ConfigurableJointMotion.Locked;

        rightWingAileronConfigurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
        rightWingAileronConfigurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
        rightWingAileronConfigurableJoint.angularZMotion = ConfigurableJointMotion.Locked;

        rightWingAileronConfigurableJoint.connectedBody = rightWingRB; //if this is connected to the fuselage then it dosen't work correctly
        rightWingAileronConfigurableJoint.connectedAnchor = wingPosition; //new Vector3(0.67f, 0, wingSize.z / 8f);
        rightWingAileronConfigurableJoint.autoConfigureConnectedAnchor = false;

        //Tail Joint
        ConfigurableJoint tailConfigurableJoint = TailPhysical.AddComponent<ConfigurableJoint> ();
		tailConfigurableJoint.anchor = Vector3.zero;

		tailConfigurableJoint.xMotion = ConfigurableJointMotion.Locked;
		tailConfigurableJoint.yMotion = ConfigurableJointMotion.Locked;
		tailConfigurableJoint.zMotion = ConfigurableJointMotion.Locked;
		
		tailConfigurableJoint.angularXMotion=ConfigurableJointMotion.Locked;
		tailConfigurableJoint.angularYMotion=ConfigurableJointMotion.Locked;
		tailConfigurableJoint.angularZMotion=ConfigurableJointMotion.Locked;
		
		tailConfigurableJoint.connectedBody = FuselageRB;
		tailConfigurableJoint.connectedAnchor = tailPosition;
		tailConfigurableJoint.autoConfigureConnectedAnchor = false;

        //Tail Aileron Joint
        ConfigurableJoint tailAileronConfigurableJoint = TailAileronPhysical.AddComponent<ConfigurableJoint>();
        tailAileronConfigurableJoint.anchor = Vector3.zero;

        tailAileronConfigurableJoint.xMotion = ConfigurableJointMotion.Locked;
        tailAileronConfigurableJoint.yMotion = ConfigurableJointMotion.Locked;
        tailAileronConfigurableJoint.zMotion = ConfigurableJointMotion.Locked;

        tailAileronConfigurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
        tailAileronConfigurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
        tailAileronConfigurableJoint.angularZMotion = ConfigurableJointMotion.Locked;

        tailAileronConfigurableJoint.connectedBody = FuselageRB;
        tailAileronConfigurableJoint.connectedAnchor = tailPosition;
        tailAileronConfigurableJoint.autoConfigureConnectedAnchor = false;

        //Tail Vertical Joint
        ConfigurableJoint tailVerticalConfigurableJoint = TailVerticalPhysical.AddComponent<ConfigurableJoint>();
        tailVerticalConfigurableJoint.anchor = Vector3.zero;

        tailVerticalConfigurableJoint.xMotion = ConfigurableJointMotion.Locked;
        tailVerticalConfigurableJoint.yMotion = ConfigurableJointMotion.Locked;
        tailVerticalConfigurableJoint.zMotion = ConfigurableJointMotion.Locked;

        tailVerticalConfigurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
        tailVerticalConfigurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
        tailVerticalConfigurableJoint.angularZMotion = ConfigurableJointMotion.Locked;

        tailVerticalConfigurableJoint.connectedBody = FuselageRB;
        tailVerticalConfigurableJoint.connectedAnchor = tailPosition;
        tailVerticalConfigurableJoint.autoConfigureConnectedAnchor = false;
        
        //Tail Vertical Aileron Joint
        ConfigurableJoint tailVerticalAileronConfigurableJoint = TailVerticalAileronPhysical.AddComponent<ConfigurableJoint>();
        tailVerticalAileronConfigurableJoint.anchor = Vector3.zero;

        tailVerticalAileronConfigurableJoint.xMotion = ConfigurableJointMotion.Locked;
        tailVerticalAileronConfigurableJoint.yMotion = ConfigurableJointMotion.Locked;
        tailVerticalAileronConfigurableJoint.zMotion = ConfigurableJointMotion.Locked;

        tailVerticalAileronConfigurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
        tailVerticalAileronConfigurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
        tailVerticalAileronConfigurableJoint.angularZMotion = ConfigurableJointMotion.Locked;

        tailVerticalAileronConfigurableJoint.connectedBody = TailVerticalRB;
        tailVerticalAileronConfigurableJoint.connectedAnchor = tailPosition;
        tailVerticalAileronConfigurableJoint.autoConfigureConnectedAnchor = false;

        //Time.timeScale = 1;

        //set initial orientation of plane
        GameObject Aircraft = GameObject.Find("Aircraft");
		Vector3 initialVelocity = startingVelocity; // velocity in earth axes

		FuselageRB.velocity = initialVelocity;
		leftWingRB.velocity = initialVelocity;
		rightWingRB.velocity = initialVelocity;
		TailRB.velocity = initialVelocity;
        TailVerticalRB.velocity = initialVelocity;
    }


}
