Shader "Custom/Glowing/WorldPos"{
	
	SubShader{

		Tags { "Queue"="Geometry" "RenderType"="Opaque" }

		Pass{
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata{
				float4 vertex : POSITION;
			};

			struct v2f{
				float4 pos : SV_POSITION;
				float3 worldpos : TEXCOORD0;
				UNITY_FOG_COORDS(1)
			};
			
			v2f vert (appdata v){
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldpos = mul(unity_ObjectToWorld, v.vertex).xyz;
				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target{
				fixed4 col = fixed4(frac(i.worldpos.xyz), 1);
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}

	Fallback "VertexLit"
}
