using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitController : MonoBehaviour {

	public Transform target;
	private float _angle = 0.01f;

	private bool _isDragging = false;
	
	public Renderer renderer;

	private SphereCollider _sphereCollider;

	// Use this for initialization
	void Start () {
		renderer = GetComponent<Renderer>();
		_sphereCollider = GetComponent<SphereCollider>();
	}
	
	// Update is called once per frame
	void Update () {
		Debug.DrawLine( this.transform.position, target.transform.position);

		this.transform.RotateAround( target.transform.position, Vector3.up, _angle);

		//Input.mousePosition

		

		if( _isDragging)
		{
			//Debug.Log("Dragging !");
			RaycastHit hitResult;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if( Physics.Raycast( ray, out hitResult))
			{
				Transform objectHit = hitResult.transform;
				Debug.DrawLine(hitResult.point, (hitResult.point + hitResult.normal * 0.2f)); 

				float span = 0.2f;
				Vector3 cross = Vector3.Cross(Camera.main.transform.right, hitResult.normal);
				Debug.DrawLine(hitResult.point, hitResult.point + span * cross, Color.green);
				//Vector3 cross2 = Vector3.Cross();
				Debug.DrawLine( hitResult.point, hitResult.point + span * Camera.main.transform.right, Color.red);

				Vector3 leftUpper = new Vector3(50, 0, 0);
				Vector3 rightLower  = new Vector3(50, -50, 0);
				
				//Vector3 cross = Camera.main.transform.right	
				//Camera.main.transform.right


				DrawingUtil.DrawBoundingBox(_sphereCollider.bounds, ColorUtil.Cyan);
			}	
		}

//		_angle += 0.01f;
	}


	/*
		Called every frame while mouse is down and hits Collider object of this gameObject.
 	*/
	/*void OnMouseDrag() {
		Debug.Log("Moues drag!");
		renderer.material.color -= Color.white * Time.deltaTime;

		RaycastHit hitResult;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if( Physics.Raycast( ray, out hitResult))
		{
			Transform objectHit = hitResult.transform;
			Debug.DrawLine(hitResult.point, (hitResult.point + hitResult.normal * 0.2f)); 


		}

	}*/

	void OnMouseDown() {
		//Debug.Log("Mouse Down > start drag!");
		_isDragging = true;
	}

	void OnMouseUp() {
		//Debug.Log("Mouse Up > end drag!");
		_isDragging = false;
	}	

	
}
