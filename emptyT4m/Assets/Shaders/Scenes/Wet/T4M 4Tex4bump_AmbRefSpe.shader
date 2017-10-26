// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

///Upadat 18.2.11:Add Spe After Baking LM
Shader "TSHD/Scene/Wet/T4M 4Tex4Bump_AmbRefSpe" {
Properties {
    //_AmbScale ("Ambient Color", Range (0.0, 1)) = 0
	_SpecColor ("Specular Color", Color) = (1, 1, 1, 1)
    _NorFactor("Baked Normal Factor",Range(0,1)) =0.0
    _MinShadowInt("Min Shadow Intensity",Range(0,1)) =0.5
    _BakeLight("_BakeLight XYZ:Direction, W:Intensity",Vector) =(0.2,1,0.2,1.5)
	_ShininessL0 ("Layer1 Shininess", Range (0.03, 1)) = 0.2
	[NoScaleOffset]_Splat0 ("Layer 1 (R)", 2D) = "white" {}
	_ShininessL1 ("Layer2 Shininess", Range (0.03, 1)) = 0.2
	[NoScaleOffset]_Splat1 ("Layer 2 (G)", 2D) = "white" {}
	_ShininessL2 ("Layer3 Shininess", Range (0.03, 1)) = 0.2
	[NoScaleOffset]_Splat2 ("Layer 3 (B)", 2D) = "white" {}
	_ShininessL3 ("Layer4 Shininess", Range (0.03, 1)) = 0.2
	[NoScaleOffset]_Splat3 ("Layer 4 (A)", 2D) = "white" {}
    _BumpSplat0 ("Layer1 Normalmap", 2D) = "bump" {}
	_BumpScale0("Bump Scale1",Range(0,10)) = 1.0
	_BumpSplat1 ("Layer2 Normalmap", 2D) = "bump" {}
	_BumpScale1("Bump Scale2",Range(0,10)) = 1.0
	_BumpSplat2 ("Layer3 Normalmap", 2D) = "bump" {}
	_BumpScale2("Bump Scale3",Range(0,10)) = 1.0
	_BumpSplat3 ("Layer4 Normalmap", 2D) = "bump" {}
	_BumpScale3("Bump Scale4",Range(0,10)) = 1.0
	_DisturbTilling("Rain Disturb Tilling",float) =10
    _Tiling1("Layer1 Tiling:xy,Layer2 Tiling:zw", Vector)=(1,1,1,1)
	_Tiling2("Layer3 Tiling:xy,Layer4 Tiling:zw", Vector)=(1,1,1,1)
	_Control ("Control (RGBA)", 2D) = "white" {}
    _RefMap ("Reflection Map(R for Refletion Mask)",2D) = "white" {}
    _WetColor ("Wet Area Color", Color) = (0.2, 0.2,0.2, 1)
    _ReflectVal("Reflect Value",Range(0,1)) = 0.5
    _RefFluseVal("Reflect Distortion",Range(0,1)) =0.8
    [HideInInspector] _ReflectionTex ("Reflection", 2D) = "black" { }

	[HideInInspector]_MainTex ("Never Used", 2D) = "white" {}
} 

CGINCLUDE
    sampler2D _Control,_RefMap,_ReflectionTex,_DisturbMap;
    sampler2D _BumpSplat0, _BumpSplat1, _BumpSplat2, _BumpSplat3;
    sampler2D _Splat0,_Splat1,_Splat2,_Splat3;
    fixed4 _ReflectColor,_WetColor;
    fixed _AmbScale,_ReflectVal,_RefLerp,_RefFluseVal,_DisturbMapFactor,_DisturbTilling;
    fixed _ShininessL0;
    fixed _ShininessL1;
    fixed _ShininessL2;
    fixed _ShininessL3;
    float4 _Tiling2, _Tiling1;
	fixed _BumpScale0, _BumpScale1, _BumpScale2, _BumpScale3;
ENDCG

SubShader {
    Lod 300
	Tags {
		"SplatCount" = "4"
		"Queue" = "Geometry-100"
		"RenderType" = "Opaque"
	}
    CGPROGRAM
    #pragma surface surf BlinnPhong1  vertex:vert exclude_path:deferred  exclude_path:prepass nometa 
    #pragma target 3.0
    #include "UnityCG.cginc"

	#include "T4MAddSpe.cginc"

    struct Input {
        float3 worldRefl;
        half2 viewDirRim;
	    float2 uv_Control : TEXCOORD0;
		float3 worldPos;
		//float2 uv_DisturbMap : TEXCOORD1;
        INTERNAL_DATA
    };

        void vert (inout appdata_full v,out Input o) {
    
            UNITY_INITIALIZE_OUTPUT(Input,o);
            half3 worldNormal = UnityObjectToWorldNormal(v.normal);
            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            half3 worldHalfDir = normalize(UnityWorldSpaceViewDir(o.worldPos)  +  _WorldSpaceLightPos0.xyz  );
            o.viewDirRim.x =saturate(1.4f -saturate(dot(worldHalfDir , worldNormal)) );
            o.viewDirRim.y =1-_RefLerp;
			
            //#if UNITY_COLORSPACE_GAMMA
            //o.viewDirRim.y =GammaToLinearSpace(o.viewDirRim.y );
            //#endif	
        }

        void surf (Input IN, inout SurfaceOutput o) {
	        half4 splat_control = tex2D (_Control, IN.uv_Control);
	        half3 col;
            //half3 orNor =IN.worldNormal;
	        half4 splat0 = tex2D (_Splat0, IN.uv_Control *_Tiling1.xy);
	        half4 splat1 = tex2D (_Splat1, IN.uv_Control *_Tiling1.zw);
	        half4 splat2 = tex2D (_Splat2, IN.uv_Control *_Tiling2.xy);
	        half4 splat3 = tex2D (_Splat3, IN.uv_Control *_Tiling2.zw);

            half refMask =saturate(tex2D(_RefMap,IN.uv_Control).r -IN.viewDirRim.y) ;
            o.Normal =half3(0,0,0);
	        col  = splat_control.r * splat0.rgb;
			//o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_BumpMap),_BumpScale);
            o.Normal += splat_control.r * UnpackScaleNormal(tex2D(_BumpSplat0, IN.uv_Control *_Tiling1.xy),_BumpScale0);
            o.Gloss = splat0.a * splat_control.r ;
	        o.Specular = _ShininessL0 * splat_control.r;

            col += splat_control.g * splat1.rgb;
            o.Normal += splat_control.g * UnpackScaleNormal(tex2D(_BumpSplat1, IN.uv_Control *_Tiling1.zw),_BumpScale1);
	        o.Gloss += splat1.a * splat_control.g;
	        o.Specular += _ShininessL1 * splat_control.g;
	
	        col += splat_control.b * splat2.rgb;
            o.Normal += splat_control.b * UnpackScaleNormal(tex2D(_BumpSplat2, IN.uv_Control *_Tiling2.xy),_BumpScale2);
	        o.Gloss += splat2.a * splat_control.b;
	        o.Specular += _ShininessL2 * splat_control.b;
	
            col += splat_control.a * splat3.rgb;
            o.Normal += splat_control.a * UnpackScaleNormal(tex2D(_BumpSplat3, IN.uv_Control *_Tiling2.zw),_BumpScale3);
            o.Gloss += splat3.a * splat_control.a;
            o.Specular += _ShininessL3 * splat_control.a; 

            // we make normal of wet to be flat
            half3 tempNor =lerp(o.Normal,half3(0,0,1),_RefFluseVal);
            o.Normal =lerp(o.Normal,tempNor,refMask);    

			//float3 worldPos = IN.worldPos;
			//float2 local_103 = float2(worldPos.x + worldPos.y, worldPos.z) * 0.0048;
			//float2 coord1 = float2(0.022, 0.0273) * (frac(_Time.y), frac(_Time.y)) + local_103;
			//float2 coord2 = float2(worldPos.x, worldPos.x + worldPos.y) * 0.00378 - float2(0.033, 0.0184) * frac(_Time.y);
			//float4 bump1 = tex2D(_DisturbMap,IN.uv_Control *_DisturbTilling + coord1);
			//float4 bump2 = tex2D(_DisturbMap,IN.uv_Control *_DisturbTilling + coord2);
			//bump1 = bump1 * 2 - 1;
			//bump2 = bump2 * 2 - 1;

			//half3 disturbMap =UnpackNormal(tex2D(_DisturbMap,IN.uv_Control *_DisturbTilling+ bump1 * bump2));
			half3 disturbMap =UnpackNormal(tex2D(_DisturbMap,IN.uv_Control *_DisturbTilling));
			//o.Normal =lerp(o.Normal,o.Normal *disturbMap,_DisturbMapFactor);   
			half3 disturbNor =lerp(o.Normal,o.Normal *disturbMap,_DisturbMapFactor);   

            //50% darker in wet area
            half darkValue =lerp(0.5,1.0,IN.viewDirRim.y);
            refMask *=_ReflectVal;
            o.Gloss =lerp(o.Gloss,o.Gloss*(darkValue-0.3),refMask);

            col.rgb =lerp(col.rgb,col.rgb *darkValue*_WetColor, refMask);

	        o.Albedo = lerp(col.rgb,col.rgb *UNITY_LIGHTMODEL_AMBIENT,_AmbScale); //mul Amb
	        o.Alpha = 0.0;
            //half3 worldRefl = WorldReflectionVector (IN, o.Normal);
			half3 worldRefl = WorldReflectionVector (IN, disturbNor);
            half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRefl);// use ref probe as reflection source
            half3 reflcol=DecodeHDR(skyData, unity_SpecCube0_HDR);
            refMask *= IN.viewDirRim.x;
            reflcol.rgb *= refMask;
            o.Emission = reflcol.rgb ;
            }
        ENDCG  
        }

    SubShader {
    Lod 200
	Tags {
		"SplatCount" = "4"
		"Queue" = "Geometry-100"
		"RenderType" = "Opaque"
	}
    CGPROGRAM
    #pragma surface surf Lambert  exclude_path:deferred  exclude_path:prepass nometa noforwardadd novertexlights noambient 
    #pragma target 3.0
    #include "UnityCG.cginc"

    struct Input {
	    float2 uv_Control : TEXCOORD0;
    };

    void surf (Input IN, inout SurfaceOutput o) {
	    half4 splat_control = tex2D (_Control, IN.uv_Control);
	    half3 col;
	    half4 splat0 = tex2D (_Splat0, IN.uv_Control *_Tiling1.xy);
	    half4 splat1 = tex2D (_Splat1, IN.uv_Control *_Tiling1.zw);
	    half4 splat2 = tex2D (_Splat2, IN.uv_Control *_Tiling2.xy);
	    half4 splat3 = tex2D (_Splat3, IN.uv_Control *_Tiling2.zw);

	    col  = splat_control.r * splat0.rgb;
        col += splat_control.g * splat1.rgb;
	    col += splat_control.b * splat2.rgb;
        col += splat_control.a * splat3.rgb;

	    o.Albedo = lerp(col.rgb,col.rgb *UNITY_LIGHTMODEL_AMBIENT,_AmbScale); //mul Amb
	    o.Alpha = 0.0;
        }
    ENDCG  
    }

    FallBack "Mobile/VertexLit"
}