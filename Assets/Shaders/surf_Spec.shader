Shader "Custom/surf_Spec"{

	Properties{
		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
		_Shininess ("Shininess", Range(0.01, 1)) = 0.078125
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	}

	SubShader{

		Tags{ "RenderType" = "Opaque" }
		LOD 300

		CGPROGRAM
		#pragma surface surf BlinnPhong

		sampler2D _MainTex;
		fixed4 _Color;
		half _Shininess;

		struct Input{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o){
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = tex.rgb * _Color.rgb;
			o.Gloss = tex.a;
			o.Alpha = tex.a * _Color.a;
			o.Specular = _Shininess;
		}

		ENDCG

	}

	Fallback "Legacy Shaders/VertexLit"

}

/*

Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_SpecMaps ("Spec Maps (R = Shinyness, B = Intensity)", 2D) = "gray" {}
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
		_SpecSlider ("Spec Slider", Range(0, 1)) = 1
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 300
	
	CGPROGRAM
	#pragma surface surf BlinnPhong

	sampler2D _MainTex;
	fixed4 _Color;
	sampler2D _SpecMaps;
	float _SpecSlider;

	struct Input {
		float2 uv_MainTex;
		float2 uv_SpecMaps;
	};

	void surf (Input IN, inout SurfaceOutput o) {
		fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
		float4 spec = tex2D(_SpecMaps, IN.uv_SpecMaps);
		o.Albedo = tex.rgb * _Color.rgb;
		o.Gloss = spec.b * _SpecSlider;
		o.Alpha = tex.a * _Color.a;
		//o.Specular = _Shininess;
		o.Specular = clamp(spec.r, 0.01, 1);
	}
		ENDCG
	}
	
	Fallback "Legacy Shaders/VertexLit"

*/
