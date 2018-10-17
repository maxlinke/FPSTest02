Shader "Custom/Glowing/SimpleHologram"{

	Properties{
		_Color ("Color", Color) = (0,0,1,1)
		_Intensity ("Intensity", float) = 1
		_Sharpness ("Sharpness", Range(0,1)) = 1
		_Minimum ("Minimum", Range(0,1)) = 0
		_LineCount ("Lines per Unit", float) = 1
		_LowerBound ("Line Black Level", Range(0,1)) = 0.0
		_UpperBound ("Line White Level", Range(0,1)) = 1.0
		_Speed ("Line Speed", float) = 60
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
			float _LineCount;
			float _Speed;
			float _LowerBound;
			float _UpperBound;

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
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target{
				float base = _Intensity * saturate(1-(i.viewDot / _Sharpness));
				float t = _Time.y * _Speed;
				float p = i.worldPos.y;
				float lines = abs(sin(UNITY_PI * (p + t) * _LineCount));
				float v = base * smoothstep(_LowerBound, _UpperBound, lines);
				v = (v + _Minimum) / (1.0 + _Minimum);
				fixed4 col =  _Color * v;
				UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0,0,0,1));
				return col;
			}
			ENDCG
		}
	}
}
