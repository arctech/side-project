using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/**
* @brief Arcball maps click coordinates into spherical coordinates 
*  with unconstrained axes.
*
* The goal ist to compute the angle theta and the rotation axis.
* The vectors OP1 and OP2 are directly taken from the click in camera coordinates.
*
* Angle theta is retrieved by arccos ( dot ( OP1 * OP2 ))
* Rotation axis is retrieved by the cross product OP1 x OP2
*
* source: Shoemake:  ARCBALL: A User Interface for Specifiying Three-Dimensional 
*  Orientation Using a Mouse
*/
public class Arcball {

	private bool _isDragging;
	private Vector2 _center;
	private Vector2 _posDown;
	private Vector2 _posCurrent;
	private float _radius;
	private Quaternion _quatCurrent;
	private Quaternion _quatNow;
	private Quaternion _quatDown;


	public Arcball(){
		_isDragging = false;
	}

	private void setMousePos(Vector2 mousePos) {
		_posCurrent = mousePos;
	}
	private void place(Vector2 center, float radius) {
		_center = center;
		_radius = radius;
	}

	private void startDragging() {
		_isDragging = true;
		_posDown = _posCurrent;
	}

	private void endDragging() {
		_isDragging = false;
		// save state of quaternion
		_quatDown = _quatNow;
	}

	private Matrix4x4 getRotationMatrix() {
		Vector3 vecFrom = convertMouseToSphere(_posDown);
		Vector3 vecTo = convertMouseToSphere( _posCurrent);

		if( _isDragging)
		{
			_quatCurrent = getQuaternion( vecFrom, vecTo);
			_quatNow = _quatCurrent * _quatDown;
		}
	
		return Matrix4x4.TRS( Vector3.zero, _quatNow, new Vector3(1,1,1));
	}

	/**
	 * @brief Convert 2D window coordinates to coordinates on 3D unit sphere.
	*/
	private Vector3 convertMouseToSphere(Vector2 mousePos) {
		Vector3 pt = Vector3.zero;

		pt.x = (mousePos.x - _center.x) / _radius;
		pt.y = (mousePos.y - _center.y) / _radius;

		float radius = pt.x * pt.x + pt.y * pt.y;

		if( radius > 1.0f )
		{
			float factor = 1.0f / Mathf.Sqrt(radius);
			pt.x = factor * pt.x;
			pt.y = factor * pt.y;
			pt.z = 0.0f;
		}
		else
		{
			pt.z = Mathf.Sqrt(1.0f - radius);
		}
		return pt;
	}

	/**
	* @brief Creates unit quaternion from two points on unit sphere.
	*/
	private Quaternion getQuaternion(Vector3 from, Vector3 to) 
	{
		Vector3 cross = Vector3.Cross(from, to);
		float dot = Vector3.Dot(from,to);
		return new Quaternion(cross.x, cross.y, cross.z, dot);
	}

	private void reset() {
		_quatDown = Quaternion.identity;
		_quatNow = Quaternion.identity;
	}
}
