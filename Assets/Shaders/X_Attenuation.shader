Shader "Custom/X_Attenuation"
{
	Properties
	{
		_SpecColor ("Specular Color", Color) = (1, 1, 1, 1)
		_SpecSmoothness ("Specular Smoothness", Range(0.01, 1)) = 0.5
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry"}
		LOD 100

		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
				float3 lightDir : TEXCOORD2;
				float3 viewDir : TEXCOORD3;
				UNITY_FOG_COORDS(4)
				LIGHTING_COORDS(5, 6)
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.worldNormal = UnityObjectToWorldNormal(v.normal).xyz;
				o.lightDir = WorldSpaceLightDir(v.vertex);
				o.viewDir = _WorldSpaceCameraPos - o.worldPos;
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				i.worldNormal = normalize(i.worldNormal);
				i.lightDir = normalize(i.lightDir);
				i.viewDir = normalize(i.viewDir);

				fixed4 col = fixed4(0, 0, 0, 1);
				fixed atten = LIGHT_ATTENUATION(i);

				col.rgb = fixed3(atten, atten, atten);

				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
	FallBack "VertexLit"
}
