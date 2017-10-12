Shader "Hidden/TerrainEngine/Splatmap/SpecularWetArea-AddPass" {
	Properties {
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
        _MinShadowInt("Min Shadow Intensity",Range(0,1)) =0.5
        _BakeLight("_BakeLight XYZ:Direction,W:Intensity",Vector) =(0.2,1,0.2,1.5)
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
	}

	SubShader {
		Tags {
			"Queue" = "Geometry-99"
			"IgnoreProjector"="True"
			"RenderType" = "Opaque"
		}

		CGPROGRAM
		#pragma surface surf BlinnPhong1 decal:add vertex:SplatmapVertWet finalcolor:SplatmapFinalColor finalprepass:SplatmapFinalPrepass finalgbuffer:SplatmapFinalGBuffer
		#pragma multi_compile_fog
		#pragma multi_compile __ _TERRAIN_NORMAL_MAP
		#pragma target 3.0
		// needs more than 8 texcoords
        //#pragma exclude_renderers gles

		#define TERRAIN_SPLAT_ADDPASS
        #include "UnityStandardUtils.cginc"
		#include "TerrainSplatmapWetArea.cginc"

		half _Shininess,_AmbScale;
        fixed4 _ReflectColor,_WetColor;

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

            //50% darker in wet area
            half darkValue =lerp(0.5,1.0,IN.viewDirRim.y);
            refMask *=_ReflectVal;
            o.Gloss =lerp(o.Gloss,o.Gloss*(darkValue-0.3),refMask);

            col.rgb =lerp(col.rgb,col.rgb *darkValue*_WetColor, refMask);
            //o.Albedo =col.rgb ;
            o.Albedo = lerp(col.rgb,col.rgb *UNITY_LIGHTMODEL_AMBIENT,_AmbScale); //mul Amb

            half3 worldRefl = WorldReflectionVector (IN, o.Normal);
            half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRefl);// use ref probe as reflection source
            half3 reflcol=DecodeHDR(skyData, unity_SpecCube0_HDR);
            refMask *= IN.viewDirRim.x;
            reflcol.rgb *= refMask;
            o.Emission = reflcol.rgb ;
		}
		ENDCG
	}

	Fallback "Hidden/TerrainEngine/Splatmap/Diffuse-AddPass"
}
