using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatController : MonoBehaviour {

	class MeshTriangle {
		public Triangle triangle;
		public int triangleId;

		// 0 vertices, 1 vertex, 2 vertices or all 3 vertices submerged;
		public int submergedState;	

		public MeshTriangle(Triangle triangle, int triangleId) {
			this.triangle = triangle;
			this.triangleId = triangleId;
			this.submergedState = 0;
		}
	}

	private List<MeshTriangle> _meshTriangleList = new List<MeshTriangle>();

	private int _totalSubmergedCount = 0;	

	private float _waterPatchCellWidth = 1.0f;
	private int _num_waterPatch_tiles = 5;
	private Vector3 _boatMeshCenter = Vector3.zero; // boat mesh bounding box center
	private float _boatMeshDiagLength = 0.0f; // length of diagonal of boat mesh bounding box

	private List<Vector3> _waterPatch_verts = new List<Vector3>();

	public GameObject _sentinelSphere = null;
	private Mesh _boatMesh = null;

	private string _debugMsg = "";

	private Vector3 _zG = new Vector3();

		
	// Use this for initialization
	void Start () {
		Mesh boatMesh = this.GetComponent<MeshFilter>().mesh;
		_boatMesh = this.GetComponent<MeshFilter>().mesh;

		_boatMeshDiagLength = (_boatMesh.bounds.max - _boatMesh.bounds.min).magnitude;
		_waterPatchCellWidth = _boatMeshDiagLength / _num_waterPatch_tiles;

		_sentinelSphere =  GameObject.Find("SentinelSphere");

		Debug.Log(boatMesh.name);
		Debug.Log(_sentinelSphere.name);

		int triangleId = 1;
		for (int i = 0; i < boatMesh.triangles.Length; i += 3)
		{
    		Vector3 p1 = boatMesh.vertices[boatMesh.triangles[i + 0]];
    		Vector3 p2 = boatMesh.vertices[boatMesh.triangles[i + 1]];
    		Vector3 p3 = boatMesh.vertices[boatMesh.triangles[i + 2]];
			_meshTriangleList.Add(new MeshTriangle(new Triangle(p1, p2, p3), triangleId++));
		}
	
		buildWaterpatch();
	}
	
	// Update is called once per frame
	void Update () {
		float start = Time.realtimeSinceStartup;

		float waterHeight = 0.0f;

		_totalSubmergedCount = 0;
		Transform t = this.transform;
		
	 	for(int i = 0; i < _meshTriangleList.Count; i++)
	   	{
			MeshTriangle mt = _meshTriangleList[i];
			mt.submergedState = 0;
			Vector3 v1_transformed = t.TransformPoint( mt.triangle.Vertex1);
			Vector3 v2_transformed = t.TransformPoint( mt.triangle.Vertex2);
			Vector3 v3_transformed = t.TransformPoint( mt.triangle.Vertex3);

			int count = 0;
			if( v1_transformed.y < waterHeight)
			{
				count += 1;
			}
			if( v2_transformed.y < waterHeight)
			{
				count += 1;
			}
			if( v3_transformed.y < waterHeight)
			{
				count += 1;
			}
			mt.submergedState = count;
			if( count == 3 )
			{
				_totalSubmergedCount++;
			}
		}

		_debugMsg = "BoatController - update: " + (Time.realtimeSinceStartup - start);


		if( Input.GetKeyDown(KeyCode.D))
		{
			_num_waterPatch_tiles = Mathf.Min(_num_waterPatch_tiles+2, 20);
			Debug.Log(_num_waterPatch_tiles);
			buildWaterpatch();
		} else if (Input.GetKeyDown(KeyCode.A)){
			_num_waterPatch_tiles = Mathf.Max(_num_waterPatch_tiles-2, 3);
			Debug.Log(_num_waterPatch_tiles);
			buildWaterpatch();
		}
		float waterPatchIncrement = 0.25f;
		if( Input.GetKeyDown(KeyCode.W))
		{
			_waterPatchCellWidth = Mathf.Min(_waterPatchCellWidth + waterPatchIncrement, 20.0f);	
			Debug.Log(_waterPatchCellWidth);
			buildWaterpatch();
		} else if(Input.GetKeyDown(KeyCode.S)) {
			Debug.Log(_waterPatchCellWidth);
			buildWaterpatch();
			_waterPatchCellWidth = Mathf.Max(_waterPatchCellWidth - waterPatchIncrement, 1.0f);	
		}
	

	/*	int test = 0;
		for(int i = 0; i < _meshTriangleList.Count; i++)
		{
			if( _meshTriangleList[i].submergedState > 0)
			{
				test++;
			}
		}

		Debug.Log("Update submerged: " + _totalSubmergedCount + " / " + test);*/
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.yellow;
       	// Gizmos.DrawSphere (transform.position, 1);
	   	Transform t = this.transform;
	   	Matrix4x4 m = this.transform.localToWorldMatrix;

	   	foreach( MeshTriangle mt in _meshTriangleList)
	   	{
			Vector3 centroid_transformed = t.TransformPoint(mt.triangle.Centroid);
			Vector3 v1_transformed = t.TransformPoint( mt.triangle.Vertex1);
			Vector3 v2_transformed = t.TransformPoint( mt.triangle.Vertex2);
			Vector3 v3_transformed = t.TransformPoint( mt.triangle.Vertex3);

			Color col = Color.black;
			switch( mt.submergedState)
			{
				case 1:
					col = Color.yellow;
					break;
				case 2:
					col = Color.green;
					break;
				case 3:
					col = Color.red;
					break;
			}

			//DrawingUtil.DrawText(mt.triangleId.ToString(), centroid_transformed, Gizmos.color);
			Gizmos.DrawWireSphere(centroid_transformed, 0.005f);
			Gizmos.color = col;
			DrawingUtil.DrawTriangle( v1_transformed, v2_transformed, v3_transformed, col);

			//Gizmos.DrawLine(centroid_transformed, v1_transformed );
			//Gizmos.DrawLine(centroid_transformed, v2_transformed );
			//Gizmos.DrawLine(centroid_transformed, v3_transformed );
	   	}

	
		Vector3 waterPatchCenter = t.TransformPoint(_boatMeshCenter);
		
		Gizmos.color = Color.white;
		// render water patch
		//Vector3 startPoint = new Vector3(
		//	_num_waterPatch_tiles * 0.5f * _boatMeshDiagLength, waterPatchCenter.y,  
		//	0);

		Gizmos.color = Color.blue;
		foreach( Vector3 v in _waterPatch_verts)
		{
			Gizmos.DrawWireSphere(t.TransformPoint(v), 0.025f);
		}

		/*for( int i = 0; i < _num_waterPatch_tiles; i++)
		{
			for( int j = 0; j < _num_waterPatch_tiles; j++)
		   	{
				Gizmos.DrawLine(waterPatchCenter, new Vector3(waterPatchCenter.x + 100, waterPatchCenter.y, waterPatchCenter.z));
		   	}
		}*/


		//Debug.Log();
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(new Vector3(waterPatchCenter.x,waterPatchCenter.y + 2, waterPatchCenter.z), 0.025f);

		// draw bounding box
		//Vector3 span = boatMesh.bounds.max - boatMesh.bounds.min;
		//Gizmos.DrawWireCube(waterPatchCenter, new Vector3(span.x, span.y, span.z));

		if( _sentinelSphere == null)
			return;	

		Gizmos.DrawWireSphere(t.TransformPoint(_zG) + new Vector3() + new Vector3(0,0.4f,0.0f), 0.01f);
		Gizmos.DrawWireSphere(_sentinelSphere.transform.position + new Vector3(0,0.4f,0.0f), 0.01f);

		Vector3 zG_transformed = t.TransformPoint(_zG);
		
		int i_ = Mathf.FloorToInt((_sentinelSphere.transform.position.x - zG_transformed.x) / ( _waterPatchCellWidth));
		int j_ = Mathf.FloorToInt((_sentinelSphere.transform.position.z- zG_transformed.z) / ( _waterPatchCellWidth));

		if( (i_ >= 0 && i_ < _num_waterPatch_tiles ) && (j_ >= 0 && j_ < _num_waterPatch_tiles))
		{
		Debug.Log(i_ + " / " + j_  + " - " + 
			 (_sentinelSphere.transform.position.x - zG_transformed.x)  + " / " + (_sentinelSphere.transform.position.z - zG_transformed.z) );
		}

		// render waterPatch triangles

	}

	void buildWaterpatch() {
		_waterPatch_verts.Clear();
		_boatMeshCenter = _boatMesh.bounds.center;
	//	_boatMeshDiagLength = (_boatMesh.bounds.max - _boatMesh.bounds.min).magnitude;
		_waterPatchCellWidth = _boatMeshDiagLength / _num_waterPatch_tiles;
	
		_zG = new Vector3(
			_boatMeshCenter.x - (_num_waterPatch_tiles / 2 * _waterPatchCellWidth), 
			_boatMeshCenter.y, 
			_boatMeshCenter.z - (_num_waterPatch_tiles / 2 * _waterPatchCellWidth));
		for( int i = 0; i < _num_waterPatch_tiles; i++)
		{
			for( int j = 0; j < _num_waterPatch_tiles; j++)
		   	{
				_waterPatch_verts.Add(new Vector3(_zG.x + i * _waterPatchCellWidth,_boatMeshCenter.y, _zG.z + j * _waterPatchCellWidth));   
		   	}
		}

		//_waterPatchTriangles
	}

	void OnGUI() {
 		GUI.Label (new Rect (0,0,100,50), _debugMsg);
	}
}
