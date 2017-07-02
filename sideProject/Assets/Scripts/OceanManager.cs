using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanManager : MonoBehaviour {

	// Use this for initialization
	
	[System.Serializable]
	public class GerstnerWave {
		
		[Range(0,10)]
		public float Q = 0;
		[Range(0,100)]
		public float WaveLength = 1;
		[Range(0.01f,10)]
		public float Amplitude = 1;
		public Vector2 Direction = Vector2.zero;
		
		[Range(0, 100)]
		public float Speed = 1;

		public GerstnerWave(float q, float w, float a, Vector2 d, float s) {
			Q = q;
			WaveLength = w;
			Amplitude = a;
			Direction = d.normalized;
			Speed = s;
		}
	}

	public int numTiles = 100;
	public int dim = 50;

	public bool showDebug = true;
	public GerstnerWave wave1 = new GerstnerWave(1, 1, 1, new Vector2(1,0), 1);
	public GerstnerWave wave2 = new GerstnerWave(1, 1, 1, new Vector2(1,0), 1);
	public GerstnerWave wave3 = new GerstnerWave(1, 1, 1, new Vector2(1,0), 1);
	public GerstnerWave wave4 = new GerstnerWave(1, 1, 1, new Vector2(1,0), 1);
	private float _elapsedSimulationTime = 0.0f;
	private float _timeFactor = 1 / 20f;
	private float _span;

	class GerstnerResult {
		public Vector3 position;
		public Vector3 normal;
	}

	private GameObject _gameObject;

	private Material _material;

	void Start () {
		_span = (float)dim / numTiles;

		Material[] materialsArray = this.GetComponent<MeshRenderer> ().materials;
		 foreach (Material material in materialsArray) {
			 if(material.name.Equals("OceanMaterial (Instance)")) {
			//	Debug.Log(material.name);
				_material = material;
			//	Debug.Log(material);
			 }
		 }
	

	//this.renderer.material.SetFloat("_Blend", someFloatValue);
	//	  Material myMaterial = Resources.Load("Materials/MyMaterial", typeof(Material)) as Material;
		//_gameObject obj =  GameObject.Find<
		
	}
	
	// Update is called once per frame
	void Update () {
		//	Debug.Log("OceanManager -> time" + Time.fixedDeltaTime);
		if(_material != null) {
			_material.SetFloat("_AppTime", Time.realtimeSinceStartup);

			_material.SetFloat("_q_1", wave1.Q);
			_material.SetFloat("_Wavelength_1", wave1.WaveLength);
			_material.SetFloat("_Wavespeed_1", wave1.Speed);
			_material.SetFloat("_Amplitude_1", wave1.Amplitude);
			_material.SetColor("_Direction_1", new Color(wave1.Direction.x, wave1.Direction.y, 0, 1));

			_material.SetFloat("_q_2", wave2.Q);
			_material.SetFloat("_Wavelength_2", wave2.WaveLength);
			_material.SetFloat("_Wavespeed_2", wave2.Speed);
			_material.SetFloat("_Amplitude_2", wave2.Amplitude);
			_material.SetColor("_Direction_2", new Color(wave2.Direction.x, wave2.Direction.y, 0, 1));

			_material.SetFloat("_q_3", wave3.Q);
			_material.SetFloat("_Wavelength_3", wave3.WaveLength);
			_material.SetFloat("_Wavespeed_3", wave3.Speed);
			_material.SetFloat("_Amplitude_3", wave3.Amplitude);
			_material.SetColor("_Direction_3", new Color(wave3.Direction.x, wave3.Direction.y, 0, 1));

			_material.SetFloat("_q_4", wave4.Q);
			_material.SetFloat("_Wavelength_4", wave4.WaveLength);
			_material.SetFloat("_Wavespeed_4", wave4.Speed);
			_material.SetFloat("_Amplitude_4", wave4.Amplitude);
			_material.SetColor("_Direction_4", new Color(wave4.Direction.x, wave4.Direction.y, 0, 1));
			//Debug.Log("Fixed update " + Time.realtimeSinceStartup);
		}
		
	}

	public Vector3 calcPoint(Vector3 pointWS) {
		GerstnerResult gw1 = calcGerstnerWave(pointWS, wave1.Q, wave1.Amplitude, wave1.Direction, wave1.WaveLength, wave1.Speed ); 
		GerstnerResult gw2 = calcGerstnerWave(pointWS, wave2.Q, wave2.Amplitude, wave2.Direction, wave2.WaveLength, wave2.Speed ); 
		GerstnerResult gw3 = calcGerstnerWave(pointWS, wave3.Q, wave3.Amplitude, wave3.Direction, wave3.WaveLength, wave3.Speed ); 
		GerstnerResult gw4 = calcGerstnerWave(pointWS, wave4.Q, wave4.Amplitude, wave4.Direction, wave4.WaveLength, wave4.Speed ); 

		//Vector3 result = pointWS;
		Vector3 sum = (gw1.position + gw2.position + gw3.position + gw4.position) / 4;
		//Debug.Log(sum);
		//sum = gw1.position / 4;
		
		//result.y += Random.Range(-0.01f, 0.01f);
		//result.y += MathUtil.RandomSign * Random.value * 0.05f;

		//float phase = Time.fixedDeltaTime * _phaseFactor;
		//float phase = _elapsedSimulationTime * _phaseFactor * 1/20f;
		//float phase = Time.realtimeSinceStartup * _phaseFactor *_timeFactor;
   		//float offset = (point.x + (point.z * 0.2f)) * 0.5f;
    	//result.y = Mathf.Sin(phase + offset) * 0.2f;
		//_elapsedSimulationTime += Time.fixedDeltaTime; 
		return pointWS + sum;
	}
	
	private GerstnerResult calcGerstnerWave(Vector3 posWS, float Q, float amplitude, Vector2 direction, float waveLength, float speed) {
		GerstnerResult result = new GerstnerResult();
		//float2 dirNormalized = normalize(new Vector3(direction.x / 256, direction.y / 256));	
		Vector2 dirNormalized = direction.normalized;
		float w = 2 / waveLength;
		float Qi = Q / (w * amplitude * 6.28318530718f * 4); // not in paper - multiply with 2pi
		float phi = speed * w;
		float term = w * Vector2.Dot(dirNormalized, new Vector2(posWS.x, posWS.z)) + Time.realtimeSinceStartup * phi;
		float temp = Qi * amplitude * Mathf.Cos(term);
		float x = dirNormalized.x * temp;
		float y = amplitude * Mathf.Sin(term);
		float z = dirNormalized.y * temp;

		result.position = new Vector3(x, y, z);
		return result;
	}
	void OnDrawGizmos() {
		if(! showDebug)
		{
			return;
		}
		float size = 0.05f;
		float offset = (_span / 2)  * numTiles;
		Gizmos.color = Color.white;
		for(int i = 0; i < numTiles; i++) {
			for(int j = 0; j < numTiles; j++) {
				Gizmos.DrawWireCube(calcPoint(new Vector3(i * _span - offset, 0 , j * _span - offset)), new Vector3(size, size, size));
			}
		}
	}

	public float getHeightAt() {
		return 0.0f;
	}
}
