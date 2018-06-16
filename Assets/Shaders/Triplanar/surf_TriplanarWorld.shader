Shader "Custom/Triplanar/surf_TriplanarWorld" {

	Properties{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Overlay Texture", 2D) = "white" {}
		_Tiling ("Tiling", Float) = 1.0
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

		struct Input {
			float2 uv_MainTex;
			float3 worldNormal;
			float3 coords;
		};

		void vert(inout appdata_full v, out Input o){
			UNITY_INITIALIZE_OUTPUT(Input, o);
			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			o.worldNormal = UnityObjectToWorldNormal(v.normal);
			o.coords = worldPos * _Tiling;
		}

		void surf (Input IN, inout SurfaceOutput o) {
			half3 blendfactor = abs(IN.worldNormal);
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
