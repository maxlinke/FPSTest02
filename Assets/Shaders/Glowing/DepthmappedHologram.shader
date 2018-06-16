Shader "Custom/Glowing/Depthmapped Hologram"{

	Properties{
		_MainTex ("Texture", 2D) = "white" {}
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
			fixed4 _Color;
			fixed4 _ColorAdd;
			half _ColorPow;
			half _Extrusion;
			half _ExtrusionPower;

			struct appdata{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f{
				float4 vertex : SV_POSITION;
				float2 uv		: TEXCOORD0;
				float4 worldPos	: TEXCOORD1;
				fixed4 col		: TEXCOORD2;
				UNITY_FOG_COORDS(3)
			};

			v2f vert (appdata v){
				v2f o;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.col = tex2Dlod(_MainTex, float4(o.uv,0,0));
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex + (v.normal * pow(o.col.r, _ExtrusionPower) * _Extrusion));
				UNITY_TRANSFER_FOG(o, o.vertex);

				#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
					#define FOG_ON 1
				#endif

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target{
				fixed4 c = i.col;
				c.rgb = pow(c.rgb, _ColorPow);
				c.rgb += _ColorAdd;
				c.rgb *= _Color;

				#if FOG_ON
					UNITY_CALC_FOG_FACTOR_RAW(length(_WorldSpaceCameraPos - i.worldPos.xyz));
					c.rgb = lerp(fixed3(0,0,0), c.rgb, saturate(unityFogFactor));
				#endif

				return c;
			}

			ENDCG
		}
	}
}
