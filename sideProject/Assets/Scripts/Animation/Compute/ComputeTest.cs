using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeTest : MonoBehaviour {

	public ComputeShader _computeShader;



	// Use this for initialization
	void Start () {
		Debug.Log("System supports compute shader: " + SystemInfo.supportsComputeShaders);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
