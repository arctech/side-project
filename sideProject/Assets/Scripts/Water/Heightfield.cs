using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heightfield : MonoBehaviour {

	private Vector3[]  _v;

	private Vector3[]  _u;
	public int _dimX = 10;
	public int _dimY = 10;
	public float _res = 20;
	private Vector3 _center = new Vector3();
	private int _totalDim = 0;


	// Use this for initialization
	void Start () {
		_v = new Vector3[_dimX * _dimY];
		_totalDim = _dimX * _dimY;

		//float incr = _dimX / _span;
		float incr = _dimX / _res;

		Vector3 origin = this.transform.position;
		
		Debug.Log(incr);
		for(int i = 0; i < _dimX; i++) {
			for(int j = 0; j < _dimY; j++) {
				int idx = getLinearIndex(i, j);
				_v[idx].x = origin.x + i * incr;
				_v[idx].y = 0.1f * Mathf.Sin(i * j);
				_v[idx].z = origin.z + j * incr;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {

		foreach (Vector3 v in _v)
		{
			drawLocator(v, 0.1f);
		} 
	}


	void FixedUpdate() {

	}


	void OnDrawGizmos() {
		//Gizmos.DrawCube();

	}


	private void drawLocator(Vector3 pos, float scale = 1.0f)  {
		float offset = 1.0f *scale;
		Debug.DrawLine(new Vector3(pos.x, pos.y - offset, pos.z), new Vector3(pos.x, pos.y + offset, pos.z), ColorUtil.CORN_FLOWER_BLUE);
		Debug.DrawLine(new Vector3(pos.x - offset, pos.y, pos.z), new Vector3(pos.x + offset, pos.y, pos.z), ColorUtil.CORN_FLOWER_BLUE);
		Debug.DrawLine(new Vector3(pos.x, pos.y, pos.z - offset), new Vector3(pos.x, pos.y, pos.z + offset), ColorUtil.CORN_FLOWER_BLUE);
	}


	private int getLinearIndex(int i, int j) {
		return i * _dimX + j;
	}
}
