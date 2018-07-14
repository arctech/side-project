using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class BuoyantSimulationSettings {
		public bool ApplyHydrostaticForce = false;

		public bool ApplyViscousForce = false;

		public bool ApplyDragForce = false;

		public bool ApplySlamForce = false;

		public bool UseDragForce = true;

		public bool UseSlamForce = true;

		public bool UseVerticalForceOnly = true;

		public float ViscousDragCoefficient = 0.5f;

		public Vector3 DragCoefficient = new Vector3(10.0f, 1.0f, .25f);

		public Vector3  SuctionCoefficient = new Vector3(1.0f, .1f, .25f);
	
		public float SlamForceMultiplier = 5000.0f;

		public float Rho = 1027.0f;

		[Range(3,9)]
		public int WaterpatchDimRows = 5;

		[Range(3,9)]
		public int WaterpatchDimCols = 5;

		[Range(0.1f,2)]
		public float DensityCorrectionModifier = .2f;
}
