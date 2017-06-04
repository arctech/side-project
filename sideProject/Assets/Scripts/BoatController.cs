using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* Implementation follows: http://www.gamasutra.com/view/news/237528/Water_interaction_model_for_boats_in_video_games.php
 */
public class BoatController : MonoBehaviour {

	class MeshTriangle {
		public Triangle triangle;
		public int triangleId;
		public Vector3 normal;

		// 0 vertices, 1 vertex, 2 vertices or all 3 vertices submerged;
		public int submergedState;	
		public float[] vertexDistances = new float[3];
		public Vector3[] worldspacePositions = new Vector3[3];
		public MeshTriangle(Triangle triangle, int triangleId, Vector3 normal) {
			this.triangle = triangle;
			this.triangleId = triangleId;
			this.submergedState = 0;
			this.normal = normal;
		}
	}

	public bool ShowDebug = true;

	public float WaterpatchScaleFactor = 1.5f;
	private List<MeshTriangle> _meshTriangleList = new List<MeshTriangle>();

	private Vector3 _boatMeshCenter = Vector3.zero; // boat mesh bounding box center

	private GameObject _sentinelSphere = null;
	private Mesh _boatMesh = null;

	private string _debugMsg = "";

	private WaterPatch _waterPatch = new WaterPatch();

	private OceanManager _oceanManager;

	private List<WaterlinePair> _waterLinePoints = new List<WaterlinePair>();
	private List<DebugInfo> _debugInfoList = new List<DebugInfo>();

	private List<SubmergedTriangle> _submergedTriangleList = new List<SubmergedTriangle>();

	private Rigidbody _rigidBody;
	public float Rho = 1000;

	class SubmergedTriangle {
		public Vector3 _normal;
		public Vector3 _pointOfApplication;
		public Triangle _triangle;

		public SubmergedTriangle(Vector3 pof, Vector3 normal, Triangle tri) {
			_pointOfApplication = pof;
			_normal = normal;
			_triangle = tri;
		}
	}

	class DebugInfo {
		public Vector3 _normal;
		public Vector3 _pointOfApplication;
		public Vector3 _force;

		public DebugInfo(Vector3 normal, Vector3 poa, Vector3 force)
		{
			this._normal = normal;
			this._pointOfApplication = poa;
			this._force = force;
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

	// Use this for initialization
	void Start () {
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
			_meshTriangleList.Add(new MeshTriangle(new Triangle(p1, p2, p3), triangleId++, calculateNormal(p1, p2, p3)));	
		}
		_boatMeshCenter = _boatMesh.bounds.center;

		_waterPatch.init(getCenterWorldPosition(), WaterpatchScaleFactor * boatMeshDiagLength, 5);
		_waterPatch.build();

		_oceanManager = gameObject.AddComponent<OceanManager>();
	}
	
	// Update is called once per frame
	void Update () {
		float start = Time.realtimeSinceStartup;

		_waterPatch.updateCenter(getCenterWorldPosition(), _oceanManager);

		
		_waterLinePoints.Clear();
		_debugInfoList.Clear();
		_submergedTriangleList.Clear();

		Transform t = this.transform;
				
	 	List<int> partiallySubmergedTriangles = new List<int>();
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
			mt.submergedState = count;
		
			//mt._transform = t;
			switch( count)
			{
				case 3:
					Vector3 normal = (t.TransformPoint((mt.triangle.Centroid + 0.1f * mt.normal))  - t.TransformPoint( mt.triangle.Centroid )).normalized;
					Triangle tri = new Triangle(v1_transformed, v2_transformed, v3_transformed);
					_submergedTriangleList.Add(new SubmergedTriangle(tri.Centroid, normal, tri));
					break;
				case 2:
				case 1:
					calcWaterLine(t, mt, _submergedTriangleList, _waterLinePoints);
					break;
				case 0:
				default:
					break;
			}	

			//_rigidBody.AddForceAtPosition(new Vector3(0, 1, 0), new Vector3(), ForceMode.Force);
		}

		_debugMsg = "BoatController - update: " + (Time.realtimeSinceStartup - start);

	/*	foreach( Triangle tri in _submergedTriangles)
		{
			_debugInfoList.Add(new DebugInfo(calculateNormal(tri), tri.Centroid, new Vector3()));

			if( _rigidBody != null) {
				// F  = -rho * g * h_center * normal;
			//	_rigidBody.AddForceAtPosition(new Vector3(0, 20, 0), tri.Centroid, ForceMode.Force);
			}
		}*/

