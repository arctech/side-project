using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* Implementation follows: http://www.gamasutra.com/view/news/237528/Water_interaction_model_for_boats_in_video_games.php
 */
public class BoatController : MonoBehaviour {

	class BVertex {
		public Vector3 Position = Vector3.zero;
		public float Depth = 0.0f;

		public BVertex(Vector3 pos) {
			Position = pos;
		}

		public BVertex(Vector3 pos, float depth) {
			Position = pos;
			Depth = depth;
		}
	}

	class SubmergedTriangle {
		public BVertex A;
		public BVertex B;
		public BVertex C;

		private float Area;

		public BVertex Center;
		public Vector3 ForceCenter;
		public Vector3 Normal;

		public Vector3 Force;

		public Vector3 _normal;
	
//		public float _depth = 1.0f;
		public float _area;

		public SubmergedTriangle(BVertex A, BVertex B, BVertex C, Vector3 normal) {
			this.A = A;
			this.B = B;
			this.C = C;

			Center = new BVertex(1.0f / 3.0f *  (A.Position + B.Position + C.Position));
			ForceCenter = Center.Position;
			_normal = normal;

			 Vector3 ab = B.Position - A.Position; 
       		 Vector3 ac = C.Position - A.Position;
			 float len = ab.magnitude * ac.magnitude;
        	_area = (len * Mathf.Sin(Vector3.Angle(ab, ac) * Mathf.Deg2Rad)) / 2f;
		}
	}

	class WaterlinePair {
		public Vector3 p1;
		public Vector3 p2;

		public WaterlinePair(Vector3 v1, Vector3 v2) {
			p1 = v1;
			p2 = v2;
		}
	}

	class MeshTriangle {
		
		public Triangle triangle;
		public int triangleId;
		public Vector3 normal;

		public float[] vertexDistances = new float[3];
		public Vector3[] worldspacePositions = new Vector3[3];
		public MeshTriangle(Triangle triangle, Vector3 normal) {
			this.triangle = triangle;
			this.normal = normal;
		}
	}

	[System.Serializable]
	public class SimulationSettings {
		public bool ApplyForce = false;

		public bool UseDragForce = true;

		public bool UseSlamForce = true;

		public bool UseVerticalForceOnly = true;
	
		public float slamForceMultiplier = 5000.0f;

		public float Rho = 1027;

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

		public bool ShowTotalForce = false;

		public bool ShowVertexHeight = true;

		public bool ShowTriangles = true;
	}

	public SimulationSettings SimSettings = new SimulationSettings();

	public DebugSettings DebugSet = new DebugSettings();

	public float WaterpatchScaleFactor = 1.5f;
	private List<MeshTriangle> _meshTriangleList = new List<MeshTriangle>();

	private Vector3 _boatMeshCenter = Vector3.zero; // boat mesh bounding box center

	private GameObject _sentinelSphere = null;
	private Mesh _boatMesh = null;

	private string _debugMsg = "";

	private WaterPatch _waterPatch = new WaterPatch();

	public OceanManager _oceanManager;

	private List<WaterlinePair> _waterLinePoints = new List<WaterlinePair>();
	private List<SubmergedTriangle> _submergedTriangleList = new List<SubmergedTriangle>();

	private Rigidbody _rigidBody;

	private Vector3 _commonCenterOfApplication = Vector3.zero;

	private Vector3 _totalForceVector = Vector3.zero;

	private int[] _sortedTriangleArray = new int[3];

	private float _totalSubmergedArea = 0.0f;

	private float _forceFactor = 1.0f;	// - rho * gravity

	private List<Vector3> _waterGrid = new List<Vector3>();

