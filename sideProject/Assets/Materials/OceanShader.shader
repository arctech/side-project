// modified version of "VR/SpatialMapping/Wireframe.shader" from Unity 5.5f2
// added colors, discard option, removed stereo support and color by distance

Shader "OceanShader"
{
	Properties
	{
	//	_LineColor ("LineColor", Color) = (1,1,1,1)
	//	_FillColor ("FillColor", Color) = (0,0,0,0)
		_BaseColor ("BaseColor", Color) = (1,1,1,1)
	//	_WireThickness ("Wire Thickness", RANGE(0, 800)) = 100
		//[MaterialToggle] ("Show Normals", Float) = 1
		//	_k ("K", RANGE(1,10)) = 1
		_Wavelength_1 ("WaveLength_1", RANGE(0.1, 30)) = 1
		_Wavespeed_1 ("WaveSpeed_1",  RANGE(0, 800)) = 1
		_Amplitude_1 ("Amplitude_1", RANGE(0.1, 1)) = 1
		_Direction_1 ("Direction_1", Color) = (1,0,0,0) 
		_q_1 ("Q_1", RANGE(0,1)) = 1
		_Wavelength_2 ("WaveLength_2", RANGE(0.1, 30)) = 1
		_Wavespeed_2 ("WaveSpeed_2",  RANGE(0, 800)) = 1
		_Amplitude_2 ("Amplitude_2", RANGE(0.1, 1)) = 1
		_Direction_2 ("Direction_2", Color) = (1,0,0,0) 
		_q_2 ("Q_2", RANGE(0,1)) = 1
		_Wavelength_3 ("WaveLength_3", RANGE(0.1, 30)) = 1
		_Wavespeed_3 ("WaveSpeed_3",  RANGE(0, 800)) = 1
		_Amplitude_3 ("Amplitude_3", RANGE(0.1, 1)) = 1
		_Direction_3 ("Direction_3", Color) = (1,0,0,0) 
		_q_3 ("Q_3", RANGE(0,1)) = 1
		_Wavelength_4 ("WaveLength_4", RANGE(0.1, 30)) = 1
		_Wavespeed_4 ("WaveSpeed_4",  RANGE(0, 800)) = 1
		_Amplitude_4 ("Amplitude_4", RANGE(0.1, 1)) = 1
		_Direction_4 ("Direction_4", Color) = (1,0,0,0) 
		_q_4 ("Q_4", RANGE(0,1)) = 1
 }

	SubShader
	{
		//Tags { "RenderType"="Opaque" }
		//Tags { "RenderType"="Transparent" }
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }

		Pass
		{
			ZWrite Off
			Blend One One // Additive

			CGPROGRAM
		//	#include "UnityCG.cginc"
 			// Physically based Standard lighting model, and enable shadows on all light types
    	//	#pragma surface surf Standard fullforwardshadows
    		// Use shader model 3.0 target, to get nicer looking lighting
    	//	#pragma target 3.0

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ USEDISCARD_ON
			#include "UnityCG.cginc"

			//float _WireThickness;
			float _Wavelength_1;
			float _Wavespeed_1;
			float _Amplitude_1;
			float4 _Direction_1;
			float _q_1;
			float _Wavelength_2;
			float _Wavespeed_2;
			float _Amplitude_2;
			float4 _Direction_2;
			float _q_2;
			float _Wavelength_3;
			float _Wavespeed_3;
			float _Amplitude_3;
			float4 _Direction_3;
			float _q_3;
			float _Wavelength_4;
			float _Wavespeed_4;
			float _Amplitude_4;
			float4 _Direction_4;
			float _q_4;
			float4 _BaseColor;
			//float _k;
			

			struct appdata
			{
				float4 vertex : POSITION;
				float4 normals : NORMAL;
			};

			struct v2g
			{
				float4 projectionSpaceVertex : SV_POSITION;
				float4 worldSpacePosition : TEXCOORD1;
				float3 normals : NORMAL;
			};


			struct sinWave {
				float4 normals;
				float4 pos; 
				//float 
			};

			sinWave calcSinWave(float3 posWS) {
				/*float frequency = 2 / _Wavelength_1;
				float phi = _Wavespeed * frequency;
				float4 directionNormalized = normalize(_Direction);
				float term = dot(directionNormalized.xz, posWS.xz) * frequency + _Time.x * phi;


				float temp2 = frequency * _Amplitude * cos(term);
				float normalx = directionNormalized.x * temp2;
				float normalz = directionNormalized.z * temp2;
				*/
				sinWave o;
				//o.pos =  float4( posWS.x, 2.0 * _Amplitude * pow((sin( term) + 1) / 2, _k), posWS.z, 1);
				//o.normals = float4( -normalx, 1.0f, -normalz, 1.0f);
				return o;
			}

			sinWave calcGerstnerWave(float3 posWS, float Q, float amplitude, float4 direction, float waveLength, float speed) {
				float2 dirNormalized = normalize(float2(direction.x / 256, direction.y / 256));	
				float w = 2 / waveLength;
				float phi = speed * w;
				float term = w * dot(dirNormalized, posWS.xz) + _Time.x * phi;
				float temp = cos(term);
				float x = Q * amplitude * dirNormalized.x * temp;
				float y = amplitude * sin(term);
				float z = Q * amplitude * dirNormalized.y * temp;
				
				// normals
				float WA = w * amplitude;
				float dir = float3(dirNormalized.x, 0, dirNormalized.y);
				float t = w *  dot(dir, posWS.xyz / 4) + _Time.x * phi;
				float S0 = sin(t);
				float C0 = cos(t);

				sinWave o;
				o.pos =  float4( x, y, z, 1);
				o.normals = float4( dirNormalized.x * WA * C0, Q * WA * S0, dirNormalized.y * WA * C0, 1);
				return o;
			}

			v2g vert (appdata v)
			{
   				float3 posWS = mul(unity_ObjectToWorld, v.vertex);
				sinWave sw1 = calcGerstnerWave(posWS, _q_1, _Amplitude_1, _Direction_1, _Wavelength_1, _Wavespeed_1);
				sinWave sw2 = calcGerstnerWave(posWS, _q_2, _Amplitude_2, _Direction_2, _Wavelength_2, _Wavespeed_2);
				sinWave sw3 = calcGerstnerWave(posWS, _q_3, _Amplitude_3, _Direction_3, _Wavelength_3, _Wavespeed_3);
				sinWave sw4 = calcGerstnerWave(posWS, _q_4, _Amplitude_4, _Direction_4, _Wavelength_4, _Wavespeed_4);
				float4 normals1 = sw1.normals;
				float4 normals2 = sw2.normals;
				float4 normals3 = sw3.normals;
				float4 normals4 = sw4.normals;
				float4 posSum = (sw1.pos + sw2.pos + sw3.pos + sw4.pos) / 4;
				float4 normals = normalize(sw1.normals + sw2.normals + sw3.normals + sw4.normals);

				posSum = (sw1.pos);
				normals = normalize(sw1.normals);
			
				v.vertex = float4(posWS.x + posSum.x, posSum.y, posWS.z + posSum.z, 1);
					


				v2g o;
				o.projectionSpaceVertex = UnityObjectToClipPos(v.vertex);
				o.worldSpacePosition = float4(posWS, 1);
				o.worldSpacePosition = float4(posWS, 1);
				o.normals = float4(-normals.x, 1 - normals.y, -normals.z,1);
				return o;
			}

			uniform fixed4 _LineColor;
			uniform fixed4 _FillColor;
			uniform fixed4 _Normals;

			//fixed4 frag (g2f i) : SV_Target
			fixed4 frag (v2g i) : SV_Target
			{
				return float4(i.normals.xyz, 1);
				//return float4(1, 1, 1, 0.5);
			//	return _BaseColor;
			}
			ENDCG
		}
	}
}