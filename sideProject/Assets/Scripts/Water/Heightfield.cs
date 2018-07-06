using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heightfield : MonoBehaviour {

	private Vector3[]  _v;
	private Vector3[]  _u;
	public int _dimX = 10;
	public int _dimZ = 10;

	[Range(0,1000)]
	public int _ticks = 20;

	[Range(0.01f,1)]
	public float _gizmoScale = 1.0f;

	public Vector3 _spherePosition = new Vector3();
	
	[Range(0.01f,10)]
	public float _sphereRadius = 5.0f;
	private Vector3 _center = new Vector3();
	private int _totalDim = 0;

	private Vector3 _lowerLeft = new Vector3();
	private Vector3 _origin = new Vector3();

	// Use this for initialization
	void Start () {
		_totalDim = (_ticks + 1) * (_ticks + 1);
		_v = new Vector3[_totalDim];
		float span = (float)(2 * _dimX) / _ticks;

		_origin = this.transform.position;
		_lowerLeft = _origin - new Vector3( _dimX, _origin.y, _dimZ);
		
		Debug.Log(span);
		for(int i = 0; i <= _ticks; i++) {
			for(int j = 0; j <= _ticks; j++) {	
				int idx = getLinearIndex(i, j);
				_v[idx].x = _lowerLeft.x + i * span;
				_v[idx].y = 0.1f * Mathf.Sin(i * j);
				_v[idx].z = _lowerLeft.z + j * span;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		float omega = 0.05f;
		float amplitude = 0.005f;

		for (int i = 0; i < _v.Length; i++)
		{	
			_v[i].y =  amplitude * Mathf.Sin(omega * Time.realtimeSinceStartup * _v[i].x * _v[i].z);
		//	Vector3 v  = _v[i];
			float distance =  (_v[i] - _spherePosition).magnitude;
			if( distance < _sphereRadius)
			{
			//	Debug.DrawLine(_spherePosition, _spherePosition + distance * (_v[i] - _spherePosition), Color.red);	
				//_v[i].y = (_v[i].y - 1.0f / distance);
				_v[i].y = (_v[i].y - (_sphereRadius -  distance));
				drawLocator(_v[i], _gizmoScale, ColorUtil.Cyan);
				
				//Debug.DrawLine(_spherePosition, _v[i], Color.red);	

			}
			else {
				drawLocator(_v[i], _gizmoScale, Color.black);
			}
		} 

		drawLocator(_lowerLeft,  1.0f, Color.red);

		drawLocator(_center,  1.0f, Color.green);

		drawLocator(_spherePosition, 0.5f, ColorUtil.LimeGreen);
	}

	void FixedUpdate() {
	}

	void OnDrawGizmos() {
		//Gizmos.DrawCube();
	}


	private void drawLocator(Vector3 pos, float scale = 1.0f, Color col = new Color())  {
		float offset = 1.0f *scale;
		Debug.DrawLine(new Vector3(pos.x, pos.y - offset, pos.z), new Vector3(pos.x, pos.y + offset, pos.z), col);
		Debug.DrawLine(new Vector3(pos.x - offset, pos.y, pos.z), new Vector3(pos.x + offset, pos.y, pos.z), col);
		Debug.DrawLine(new Vector3(pos.x, pos.y, pos.z - offset), new Vector3(pos.x, pos.y, pos.z + offset), col);
	}


	private int getLinearIndex(int i, int j) {
		return i * _ticks + j;
	}
}
