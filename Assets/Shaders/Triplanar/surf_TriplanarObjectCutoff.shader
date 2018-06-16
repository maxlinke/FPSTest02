Shader "Custom/Triplanar/surf_TriplanarObjectCutoff" {

	Properties{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Overlay Texture", 2D) = "white" {}
		_Tiling ("Tiling", Float) = 1.0
		_Cutoff ("Cutoff", Range(0, 1)) = 1.0
	}

	SubShader {

		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert vertex:vert

		sampler2D _MainTex;
		fixed4 _Color;
		fixed4 _OverlayColor;
		float _Tiling;
		float _Cutoff;

		struct Input {
			float2 uv_MainTex;
			float3 coords;
			float3 normal;
		};

		void vert(inout appdata_full v, out Input o){
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.coords = v.vertex * _Tiling;
			o.normal = v.normal;
		}

		void surf (Input IN, inout SurfaceOutput o) {
			half3 blendfactor = abs(IN.normal);
				half3 cubeVector = blendfactor;
				half minim = min(cubeVector.x, min(cubeVector.y, cubeVector.z)) * _Cutoff;
				cubeVector -= half3(minim, minim, minim);
				minim = min( min( max(cubeVector.x, cubeVector.y), max(cubeVector.x, cubeVector.z)), max(cubeVector.y, cubeVector.z)) * _Cutoff;
				cubeVector -= half3(minim, minim, minim);
				cubeVector = normalize(saturate(cubeVector));
				blendfactor = cubeVector;
			blendfactor /= dot(blendfactor, 1.0);

			fixed4 cx = tex2D(_MainTex, IN.coords.yz);
			fixed4 cy = tex2D(_MainTex, IN.coords.xz);
			fixed4 cz = tex2D(_MainTex, IN.coords.xy);
			fixed4 tex = cx * blendfactor.x + cy * blendfactor.y + cz * blendfactor.z;
			tex *= _Color;
			tex.a = 1.0;

			fixed4 c = tex;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}

		ENDCG
	}

	Fallback "Legacy Shaders/VertexLit"
}