	// Use this for initialization
	void Start () {
		Application.targetFrameRate = 30;

		_rigidBody = this.GetComponent<Rigidbody>();
	
		Mesh boatMesh = this.GetComponent<MeshFilter>().mesh;
		_boatMesh = this.GetComponent<MeshFilter>().mesh;

		// length of diagonal of boat mesh bounding box
		float boatMeshDiagLength = (_boatMesh.bounds.max - _boatMesh.bounds.min).magnitude;

		_sentinelSphere =  GameObject.Find("SentinelSphere");

		int triangleId = 1;
		for (int i = 0; i < boatMesh.triangles.Length; i += 3)
		{
    		Vector3 p1 = boatMesh.vertices[boatMesh.triangles[i + 0]];
    		Vector3 p2 = boatMesh.vertices[boatMesh.triangles[i + 1]];
    		Vector3 p3 = boatMesh.vertices[boatMesh.triangles[i + 2]];
			_meshTriangleList.Add(new MeshTriangle(new Triangle(p1, p2, p3), calculateNormal(p1, p2, p3)));	
		}
		_boatMeshCenter = _boatMesh.bounds.center;

		_waterPatch.init(getCenterWorldPosition(), WaterpatchScaleFactor * boatMeshDiagLength, SimSettings.WaterpatchDimRows);
		_waterPatch.build();

		// force pre-factor
		_forceFactor = -(SimSettings.Rho * Physics.gravity.y);
	}
	
	// Update is called once per frame
	void Update () {
	/*	_waterGrid.Clear();
		_waterGrid.
		// calc grid points
		for(int i = 0; i < waterPatchDimX * waterPatchDimY; i++) {

		}
*/

		float start = Time.realtimeSinceStartup;
		calcBuoyancy();

		_debugMsg = "BoatController - update: " + (Time.realtimeSinceStartup - start);

		if( Input.GetKeyDown(KeyCode.D))
		{
	//		_waterPatch.incrementNumTiles();
		} else if (Input.GetKeyDown(KeyCode.A)){
	//		_waterPatch.decrementNumTiles();
		}
	
		/*if( Input.GetKeyDown(KeyCode.W))
		{
			_waterPatchCellWidth = Mathf.Min(_waterPatchCellWidth + waterPatchIncrement, 20.0f);	
			Debug.Log(_waterPatchCellWidth);
			buildWaterpatch();
		} else if(Input.GetKeyDown(KeyCode.S)) {
			Debug.Log(_waterPatchCellWidth);
			buildWaterpatch();
			_waterPatchCellWidth = Mathf.Max(_waterPatchCellWidth - waterPatchIncrement, 1.0f);	
		}*/
	}


	public void FixedUpdate() {
		_totalSubmergedArea = 0.0f;
		_totalForceVector = Vector3.zero;
		_commonCenterOfApplication = Vector3.zero;
		//foreach(SubmergedTriangle tri in _submergedTriangleList) {
		for(int i = 0; i < _submergedTriangleList.Count; i++) {	
			SubmergedTriangle tri = _submergedTriangleList[i];
			_totalSubmergedArea += tri._area;
			//Vector3 force = checkForceIsValid(_forceFactor * (tri._depth) * tri._area * tri._normal.normalized, "buoyancy force"); 
			
			if(tri._area < 0.000001f) {
				Debug.Log("Area too small!");
				continue;
			}
			Vector3 force = _forceFactor * (getDistanceToWaterpatch( tri.ForceCenter)) * tri._area * tri._normal.normalized; 
//			Debug.Log("F: " + force);

			tri.Force = force;
	//		Debug.Log("F: " + force + " tri: " + _submergedTriangleList[i].Force);
			// calc hydrostatic force
			if( SimSettings.ApplyForce) {
				if(SimSettings.UseVerticalForceOnly) { 
					force.x = 0.0f;
					force.z = 0.0f;
				}
				if( !Mathf.Approximately(force.sqrMagnitude, 0.0f) && isForceValid(force, "buoyancy force")) {
					_rigidBody.AddForceAtPosition(force, tri.ForceCenter);			
				}
			}
			
			_totalForceVector += force;
			_commonCenterOfApplication += tri.ForceCenter;
		}
		
		_commonCenterOfApplication /= _submergedTriangleList.Count;
	//	_totalForceVector /= _submergedTriangleList.Count;
	}

