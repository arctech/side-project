using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuoyancyComponent : MonoBehaviour {

	class BVertex {
		public Vector3 Position = Vector3.zero;
		float Depth = 0.0f;
	}

	class BTriangle {
		public BVertex A;
		public BVertex B;
		public BVertex C;

		private float Area;

		BVertex Center;
		Vector3 ForceCenter;
		Vector3 Normal;

		Vector3 Force;

		public BTriangle () {}
	}

	class WaterlinePair {
		public Vector3 p1;
		public Vector3 p2;

		public WaterlinePair(Vector3 v1, Vector3 v2) {
			p1 = v1;
			p2 = v2;
		}
	}

	[System.Serializable]
	public class SimulationSettings {
		public bool ApplyForce = false;

		public bool UseDragForce = true;

		public bool UseSlamForce = true;

		public float slamForceMultiplier = 5000.0f;

		public float Rho = 1000;

		[Range(3,9)]
		public int WaterpatchDimRows = 5;

		[Range(3,9)]
		public int WaterpatchDimCols = 5;
	}

	[System.Serializable]
	public class DebugSettings {
		public bool ShowDebug = true;

		public bool ShowWaterLine = true;

		public bool ShowSubmergedVolume = true;

		public bool ShowNormalsSubmerged = true;

		public bool ShowForce = true;

		public bool ShowWaterPatch = true;

		public bool ShowNormalsOriginalMesh = true;
	}

	public SimulationSettings SimSettings = new SimulationSettings();

	public DebugSettings DebugSet = new DebugSettings();

	private List<BTriangle> _waterGrid = new List<BTriangle>();

	private Rigidbody _rigidBody;

	private List<WaterlinePair> _waterLinePoints = new List<WaterlinePair>();

	// Use this for initialization
	void Start () {
		_rigidBody = this.GetComponent<Rigidbody>();
	
		Mesh boatMesh = this.GetComponent<MeshFilter>().mesh;
		//_boatMesh = this.GetComponent<MeshFilter>().mesh;	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnDrawGizmos() {
		if(! DebugSet.ShowDebug)
		{
			return;
		}

		if(DebugSet.ShowWaterLine) {

		}

		if(DebugSet.ShowForce) {

		}

		/*if(DebugSet.ShowNormals) {

		}*/

		if(DebugSet.ShowSubmergedVolume) {

		}
	}
}
