using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanManager : MonoBehaviour {

	// Use this for initialization
	public float _amplitude = 0.5f;
	public float _phaseFactor = 20.0f;

	private float _elapsedSimulationTime = 0.0f;

	private float _timeFactor = 1 / 20f;
	
	
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	//	Debug.Log("OceanManager -> time" + Time.fixedDeltaTime);
	}

	public Vector3 calcPoint(Vector3 point) {
		Vector3 result = point;
		//result.y += Random.Range(-0.01f, 0.01f);
		//result.y += MathUtil.RandomSign * Random.value * 0.05f;

		//float phase = Time.fixedDeltaTime * _phaseFactor;
		//float phase = _elapsedSimulationTime * _phaseFactor * 1/20f;
		//float phase = Time.realtimeSinceStartup * _phaseFactor *_timeFactor;
   		//float offset = (point.x + (point.z * 0.2f)) * 0.5f;
    	//result.y = Mathf.Sin(phase + offset) * 0.2f;
		//_elapsedSimulationTime += Time.fixedDeltaTime; 
		return result;
	}
}
