using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRig : MonoBehaviour {

	public Transform followTarget;

	public Vector3 lookAtOffset;

	public Camera attachedCamera;

	[RangeAttribute(0.1f,10)]
	public float springArmLength = 1;

	private float temp = .001f;

	private Vector3 _debugPos;

	private Vector3 _mousePosOld;

	private Vector3 cameraToObjectRay = Vector3.zero;

	// Use this for initialization
	void Start () {
		//	_mousePosOld = Input.mousePosition;
		//	cameraToObjectRay = attachedCamera.transform.position - followTarget.transform.position.normalized;
		//	attachedCamera.transform.position = followTarget.transform.position + cameraToObjectRay * springArmLength;
			//attachedCamera.transform.position = 
		//	attachedCamera.transform.LookAt(followTarget);
		setSpringArmLength();
	}
	
	// Update is called once per frame
	void Update () {
		//float h = Input.GetAxis("Horizontal");
        //float v = Input.GetAxis("Vertical");
		springArmLength =  Mathf.Clamp(springArmLength - Input.GetAxis("Mouse ScrollWheel"), 0.1f, 10);
		float mouseDeltaX = Input.mousePosition.x - _mousePosOld.x;
		float mouseDeltaY = Input.mousePosition.y - _mousePosOld.y;

		setSpringArmLength();
		_mousePosOld = Input.mousePosition;
		
		Transform targetTransform = followTarget;
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
		#if DEBUG
		  	Debug.DrawLine(targetTransform.position, targetTransform.position + targetTransform.forward.normalized * springArmLength, Color.blue);
		#endif
		//followTarget = 
	
	//	camera.transform.position = targetTransform.position;
		temp += 0.01f;
		//camera.transform.position = followTarget.position - 
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(_debugPos, 0.2f);
	}

	private void setSpringArmLength() {
		cameraToObjectRay = (attachedCamera.transform.position - followTarget.transform.position).normalized;
		attachedCamera.transform.position = followTarget.transform.position + cameraToObjectRay * springArmLength;
		attachedCamera.transform.LookAt(followTarget);
	}
}
