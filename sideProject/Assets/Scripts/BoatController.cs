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

	private Vector3 _boatMeshCenter = Vector3.zero; // boat mesh bounding box center

	public GameObject _sentinelSphere = null;
	private Mesh _boatMesh = null;

	private string _debugMsg = "";

	private WaterPatch _waterPatch = new WaterPatch();

		
	// Use this for initialization
	void Start () {
		Mesh boatMesh = this.GetComponent<MeshFilter>().mesh;
		_boatMesh = this.GetComponent<MeshFilter>().mesh;

		// length of diagonal of boat mesh bounding box
		float boatMeshDiagLength = (_boatMesh.bounds.max - _boatMesh.bounds.min).magnitude;
		//_waterPatchCellWidth = _boatMeshDiagLength / _num_waterPatch_tiles;

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
		_boatMeshCenter = _boatMesh.bounds.center;

		_waterPatch.init(getCenterWorldPosition(), boatMeshDiagLength, 5);
		_waterPatch.build();
	}
	
	// Update is called once per frame
	void Update () {
		float start = Time.realtimeSinceStartup;

		_waterPatch.updateCenter(getCenterWorldPosition());

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

		Vector3 center = getCenterWorldPosition();
		
		/*Gizmos.color = Color.blue;
		for( int i = 0; i < _waterPatch.NumTiles; i++)
		{
				for( int j = 0; j < _waterPatch.NumTiles; j++)
				{
					Vector3 temp = _waterPatch.get(i, j);
					//Gizmos.DrawWireSphere(t.TransformPoint(temp), 0.025f);
				//	Gizmos.DrawWireSphere(temp, 0.025f);
				}
		}*/

		//Debug.Log();
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(new Vector3(center.x,center.y + 1, center.z), 0.025f);

		// draw bounding box
		//Vector3 span = boatMesh.bounds.max - boatMesh.bounds.min;
		//Gizmos.DrawWireCube(waterPatchCenter, new Vector3(span.x, span.y, span.z));

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

		Gizmos.DrawWireSphere(_waterPatch.zG + new Vector3(0,0.4f,0.0f), 0.01f);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(_sentinelSphere.transform.position + new Vector3(0,0.4f,0.0f), 0.01f);

		Vector2 cellIndices = _waterPatch.getCellIndicesForPoint(_sentinelSphere.transform.position);
		Vector2 cellCoordinates = _waterPatch.getCellCoordinatesForPoint(_sentinelSphere.transform.position);
		if(( cellIndices.x >= 0 && cellIndices.x < _waterPatch.NumTiles - 1) && cellIndices.y >= 0 && cellIndices.y < _waterPatch.NumTiles - 1)
		{
			int i = (int)cellIndices.x;
			int j = (int)cellIndices.y;
			float xpc = cellCoordinates.x;
			float zpc = cellCoordinates.y;
			Debug.Log(i + " / " + j  + " - " );
			// + 
			//	 (_sentinelSphere.transform.position.x - zG_transformed.x)  + " / " + (_sentinelSphere.transform.position.z - zG_transformed.z) );

			Vector3 A = _waterPatch.get(i, j);
			Vector3 B = _waterPatch.get(i + 1, j);
			Vector3 D = _waterPatch.get(i, j + 1);
			Vector3 C = _waterPatch.get(i + 1, j + 1); 	

			Gizmos.color = Color.yellow;
			if( xpc <= zpc)
			{
				Gizmos.DrawWireSphere(A, 0.025f);
				Gizmos.DrawWireSphere(B, 0.025f);
				Gizmos.DrawWireSphere(C, 0.025f);
			}
			else
			{
				Gizmos.DrawWireSphere(A, 0.025f);
				Gizmos.DrawWireSphere(D, 0.025f);
				Gizmos.DrawWireSphere(C, 0.025f);
			}

		}
	}

	private Vector3 getCenterWorldPosition() {
		return this.transform.TransformPoint(_boatMeshCenter);
	}

	void OnGUI() {
 		GUI.Label (new Rect (0,0,100,50), _debugMsg);
	}
}
