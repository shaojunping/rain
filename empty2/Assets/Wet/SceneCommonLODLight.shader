// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

///LOD 300 Bump + Re +ForwardAdd +Amb
///LOD 200 Re + Amb
///Upadat 18.2.11:Add Spe After Baking LM
Shader "TSHD/Scene/Wet/SceneCommonLODLight"
{
	Properties
	{
        //_AmbScale ("Ambient Color", Range (0.0, 1)) = 0
		_MainTex ("Main Texture", 2D) = "white" {}
		_MainColor ("Main Color", Color) = (1.0, 1.0, 1.0, 1)
		_MainColorScale ("Main Color Scale", Range (0.0, 1)) = 0
        _NorFactor("Baked Normal Factor",Range(0,1)) =0.0
        _MinShadowInt("Min Shadow Intensity",Range(0,1)) =0.5
        _BakeLight("_BakeLight XYZ:Direction,W:Intensity",Vector) =(0.2,1,0.2,1.5)
        _BumpScale("Bump Scale", Float) = 1.0
        _BumpMap ("Normalmap", 2D) = "bump" {}
        [NoScaleOffset]_SpecularMap ("SpecularMap(RGB for Specular,A for Refletion Mask)",2D) = "white" {}
        _SpecColor ("Specular Color", Color) = (1.0, 1.0, 1.0, 1)
        _Shininess ("Shininess", Range (0.01, 1)) = 0.078125
        _ReflectVal("Reflect Value",Range(0,1)) = 0.2
        _RefFluseVal("Reflect Distortion",Range(0,1)) = 0.8
        _WetColor ("Wet Area Color", Color) = (0.2, 0.2,0.2, 1)
        _FogVal ("Fog Density",Range(0,1)) = 1.0
        _FogHeiDen("Height Fog Density",Range(0,1)) = 0.0
        _FogHeiScale("Height Fog Adjustment",Float) = 0.0
        _DisturbTilling("Rain Disturb Tilling",float) = 10
		_OwnRainNormalFactor ("object's own disturb factor",Range(0,1)) =1.0
        [HideInInspector] _ReflectionTex ("Reflection", 2D) = "black" { }
	}
    SubShader
    {
        Tags { "RenderType"="Opaque" 		"Queue" = "Geometry"}
        LOD 300

		CGPROGRAM
        #pragma surface surf BlinnPhong1  vertex:vert finalcolor:fogColor exclude_path:deferred  exclude_path:prepass nometa  noforwardadd 
        #pragma target 3.0
        #pragma multi_compile_fog

        #include "T4MAddSpe.cginc"

        sampler2D _MainTex;
        sampler2D _BumpMap;
		sampler2D _DisturbMap;
        sampler2D _SpecularMap;

        fixed _BumpScale,_AmbScale,_RefFluseVal,_RefLerp, _MainColorScale, _DisturbMapFactor, _OwnRainNormalFactor;
        fixed4 _Color,_WetColor, _MainColor;
        fixed4 _ReflectColor;
        half _Shininess, _DisturbTilling;
        float _ReflectVal;
        fixed _FogVal,_FogHeiDen,_FogHeiParaZ,_FogHeiParaW,_FogHeiScale;

        struct Input {
	        float2 uv_MainTex;
	        float2 uv_BumpMap;
	        float3 worldRefl;
            half2 viewDirRim;
	        INTERNAL_DATA
            float2 fogCoord : TEXCOORD5;
        };
        
        
        void vert (inout appdata_full v,out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            half3 worldNormal = UnityObjectToWorldNormal(v.normal);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            half3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos)  +  _WorldSpaceLightPos0.xyz  );
            o.viewDirRim.x = saturate(1.4f -saturate(dot(worldViewDir , worldNormal)) );
            o.viewDirRim.y =1 - _RefLerp;
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
			o.fogCoord.y =saturate((worldPos.y +_FogHeiScale)  *_FogHeiParaZ +_FogHeiParaW);
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
	        half4 c  = tex2D(_MainTex, IN.uv_MainTex);
            half4 speCol =tex2D(_SpecularMap,IN.uv_MainTex);
            _SpecColor.rgb *=speCol.rgb;

            o.Gloss = _SpecColor.a;
	        o.Specular = _Shininess;

            o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_BumpMap),_BumpScale);

			half3 disturbMap = tex2D(_DisturbMap, IN.uv_BumpMap * _DisturbTilling);
			half3 disturbNor =lerp(o.Normal,o.Normal * disturbMap, _DisturbMapFactor * _OwnRainNormalFactor);

			//o.Normal = half3(1.0, 1.0, 1.0);
            half refMask =saturate( speCol.a -IN.viewDirRim.y) ;
            //// we make normal of wet to be flat
            half3 tempNor =lerp(o.Normal,half3(0,0,1),_RefFluseVal);
            o.Normal =lerp(o.Normal,tempNor,refMask);    

             //50% darker in wet area
            half darkValue =lerp(0.5,1.0,IN.viewDirRim.y);
            refMask *= _ReflectVal;
            o.Gloss =lerp(o.Gloss,o.Gloss*(darkValue-0.3),refMask);
			c.rgb = lerp(c.rgb, _MainColor.rgb*c.rgb, _MainColorScale);
            c.rgb =lerp(c.rgb,c.rgb *darkValue*_WetColor, refMask);
	        
            o.Albedo = lerp(c.rgb,c.rgb *UNITY_LIGHTMODEL_AMBIENT,_AmbScale); //mul Amb
	        half3 worldRefl = WorldReflectionVector (IN, disturbNor);
            half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRefl);// use ref probe as reflection source
            half3 reflcol=DecodeHDR(skyData, unity_SpecCube0_HDR);
            reflcol *= refMask;
            o.Emission = reflcol.rgb;
        }
        ENDCG
        }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Lambert exclude_path:deferred  exclude_path:prepass nometa noforwardadd noshadow noambient novertexlights 

        sampler2D _MainTex;
        fixed4 _Color;
        half _AmbScale;

        struct Input {
	        float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o) {
	        fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	        o.Albedo = lerp(c.rgb,c.rgb *UNITY_LIGHTMODEL_AMBIENT,_AmbScale); //mul Amb
        }
        ENDCG
    }

    FallBack "Mobile/VertexLit"
}
