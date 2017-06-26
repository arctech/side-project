using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float forwardSpeed = 0.1f;
	private Animator _animator;
	private Rigidbody _rigidbody;
	private int _floorMask;
	private float _camRayLength = 100f;

	private bool _hasDied = false;

	void Awake() {
		_rigidbody = this.GetComponent<Rigidbody>();
		_animator = this.GetComponent<Animator>();
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log("RB: " + _rigidbody.velocity);

		//Ray ray = 

		//Ray ray = camera.ScreenPointToRay(new Vector3(camera.pixelWidth / 2, camera.pixelHeight / 2, 0));
		//Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
		//Vector3 facingDir = this.transform..LookAt;
		Vector3 translation = Vector3.zero;
		if (Input.GetKey(KeyCode.W))
        {
		//	translation += forwardSpeed;
		}

		if (Input.GetKey(KeyCode.S))
        {
		//	translation -= forwardSpeed;
		}

		this.transform.position += translation;

		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
			//_animator.["animationName"].speed = somefloat;
           //  agent.speed = 4f;
		   _animator.SetFloat("Speed",6);
        }
        else
        {
			_animator.SetFloat("Speed",0);
            // agent.speed = 1f;
        }
		if(Input.GetKey(KeyCode.Space)) {

		} 
		if(Input.GetKey(KeyCode.F)) {
			_animator.SetTrigger("Die");
		//	if(!_hasDied) {
 		//		StartCoroutine(triggerDelayedGetupAnimation());
		//	}
		}
	}

	void FixedUpdate() {
		float h = Input.GetAxisRaw("Horizontal");
		float v = Input.GetAxisRaw("Vertical");

		if(v != 0 || h != 0) {
			//Debug.Log(h + "fixed update!");
		}

	/*	if(Input.GetButtonDown("Interact")) 
		{
			Debug.Log("Interact pressed!");
		}*/
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

		 // Cache the current value of the AimWeight curve.
    	//float aimWeight = anim.GetFloat(hash.aimWeightFloat);
 
    	// Set the IK position of the right hand to the player's centre.
    	//anim.SetIKPosition(AvatarIKGoal.RightHand, player.position + Vector3.up * 1.5f);
 
    	// Set the weight of the IK compared to animation to that of the curve.
   		 //anim.SetIKPositionWeight(AvatarIKGoal.RightHand, aimWeight);
	}

	 void OnCollisionEnter(Collision col) {
       /* if (col.gameObject.CompareTag("Enemy"))
        {
            _animator.SetTrigger("Die");
        }*/
    }

	 // The delay coroutine
     IEnumerator triggerDelayedGetupAnimation ()
     {
         yield return new WaitForSeconds(2);
         _animator.Play("get_up");
     }
}
