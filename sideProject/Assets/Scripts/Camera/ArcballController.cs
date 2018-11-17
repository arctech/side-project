using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcballController : MonoBehaviour {


	public Camera camera;
	public Transform target;


	private Vector3 _initialCamPos;
	private Quaternion _initialCamRot;

	/// Arcball logic
	private Arcball _arcball = new Arcball();
	
	private bool _isLeftMouseDragging;

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


	void Awake() {
		_initialCamPos = this.camera.transform.position;
		_initialCamRot = this.camera.transform.rotation;

		_arcball.place( new Vector2( Screen.width / 2.0f, Screen.height / 2.0f ),
			0.5f * Mathf.Sqrt( Screen.width * Screen.width + Screen.height * Screen.height ) );
	}


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
	void Update ()
    {
        queryMouseState();
		queryKeyInput();

		if( _isLeftMouseDragging ) 
		{
/*			var rotMatrix = _arcball.getRotationMatrix();

			Quaternion rotation = Quaternion.LookRotation(
 			    rotMatrix.GetColumn(2),
 			    rotMatrix.GetColumn(1)
			);*/
			float angle = 0.0f;
			Vector3 axis;
			var rotation = _arcball.getRotationMatrix();
			angle *= 0.01f;
			rotation.ToAngleAxis( out angle, out axis );

			target.rotation = rotation;
		}


	//	this.camera.transform.SetPositionAndRotation( 
			//new Vector3(), rotMatrix.rotation );
			//this.camera.transform.position, rotMatrix.rotation);
    }

    private void queryKeyInput()
    {
        if (Input.GetKeyDown("space"))
        {
            Debug.Log("F pressed!");
            this.camera.transform.position = _initialCamPos;
            this.camera.transform.rotation = _initialCamRot;
			_arcball.reset();
        }
    }

    private void queryMouseState()
    {
		if (Input.GetMouseButtonDown(0))
        {
            _isLeftMouseDragging = true;
			_arcball.startDragging();
        }
        else if (Input.GetMouseButtonUp(0))
        {
			_arcball.endDragging();
            _isLeftMouseDragging = false;
        }


        if (Input.GetMouseButtonDown(1))
        {
            //Debug.Log("Pressed right click.");
            //_isZooming = true;
        }
        if (Input.GetMouseButtonDown(2))
        {
            //Debug.Log("Pressed middle click.");
        }

        if (Input.GetMouseButtonUp(1))
        {
            //			_isZooming = false//;
            //	Debug.Log( "MouseButton 1");
        }


        var value = Input.GetAxis("Mouse ScrollWheel");
        if ((Mathf.Abs(value) - 0.0f) > 1e-5)
        {
            camera.transform.position += camera.transform.forward * value;
        }

		if( _isLeftMouseDragging == true )
		{
			_arcball.setMousePos( new Vector2( Input.mousePosition.x, Input.mousePosition.y ) );
		//	Debug.Log( "left drag! ");
		}
    }
}
