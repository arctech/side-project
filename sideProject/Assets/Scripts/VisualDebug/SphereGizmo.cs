using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereGizmo : MonoBehaviour {

	public bool ShowGizmos = true;
	
	public Color GizmoColor = Color.red;
	public string GizmoIcon = string.Empty;

	[Range(0f, 100f)]
	public float Range = 10f;


	void OnDrawGizmos() {
		if( !ShowGizmos )
		{
			return;
		}

		//Gizmos.DrawIcon(transform.position, GizmoIcon, true);
		Gizmos.color = GizmoColor;
		Gizmos.DrawWireSphere( transform.position, Range);

		// firward vector
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(transform.position, transform.position + transform.forward * Range);
	}
}
