// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

///LOD 300 Bump + Re +ForwardAdd +Amb
///LOD 200 Re + Amb
///Upadat 18.2.11:Add Spe After Baking LM
Shader "TSHD/Scene/Wet/SceneCommonLOD_RealRef"
{
	Properties
	{
        //_AmbScale ("Ambient Color", Range (0.0, 1)) = 0
		_MainTex ("Main Texture", 2D) = "white" {}
        _BumpScale("Bump Scale", Float) = 1.0
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _SpecularMap ("SpecularMap(RGB for Specular,A for Refletion Mask)",2D) = "white" {}
        _SpecColor ("Specular Color", Color) = (1.0, 1.0, 1.0, 1)
        _Shininess ("Shininess", Range (0.01, 5)) = 0.078125
        _ReflectVal("Reflect Value",Range(0,1)) = 0.2
        _RefFluseVal("Reflect Distortion",Range(0,1)) =0.8
        _WetColor ("Wet Area Color", Color) = (0.2, 0.2,0.2, 1)
		_DisturbTilling("Rain Disturb Tilling",float) =10
        [HideInInspector] _ReflectionTex ("Reflection", 2D) = "black" { }
	}
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

		CGPROGRAM
        #pragma surface surf BlinnPhong2 vertex:vert exclude_path:deferred  exclude_path:prepass nometa  
        #pragma target 3.0
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        sampler2D _BumpMap;
		sampler2D _DisturbMap;
        sampler2D _SpecularMap,_ReflectionTex;

        fixed _BumpScale,_AmbScale,_RefFluseVal,_RefLerp, _DisturbMapFactor;
        fixed4 _Color,_WetColor;
        fixed4 _ReflectColor;
        half _Shininess, _DisturbTilling;
        float _ReflectVal;

        struct Input {
	        float2 uv_MainTex;
	        float2 uv_BumpMap;
	        float3 worldRefl;
            float4 screenPos;
            half2 viewDirRim;
	        INTERNAL_DATA
        };
        
        inline fixed4 UnityBlinnPhongLight2 (SurfaceOutput s, half3 viewDir, UnityLight light)
        {
	        half3 h = normalize (light.dir + viewDir);
	
	        fixed diff = max (0, dot (s.Normal, light.dir));
	
	        float nh = max (0, dot (s.Normal, h));
	        float spec = pow (nh, s.Specular*128.0) * s.Gloss;
	
	        fixed4 c;
	        c.rgb = s.Albedo * light.color * diff + light.color * _SpecColor.rgb * spec;
	        c.a = s.Alpha;
    
            #ifndef LIGHTMAP_OFF  
                half3 h1 = normalize (_WorldSpaceLightPos0.xyz + viewDir);
                half nh1 = max (0, dot (s.Normal, h1));

	            half spec1 = pow (nh1, s.Specular*128.0) * s.Gloss;

	            c.rgb += _LightColor0.rgb  * _SpecColor.rgb * spec1;
            #endif
	        return c;
        }

        inline fixed4 LightingBlinnPhong2 (SurfaceOutput s, half3 viewDir, UnityGI gi)
        {
	        fixed4 c;
	        c = UnityBlinnPhongLight2 (s, viewDir, gi.light);

	        #if defined(DIRLIGHTMAP_SEPARATE)
		        #ifdef LIGHTMAP_ON
			        c += UnityBlinnPhongLight (s, viewDir, gi.light2);
		        #endif
		        #ifdef DYNAMICLIGHTMAP_ON
			        c += UnityBlinnPhongLight (s, viewDir, gi.light3);
		        #endif
	        #endif

	        #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
		        c.rgb += s.Albedo * gi.indirect.diffuse;
	        #endif

	        return c;
        }

        inline void LightingBlinnPhong2_GI (
	        SurfaceOutput s,
	        UnityGIInput data,
	        inout UnityGI gi)
        {
	        gi = UnityGlobalIllumination (data, 1.0, s.Normal);
        }

        
        void vert (inout appdata_full v,out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            half3 worldNormal = UnityObjectToWorldNormal(v.normal);
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            half3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos)  +  _WorldSpaceLightPos0.xyz  );
            o.viewDirRim.x =saturate(1.4f -saturate(dot(worldViewDir , worldNormal)) );
            o.viewDirRim.y =1-_RefLerp;
        }

        void surf (Input IN, inout SurfaceOutput o) {
	        half4 c  = tex2D(_MainTex, IN.uv_MainTex);
            half4 speCol =tex2D(_SpecularMap,IN.uv_MainTex);
            _SpecColor.rgb *=speCol.rgb;

            o.Gloss = _SpecColor.a;
	        o.Specular = _Shininess;
           
            o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_BumpMap),_BumpScale);
            
            half refMask =saturate( speCol.a -IN.viewDirRim.y) ;
            // we make normal of wet to be flat
            half3 tempNor =lerp(o.Normal,half3(0,0,1),_RefFluseVal);
            o.Normal =lerp(o.Normal,tempNor,refMask);    

			half3 disturbMap = tex2D(_DisturbMap, IN.uv_BumpMap * _DisturbTilling);
			half3 disturbNor =lerp(o.Normal,o.Normal * disturbMap, _DisturbMapFactor);

             //50% darker in wet area
            half darkValue =lerp(0.5,1.0,IN.viewDirRim.y);
            refMask *=_ReflectVal;
            o.Gloss =lerp(o.Gloss,o.Gloss*(darkValue-0.3),refMask);
            c.rgb =lerp(c.rgb,c.rgb *darkValue*_WetColor, refMask);
	        
            o.Albedo = lerp(c.rgb,c.rgb *UNITY_LIGHTMODEL_AMBIENT,_AmbScale); //mul Amb
	        half3 worldRefl = WorldReflectionVector (IN, disturbNor);
            half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRefl);// use ref probe as reflection source
            half3 reflcol=DecodeHDR(skyData, unity_SpecCube0_HDR);

            //realtime reflection
            half4 projTC = UNITY_PROJ_COORD(IN.screenPos);                
            half4 reflection2 = tex2Dproj(_ReflectionTex, projTC);
            reflcol =lerp(reflcol,reflection2.rgb,reflection2.a);          

            reflcol *= refMask;
            o.Emission = reflcol.rgb ;
        }
        ENDCG
        }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 250
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

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert  exclude_path:deferred  exclude_path:prepass nometa noforwardadd noshadow noambient novertexlights

        sampler2D _MainTex;

        struct Input {
	        float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o) {
	        fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	        o.Albedo = c.rgb;
        }
        ENDCG
    }
        FallBack "Mobile/VertexLit"
}