	private void calcBuoyancy() {
		_waterPatch.updateCenter(getCenterWorldPosition(), _oceanManager);
//		Debug.Log("OM1: " + _oceanManager);
		_waterLinePoints.Clear();
		_submergedTriangleList.Clear();
	
		Transform t = this.transform;
				
		for(int i = 0; i < _meshTriangleList.Count; i++)
	   	{
			MeshTriangle mt = _meshTriangleList[i];
			Vector3 v1_transformed = t.TransformPoint( mt.triangle.Vertex1);
			Vector3 v2_transformed = t.TransformPoint( mt.triangle.Vertex2);
			Vector3 v3_transformed = t.TransformPoint( mt.triangle.Vertex3);
			
			float distanceToWater1 = getDistanceToWaterpatch(v1_transformed);
			float distanceToWater2 = getDistanceToWaterpatch(v2_transformed);
			float distanceToWater3 = getDistanceToWaterpatch(v3_transformed);

			int count = 0;
			if( distanceToWater1 <= 0 )
			{
				count += 1;
			}

			if( distanceToWater2 <= 0 )
			{
				count += 1;
			}

			if( distanceToWater3 <= 0 )
			{
				count += 1;
			}

			mt.vertexDistances[0] = distanceToWater1;
			mt.vertexDistances[1]  = distanceToWater2;
			mt.vertexDistances[2]  = distanceToWater3;

			mt.worldspacePositions[0] = v1_transformed;
			mt.worldspacePositions[1] = v2_transformed;
			mt.worldspacePositions[2] = v3_transformed;
		
			Vector3 normal = (t.TransformPoint((mt.triangle.Centroid + 0.1f * mt.normal))  - t.TransformPoint( mt.triangle.Centroid )).normalized;
			switch( count)
			{
				// totally submerged triangles
				case 3:
					_submergedTriangleList.Add(new SubmergedTriangle(
						new BVertex(v1_transformed, getDistanceToWaterpatch(v1_transformed)), 
						new BVertex(v2_transformed, getDistanceToWaterpatch(v2_transformed)), 
						new BVertex(v3_transformed, getDistanceToWaterpatch(v3_transformed)), 
						normal));
					break;
				// partially submerged triangles - need to be cut
				case 2:
				case 1:
					calcWaterLine(v1_transformed, v2_transformed, v3_transformed, normal);
					break;
				case 0:
				default:
					break;
			}
		}
	}

