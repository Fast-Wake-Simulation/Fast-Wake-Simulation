using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
// class to calculate free roll up of a static vortex sheet
//2/3/17
public class VortexMesh : MonoBehaviour {
	
	//mesh is based on mesh coordinate x y system from http://catlikecoding.com/unity/tutorials/procedural-grid/. READ THIS
	private int xSize; //number of x elements
	private int ySize; // number of y elements

	private Mesh mesh; // wake mesh component on parent game object
	private int meshVertexCount; // number of vertices in the mesh (total number of blob points)

	private Vector3[,] position;// xyz coords of points in wake grid as 2d array. Order is x=0, z= 0:zSize-1, x=1, z=0:zSize-1,..., x=xSize-1
	private Vector3[] vertices;// xyz coords stored as single vertex list
	private Vector3 [,] vorticityVector; // normalised vorticity vector of vorticity at each grid point
	private Vector2 [,] vorticityMagnitude; // magnitude of vorticity in x and y directions
	private float[] vorticity;// vorticity magnitude used for colouring spheres

	private Vector2[] uv;
	private Vector3 inducedVelocity; // temporary variable to accumulate induced velocity at each target point
	private Vector3 r; //vector between source and target points for induced vellocity calculation
	private Vector3 gamma; // vorticity vector
	public float vortexStrength { get; set; } // Strength of shed vorticity for horshoe test. {...} used to make accessible to GUI element
	public float maxdv {get; set;}// radius of vortex core. Radius for induced velocity is lower clamped at this value
	//public bool isRunning=true; // bool to set scripting running from another script

	// Use this for initialization
	void Start () {
		print ("initialise started");
		xSize = 10; // change these to change the size of the grid
		ySize = 10; //equiv to z
		meshVertexCount = (xSize + 1) * (ySize + 1);
		position = new Vector3[(xSize + 1), (ySize + 1)]; // create empty position array
		vertices = new Vector3[ meshVertexCount];
		vorticityVector = new Vector3 [(xSize + 1), (ySize + 1)];// create empty vorticity array
		vorticityMagnitude=new Vector2 [(xSize + 1), (ySize + 1)];// create empty vorticity array
		vorticity=new float[meshVertexCount];//for visualisation
		maxdv= 5f; //limits max induced velocity, to avoid infinite velocity when distance between vortons is zero

		// Set up initial grid positions and initial vorticity
		for ( int y = 0, i=0; y <=  ySize; y++)
		{
			for (int x= 0; x <= xSize; x++,i++)
			{
					position[x, y] = new Vector3(x, 0, y);
					vertices[i] = position[x, y]; // duplicate I know but makes it much easier
				//now set up the vortex ring as a circle of vorticity
				Vector2 coordinate=new Vector2(x-xSize/2,y-ySize/2);
				Vector2 direction=new Vector2(y-ySize/2,-(x-xSize/2));
				float radius = coordinate.magnitude;
				float magnitude = normalDistribution (radius); //normalDistribution method sets mean and standard deviation to control shape
				vorticityMagnitude[x,y]=5*magnitude*direction.normalized   ; // vorticity vector is always in the plane of the mesh so only 2 components are needed, hence vector 2
				vorticity [i] = (float) vorticityMagnitude [x, y].magnitude; // used for colouring gizmos balls - zero vorticity black, high red
			}

		}


		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Vortex Wake";
		print("mesh created");
		mesh.vertices = vertices;

		int[] triangles = new int[xSize * ySize * 6];
		for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
		{
			for (int x = 0; x < xSize; x++, ti += 6, vi++)
			{

				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
				triangles[ti + 5] = vi + xSize + 2;
			}
		}
		mesh.triangles = triangles;// .Concat(triangles.Reverse().ToArray()).ToArray(); //doublesided mesh
		//mesh.triangles = triangles;
		mesh.RecalculateNormals();

	}
	//http://statistics.about.com/od/Formulas/ss/The-Normal-Distribution-Or-Bell-Curve.htm
	//Method to spread out vortex ring filament across grid. Generates a normal distribution
	float normalDistribution(float radius){
		float sigma = xSize/10; //standard deviation
		float mean= xSize/3; // radius of  ring
		float value=(1/(sigma*Mathf.Sqrt(2*Mathf.PI)))*Mathf.Exp(-(Mathf.Pow((radius-mean),2)/(2*sigma*sigma)));
		return value;
	}
		
//
//	// Update is called once per frame
	void FixedUpdate()
	{
		//if (isRunning == false)
			//return; // jump out if the user has not clicked run
		
		for (int yTarget = 0, i=0; yTarget <= ySize; yTarget++) {
			for (int xTarget = 0; xTarget <= xSize; xTarget++,i++) {
				
				//loop through all the source points inset by one from the edges of the mesh (this makes calculating gradients easier)
					inducedVelocity = Vector3.zero; // reset the induced velocity for this target point to zero
					for (int xSource = 1; xSource < xSize; xSource++) {
						for (int ySource = 1; ySource < ySize; ySource++) {
							// if target and source are not the same point
							if (xTarget != xSource && yTarget != ySource) {

								// work out vorticity vector of source point

								// vorticity vectory in local grid x direction
								Vector3	gammax = vorticityMagnitude [xSource, ySource].x * (position [xSource + 1, ySource] - position [xSource-1, ySource]).normalized;
							// vorticity vectory in local grid y direction
							Vector3	gammay = vorticityMagnitude [xSource, ySource].y * (position [xSource, ySource+1] - position [xSource, ySource-1]).normalized;
							//Overall vorticity vector
							gamma = gammax + gammay;
									
								//Implement Biot Savart law								
								r =  position [xTarget, yTarget] - position [xSource, ySource];

								inducedVelocity = inducedVelocity + Vector3.ClampMagnitude ((1 / (4 * Mathf.PI)) * (Vector3.Cross (gamma, r) / Mathf.Pow (r.magnitude, 2)), maxdv);

							}

						}

					}

				// update position of target particle with accumulated velocity
					position [xTarget, yTarget] = position [xTarget, yTarget] + ( inducedVelocity  * Time.deltaTime);// convect grid points with free stream velocity
					vertices [i] = position [xTarget, yTarget];

			}

		}
		mesh.vertices = vertices;


	}
//
	private void OnDrawGizmos() // this shows control points as red spheres in the edit window
	{
		if (vertices == null)
		{
			return;
		}


		for (int i = 0; i < vertices.Length; i++)
		{
			Gizmos.color = Color.Lerp(Color.black, Color.red, vorticity[i]/2); // note that for proper colouring the maximum value of vorticity needs working out so that at max vorticity the lerp goes to 1
			Gizmos.DrawSphere(vertices[i], 0.1f);
		}
	}

}
