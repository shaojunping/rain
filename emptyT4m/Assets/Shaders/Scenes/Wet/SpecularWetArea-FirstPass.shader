Shader "Nature/Terrain/SpecularWetArea" {
	Properties {
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
        _NorFactor("Baked Normal Factor",Range(0,1)) =0.0
        _MinShadowInt("Min Shadow Intensity",Range(0,1)) =0.5
        _BakeLight("_BakeLight XYZ:Direction,W:Intensity",Vector) =(0.2,1,0.2,1.2)
        _RefMap ("Reflection Map(R for Refletion Mask)",2D) = "white" {}
        _WetColor ("Wet Area Color", Color) = (0.2, 0.2,0.2, 1)
        _ReflectVal("Reflect Value",Range(0,1)) = 0.5
        _RefFluseVal("Reflect Distortion",Range(0,1)) =0.8
        //_Splat1_ST("Layer1", Vector)=(1,1,0,0)
        //_Splat2_ST("Layer2", Vector)=(1,1,0,0)
        //_Splat3_ST("Layer2", Vector)=(1,1,0,0)
        //_Splat4_ST("Layer2", Vector)=(1,1,0,0)
		// set by terrain engine
		[HideInInspector] _Control ("Control (RGBA)", 2D) = "red" {}
		[HideInInspector] _Splat3 ("Layer 3 (A)", 2D) = "white" {}
		[HideInInspector] _Splat2 ("Layer 2 (B)", 2D) = "white" {}
		[HideInInspector] _Splat1 ("Layer 1 (G)", 2D) = "white" {}
		[HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "white" {}
		[HideInInspector] _Normal3 ("Normal 3 (A)", 2D) = "bump" {}
		[HideInInspector] _Normal2 ("Normal 2 (B)", 2D) = "bump" {}
		[HideInInspector] _Normal1 ("Normal 1 (G)", 2D) = "bump" {}
		[HideInInspector] _Normal0 ("Normal 0 (R)", 2D) = "bump" {}
		// used in fallback on old cards & base map
		[HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
		[HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)
        
        _FogVal ("Fog Density",Range(0,1)) =1.0
        _FogHeiDen("Height Fog Density",Range(0,1)) =0.0
        //_FogHeiScale("Height Fog Adjustment",Float) =0.0

		_DisturbTilling("Rain Disturb Tilling",float) =10
		_DisturbMap1 ("Disturb Map", 2D) = "white" {}
		_WaveXSpeed ("Wave Horizontal Speed", Range(-0.1, 0.1)) = 0.01
		_WaveYSpeed ("Wave Vertical Speed", Range(-0.1, 0.1)) = 0.01
	}

	SubShader {
		Tags {
			"Queue" = "Geometry-100"
			"RenderType" = "Opaque"
		}

		CGPROGRAM
		#pragma surface surf BlinnPhong1 vertex:SplatmapVertWet finalcolor:SplatmapFinalColor finalprepass:SplatmapFinalPrepass finalgbuffer:SplatmapFinalGBuffer 
		#pragma multi_compile_fog
		#pragma multi_compile __ _TERRAIN_NORMAL_MAP
		#pragma target 3.0
        //#include "UnityCG.cginc"
        
		#include "TerrainSplatmapWetArea.cginc"

		sampler2D _DisturbMap1;
		half _Shininess,_AmbScale, _DisturbTilling;
        fixed4 _ReflectColor,_WetColor;
		fixed _DisturbMapFactor;
		fixed _WaveXSpeed;
		fixed _WaveYSpeed;
        
		void surf(Input IN, inout SurfaceOutput o)
		{
			half4 splat_control;
			half weight;
			fixed4 mixedDiffuse;
			SplatmapMix(IN, splat_control, weight, mixedDiffuse, o.Normal);
            o.Albedo = mixedDiffuse.rgb;
            half4 col;
            col.rgb =mixedDiffuse.rgb;

			o.Alpha = weight;
			o.Gloss = mixedDiffuse.a;
			o.Specular = _Shininess;
            
            //half refMask =saturate(tex2D(_RefMap,IN.tc_Control).r -IN.viewDirRim.y) ;
            half refMask =saturate(tex2D(_RefMap,IN.ref_Control).r -IN.viewDirRim.y) ;
            // we make normal of wet to be flat
            half3 tempNor =lerp(o.Normal,half3(0,0,1),_RefFluseVal);
            o.Normal =lerp(o.Normal,tempNor,refMask);    

			float2 speed = _Time.y * float2(_WaveXSpeed, _WaveYSpeed);
			half3 disturbMap1 =UnpackNormal(tex2D(_DisturbMap1, IN.disturb_Control *_DisturbTilling + speed)).rgb;
			half3 disturbMap2 =UnpackNormal(tex2D(_DisturbMap1, IN.disturb_Control *_DisturbTilling - speed)).rgb;
			half3 disturbMap = normalize(disturbMap1 + disturbMap2);

			half3 disturbNor =lerp(o.Normal,o.Normal * disturbMap, _DisturbMapFactor);

            //50% darker in wet area
            half darkValue =lerp(0.5,1.0,IN.viewDirRim.y);
            refMask *=_ReflectVal;
            o.Gloss =lerp(o.Gloss,o.Gloss*(darkValue-0.3),refMask);

            col.rgb =lerp(col.rgb,col.rgb *darkValue*_WetColor, refMask);
            
            //o.Albedo =col.rgb ;
            o.Albedo = lerp(col.rgb,col.rgb *UNITY_LIGHTMODEL_AMBIENT,_AmbScale); //mul Amb

            half3 worldRefl = WorldReflectionVector (IN, disturbNor);
            half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRefl);// use ref probe as reflection source
            half3 reflcol=DecodeHDR(skyData, unity_SpecCube0_HDR);
            refMask *= IN.viewDirRim.x;
            reflcol.rgb *= refMask;
            o.Emission = reflcol.rgb ;
		}
		ENDCG
	}

	Dependency "AddPassShader" = "Hidden/TerrainEngine/Splatmap/SpecularWetArea-AddPass"
	Dependency "BaseMapShader" = "Hidden/TerrainEngine/Splatmap/SpecularWet-Base"

    //Fallback "Nature/Terrain/Diffuse"
    FallBack "Mobile/VertexLit"
}
