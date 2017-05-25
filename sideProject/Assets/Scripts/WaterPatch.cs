using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPatch 
{
	private List<Vector3> _pointList = new List<Vector3>();
	private int _numTiles = 5;
	private float _span = 10.0f;
	private int minNumTiles = 3;
	private int maxNumTiles = 30;
	private Vector3 _zG = Vector3.zero;	// lower left corner of waterPatchGrid
	private Vector3 _center = Vector3.zero;	// centerpoint of grid
		
	public void clear() {
		_pointList.Clear();
	}

	/*	public void setAt(int i, int j, Vector3 p) 
		{
			int linearIndex = i + j * _numTiles;
			if( linearIndex >= 0 && linearIndex < _pointList.Count)
			{
				Debug.Log("Waterpatch::setAt - Invalid index: " + linearIndex + "!");
				return;
			}
			_pointList[linearIndex] = p;
		}*/

	public void init(Vector3 center, float span, int numTiles) 
	{
		_center = center;
		_numTiles = numTiles;
		_span = span;
	}
	public void build() {
		if( _numTiles < minNumTiles || _numTiles > maxNumTiles)
		{
			Debug.Log("Waterpatch::build - numTiles must be valid:" + _numTiles + "!");
			return;
		}
	
		clear();
		//_boatMeshCenter = _boatMesh.bounds.center;
		//	_boatMeshDiagLength = (_boatMesh.bounds.max - _boatMesh.bounds.min).magnitude;
		//_waterPatchCellWidth = _boatMeshDiagLength / _num_waterPatch_tiles;
		float incr = getCellWidth();

		_zG = new Vector3(
		_center.x - (_numTiles / 2 * incr), 
			_center.y, 
			_center.z - (_numTiles / 2 * incr));
		for( int i = 0; i < _numTiles; i++)
		{
			for( int j = 0; j < _numTiles; j++)
		   	{
				//_pointList.Add(new Vector3(_zG.x + i * incr, _center.y, _zG.z + j * incr));   
				_pointList.Add(new Vector3(_zG.x + i * incr, .0f, _zG.z + j * incr));   
	  	 	}
		}
	//	Debug.Log("WaterPatch::build mesh size: " + _pointList.Count + "!");
	}

	public void updateCenter(Vector3 patchCenter, OceanManager om) {
		_center.x = patchCenter.x;
		_center.z = patchCenter.z;
		build();

		for(int i = 0; i < _pointList.Count; i++)
		{
			_pointList[i] = new Vector3(_pointList[i].x, om.calcPoint(_pointList[i]).y,_pointList[i].z );
			//_pointList[i] = new Vector3(_pointList[i].x,_pointList[i].y + Random.Range(-0.01f, 0.01f),_pointList[i].z );
			//result.y += Random.Range(-0.01f, 0.01f);
		}
	}

	public Vector2  getCellIndicesForPoint(Vector3 testPoint)
	{	
		float cellWidth = getCellWidth();
		return new Vector2(Mathf.FloorToInt((testPoint.x - _zG.x) / cellWidth ),
				Mathf.FloorToInt((testPoint.z - _zG.z) / cellWidth));
	}

	public Vector2 getCellCoordinatesForPoint(Vector3 testPoint)
	{
		Vector2 indices = getCellIndicesForPoint(testPoint);
		
		if ((indices[0] >= 0 && indices[0] < _numTiles - 1) && (indices[1] >= 0 && indices[1] < _numTiles - 1))
		{
			float xpc = (testPoint.x - _zG.x) - indices[1] * getCellWidth();
			float zpc = (testPoint.z - _zG.z) - indices[0] * getCellWidth();
			return new Vector2(xpc, zpc);
		}
		return Vector2.zero;
	}		

	public Vector3 get(int i, int j) 
	{
		int linearIndex = i + j * _numTiles;
		if( linearIndex >= 0 && linearIndex < _pointList.Count)
		{
			return _pointList[linearIndex];
		}
		Debug.Log("Waterpatch::get - Invalid index: " + linearIndex + " vs count:"  + _pointList.Count + "!");
		return Vector3.zero;
	}

	public void incrementNumTiles() {
		_numTiles = Mathf.Min(_numTiles + 2, maxNumTiles);
		if( _numTiles < maxNumTiles - 2)
		{
			_numTiles = _numTiles + 2;
			build();
		}
	}

	public void decrementNumTiles() {
		if( _numTiles > minNumTiles + 2)
		{
			_numTiles = _numTiles - 2;
			build();
		}
	}

	public float getCellWidth() {
		return _span / _numTiles;
	}

	public int NumTiles {
		get 
		{
			return _numTiles;
		}
	}

	public Vector3 zG {
		get
		{
			return _zG;
		}
	}
}	