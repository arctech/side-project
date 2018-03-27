using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sierpinski : MonoBehaviour {

	List<Vector3> _pointList = new List<Vector3>();
	List<Triangle> _triangleList = new List<Triangle>();
	Vector3 _A = new Vector3(10, 0, 0);
	Vector3 _B = new Vector3(0, 0, 0);
	Vector3 _C = new Vector3(5, 8.66f, 0);

	private int SUBD_LIMIT = 1200;

	struct PointPair {
		public Vector3 V1;
		public Vector3 V2; 

		public PointPair(Vector3 v1, Vector3 v2) {
			V1 = v1;
			V2 = v2;
		}
	}

	List<PointPair> _linePairs = new List<PointPair>();

	// Use this for initialization
	void Start () {
		_pointList.Add(_A);
		_pointList.Add(_B);
		_pointList.Add(_C);

		_triangleList.Add(new Triangle(_A, _B, _C));
	}

	List<Triangle> subdivideTriangle(Triangle t) {
		List<Triangle> result = new List<Triangle>();

		Vector3 mid1 = MathUtil.GetMidPointBetweenPoints(t.Vertex1, t.Vertex2);
		Vector3 mid2 = MathUtil.GetMidPointBetweenPoints(t.Vertex2, t.Vertex3);
		Vector3 mid3 = MathUtil.GetMidPointBetweenPoints(t.Vertex3, t.Vertex1);

		result.Add(new Triangle(t.Vertex1, mid1, mid3));
		result.Add(new Triangle(mid1, t.Vertex2, mid2));
		result.Add(new Triangle(mid3, mid2, t.Vertex3));

		return result;
	}
	
	// Update is called once per frame
	void Update () {

		if(_triangleList.Count < SUBD_LIMIT)
		{
			List<Triangle> temp = new List<Triangle>();
			foreach(Triangle tri in _triangleList) {
				List<Triangle> tiles = subdivideTriangle(tri);
				foreach(Triangle t in tiles) {
					temp.Add(t);
				}
			}
			foreach(Triangle t in temp) {
				_triangleList.Add(t);
			}
		} 
	}


	void OnDrawGizmos() { 

		foreach(Triangle tri in _triangleList)
		{
			DrawingUtil.DrawTriangle(tri, ColorUtil.CORN_FLOWER_BLUE);
		} 
	}
}
