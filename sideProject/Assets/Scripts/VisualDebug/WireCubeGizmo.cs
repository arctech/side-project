using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireCubeGizmo : MonoBehaviour {
	
	public bool ShowGizmos = true;
	
	public Color GizmoColor = Color.red;
	public string GizmoIcon = string.Empty;

	[Range(0f, 100f)]
	public float RangeX = 10f;
	[Range(0f, 100f)]	
	public float RangeY = 10f;	
	[Range(0f, 100f)]
	public float RangeZ = 10f;


	void OnDrawGizmos() {
		if( !ShowGizmos )
		{
			return;
		}

		Gizmos.DrawIcon(transform.position, GizmoIcon, true);
		
		Gizmos.color = GizmoColor;
		Gizmos.DrawWireCube( transform.position, new Vector3(RangeX, RangeY, RangeZ));
		
		float minRange  = 0.5f * Mathf.Min( RangeX, Mathf.Min(RangeY, RangeZ));

		// firward vector
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(transform.position, transform.position + transform.forward * minRange);
	}
}
