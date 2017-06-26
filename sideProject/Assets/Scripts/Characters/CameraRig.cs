using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRig : MonoBehaviour {

	public enum CameraShake{
		Shock,
		Wind
	}

	public enum FollowSide{
		Right,
		Left
	}

	[System.Serializable]
	public class Settings{
		public float cameraRotationSpeed = 1f;
		public float minYAngle = -30.0f;
		public float maxYAngle = 70.0f;

		public float rotationMultiplier = 5.0f;

		public bool LockCursor = true;
	}

	public Settings settings = new Settings();

	public CameraShake activeCameraShake = CameraShake.Shock;

	public Transform followTarget;

	public Vector3 lookAtOffset;

	public Camera attachedCamera;

	[RangeAttribute(0.1f,10)]
	public float springArmLength = 1;

	private float temp = .001f;

	private Vector3 _debugPos;

	private Vector3 _mousePosOld;

	private Vector3 cameraToObjectRay = Vector3.zero;

//	private Quaternion _prevRotation;

	private Vector2 _camRotation = Vector2.zero;

//	private Transform _childTransform;

	// Use this for initialization
	void Start () {
		setSpringArmLength();
		setCameraLookAt();

		if(settings.LockCursor)
		{
			 Screen.lockCursor = true;
		} 
	}
	
	// Update is called once per frame
	void Update () {
		//float h = Input.GetAxis("Horizontal");
        //float v = Input.GetAxis("Vertical");
		springArmLength =  Mathf.Clamp(springArmLength - Input.GetAxis("Mouse ScrollWheel"), 0.1f, 10);
		float x = Input.GetAxis("Mouse X");
		float y = Input.GetAxis("Mouse Y");
		_mousePosOld = Input.mousePosition;
		
		//float mouseDeltaX = Input.mousePosition.x - _mousePosOld.x;
		//float mouseDeltaY = -Input.mousePosition.y - _mousePosOld.y;
	//	_camRotation.x += x * settings.rotationMultiplier;
	//	_camRotation.y += y * settings.rotationMultiplier;

		Vector3 eulerAngleAxis = new Vector3(y * Time.deltaTime * settings.cameraRotationSpeed, x * Time.deltaTime * settings.cameraRotationSpeed, 0);
		//Vector3 eulerAngleAxis = new Vector3(_camRotation.y, _camRotation.x, 0);
	//	Vector3 eulerAngleAxis = new Vector3(0, _camRotation.x, 0);

		_camRotation.x = Mathf.Repeat( _camRotation.x, 360);
        _camRotation.y = Mathf.Clamp(_camRotation.y, settings.minYAngle, settings.maxYAngle);

		Quaternion newRotation = Quaternion.Slerp(followTarget.localRotation, Quaternion.Euler(eulerAngleAxis), Time.deltaTime * settings.cameraRotationSpeed);
		followTarget.localRotation = newRotation;

		
		//Debug.Log(Input.GetAxis(input.verticalAxis));
		
		Vector3 axis = Vector3.zero;
		float angle = 0;
		newRotation.ToAngleAxis(out angle, out axis);
		//this.transform.localRotation = newRotation;

		this.transform.RotateAround(followTarget.position, axis, angle); 

		attachedCamera.transform.position = transform.position;
		setSpringArmLength();
		setCameraLookAt();

		//_prevRotation.
		
	//	Transform targetTransform = followTarget;
	//	Vector3 targetPos = followTarget.position;
		
		//Vector3 diff = camera.transform.forward;
	//	targetTransform.Rotate(0f, 0f, mouseDeltaX, Space.Self); // roll
	//	targetTransform.Rotate(mouseDeltaY, 0f, 0f, Space.Self);	// pitch
	//	targetTransform.Rotate(0f, mouseDeltaX, 0f, Space.Self); // yaw
		
	//	attachedCamera.transform.LookAt(followTarget);
		
		//camera.transform.Rotate(0f, Input.mousePosition.x, 0f, Space.World);
	//	attachedCamera.transform.position = targetTransform.position + targetTransform.forward.normalized * springArmLength;
		
		_debugPos = followTarget.transform.position + cameraToObjectRay * springArmLength;
		//_debugPos = targetTransform.position + targetTransform.forward.normalized * springArmLength;
		
		//_debugPos.Rotate(0f, mouseDeltaX, 0f, Space.Self);
	
		//followTarget = 
	
	//	camera.transform.position = targetTransform.position;
		temp += 0.01f;
		//camera.transform.position = followTarget.position - 
	}

	private void CheckCollision() {
		RaycastHit hit;
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(_debugPos, 0.2f);

		Gizmos.DrawLine(followTarget.transform.position, _debugPos);

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, 0.2f);
	}

	private void setSpringArmLength() {
		cameraToObjectRay = (attachedCamera.transform.position - followTarget.transform.position).normalized;
		attachedCamera.transform.position = followTarget.transform.position + cameraToObjectRay * springArmLength;
	}

	private void setCameraLookAt() {
		attachedCamera.transform.LookAt(followTarget);
	}

	private void triggerCameraShake() {
		// position
		// rotation
		// 
		switch(activeCameraShake) 
		{
			case CameraShake.Wind:
			{
				break;
			}
			case CameraShake.Shock:
			{
				break;
			}
		}
	}
}
