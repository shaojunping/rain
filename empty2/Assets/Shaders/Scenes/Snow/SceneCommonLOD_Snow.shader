// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

///LOD 300 Bump + Re +ForwardAdd +Amb
///LOD 200 Re + Amb
///Upadat 18.2.11:Add Spe After Baking LM
Shader "TSHD/Scene/Snow/SceneCommonLOD_Snow"
{
	Properties
	{
        //_AmbScale ("Ambient Color", Range (0.0, 1)) = 0
		_MainTex ("Main Texture", 2D) = "white" {}
        _NorFactor("Baked Normal Factor",Range(0,1)) =0.0
        _MinShadowInt("Min Shadow Intensity",Range(0,1)) =0.5
        _BakeLight("_BakeLight XYZ:Direction,W:Intensity",Vector) =(0.2,1,0.2,1.5)
        _BumpScale("Bump Scale", Float) = 1.0
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _SpecularMap ("SpecularMap(RGB for Specular,A for Refletion Mask)",2D) = "white" {}
        _SpecColor ("Specular Color", Color) = (1.0, 1.0, 1.0, 1)
        _Shininess ("Shininess", Range (0.01, 5)) = 0.078125
        _ReflectVal("Reflect Value",Range(0,1)) = 0.2
        _RefFluseVal("Reflect Distortion",Range(0,1)) =0.8
        _WetColor ("Wet Area Color", Color) = (0.2, 0.2,0.2, 1)
        
        _SnowColor ("Snow Color", Color) = (1.0,1.0,1.0,1.0)
        _SnowDirection ("Snow Direction(XYZ: Direction,W:Snow Leve Refine Factor", Vector) = (0,1,0,1)
        _SnowUV("Snow UV Scale",float) =1.0
        _SnowDepth ("Snow Depth", Range(0,0.5)) = 0.1
        _Wetness ("Wetness", Range(0, 0.5)) = 0.3
	}
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

		CGPROGRAM
        #pragma surface surf BlinnPhong1 vertex:vert exclude_path:deferred  exclude_path:prepass nometa  
        #pragma target 3.0
        #include "../Wet/T4MAddSpe.cginc"


        sampler2D _MainTex,_SnowTex;
        sampler2D _BumpMap;
        sampler2D _SpecularMap;

        fixed _BumpScale,_AmbScale,_RefFluseVal,_RefLerp;
        fixed4 _Color,_WetColor;
        fixed4 _ReflectColor;
        half _Shininess;
        float _ReflectVal;

        float _SnowLevel;
        float4 _SnowColor;
        float4 _SnowDirection;
        float _SnowDepth;
        float _Wetness,_SnowUV;

        struct Input {
	        float2 uv_MainTex;
	        float2 uv_BumpMap;
	        float3 worldRefl;
            half2 viewDirRim;
            float3 worldNormal;
	        INTERNAL_DATA
        };

        
        void vert (inout appdata_full v,out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            half3 worldNormal = UnityObjectToWorldNormal(v.normal);
             float4 sn = mul(UNITY_MATRIX_IT_MV, _SnowDirection);
            v.vertex.xyz =dot(v.normal, _SnowDirection.xyz) >= lerp(1,-1, ((1-_Wetness) * _SnowLevel*2)/3) ? v.vertex.xyz +(sn.xyz + v.normal) * _SnowDepth * _SnowLevel : v.vertex.xyz;
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

             //50% darker in wet area
            half darkValue =lerp(0.5,1.0,IN.viewDirRim.y);
            refMask *=_ReflectVal;
            o.Gloss =lerp(o.Gloss,o.Gloss*(darkValue-0.3),refMask);
            c.rgb =lerp(c.rgb,c.rgb *darkValue*_WetColor, refMask);

            half4 snowC =tex2D (_SnowTex, IN.uv_MainTex*_SnowUV);
            half difference = dot(WorldNormalVector(IN, o.Normal), _SnowDirection.xyz) - lerp(1,-1,_SnowLevel*_SnowDirection.w);
            difference = saturate(difference / _Wetness);
            o.Albedo = difference*_SnowColor.rgb*snowC.rgb + (1-difference) *c;
	        
            o.Albedo = lerp(o.Albedo,o.Albedo *UNITY_LIGHTMODEL_AMBIENT,_AmbScale); //mul Amb
            o.Gloss = lerp(o.Gloss ,snowC.a,difference);

	        half3 worldRefl = WorldReflectionVector (IN, o.Normal);
            half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRefl);// use ref probe as reflection source
            half3 reflcol=DecodeHDR(skyData, unity_SpecCube0_HDR);
            reflcol *= refMask;
            o.Emission = reflcol.rgb ;
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

    //SubShader
    //{
    //    Tags { "RenderType"="Opaque" }
    //    LOD 200

    //    CGPROGRAM
    //    #pragma surface surf Lambert  exclude_path:deferred  exclude_path:prepass nometa noforwardadd noshadow noambient novertexlights

    //    sampler2D _MainTex;

    //    struct Input {
    //        float2 uv_MainTex;
    //    };

    //    void surf (Input IN, inout SurfaceOutput o) {
    //        fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
    //        o.Albedo = c.rgb;
    //    }
    //    ENDCG
    //}
        FallBack "Mobile/VertexLit"
}
