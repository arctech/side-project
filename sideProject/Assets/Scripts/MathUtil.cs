using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathUtil  {

	public static int RandomSign()  {
    	return Random.value < .5? 1 : -1;
	}

	public static float calcTriangleArea(Vector3 A, Vector3 B, Vector3 C) {
		float a = Vector3.Distance(A, B);
        float c = Vector3.Distance(A, C);
        return (a * c * Mathf.Sin(Vector3.Angle(B - A, C - A) * Mathf.Deg2Rad)) / 2f;
	}

	public static Vector3 GetMidPointBetweenPoints(Vector3 start, Vector3 end) {
		return start + ((end - start) * 0.5f);
	}


	/*public static void GetCameraRaycastHit(Camera camera, Vector3 position, RaycastHit hitResult) {
		//RaycastHit hitResult;
		Ray ray = camera.ScreenPointToRay(position);

		if( Physics.Raycast( ray, out hitResult))
		{
			Transform objectHit = hitResult.transform;
			//Debug.DrawLine(hitResult.point, (hitResult.point + hitResult.normal * 0.2f)); 
		}
		
	}*/


}
