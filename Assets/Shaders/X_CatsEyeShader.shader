Shader "Custom/X_CatsEyeShader"{

	Properties{
		_SpecColor ("Specular Color", Color) = (1, 1, 1, 1)
		_SpecSmoothness ("Specular Smoothness", Range(0.01, 1)) = 0.5
	}

	SubShader{

		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
		LOD 100

		Pass{

			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			fixed4 _LightColor0;
			fixed3 _SpecColor;
			float _SpecSmoothness;

			struct appdata{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f{
				float4 pos : SV_POSITION;
				float4 worldPos : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
				float3 lightDir : TEXCOORD2;
				float3 viewDir : TEXCOORD3;
				UNITY_FOG_COORDS(4)
				LIGHTING_COORDS(5, 6)
			};
			
			v2f vert (appdata v){
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal).xyz;
				o.lightDir = WorldSpaceLightDir(v.vertex);
				o.viewDir = _WorldSpaceCameraPos - o.worldPos;
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target{
				i.worldNormal = normalize(i.worldNormal);
				i.lightDir = normalize(i.lightDir);
				i.viewDir = normalize(i.viewDir);

				fixed4 col = fixed4(0, 0, 0, 1);
				fixed atten = LIGHT_ATTENUATION(i);

				fixed3 lightColor = _LightColor0.rgb;
				float3 spec = pow(saturate(dot(i.lightDir, i.viewDir)), _SpecSmoothness * 100);

				col.rgb += spec * atten * _SpecColor * lightColor;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}

			ENDCG
		}

		Pass{

			Tags { "LightMode" = "ForwardAdd" }
			Blend One One
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdadd
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			fixed4 _LightColor0;
			fixed3 _SpecColor;
			float _SpecSmoothness;

			struct appdata{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f{
				float4 pos : SV_POSITION;
				float4 worldPos : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
				float3 lightDir : TEXCOORD2;
				float3 viewDir : TEXCOORD3;
				UNITY_FOG_COORDS(4)
				LIGHTING_COORDS(5, 6)
			};

			v2f vert (appdata v){
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal).xyz;
				o.lightDir = WorldSpaceLightDir(v.vertex);
				o.viewDir = _WorldSpaceCameraPos - o.worldPos;
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				UNITY_TRANSFER_FOG(o,o.pos);

				#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
					#if !defined(FOG_DISTANCE)
						#define FOG_DEPTH 1
					#endif
					#define FOG_ON 1
				#endif

				return o;
			}

			fixed4 frag (v2f i) : SV_Target{
				i.worldNormal = normalize(i.worldNormal);
				i.lightDir = normalize(i.lightDir);
				i.viewDir = normalize(i.viewDir);

				fixed4 col = fixed4(0, 0, 0, 1);
				fixed atten = LIGHT_ATTENUATION(i);

				fixed3 lightColor = _LightColor0.rgb;
				float3 spec = pow(saturate(dot(i.lightDir, i.viewDir)), _SpecSmoothness * 100);

				col.rgb += spec * atten * _SpecColor * lightColor;

				#if FOG_ON
					UNITY_CALC_FOG_FACTOR_RAW(length(_WorldSpaceCameraPos - i.worldPos.xyz));
					c.rgb = lerp(fixed3(0,0,0), c.rgb, saturate(unityFogFactor));
				#endif

				return col;
			}

			ENDCG
		}
	}
	FallBack "VertexLit"
}
