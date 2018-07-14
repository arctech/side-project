using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour {

	private CharacterController _characterController;

	private bool _isWalking;
	private bool _isRunning;
	private bool _isSprinting;
	private Quaternion _rotation;

	// Use this for initialization
	void Start () {
		this._characterController = this.GetComponent<CharacterController>();


	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void Move() {
		//this.

	}
}