		if( Input.GetKeyDown(KeyCode.D))
		{
			_waterPatch.incrementNumTiles();
		} else if (Input.GetKeyDown(KeyCode.A)){
			_waterPatch.decrementNumTiles();
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

	private void calcWaterLine(Transform t, MeshTriangle mt, List<SubmergedTriangle> submergedTriangleList, List<WaterlinePair> waterLinePoints ) {
		int[] sortedIndices = new int[3] {0,1,2};

		if (mt.vertexDistances[sortedIndices[0]] > mt.vertexDistances[sortedIndices[2]]) 
		{
			int temp = sortedIndices[0];
			sortedIndices[0] = sortedIndices[2];
			sortedIndices[2] = temp;
		}
		if(mt.vertexDistances[sortedIndices[0]] > mt.vertexDistances[sortedIndices[1]]) 
		{
			int temp = sortedIndices[0];
			sortedIndices[0] = sortedIndices[1];
			sortedIndices[1] = temp;
		}
		if(mt.vertexDistances[sortedIndices[1]] > mt.vertexDistances[sortedIndices[2]]) 
		{
			int temp = sortedIndices[1];
			sortedIndices[1] = sortedIndices[2];
			sortedIndices[2] = temp;
		}

		Vector3 L = mt.worldspacePositions[sortedIndices[0]];
		Vector3 M = mt.worldspacePositions[sortedIndices[1]];
		Vector3 H = mt.worldspacePositions[sortedIndices[2]];
		Vector3 normal = (t.TransformPoint((mt.triangle.Centroid + 0.1f * mt.normal))  - t.TransformPoint( mt.triangle.Centroid )).normalized;

		// case 1:
		if( (mt.vertexDistances[sortedIndices[0]] <= 0 && mt.vertexDistances[sortedIndices[1]] <= 0) && mt.vertexDistances[sortedIndices[2]] >= 0) 
		{
			float tm = -mt.vertexDistances[sortedIndices[1]] / (mt.vertexDistances[sortedIndices[2]] - mt.vertexDistances[sortedIndices[1]]);
			float tl = -mt.vertexDistances[sortedIndices[0]] / (mt.vertexDistances[sortedIndices[2]] - mt.vertexDistances[sortedIndices[0]]);

			Vector3 MH = H - M;
			Vector3 LH = H - L;

			Vector3 IM = M + tm * MH;
			Vector3 IL = L + tl * LH;


			Triangle t1 = new Triangle(IM,M,L);
			Vector3 normal1 = calculateNormal(IM, M, L);
			Vector3 orig = normal1;
			if(! (normal == normal1))
			{
				normal1 *= -1;
			}
				
			_submergedTriangleList.Add(new SubmergedTriangle(t1.Centroid, normal1, t1));
			Vector3 normal2 = calculateNormal(IL, IM, L);
			if(! (normal == normal2))
			{
				normal2 *= -1;
			}
			Triangle t2 = new Triangle(IL,IM,L);
			_submergedTriangleList.Add(new SubmergedTriangle(t2.Centroid, normal2, t2));

			waterLinePoints.Add(new WaterlinePair(IM, IL));
		}
		// case 2:
		if( (mt.vertexDistances[sortedIndices[0]] <= 0 &&  mt.vertexDistances[sortedIndices[1]] >= 0) && mt.vertexDistances[sortedIndices[2]] >= 0) {
			float tm = -mt.vertexDistances[sortedIndices[0]] / (mt.vertexDistances[sortedIndices[1]] - mt.vertexDistances[sortedIndices[0]]);
			float tl = -mt.vertexDistances[sortedIndices[0]] / (mt.vertexDistances[sortedIndices[2]] - mt.vertexDistances[sortedIndices[0]]);

			Vector3 LM = M - L;
			Vector3 LH = H - L;

			Vector3 IM = L + tm * LM;
			Vector3 IH = L + tl * LH;

			Triangle tri = new Triangle(IM,IH,L);
			Vector3 normal3 = calculateNormal(IM, M, L);
			if(! (normal == normal3))
			{
				normal3 *= -1;
			}
			_submergedTriangleList.Add(new SubmergedTriangle(tri.Centroid, normal3, tri));
		
			waterLinePoints.Add(new WaterlinePair(IM, IH));
		}
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
			if(Mathf.Approximately(cross.y, 0))
			{
				return 0;
			}
			distance = point.y - ((d - cross.x * point.x - cross.z * point.z) / cross.y);
		}
		return distance;
	}

	void OnDrawGizmos() {
		if(! ShowDebug)
		{
			return;
		}

		Transform t = this.transform;
	   	Matrix4x4 m = this.transform.localToWorldMatrix;

		foreach(MeshTriangle mt in _meshTriangleList) 
		{
			Gizmos.color = Color.green;
			Vector3 A = t.TransformPoint(mt.triangle.Centroid + 0.1f * mt.normal);
			Vector3 E = t.TransformPoint(mt.triangle.Centroid + 0.2f * mt.normal);
			Gizmos.DrawLine(A,E);
		}

		foreach(SubmergedTriangle tri in _submergedTriangleList) {
			GLUtil.RenderTriangle(tri._triangle, DrawingUtil.LimeGreen);
			DrawingUtil.DrawTriangle(tri._triangle, Color.black);
			
			Gizmos.color = Color.red;
			//Vector3 A = t.TransformPoint(tri._pointOfApplication);
			//Vector3 E = t.TransformPoint(tri._pointOfApplication + 0.1f * tri._normal);
			//Vector3 A = tri._pointOfApplication;
			//Vector3 E = tri._pointOfApplication + 0.1f * tri._normal;
			Gizmos.DrawLine(tri._pointOfApplication,tri._pointOfApplication + 0.1f * tri._normal);
			Gizmos.DrawSphere(tri._pointOfApplication , 0.005f);
		}

		Gizmos.color = DrawingUtil.Cyan;
		foreach(WaterlinePair pair in _waterLinePoints) {
			Gizmos.DrawLine(pair.p1, pair.p2);
		}
		
		foreach(DebugInfo  dbInfo in _debugInfoList) {
		//	Gizmos.DrawSphere(dbInfo._pointOfApplication , 0.01f);
		//	Gizmos.color = Color.red;
		//	Gizmos.DrawLine(dbInfo._pointOfApplication , dbInfo._pointOfApplication +  0.1f * dbInfo._normal);
		}
		
		Vector3 center = getCenterWorldPosition();
	
	//	Gizmos.color = Color.red;
	//	Gizmos.DrawWireSphere(new Vector3(center.x,center.y + 1, center.z), 0.025f);

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

	private Vector3 getCenterWorldPosition() {
		return this.transform.TransformPoint(_boatMeshCenter);
	}

	void OnGUI() {
 		GUI.Label (new Rect (0,0,100,50), _debugMsg);
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
}
