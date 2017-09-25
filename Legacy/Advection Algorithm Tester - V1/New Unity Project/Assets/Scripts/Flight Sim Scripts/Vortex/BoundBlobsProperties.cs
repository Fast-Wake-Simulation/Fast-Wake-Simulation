//################################################################################
//# Script that stores the vortex strength and actuation direction of the vortex 
//# blobs present in the wings.
//# Both properties are updated and exported to/from other scripts such as: 
//# "VortexBlobBehaviour" and "BoundVortexBlobs"
//# 
//# Joao Vieira, 2016

using UnityEngine;
using System.Collections;

public class BoundBlobsProperties : MonoBehaviour {

	public float VortexStrength;
	public Vector3 actuationDir;
}
