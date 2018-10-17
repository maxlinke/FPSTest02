Shader "Custom/Glowing/Depthmapped Hologram (Single Texture)"{

	Properties{
		_MainTex ("Texture", 2D) = "grey" {}
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

			sampler2D _MainTex;
			float4 _MainTex_ST;

			inline fixed4 sampleTexture(float2 uv){
				return tex2Dlod(_MainTex, float4(uv,0,0));
			}

			inline float2 transformUV(float2 uv){
				return TRANSFORM_TEX(uv, _MainTex);
			}

			#include "DepthmappedHologram.cginc"

			ENDCG
		}
	}
}
