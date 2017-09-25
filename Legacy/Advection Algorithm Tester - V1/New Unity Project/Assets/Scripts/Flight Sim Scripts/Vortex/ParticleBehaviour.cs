//################################################################################
//# Unused script that controlled particle behaviour in the presence of vortex 
//# filaments. Precursor to "VortexBlobBehaviour".


using UnityEngine;
using System.Collections;

public class ParticleBehaviour : MonoBehaviour {

	//Declaration of variables

	public Vector3 initialVelocity = new Vector3 (1.0f, 0.0f,0.0f);

	private VortexFilamentProperties FilamentPropertiesScript;
	private Rigidbody RB;
	private Vector3 filamentVelocityDifferential = new Vector3 (0.0f, 0.0f,0.0f);
	public Vector3 distanceV;
	private float velocityMagn;
	private GameObject[] vortexFilamentArray;
	void Start () 
	{
		RB = GetComponent<Rigidbody>();		
	}
	

	void FixedUpdate () {




		vortexFilamentArray = GameObject.FindGameObjectsWithTag("VortexFilament");

		filamentVelocityDifferential = new Vector3 (0.0f, 0.0f,0.0f);

		//Analyses the vortex filament
		foreach (GameObject vortexFilament in vortexFilamentArray)
		{
			ReferenceLine referenceLineScript = vortexFilament.GetComponent<ReferenceLine>();
			Vector3 lineStart = referenceLineScript.lineStartP;
			Vector3 lineEnd = referenceLineScript.lineEndP;
			Vector3 filamentDir = (lineEnd-lineStart).normalized;

			//Obtains distance to filament 
			distanceV = DistanceToLineVector2(lineStart,lineEnd,filamentDir);
			float distance = distanceV.magnitude;

			//Computes the magnitude and direction of the velocity diferential caused by the filament
			Vector3 velocityDir = Vector3.Cross(distanceV.normalized,filamentDir);
			FilamentPropertiesScript = vortexFilament.GetComponent<VortexFilamentProperties>(); 
			if (distance > 0.1f)
				 velocityMagn = FilamentPropertiesScript.vortexFilamentStrength/(2.0f*Mathf.PI*distance);
			else
				velocityMagn = 0.0f;
		

			//Checks if the probe is in a position that can be affected by the filament
			Vector3 pointOnLine = transform.position + distanceV;
			bool pointOnLineBool = CheckIfPointOnLine(lineStart,lineEnd,pointOnLine);

			if(pointOnLineBool)
				filamentVelocityDifferential += velocityMagn*velocityDir;
			
		}


	
		RB.velocity = initialVelocity + filamentVelocityDifferential;
		transform.rotation = Quaternion.LookRotation(RB.velocity);
		transform.rotation = transform.rotation * Quaternion.Euler(0,-90,-90);
		//transform.Rotate(0,90,90);


	}


	//Returns a vector starting at transform and that is perpendicular to a line defined by lineStart and lineEnd.
	Vector3 DistanceToLineVector2(Vector3 lineStart, Vector3 lineEnd,Vector3 filamentDir)
	{
		Vector3 subtraction1 = lineStart-transform.position;
		float computation1 = Vector3.Dot(subtraction1,filamentDir);
		Vector3 result = subtraction1 - computation1*filamentDir;
		return result;
	}

	bool CheckIfPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 pointOnLine)
	{
		float distanceFromStart = (pointOnLine - lineStart).magnitude;
		float distanceFromEnd = (pointOnLine - lineEnd).magnitude;
		float maxAllowedDistance = (lineEnd-lineStart).magnitude;
		if( (distanceFromStart+distanceFromEnd)*0.99f > maxAllowedDistance)
			return false;
		else
			return true;
	}
}
