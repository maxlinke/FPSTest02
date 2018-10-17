Shader "Custom/Glowing/SimpleGlow"{

	Properties{
		_Color ("Color", Color) = (0,0,1,1)
		_Intensity ("Intensity", float) = 1
		_Sharpness ("Sharpness", Range(0,1)) = 1
		_Minimum ("Minimum", Range(0,1)) = 0
	}

	SubShader{

		Tags { "RenderType"="Transparent" "Queue"="Transparent"}

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

			struct appdata{
				float4 vertex : POSITION;
				float3 normal : NORMAL; 
			};

			struct v2f{
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
				float3 viewDir : TEXCOORD2;
				UNITY_FOG_COORDS(3)
			};

			v2f vert (appdata v){
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.worldNormal = UnityObjectToWorldNormal(v.normal).xyz;
				o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target{
				half viewDot = abs(dot(normalize(i.viewDir), normalize(i.worldNormal)));
				half v = saturate(1-(viewDot / _Sharpness));
				v = (v + _Minimum) / (1.0 + _Minimum);
				v *= _Intensity;
				fixed4 col = _Color * v;
				UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0,0,0,1));
				return col;
			}

			ENDCG
		}
	}
}
