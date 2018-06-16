Shader "Custom/X_WaterTest" 
{
    Properties 
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _Opacity ("Opacity", Range(0.0, 1.0)) = 0.5
        _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
        _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
        _SpecSmoothness ("Smoothness", Range(0.01, 1)) = 0.5
    }
    SubShader 
    {
    
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass 
        {
            Tags {"LightMode" = "ForwardBase"}                      // This Pass tag is important or Unity may not give it the correct light information.
           		CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdbase                       // This line tells Unity to compile this pass for forward base.
                #pragma multi_compile_fog
                
                #include "UnityCG.cginc"
                #include "AutoLight.cginc"
               
               	struct vertex_input
               	{
               		float4 vertex : POSITION;
               		float3 normal : NORMAL;
               		float2 texcoord : TEXCOORD0;
               	};
                
                struct vertex_output
                {
                    float4  pos         : SV_POSITION;
                    float2  uv          : TEXCOORD0;
                    float3  lightDir    : TEXCOORD1;
                    float3  normal		: TEXCOORD2;
                    LIGHTING_COORDS(3,4)                            // Macro to send shadow & attenuation to the vertex shader.
                	float3  vertexLighting : TEXCOORD5;
                	UNITY_FOG_COORDS(6)
                	float3 worldPos : TEXCOORD7;
                	float3 worldNormal : TEXCOORD8;
                };
                
                sampler2D _MainTex;
                float4 _MainTex_ST;
                fixed4 _Color;
                fixed4 _LightColor0; 
                float _SpecSmoothness;
                float4 _SpecColor;
                float  _Opacity;
                
                vertex_output vert (vertex_input v)
                {
                    vertex_output o;
                    o.pos = UnityObjectToClipPos( v.vertex);
                    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
					
					o.lightDir = WorldSpaceLightDir(v.vertex);
					
					o.normal = v.normal;
					o.worldNormal = UnityObjectToWorldNormal(v.normal).xyz;
					o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    
                    TRANSFER_VERTEX_TO_FRAGMENT(o);                 // Macro to send shadow & attenuation to the fragment shader.
                    
                    o.vertexLighting = float3(0.0, 0.0, 0.0);
		            
		            #ifdef VERTEXLIGHT_ON
  					
  					float3 worldN = mul((float3x3)unity_ObjectToWorld, SCALED_NORMAL);
		          	float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
		            
		            for (int index = 0; index < 4; index++)
		            {    
		               float4 lightPosition = float4(unity_4LightPosX0[index], 
		                  unity_4LightPosY0[index], 
		                  unity_4LightPosZ0[index], 1.0);
		 
		               float3 vertexToLightSource = float3(lightPosition.xyz - worldPos.xyz);        
		               
		               float3 lightDirection = normalize(vertexToLightSource);
		               
		               float squaredDistance = dot(vertexToLightSource, vertexToLightSource);
		               
		               float attenuation = 1.0 / (1.0  + unity_4LightAtten0[index] * squaredDistance);
		               
		               float3 diffuseReflection = attenuation * float3(unity_LightColor[index].rgb) 
		                  * float3(_Color.rgb) * max(0.0, dot(worldN, lightDirection));         
		 
		               o.vertexLighting = o.vertexLighting + diffuseReflection;
		            }
		                  
		         
		            #endif

		            UNITY_TRANSFER_FOG(o,o.pos);
                    
                    return o;
                }
                
                fixed4 frag(vertex_output i) : SV_Target
                {
                    i.lightDir = normalize(i.lightDir);
                    i.worldNormal = normalize(i.worldNormal);
                    fixed atten = LIGHT_ATTENUATION(i); // Macro to get you the combined shadow & attenuation value.
                    
                    fixed4 tex = tex2D(_MainTex, i.uv);
                    tex *= _Color + fixed4(i.vertexLighting, 1.0);

                    float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
					//float3 lightDir = _WorldSpaceLightPos0.xyz;
					float3 lightColor = _LightColor0.rgb;
					float3 reflectionDir = reflect(-i.lightDir, i.worldNormal.xyz);
					float3 halfVector = normalize(i.lightDir + viewDir);
					float3 spec = pow(saturate(dot(halfVector, i.worldNormal)), _SpecSmoothness * 100) * _LightColor0.rgb * _SpecColor * atten;

                    fixed diff = saturate(dot(i.worldNormal, i.lightDir));
                                        
                    fixed4 c;
                    //c.rgb = (UNITY_LIGHTMODEL_AMBIENT.rgb * tex.rgb);         // OLD Ambient term. Only do this in Forward Base. It only needs calculating once.
                    c.rgb = ShadeSH9(half4(i.worldNormal, 1)) * tex.rgb;				// New Ambient, works with Skybox and Gradient too (as opposed to only color)
                    c.rgb += (tex.rgb * _LightColor0.rgb * diff) * atten; 		// Diffuse and specular.
                    c.a = tex.a + _LightColor0.a * atten;

                    c.a =  _Opacity;

                    c.rgb += spec;

                    UNITY_APPLY_FOG(i.fogCoord, c);

                    return c;
                }
            ENDCG
        }
 
        Pass {
            Tags {"LightMode" = "ForwardAdd"}                       // Again, this pass tag is important otherwise Unity may not give the correct light information.
            Blend One One                                           // Additively blend this pass with the previous one(s). This pass gets run once per pixel light.
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdadd                        // This line tells Unity to compile this pass for forward add, giving attenuation information for the light.
                #pragma multi_compile_fog
                
                #include "UnityCG.cginc"
                #include "AutoLight.cginc"
                
                struct v2f
                {
                    float4  pos         : SV_POSITION;
                    float2  uv          : TEXCOORD0;
                    float3  lightDir    : TEXCOORD2;
                    float3 normal		: TEXCOORD1;
                    LIGHTING_COORDS(3,4)                            // Macro to send shadow & attenuation to the vertex shader.
                    UNITY_FOG_COORDS(5)
                    float4 worldpos : TEXCOORD6;
                    float3 worldnorm : TEXCOORD7;
                };

                float4 _MainTex_ST;
 
                v2f vert (appdata_tan v)
                {
                    v2f o;
                    
                    o.pos = UnityObjectToClipPos( v.vertex);
                    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                   	
					o.lightDir = WorldSpaceLightDir(v.vertex);

					o.worldpos = mul(unity_ObjectToWorld, v.vertex);
					o.worldnorm = UnityObjectToWorldNormal(v.normal);

					o.normal =  v.normal;
                    TRANSFER_VERTEX_TO_FRAGMENT(o);                 // Macro to send shadow & attenuation to the fragment shader.
                    UNITY_TRANSFER_FOG(o,o.pos);
                    return o;
                }

                sampler2D _MainTex;
                fixed4 _Color;
                float _SpecSmoothness;
                float4 _SpecColor;
                fixed4 _LightColor0; // Colour of the light used in this pass.
                float  _Opacity;
 
                fixed4 frag(v2f i) : SV_Target
                {
                    i.lightDir = normalize(i.lightDir);
                    i.worldnorm = normalize(i.worldnorm);
                    
                    fixed atten = LIGHT_ATTENUATION(i); // Macro to get you the combined shadow & attenuation value.
                    fixed4 tex = tex2D(_MainTex, i.uv);
                    tex *= _Color;
                    float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldpos);

                    float3 spec = atten * _LightColor0.rgb * _SpecColor.rgb * pow(saturate(dot(reflect(-i.lightDir, i.worldnorm), viewDir)), _SpecSmoothness*100);
                    fixed diff = saturate(dot(i.worldnorm, i.lightDir));
                    
                    fixed4 c;
                    c.rgb = (tex.rgb * _LightColor0.rgb * diff) * atten;
                    c.a = tex.a;

                    c.rgb += spec;

                    fixed4 fogC = fixed4(0, 0, 0, 1);
                	UNITY_APPLY_FOG(i.fogCoord, fogC);
                	float fogF = distance(fogC, unity_FogColor) / length(unity_FogColor.rgb);

                    //maybe this will work one day
                    //UNITY_CALC_FOG_FACTOR_RAW(length(_WorldSpaceCameraPos - i.worldpos));
                    //use unityFogFactor instead of fogF

                    float viewDistance = length(_WorldSpaceCameraPos - i.worldpos);
                    c.rgb = lerp(fixed3(0,0,0), c.rgb, saturate(fogF));

                    c.a = _Opacity;

                    c.rgb *= c.a;

                    return c;
                }
            ENDCG
        }
    }
    FallBack "VertexLit"    // Use VertexLit's shadow caster/receiver passes.
}