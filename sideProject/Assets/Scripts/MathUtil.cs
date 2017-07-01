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
}
