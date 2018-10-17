Shader "Custom/Glowing/OutputLinearDepth"{

	Properties{
		_FogStrength ("Fog Influence", Range(0,1)) = 0.0
		_Power ("Power", Float) = 1.0
	}

	SubShader{

		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			float _FogStrength;
			float _Power;

			struct appdata{
				float4 vertex : POSITION;
			};

			struct v2f{
				float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				UNITY_FOG_COORDS(1)
			};
			
			v2f vert (appdata v){
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target{
				float d = length(_WorldSpaceCameraPos.xyz - i.worldPos);
				d -= _ProjectionParams.y;
				d /= (_ProjectionParams.z - _ProjectionParams.y);
				d = 1.0 - d;
				d = pow(d, _Power);

				fixed4 fogged = d;
				UNITY_APPLY_FOG(i.fogCoord, fogged);
				fixed4 unfogged = d;
				return lerp(unfogged, fogged, _FogStrength);
			}
			ENDCG
		}
	}
}
