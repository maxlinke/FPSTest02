
fixed4 _Color;
fixed4 _ColorAdd;
half _ColorPow;
half _Extrusion;
half _ExtrusionPower;

struct appdata{
	float4 vertex : POSITION;
	float4 normal : NORMAL;
	float2 uv : TEXCOORD0;
};

struct v2f{
	float4 vertex : SV_POSITION;
	float2 uv		: TEXCOORD0;
	float4 worldPos	: TEXCOORD1;
	fixed4 col		: TEXCOORD2;
	UNITY_FOG_COORDS(3)
};

v2f vert (appdata v){
	v2f o;
	o.uv = transformUV(v.uv);
	o.col = sampleTexture(o.uv);
	o.worldPos = mul(unity_ObjectToWorld, v.vertex);
	o.vertex = UnityObjectToClipPos(v.vertex + (v.normal * pow(o.col.r, _ExtrusionPower) * _Extrusion));
	UNITY_TRANSFER_FOG(o, o.vertex);
	return o;
}

fixed4 frag (v2f i) : SV_Target{
	fixed4 c = i.col;
	c.rgb = pow(c.rgb, _ColorPow);
	c.rgb += _ColorAdd;
	c.rgb *= _Color;
	UNITY_APPLY_FOG_COLOR(i.fogCoord, c, fixed4(0,0,0,1));
	return c;
}