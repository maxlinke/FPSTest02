Shader "Custom/WorldPosLit"{

	Properties{

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

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			fixed4 _LightColor0;

			struct vertex_input{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct vertex_output{
				float4  pos         : SV_POSITION;
                float3  lightDir    : TEXCOORD0;
                float3  vertexLighting : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
                LIGHTING_COORDS(4,5)
                UNITY_FOG_COORDS(6)
			};

			vertex_output vert (vertex_input v){
				vertex_output o;
				o.pos = UnityObjectToClipPos( v.vertex);
				o.lightDir = WorldSpaceLightDir(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal).xyz;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				o.vertexLighting = float3(0.0, 0.0, 0.0);
		            
				#ifdef VERTEXLIGHT_ON
		            	for (int index = 0; index < 4; index++){    
		            		float4 lightPosition = float4(unity_4LightPosX0[index], unity_4LightPosY0[index], unity_4LightPosZ0[index], 1.0);
		               		float3 vertexToLightSource = float3(lightPosition.xyz - o.worldPos.xyz);        
		               		float3 lightDirection = normalize(vertexToLightSource);
		               		float squaredDistance = dot(vertexToLightSource, vertexToLightSource);
		               		float attenuation = 1.0 / (1.0  + unity_4LightAtten0[index] * squaredDistance);
		               		float3 diffuseReflection = attenuation * float3(unity_LightColor[index].rgb) * float3(1,1,1) * max(0.0, dot(o.worldNormal, lightDirection));         
		               		o.vertexLighting = o.vertexLighting + diffuseReflection;
		            	}
				#endif

				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}

			fixed4 frag(vertex_output i) : SV_Target{
				i.lightDir = normalize(i.lightDir);
				i.worldNormal = normalize(i.worldNormal);
				fixed atten = LIGHT_ATTENUATION(i);
				fixed3 lightColor = _LightColor0.rgb;

				fixed4 tex = fixed4(1, 1, 1, 1);
				tex.rgb = i.worldPos.rgb - floor(i.worldPos.xyz);

				fixed diff = saturate(dot(i.worldNormal, i.lightDir));
				fixed3 amb = ShadeSH9(half4(i.worldNormal, 1));

				fixed4 c;
				c.rgb = tex.rgb * (amb + i.vertexLighting + (diff * atten * lightColor));
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

			float4 _MainTex_ST;
			sampler2D _MainTex;
			fixed4 _Color;
			fixed4 _LightColor0;

			struct v2f{
				float4  pos         : SV_POSITION;
				float3  lightDir    : TEXCOORD0;
				float4 worldPos : TEXCOORD1;
				float3 worldNormal : TEXCOORD2;
				LIGHTING_COORDS(3,4)
				UNITY_FOG_COORDS(5)
			};
 
			v2f vert (appdata_tan v){
				v2f o;
				o.pos = UnityObjectToClipPos( v.vertex);
				o.lightDir = WorldSpaceLightDir(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
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

			fixed4 frag(v2f i) : SV_Target{
				i.lightDir = normalize(i.lightDir);
				i.worldNormal = normalize(i.worldNormal);
				fixed atten = LIGHT_ATTENUATION(i);
				fixed3 lightColor = _LightColor0.rgb;

				fixed4 tex = fixed4(1, 1, 1, 1);
				tex.rgb = i.worldPos.rgb - floor(i.worldPos.xyz);

				fixed diff = saturate(dot(i.worldNormal, i.lightDir));

				fixed4 c;
				c.rgb = tex.rgb * diff * atten * lightColor;
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