	private void calcWaterLine(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 origNormal) {
		BVertex H = new BVertex(v1);
		BVertex M = new BVertex(v2);
		BVertex L = new BVertex(v3);
		BVertex temp = new BVertex(new Vector3());

		float hH = getDistanceToWaterpatch(H.Position);
		float hM = getDistanceToWaterpatch(M.Position);
		float hL = getDistanceToWaterpatch(L.Position);

		H.Depth = hH;
		M.Depth = hM;
		L.Depth = hL;

		if(M.Depth > H.Depth)
		{
			temp = H;
			H = M;
			M = temp;
		}
		if (L.Depth > H.Depth ){
			temp = H;
			H = L;
			L = temp;
		}
		if(L.Depth > M.Depth) 
		{
			temp = M;
			M = L;
			L = temp;
		}

		if(L.Depth > M.Depth) {
			Debug.Log("hL > hM!");
		}
		if(M.Depth > H.Depth) {
			Debug.Log("hM > hH!");
		}
	
		// no vertices under water
		if(L.Depth > 0) return;

		// case 1:
		if( L.Depth <= 0 && M.Depth <= 0 && H.Depth >= 0) 
		//if( L.Depth < 0 && H.Depth > 0) 
		{
			float tm = -M.Depth / (H.Depth - M.Depth);
			float tl = -L.Depth / (H.Depth - L.Depth);

			Vector3 MH = H.Position - M.Position;
			Vector3 LH = H.Position - L.Position;

			Vector3 IM = M.Position + tm * MH;
			Vector3 IL = L.Position + tl * LH;


			//Triangle t1 = new Triangle(IM,M,L);
			Vector3 normal1 = calculateNormal(IM, M.Position, L.Position);
			if(! (origNormal == normal1))
			{
				normal1 *= -1;
			}

			cutTriangle(IM, M.Position, L.Position, normal1);
			
			_submergedTriangleList.Add(new SubmergedTriangle(new BVertex(IM, getDistanceToWaterpatch(IM)), 
				new BVertex(M.Position, getDistanceToWaterpatch(M.Position)), new BVertex(L.Position, getDistanceToWaterpatch(L.Position)), normal1));
			
			Vector3 normal2 = calculateNormal(IL, IM, L.Position);
			if(! (origNormal == normal2))
			{
				normal2 *= -1;
			}
			
			cutTriangle(IL, IM, L.Position, normal2);
			_submergedTriangleList.Add(new SubmergedTriangle(new BVertex(IL,getDistanceToWaterpatch(IL) ), 
				new BVertex(IM,getDistanceToWaterpatch(IM) ), new BVertex(L.Position,getDistanceToWaterpatch(L.Position) ), normal2));

			_waterLinePoints.Add(new WaterlinePair(IM, IL));
		}
		// case 2:
		if( L.Depth < 0 &&  M.Depth > 0 && H.Depth >= 0) {
		//if( L.Depth < 0 &&  M.Depth > 0) {
			float tm = -L.Depth / (M.Depth - L.Depth);
			float tl = -L.Depth / (H.Depth - L.Depth);

			Vector3 LM = M.Position - L.Position;
			Vector3 LH = H.Position - L.Position;

			Vector3 IM = L.Position + tm * LM;
			Vector3 IH = L.Position + tl * LH;

			Vector3 normal3 = calculateNormal(IM, IH, L.Position);
			if(! (origNormal == normal3))
			{
				normal3 *= -1;
			}

			cutTriangle(IH, IM, L.Position, normal3);
		
			_submergedTriangleList.Add(new SubmergedTriangle(new BVertex(IM,getDistanceToWaterpatch(IM) ), 
				new BVertex(IH,getDistanceToWaterpatch(IH) ), new BVertex(L.Position,getDistanceToWaterpatch(L.Position) ), normal3));
		
			_waterLinePoints.Add(new WaterlinePair(IM, IH));
		}

	}

