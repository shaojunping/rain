Shader "TSHD/Ambient/Cutout/AmbientDiffuseFog" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
	_AmbScale ("Ambient Scale", Range (0.0, 1)) = 0.5
	_FogVal ("Fog Density",Range(0,1)) = 1.0
    _FogHeiDen("Height Fog Density",Range(0,1)) = 0.0
    _FogHeiScale("Height Fog Adjustment",Float) = 0.0
}

SubShader {
	Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
	LOD 200
	
CGPROGRAM
#pragma surface surf Lambert vertex:vert alphatest:_Cutoff finalcolor:fogColor

sampler2D _MainTex;
fixed4 _Color;
half  _AmbScale;
fixed _FogVal, _FogHeiDen, _FogHeiParaZ, _FogHeiParaW, _FogHeiScale;

struct Input {
	float2 uv_MainTex;
	float2 fogCoord : TEXCOORD1;
};

void vert (inout appdata_full v,out Input o) {
    UNITY_INITIALIZE_OUTPUT(Input,o);
	float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
	float4 pos = mul (UNITY_MATRIX_MVP, v.vertex);
    #if defined(FOG_LINEAR)
	// factor = (end-z)/(end-start) = z * (-1/(end-start)) + (end/(end-start))
		float unityFogFactor = (pos.z) * unity_FogParams.z + unity_FogParams.w;
	#elif defined(FOG_EXP)
		// factor = exp(-density*z)
			float unityFogFactor = unity_FogParams.y * (pos.z); 
			unityFogFactor = exp2(-unityFogFactor);
	#elif defined(FOG_EXP2)
		// factor = exp(-(density*z)^2)
		float unityFogFactor = unity_FogParams.x * (pos.z);
		unityFogFactor = exp2(-unityFogFactor*unityFogFactor);
	#else
		float unityFogFactor = 1.0;
	#endif
//data.fogCoord .x = unityFogFactor;
	o.fogCoord.x = saturate(unityFogFactor);
	o.fogCoord.y =saturate((worldPos.y +_FogHeiScale)  * _FogHeiParaZ + _FogHeiParaW);
}

void fogColor(Input IN, SurfaceOutput o, inout fixed4 color)
{
    #ifdef UNITY_PASS_FORWARDADD
        color.rgb = lerp(fixed3(0,0,0), (color).rgb, saturate(IN.fogCoord.x));
    #else
		fixed3 FarFog = lerp(unity_FogColor, color.rgb, IN.fogCoord.x);//Far Fog
		fixed3 HeiFog =lerp(unity_FogColor,FarFog,IN.fogCoord.y); //Height Fog
		fixed3 tempC =lerp(FarFog,HeiFog,_FogHeiDen);//Height Fog Density
		color.rgb = lerp(color.rgb,tempC,_FogVal);
    #endif
}

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	c.rgb =lerp(c.rgb,c.rgb *UNITY_LIGHTMODEL_AMBIENT,_AmbScale);
	o.Albedo = c.rgb;
	o.Alpha = c.a;
}
ENDCG
}

Fallback "Legacy Shaders/Transparent/Cutout/VertexLit"
}
