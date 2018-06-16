Shader "Custom/surf_WorldPosLit" {

	Properties {}

	SubShader {

		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert vertex:vert

		struct Input {
			float3 worldPos;
			float3 worldNormal;
		};

		void vert(inout appdata_full v, out Input o){
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			o.worldNormal = UnityObjectToWorldNormal(v.normal);
		}

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c;
			c.rgb = IN.worldPos.xyz - floor(IN.worldPos.xyz);
			c.a = 1.0;

			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}

		ENDCG
	}

	Fallback "Legacy Shaders/VertexLit"
}