	private void cutTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 normal) {
		// do the sort dance again
		BVertex H = new BVertex(v1);
		BVertex M = new BVertex(v2);
		BVertex L = new BVertex(v3);
		BVertex temp = new BVertex(new Vector3());

		float hH = getDistanceToWaterpatch(H.Position);
		float hM = getDistanceToWaterpatch(M.Position);
		float hL = getDistanceToWaterpatch(L.Position);

		H.Depth = hH;
		M.Depth = hM;
		L.Depth = hL;

		if(M.Depth > H.Depth)
		{
			temp = H;
			H = M;
			M = temp;
		}
		if (L.Depth > H.Depth ){
			temp = H;
			H = L;
			L = temp;
		}
		if(L.Depth > M.Depth) 
		{
			temp = M;
			M = L;
			L = temp;
		}

		if(L.Depth > M.Depth) {
			Debug.Log("hL > hM!");
		}
		if(M.Depth > H.Depth) {
			Debug.Log("hM > hH!");
		}



		//_submergedTriangleList.Add(new SubmergedTriangle(new BVertex(IM, getDistanceToWaterpatch(IM)), 
		//		new BVertex(M, getDistanceToWaterpatch(M)), new BVertex(L, getDistanceToWaterpatch(L)), normal1));
	}

	private float getDistanceToWaterpatch(Vector3 point) {
		float distance = Mathf.Infinity;
		Vector2 cellIndices = _waterPatch.getCellIndicesForPoint(point);
		Vector2 cellCoordinates = _waterPatch.getCellCoordinatesForPoint(point);
		if(( cellIndices.x >= 0 && cellIndices.x < _waterPatch.NumTiles - 1) && cellIndices.y >= 0 && cellIndices.y < _waterPatch.NumTiles - 1)
		{
			int i = (int)cellIndices.x;
			int j = (int)cellIndices.y;
			float xpc = cellCoordinates.x;
			float zpc = cellCoordinates.y;
		
			Vector3 A = _waterPatch.get(i, j);
			Vector3 B = _waterPatch.get(i, j + 1);
			Vector3 C = _waterPatch.get(i + 1, j + 1); 	
			Vector3 D = _waterPatch.get(i + 1, j);

			Vector3 B1 = B;
			Gizmos.color = Color.green;
			if( xpc > zpc) {
				B1 = D;
			}		

			// calculate height
			Vector3 AB = B1 - A;
			Vector3 AC = C - A;
			Vector3 cross = Vector3.Cross(AB, AC);
			float d = Vector3.Dot(cross, A);
			distance = point.y - ((d - cross.x * point.x - cross.z * point.z) / cross.y);
		}
		return distance;
	}

	void OnDrawGizmos() {
		if(! DebugSet.ShowDebug)
		{
			return;
		}

		Transform t = this.transform;

		if(DebugSet.ShowNormalsOriginalMesh) {
			foreach(MeshTriangle mt in _meshTriangleList) 
			{
				Gizmos.color = Color.green;
				Vector3 A = t.TransformPoint(mt.triangle.Centroid + 0.1f * mt.normal);
				Vector3 E = t.TransformPoint(mt.triangle.Centroid + 0.2f * mt.normal);
				Gizmos.DrawLine(A,E);
			}
		}

		foreach(SubmergedTriangle tri in _submergedTriangleList) {

			if(DebugSet.ShowSubmergedVolume) {
				Color col = new Color(0.0f, tri._area / _totalSubmergedArea, 0.0f, 1.0f);
				//Debug.Log( tri._area / _totalSubmergedArea);
				//GLUtil.RenderTriangle(tri._triangle, DrawingUtil.LimeGreen * tri._area / _totalSubmergedArea);
				GLUtil.RenderTriangle(tri.A.Position,tri.B.Position,tri.C.Position, col);
			}

			if(DebugSet.ShowTriangles) {
				DrawingUtil.DrawTriangle(tri.A.Position,tri.B.Position,tri.C.Position, Color.white);

				float offset = 0.01f;
				Gizmos.color = Color.blue;
				Gizmos.DrawSphere(tri.A.Position  - (tri.A.Position - tri.Center.Position).normalized * 3 * offset, offset);
				Gizmos.color = Color.green;
				Gizmos.DrawSphere(tri.B.Position  - (tri.B.Position - tri.Center.Position).normalized * 3 * offset, offset);
				Gizmos.color = Color.red;
				Gizmos.DrawSphere(tri.C.Position  - (tri.C.Position - tri.Center.Position).normalized * 3 * offset, offset);
			}

			if(DebugSet.ShowNormalsSubmerged) {
				Gizmos.color = Color.red;
				//Vector3 A = t.TransformPoint(tri._pointOfApplication);
				//Vector3 E = t.TransformPoint(tri._pointOfApplication + 0.1f * tri._normal);
				//Vector3 A = tri._pointOfApplication;
				//Vector3 E = tri._pointOfApplication + 0.1f * tri._normal;
				Gizmos.DrawLine(tri.Center.Position,tri.Center.Position + 0.1f * tri._normal);
				Gizmos.DrawSphere(tri.Center.Position, 0.005f);
			}

			if(DebugSet.ShowVertexHeight) {
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(tri.ForceCenter, new Vector3(tri.ForceCenter.x, tri.ForceCenter.y - getDistanceToWaterpatch(tri.ForceCenter), tri.ForceCenter.z ));

				Gizmos.color = Color.gray;
				Gizmos.DrawLine(tri.A.Position, new Vector3(tri.A.Position.x, tri.A.Position.y - tri.A.Depth, tri.A.Position.z ));
				Gizmos.DrawLine(tri.B.Position, new Vector3(tri.B.Position.x, tri.B.Position.y - tri.B.Depth, tri.B.Position.z ));
				Gizmos.DrawLine(tri.C.Position, new Vector3(tri.C.Position.x, tri.C.Position.y - tri.C.Depth, tri.C.Position.z ));
			}

			if(DebugSet.ShowForce) {
				Gizmos.color = Color.cyan;
				//Gizmos.DrawLine(tri.ForceCenter, tri.ForceCenter + (-1.0f *  tri.Force.magnitude / maxForce ) * tri.Force.normalized);
				Gizmos.DrawWireSphere(tri.ForceCenter, 0.01f);
				Gizmos.DrawLine(tri.ForceCenter, tri.ForceCenter + (-1.0f *  tri.Force.magnitude ) * tri.Force.normalized);
		//		Gizmos.DrawLine(tri.ForceCenter, tri.ForceCenter + (-1.0f *  tri._forceVector.magnitude / maxForce ) * tri._forceVector.normalized);
			//	maxForce = Mathf.Max(maxForce, tri._forceVector.magnitude);
				Debug.Log("F: " + tri.Force);
			}
		}
		
		
		if(DebugSet.ShowTotalForce) {
			Gizmos.color = DrawingUtil.Cyan;
			Gizmos.DrawSphere(_commonCenterOfApplication, 0.25f);
			//Debug.Log(_commonCenterOfApplication);
			Gizmos.DrawLine(_commonCenterOfApplication, _commonCenterOfApplication + 1.0f * _totalForceVector);
		}

		if(DebugSet.ShowWaterLine) {
			foreach(WaterlinePair pair in _waterLinePoints) {
				Gizmos.DrawLine(pair.p1, pair.p2);
			}
		}
		

		
		Vector3 center = getCenterWorldPosition();
	
	//	Gizmos.color = Color.red;
	//	Gizmos.DrawWireSphere(new Vector3(center.x,center.y + 1, center.z), 0.025f);

		if(DebugSet.ShowWaterPatch) {
			// draw waterPatch wire
			Gizmos.color = Color.blue;
			for( int i = 0; i < _waterPatch.NumTiles - 1; i++)
			{
				for( int j = 0; j < _waterPatch.NumTiles - 1; j++)
				{
					Vector3 A = _waterPatch.get(i, j);
					Vector3 B = _waterPatch.get(i + 1, j);
					Vector3 D = _waterPatch.get(i, j + 1);
					Vector3 C = _waterPatch.get(i + 1, j + 1);

					Gizmos.DrawLine(A,B);
					Gizmos.DrawLine(B,C);
					Gizmos.DrawLine(C,D);
					Gizmos.DrawLine(A,D);
					Gizmos.DrawLine(A,C);
				}
			}

			// test coordinates
			if( _sentinelSphere == null)
				return;	

			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(_waterPatch.zG + new Vector3(0,0,0.0f), 0.01f);
			Gizmos.DrawWireSphere(_sentinelSphere.transform.position + new Vector3(0,0.4f,0.0f), 0.01f);
	
			float distance = getDistanceToWaterpatch(_sentinelSphere.transform.position);	
			if(distance <= 0.0f)
			{
				Gizmos.DrawLine(_sentinelSphere.transform.position, _sentinelSphere.transform.position + new Vector3(0, (-1) * distance, 0));
			}
		}
	}

	private Vector3 getCenterWorldPosition() {
		return this.transform.TransformPoint(_boatMeshCenter);
	}

	void OnGUI() {
 //		GUI.Label (new Rect (0,0,100,50), _debugMsg);
	}

	private Vector3 calculateNormal(Triangle t) {
		return calculateNormal(t.Vertex1, t.Vertex2, t.Vertex3);
	}

	private Vector3 calculateNormal(Vector3 v1, Vector3 v2, Vector3 v3) {
		//Vector3 AB = 
		Vector3 AB = v2 - v1;
		Vector3 CB = v3 - v1;
		return Vector3.Cross(AB, CB).normalized;
	}

	 //Check that a force is not NaN
    private static Vector3 checkForceIsValid(Vector3 force, string forceName)
    {
        if (!float.IsNaN(force.x + force.y + force.z))
        {
            return force;
        }
        else
        {
            Debug.Log(forceName += " force is NaN");
            return Vector3.zero;
        }
    }


	private bool isForceValid(Vector3 force, string forceName) {
		 if (float.IsNaN(force.x + force.y + force.z))
        {
            return false;
        }
		return true;
	}
}
