using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereAnimator : MonoBehaviour {

	public float MaxAcceleration = 10.0f;

	[Range(0.001f,0.01f)]
	public float Acceleration = 0.025f;

	public float Velocity = 0f;

	public float MinHeight = 0.0f;

	public float MaxHeight = 10.0f;

	public float Dir = 1.0f;

	private float _currAccel = 0.0f;

	// Use this for initialization
	void Start () {
		_currAccel = Acceleration;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		this.transform.Translate( new Vector3(0.0f, Velocity, 0.0f));
		if(this.transform.position.y < MinHeight ) {
			
		//	Dir *= -1.0f;
		//	Velocity *= -1.0f;
		//	Acceleration = 0.0f;
			Velocity = 0.0f;
			_currAccel = Acceleration;
		}
		else if (this.transform.position.y > MaxHeight) {
			Velocity = 0.0f;
			_currAccel = -Acceleration;
		}

		Velocity +=  _currAccel;
	}
}
