// modified version of "VR/SpatialMapping/Wireframe.shader" from Unity 5.5f2
// added colors, discard option, removed stereo support and color by distance

Shader "UnityLibrary/Effects/Wireframe"
{
	Properties
	{
		_LineColor ("LineColor", Color) = (1,1,1,1)
		_FillColor ("FillColor", Color) = (0,0,0,0)
		_WireThickness ("Wire Thickness", RANGE(0, 800)) = 100
		//[MaterialToggle] ("Show Normals", Float) = 1
		_Wavelength ("WaveLength", RANGE(0.1, 30)) = 1
		_Wavespeed ("WaveSpeed",  RANGE(0, 800)) = 1
		_Amplitude ("Amplitude", RANGE(0.1, 1)) = 1
		_Direction ("Direction", Vector) = (1,0,0,0) 
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
			#pragma vertex vert
			#pragma fragment frag
		//	#pragma multi_compile _ USEDISCARD_ON
			#include "UnityCG.cginc"

			float _WireThickness;
			float _Wavelength;
			float _Wavespeed;
			float _Amplitude;
			float4 _Direction;

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
				float frequency = 2 / _Wavelength;
				float phi = _Wavespeed * frequency;
				float4 directionNormalized = normalize(_Direction);
				float term = dot(directionNormalized.xz, posWS.xz) * frequency + _Time.x * phi;

				float temp2 = frequency * _Amplitude * cos(term);
				float normalx = directionNormalized.x * temp2;
				float normalz = directionNormalized.z * temp2;
				
				sinWave o;
				o.pos =  float4( posWS.x,_Amplitude * sin( term), posWS.z, 1);
				o.normals = float4( -normalx, 1.0f, -normalz, 1.0f);
				return o;
			}

			v2g vert (appdata v)
			{
   				float3 posWS = mul(unity_ObjectToWorld, v.vertex);

				// sinWave   
				sinWave sw = calcSinWave(posWS);
				v.vertex.y = sw.pos.y;
	
				v2g o;
				o.projectionSpaceVertex = UnityObjectToClipPos(v.vertex);
				o.worldSpacePosition = float4(posWS, 1);
				// sinWave
				o.normals = sw.normals;
				return o;
			}

			uniform fixed4 _LineColor;
			uniform fixed4 _FillColor;
			uniform fixed4 _Normals;

			//fixed4 frag (g2f i) : SV_Target
			fixed4 frag (v2g i) : SV_Target
			{
				//return float4(i.normals.xyz, 1);
				return float4(1,1,1, 1);
			}
			ENDCG
		}
	}
}