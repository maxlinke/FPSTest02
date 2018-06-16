//a somewhat improved version of the legacy specular shader
//it doesn't support lightmapping, so this one is only for dynamic objects
//it is based on kyle halladay's "better diffuse" shader (http://kylehalladay.com/blog/tutorial/bestof/2013/10/13/Multi-Light-Diffuse.html)
//some improvements came from reading the tutorials on catlikecoding (http://catlikecoding.com/unity/tutorials/)

//some stuff can be changed
//for example the fancy shadow blending can be removed
// > remove "unityshadowlibrary"-include
// > remove multicompile for handle_shadow_blending_in_gi
// > remove the corresponding part in the fragment shader
//shadesh9 could be moved into the fragment shader

Shader "Custom/SpecularUV"{

	Properties{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
		_SpecHardness ("Specular Hardness", Range(0,1)) = 0.5
		_SpecColor ("Specular Color", Color) = (1,1,1,1)
	}

	SubShader{
		
		Tags {"Queue" = "Geometry" "RenderType" = "Opaque"}

		Pass{

			Tags {"LightMode" = "ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			#pragma multi_compile _ VERTEXLIGHT_ON
			#pragma multi_compile HANDLE_SHADOWS_BLENDING_IN_GI

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "UnityShadowLibrary.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			fixed4 _LightColor0;

			half _SpecHardness;
			fixed4 _SpecColor;

			struct vertex_input{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct vertex_output{
				float4 pos			: SV_POSITION;
                float2 uv			: TEXCOORD0;
                float3 lightDir		: TEXCOORD1;
                float3 amb			: TEXCOORD2;
                float3 worldPos		: TEXCOORD3;
                float3 worldNormal	: TEXCOORD4;
                UNITY_FOG_COORDS(5)
                LIGHTING_COORDS(6,7)
			};

			vertex_output vert (vertex_input v){
				vertex_output o;
				o.pos = UnityObjectToClipPos( v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.lightDir = WorldSpaceLightDir(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal).xyz;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.amb = ShadeSH9(half4(o.worldNormal, 1));

				#if defined(VERTEXLIGHT_ON)								//legacy diffuse has too little vertex light
					float3 vertexLighting = float3(0.0, 0.0, 0.0);
					vertexLighting = Shade4PointLights(
					unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
					unity_LightColor[0].rgb, unity_LightColor[1].rgb,
					unity_LightColor[2].rgb, unity_LightColor[3].rgb,
					unity_4LightAtten0, o.worldPos, o.worldNormal);
					o.amb += vertexLighting;
				#endif

				UNITY_TRANSFER_FOG(o,o.pos);
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				return o;
			}
			                
			fixed4 frag(vertex_output i) : SV_Target{
				i.lightDir = normalize(i.lightDir);
				i.worldNormal = normalize(i.worldNormal);		//legacy diffuse doesn't renormalize normal vector
				fixed atten = LIGHT_ATTENUATION(i);
				fixed3 lightColor = _LightColor0.rgb;

				#if HANDLE_SHADOWS_BLENDING_IN_GI
					float viewZ = dot(_WorldSpaceCameraPos - i.worldPos, UNITY_MATRIX_V[2].xyz);
					float shadowFadeDistance = UnityComputeShadowFadeDistance(i.worldPos, viewZ);
					float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
					atten = saturate(atten + shadowFade);
				#endif

				float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos.xyz);
				float3 halfVector = normalize(i.lightDir + viewDir);
				fixed3 spec = atten * lightColor * _SpecColor * pow(saturate(dot(halfVector, i.worldNormal)), _SpecHardness * 100); 

				fixed4 tex = tex2D(_MainTex, i.uv) * _Color;;
				fixed diff = saturate(dot(i.worldNormal, i.lightDir));
				fixed4 c;
				c.rgb = tex.rgb * (i.amb + (diff * atten * lightColor));
				c.rgb += spec;
				c.a = tex.a;

				UNITY_APPLY_FOG(i.fogCoord, c);
				return c;
			}

			ENDCG
        }
 
        Pass{

			Tags {"LightMode" = "ForwardAdd"}
			Blend One One

            CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdadd
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			fixed4 _LightColor0;

			half _SpecHardness;
			fixed4 _SpecColor;

			struct v2f{
				float4 pos			: SV_POSITION;
				float2 uv			: TEXCOORD0;
				float3 lightDir		: TEXCOORD1;
				float4 worldPos		: TEXCOORD2;
				float3 worldNormal	: TEXCOORD3;
				UNITY_FOG_COORDS(4)
				LIGHTING_COORDS(5,6)
			};
 
			v2f vert (appdata_tan v){
				v2f o;
				o.pos = UnityObjectToClipPos( v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.lightDir = WorldSpaceLightDir(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				UNITY_TRANSFER_FOG(o,o.pos);
				TRANSFER_VERTEX_TO_FRAGMENT(o);

				#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
					#define FOG_ON 1
				#endif

				return o;
			}

			fixed4 frag(v2f i) : SV_Target{
				i.lightDir = normalize(i.lightDir);
				i.worldNormal = normalize(i.worldNormal);		//legacy diffuse doesn't renormalize normal vector
				fixed atten = LIGHT_ATTENUATION(i);
				fixed3 lightColor = _LightColor0.rgb;

				float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos.xyz);
				float3 halfVector = normalize(i.lightDir + viewDir);
				fixed3 spec = atten * lightColor * _SpecColor * pow(saturate(dot(halfVector, i.worldNormal)), _SpecHardness * 100); 

				fixed4 tex = tex2D(_MainTex, i.uv) * _Color;
				fixed diff = saturate(dot(i.worldNormal, i.lightDir));
				fixed4 c;
				c.rgb = tex.rgb * diff * atten * lightColor;
				c.rgb += spec;
				c.a = tex.a;

				#if FOG_ON
					UNITY_CALC_FOG_FACTOR_RAW(length(_WorldSpaceCameraPos - i.worldPos.xyz));
					c.rgb = lerp(fixed3(0,0,0), c.rgb, saturate(unityFogFactor));
				#endif

				return c;
			}

            ENDCG
        }
    }
    FallBack "VertexLit"
}
