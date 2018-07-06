using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitController : MonoBehaviour {

	public Transform target;
	private float _angle = 0.01f;

	private bool _isDragging = false;
	
	public Renderer renderer;

	// Use this for initialization
	void Start () {
		renderer = GetComponent<Renderer>();
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
			}	
		}

//		_angle += 0.01f;
	}


	/*

		Called every frame while mouse is down.
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
