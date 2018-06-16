Shader "Custom/surf_DiffEmissiveUV" {

	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_EmissionTex ("Emissive Color and Strenth", 2D) = "black" {}
	}

	SubShader {

		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		sampler2D _EmissionTex;
		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
			float2 uv_EmissionTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			o.Emission = tex2D(_EmissionTex, IN.uv_EmissionTex);
		}

		ENDCG
	}

	Fallback "Legacy Shaders/VertexLit"
}
