Shader "Custom/Glowing/Depthmapped Hologram (Texture Array)"{

	Properties{
		[HideInInspector] _TexArray ("Textures", 2DArray) = "" {}
		[HideInInspector] _Number ("Number of Textures", Float) = 1.0
		_Speed ("Speed", Float) = 1.0
		_Color ("Color", Color) = (1,1,1,1)
		_ColorAdd ("Color Addition", Color) = (0,0,0,0)
		_ColorPow ("Color Power", Range(0,10)) = 1.0
		_Extrusion ("Extrusion", Range(0,1)) = 1.0
		_ExtrusionPower ("Extrusion Power", Range(0,10)) = 1.0
	}

	SubShader{

		Tags { "RenderType"="Opaque" "Queue" = "Transparent" }
		LOD 100

		Blend One One
		Cull Off

		Pass{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			UNITY_DECLARE_TEX2DARRAY(_TexArray);
			float _Number;
			float _Speed;

			inline fixed4 sampleTexture(float2 uv){
				float t = _Time.y * _Speed;
				t = fmod(t, _Number) - 0.5;
				return UNITY_SAMPLE_TEX2DARRAY_LOD(_TexArray, float3(uv, t), 0);
			}

			inline float2 transformUV(float2 uv){
				return uv;
			}

			#include "DepthmappedHologram.cginc"

			ENDCG
		}
	}
}
