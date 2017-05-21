using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour {

	public Camera camera;

	private float alpha = 0.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Ray ray = camera.ScreenPointToRay(new Vector3(camera.pixelWidth / 2, camera.pixelHeight / 2, 0));
		Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);


		//MathUtil.Test();


	/*	GameObject sphere = GameObject.Find("Sphere");
		Vector3 sphere_wp = sphere.transform.position;
		float radius = Mathf.Sqrt(sphere_wp.x * sphere_wp.x +  sphere_wp.z * sphere_wp.z);
		sphere.transform.position = new Vector3(radius * Mathf.Cos(alpha),sphere_wp.y, radius * Mathf.Sin(alpha));
		
		//Debug.DrawRay();

		//Debug.Log(sphere.transform.position);
		alpha += 0.01f;
*/
	}
}
