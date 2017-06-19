using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	public float speed = 6f;

	private Vector3 _movement;
	private Animator _animator;

	private Rigidbody _playerRigidbody;
	private int _floorMask;
	private float _camRayLength = 100f;

	void Awake() {
		_animator = GetComponent<Animator>();
		_playerRigidbody = GetComponent<Rigidbody>();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate() {
		float h = Input.GetAxisRaw("Horizontal");
		float v = Input.GetAxisRaw("Vertical");

		if(v != 0 || h != 0) {
			//Debug.Log(h + "fixed update!");
		}

		if(Input.GetButtonDown("Interact")) 
		{
			Debug.Log("Interact pressed!");
		}
		if(Input.GetButtonDown("Jump")) 
		{
			Debug.Log("Jump pressed!");
		}

		//   bool down = Input.GetButtonDown("Jump");
        //bool held = Input.GetButton("Jump");
        //bool up = Input.GetButtonUp("Jump");

		//Mouse X
		// Mouse Y
		//Mouse ScrollWheel
		// Jump
		// Fire1  - space
		// Fire2 - left alt
		// Fire3 - left shift
		// Interact - E

		Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit rayCastHit;

		if(Physics.Raycast(camRay,out rayCastHit, _camRayLength, _floorMask)) 
		{
			Vector3 playerToMouse = rayCastHit.point - transform.position;
			//playerToMouse.y = 0f;

			//Quaternion newRotation = Quaternion.LookRotation(playerToMouse);
			//_playerRigidbody.MoveRotation(newRotation);
		}
	}

}
