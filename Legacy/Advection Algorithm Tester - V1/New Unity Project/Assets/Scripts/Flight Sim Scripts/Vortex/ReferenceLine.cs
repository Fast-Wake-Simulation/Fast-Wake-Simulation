using UnityEngine;
using System.Collections;

public class ReferenceLine : MonoBehaviour {


	public Vector3 lineStartP;
	public Vector3 lineEndP;

	private GameObject lineStart;
	private GameObject lineEnd;

	// Use this for initialization
	void Start () 
	{

		//Get the current size of the cylinder NOT NEEDED FOR NOW
		//cylinderSize = transform.localScale.y; 

		//Place marker points at a convinient distance from its centre
		lineStart = new GameObject();
		lineStart.transform.parent = gameObject.transform;
		lineStart.transform.localPosition = new Vector3(0.0f, -1.0f, 0.0f);

		lineEnd = new GameObject();
		lineEnd.transform.parent = gameObject.transform;
		lineEnd.transform.localPosition= new Vector3(0.0f, 1.0f, 0.0f);

		//Apply this points to a line renderer for debug reasons
		LineRenderer LR = GetComponent<LineRenderer>();
		if(LR != null)
		{
		Vector3[] LRPoints = new Vector3[] {lineStart.transform.position,lineEnd.transform.position};
		LR.SetPositions(LRPoints);
		}
		//Export Variables

	}
	
	// Update is called once per frame
	void Update () {
		lineStartP = lineStart.transform.position;
		lineEndP = lineEnd.transform.position;
	
	}
}
