using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPatch  {
	

	private List<Vector3> _waterPatch_verts = new List<Vector3>();
	private List<Triangle> _waterPatchTriangles = new List<Triangle>();

	private Vector3 _waterPatchCenter = Vector3.zero;

	private int _num_waterPatch_tiles = 5;

	private float _waterPatchCellWidth = 1.0f;

	private Vector3 _zG = new Vector3();	// lower, left corner position of waterPatch

	public WaterPatch()
	{

	}


	public void update(Transform transform) {
		foreach( Vector3 v in _waterPatch_verts)
		{

		}
	}

	public void build(Vector3 center, float span) {
		_waterPatch_verts.Clear();
	//	_boatMeshCenter = _boatMesh.bounds.center;
	//	_boatMeshDiagLength = (_boatMesh.bounds.max - _boatMesh.bounds.min).magnitude;
	//	_waterPatchCellWidth = _boatMeshDiagLength / _num_waterPatch_tiles;
	
		_zG = new Vector3(
			center.x - (_num_waterPatch_tiles / 2 * _waterPatchCellWidth), 
			center.y, 
			center.z - (_num_waterPatch_tiles / 2 * _waterPatchCellWidth));
		for( int i = 0; i < _num_waterPatch_tiles; i++)
		{
			for( int j = 0; j < _num_waterPatch_tiles; j++)
		   	{
				_waterPatch_verts.Add(new Vector3(_zG.x + i * _waterPatchCellWidth,center.y, _zG.z + j * _waterPatchCellWidth));   
		   	}
		}

		//_waterPatchTriangles
	}

	public  List<Vector3> PatchVertices{
		get{
			return _waterPatch_verts;
		}
	}

	public float CellWidth {
		get{
			return _waterPatchCellWidth;
		}
		set
		{
			_waterPatchCellWidth = value;
		}
	}

}
