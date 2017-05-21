using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcballController : MonoBehaviour {

	/// Arcball logic
	private Arcball _arcball = new Arcball();
	private bool _isDragging;

	/// camera logic
	private float _zoomMin;
	private float _zoomMax;

	private float _zoomFactor;

	public float _fov;

	private bool _isZooming;
	private bool _isPanning;
	private Vector2 _rightMouseDown;


	private bool _isMovingForward;
	private bool _isMovingBackward;

	private bool _isMovingLeft;
	private bool _isMovingRight;

	private bool _isMovingCameraUp;
	private bool _isMovingCameraDown;

	private float _cameraMoveSpeed;

	// Use this for initialization
	void Start () {
		_zoomMin = 1e-3f;
		_zoomMax = 100.0f;
		_zoomFactor = 1.0f;
		_fov = 0.25f * Mathf.PI;

		_isMovingForward = false;
		_isMovingBackward = false;
		_isMovingLeft = false;
		_isMovingRight = false;
		_isMovingCameraUp = false;
		_isMovingCameraDown = false;

		_cameraMoveSpeed = 10.0f;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButtonDown(0)) {
        	Debug.Log("Pressed left click.");
		}
   		if(Input.GetMouseButtonDown(1))
		{
        	//Debug.Log("Pressed right click.");
			_isZooming = true;
		}
    	if(Input.GetMouseButtonDown(2))
		{
        	Debug.Log("Pressed middle click.");
		}


		if( Input.GetMouseButtonUp(0))
		{

		}

		if( Input.GetMouseButtonUp(1))
		{
			_isZooming = false;
		}






		if (Input.GetAxis("Mouse ScrollWheel") > 0) 
		{
			Debug.Log("Mouse Wheel");
		}
	
		Camera camera = GetComponent<Camera>();
		if( camera == null)
		{
			Debug.Log("ArcballController - No camera object has been set!");
		}
		else{
		//	Debug.Log(camera.name);

		}
	}
}
