Shader "Custom/WaterShader" {
	Properties {
		_AppTime ("AppTime", Float) = 1
		_SkyColor ("SkyColor", Color) = (1,1,1,1)

		

		_OceanColor ("OceanColor", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}


		[MaterialToggle] ShowNormals("Show Normals", Float) = 1

		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Wavelength_1 ("WaveLength_1", RANGE(0.1, 30)) = 1
		_Wavespeed_1 ("WaveSpeed_1",  RANGE(0, 800)) = 1
		_Amplitude_1 ("Amplitude_1", RANGE(0.1, 5)) = 1
		_Direction_1 ("Direction_1", Color) = (1,0,0,0) 
		_q_1 ("Q_1", RANGE(0,1)) = 1
		_Wavelength_2 ("WaveLength_2", RANGE(0.1, 30)) = 1
		_Wavespeed_2 ("WaveSpeed_2",  RANGE(0, 800)) = 1
		_Amplitude_2 ("Amplitude_2", RANGE(0.1, 5)) = 1
		_Direction_2 ("Direction_2", Color) = (1,0,0,0) 
		_q_2 ("Q_2", RANGE(0,1)) = 1
		_Wavelength_3 ("WaveLength_3", RANGE(0.1, 30)) = 1
		_Wavespeed_3 ("WaveSpeed_3",  RANGE(0, 800)) = 1
		_Amplitude_3 ("Amplitude_3", RANGE(0.1, 5)) = 1
		_Direction_3 ("Direction_3", Color) = (1,0,0,0) 
		_q_3 ("Q_3", RANGE(0,1)) = 1
		_Wavelength_4 ("WaveLength_4", RANGE(0.1, 30)) = 1
		_Wavespeed_4 ("WaveSpeed_4",  RANGE(0, 800)) = 1
		_Amplitude_4 ("Amplitude_4", RANGE(0.1, 5)) = 1
		_Direction_4 ("Direction_4", Color) = (1,0,0,0) 
		_q_4 ("Q_4", RANGE(0,1)) = 1
	}
	SubShader {
		//Tags { "RenderType"="Opaque" }
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }

		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		#pragma multi_compile _ SHOWNORMALS_ON
		#include "UnityCG.cginc"

		float4 _SkyColor;
		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
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
		float _AppTime;
		float4 _OceanColor;

		struct v2g
		{
			float4 projectionSpaceVertex : SV_POSITION;
			float4 worldSpacePosition : TEXCOORD1;
			float3 normal : NORMAL;
		};


		struct sinWave {
				float4 normals;
				float4 pos; 
		};

		sinWave calcGerstnerWave(float3 posWS, float Q, float amplitude, float4 direction, float waveLength, float speed) {
				float2 dirNormalized = normalize(float2(direction.x / 256, direction.y / 256));	
				float w = 2 / waveLength;
				float Qi = Q / (w * amplitude);
			//	float Qi = Q / (w * amplitude * 6.28318530718 * 4); // not in paper - multiply with 2pi
				float phi = speed * w;
				//float term = w * dot(dirNormalized, posWS.xz) + _Time.x * phi;
				float term = w * dot(dirNormalized, posWS.xz) + _AppTime * phi;
				float temp = Qi * amplitude * cos(term);
				float x = dirNormalized.x * temp;
				float y = amplitude * sin(term);
				float z = dirNormalized.y * temp;
				
				// normals
				float WA = w * amplitude;
				float dir = float3(dirNormalized.x, 0, dirNormalized.y);
				float t = w *  dot(dir, posWS.xyz / 4) + _Time.x * phi;
				float S0 = sin(t);
				float C0 = cos(t);

				sinWave o;
				o.pos =  float4( x, y, z, 1);
				o.normals = float4( dirNormalized.x * WA * C0, Qi * WA * S0, dirNormalized.y * WA * C0, 1);
				return o;
			}

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			//o.Albedo = c.rgb;
			o.Albedo = _OceanColor;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
