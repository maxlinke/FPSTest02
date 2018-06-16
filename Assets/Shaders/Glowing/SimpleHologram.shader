Shader "Custom/Glowing/SimpleHologram"{

	Properties{
		_Color ("Color", Color) = (0,0,1,1)
		_Intensity ("Intensity", float) = 1
		_Sharpness ("Sharpness", Range(0,1)) = 1
		_Minimum ("Minimum", Range(0,1)) = 0
		_LineHeight ("Line Height", float) = 1
		_Speed ("Speed", float) = 60
	}

	SubShader{

		Tags { "RenderType"="Opaque" "Queue"="Transparent"}

		Pass{

			ZWrite Off
			Blend One One
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			float4 _Color;
			float _Intensity;
			float _Sharpness;
			float _Minimum;
			float _LineHeight;
			float _Speed;

			struct appdata{
				float4 vertex : POSITION;
				float3 normal : NORMAL; 
			};

			struct v2f{
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float viewDot : TEXCOORD1;
				UNITY_FOG_COORDS(2)
			};

			v2f vert (appdata v){
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				float3 worldNormal = UnityObjectToWorldNormal(v.normal);
				float3 viewDir = normalize(_WorldSpaceCameraPos - o.worldPos);
				o.viewDot = abs(dot(viewDir, worldNormal));
				UNITY_TRANSFER_FOG(o,o.vertex);

				#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
					#if !defined(FOG_DISTANCE)
						#define FOG_DEPTH 1
					#endif
					#define FOG_ON 1
				#endif

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target{
				float base = _Intensity * saturate(1-(i.viewDot / _Sharpness));
				float lines = abs(sin(_LineHeight * (i.worldPos.y + _Time.x * _Speed)));
				fixed4 col =  _Color * (_Minimum + (base * lines));

				#if FOG_ON
					UNITY_CALC_FOG_FACTOR_RAW(length(_WorldSpaceCameraPos - i.worldPos.xyz));
					col.rgb = lerp(fixed3(0,0,0), col.rgb, saturate(unityFogFactor));
				#endif

				return col;
			}
			ENDCG
		}
	}
}
