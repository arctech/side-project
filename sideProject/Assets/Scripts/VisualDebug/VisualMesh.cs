using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class VisualMesh : MonoBehaviour {
public class VisualMesh {
	private Mesh _mesh;
	private static int instanceCounter = 0;
	
	
	// Use this for initialization
	//void Start () {
	public VisualMesh() {
		_mesh = new Mesh();
		_mesh.name = "VisualMesh" + instanceCounter;
		_mesh.Clear();
		instanceCounter = instanceCounter + 1;

		GameObject go = new GameObject("Go" + instanceCounter);
		MeshFilter mf = go.AddComponent<MeshFilter>();
		mf.mesh = _mesh;
		go.AddComponent<MeshRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	public void setTriangles(List<Triangle> triangles) {
		if(_mesh == null)
		{
			return;
		}
		_mesh.Clear();
		int size = triangles.Count * 3;
		Vector3[] verts = new Vector3[size];
		Vector3[] normals = new Vector3[size];
		Vector2[] uvs = new Vector2[size];
		int[] tris  = new int[size];
		
		for(int i = 0; i < triangles.Count; i++)  
		{
			verts[i * 3] = triangles[i].Vertex1;
			verts[i * 3 + 1] = triangles[i].Vertex2;
			verts[i * 3 + 2] = triangles[i].Vertex3;

			tris[i * 3] = i * 3;
			tris[i * 3 + 1] = i * 3 + 1;
			tris[i * 3 + 2] = i * 3 + 2;
		}

		_mesh.vertices = verts;
	}
}
