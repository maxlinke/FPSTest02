Shader "Custom/Glowing/LauchpadEffect" {

	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_TimeScale1 ("UV Scroll Timescale 1", Float) = 1.0
		_TimeScale2 ("UV Scroll Timescale 2", Float) = -1.0
	}

	SubShader {

		ZWrite Off
		Cull Off
		Blend One One

		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 100

		Pass {

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			float _TimeScale1;
			float _TimeScale2;

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				float2 uvOff1 = float2(_Time.y * _TimeScale1, 0.0);
				float2 uvOff2 = float2(_Time.y * _TimeScale2, 0.0);
				fixed4 col = tex2D(_MainTex, i.uv + uvOff1) + tex2D(_MainTex, i.uv + uvOff2);
				col /= 2.0;
				col *= _Color;
				UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0,0,0,1));
				return col;
			}
			ENDCG
		}
	}
}
