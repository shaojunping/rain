// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

#ifndef TERRAIN_SPLATMAP_COMMON_CGINC_INCLUDED
#define TERRAIN_SPLATMAP_COMMON_CGINC_INCLUDED

struct Input
{
	//float2 uv_Splat0 : TEXCOORD0;
	//float2 uv_Splat1 : TEXCOORD1;
	//float2 uv_Splat2 : TEXCOORD2;
	//float2 uv_Splat3 : TEXCOORD3;
	float2 tc_Control : TEXCOORD4;	// Not prefixing '_Contorl' with 'uv' allows a tighter packing of interpolators, which is necessary to support directional lightmap.
	//UNITY_FOG_COORDS(5)
	float2 fogCoord : TEXCOORD5;
	float2 ref_Control : TEXCOORD6;
	float2 disturb_Control : TEXCOORD7;
	float3 worldRefl;
    half2 viewDirRim;
	float3 worldNormal;
    INTERNAL_DATA
};

sampler2D _Control,_RefMap,_ReflectionTex;
float4 _Control_ST,_RefMap_ST, _DisturbMap_ST;
sampler2D _Splat0,_Splat1,_Splat2,_Splat3;
half _ReflectVal,_RefLerp,_RefFluseVal,_BumpScale;
float4 _Splat1_ST, _Splat2_ST,_Splat3_ST,_Splat0_ST;
fixed _NorFactor,_MinShadowInt;
fixed _FogVal,_FogHeiDen,_FogHeiParaZ,_FogHeiParaW;
half4 _BakeLight;

#ifdef _TERRAIN_NORMAL_MAP
	sampler2D _Normal0, _Normal1, _Normal2, _Normal3;
#endif

// 全局环境颜色
void TGameAmbient(inout fixed4 color,fixed ambientImpact,fixed diffMultiplier)
{
	//if(ambientImpact > 0.1)
	//{
		// 白色
		fixed4 white = fixed4(1,1,1,1);
			
		// 环境光的影响值
		fixed4 amb = lerp(white, unity_AmbientSky , ambientImpact);
			
		// 最终颜色
		color = color * amb * diffMultiplier;	

	//}
}


void SplatmapVert(inout appdata_full v, out Input data)
{
	UNITY_INITIALIZE_OUTPUT(Input, data);
	data.tc_Control = TRANSFORM_TEX(v.texcoord, _Control);	// Need to manually transform uv here, as we choose not to use 'uv' prefix for this texcoord.
	data.ref_Control = TRANSFORM_TEX(v.texcoord, _RefMap);
	data.disturb_Control = TRANSFORM_TEX(v.texcoord, _DisturbMap);
	float4 pos = mul (UNITY_MATRIX_MVP, v.vertex);
	float4 worldPos =mul(unity_ObjectToWorld,v.vertex);
	//UNITY_TRANSFER_FOG(data, pos);
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
			data.fogCoord.x = saturate(unityFogFactor);
			data.fogCoord.y =saturate(worldPos.y  *_FogHeiParaZ +_FogHeiParaW);

	data.viewDirRim.y =1-_RefLerp;
#ifdef _TERRAIN_NORMAL_MAP
	v.tangent.xyz = cross(v.normal, float3(0,0,1));
	v.tangent.w = -1;
#endif
}

void SplatmapVertWet(inout appdata_full v, out Input data)
{
	UNITY_INITIALIZE_OUTPUT(Input, data);
	data.tc_Control = TRANSFORM_TEX(v.texcoord, _Control);	// Need to manually transform uv here, as we choose not to use 'uv' prefix for this texcoord.
	data.ref_Control = TRANSFORM_TEX(v.texcoord,_RefMap);
	float4 pos = mul (UNITY_MATRIX_MVP, v.vertex);
	//UNITY_TRANSFER_FOG(data, pos);

	half3 worldNormal = UnityObjectToWorldNormal(v.normal);
	float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
	half3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos)  +  _WorldSpaceLightPos0.xyz  );
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
			data.fogCoord.x = saturate(unityFogFactor);
			data.fogCoord.y =saturate(worldPos.y *_FogHeiParaZ +_FogHeiParaW);

	data.viewDirRim.x =saturate(1.4f -saturate(dot(worldViewDir , worldNormal)) );
	data.viewDirRim.y =1-_RefLerp;
#ifdef _TERRAIN_NORMAL_MAP
	v.tangent.xyz = cross(v.normal, float3(0,0,1));
	v.tangent.w = -1;
#endif
}        

#ifdef TERRAIN_STANDARD_SHADER
void SplatmapMix(Input IN, half4 defaultAlpha, out half4 splat_control, out half weight, out fixed4 mixedDiffuse, inout fixed3 mixedNormal)
#else
void SplatmapMix(Input IN, out half4 splat_control, out half weight, out fixed4 mixedDiffuse, inout fixed3 mixedNormal)
#endif
{
	splat_control = tex2D(_Control, IN.tc_Control);
	weight = dot(splat_control, half4(1,1,1,1));

	#if !defined(SHADER_API_MOBILE) && defined(TERRAIN_SPLAT_ADDPASS)
		clip(weight - 0.0039 /*1/255*/);
	#endif

	// Normalize weights before lighting and restore weights in final modifier functions so that the overal
	// lighting result can be correctly weighted.
	splat_control /= (weight + 1e-3f);

	mixedDiffuse = 0.0f;
	#ifdef TERRAIN_STANDARD_SHADER
		mixedDiffuse += splat_control.r * tex2D(_Splat0, IN.tc_Control *_Splat0_ST.xy) * half4(1.0, 1.0, 1.0, defaultAlpha.r);
		mixedDiffuse += splat_control.g * tex2D(_Splat1, IN.tc_Control *_Splat1_ST.xy) * half4(1.0, 1.0, 1.0, defaultAlpha.g);
		mixedDiffuse += splat_control.b * tex2D(_Splat2, IN.tc_Control *_Splat2_ST.x) * half4(1.0, 1.0, 1.0, defaultAlpha.b);
		mixedDiffuse += splat_control.a * tex2D(_Splat3, IN.tc_Control *_Splat3_ST.x) * half4(1.0, 1.0, 1.0, defaultAlpha.a);
	#else
		mixedDiffuse += splat_control.r * tex2D(_Splat0, IN.tc_Control *_Splat0_ST.xy);
		mixedDiffuse += splat_control.g * tex2D(_Splat1, IN.tc_Control *_Splat1_ST.xy);
		mixedDiffuse += splat_control.b * tex2D(_Splat2, IN.tc_Control *_Splat2_ST.xy);
		mixedDiffuse += splat_control.a * tex2D(_Splat3, IN.tc_Control *_Splat3_ST.xy);
	#endif

	#ifdef _TERRAIN_NORMAL_MAP
		fixed4 nrm = 0.0f;
		nrm += splat_control.r * tex2D(_Normal0, IN.tc_Control *_Splat0_ST.x);
		nrm += splat_control.g * tex2D(_Normal1, IN.tc_Control *_Splat1_ST.x);
		nrm += splat_control.b * tex2D(_Normal2, IN.tc_Control *_Splat2_ST.x);
		nrm += splat_control.a * tex2D(_Normal3, IN.tc_Control *_Splat3_ST.x);
		mixedNormal = UnpackNormal(nrm);
		//mixedNormal = UnpackScaleNormal(nrm,_BumpScale);
	#endif
}

#ifndef TERRAIN_SURFACE_OUTPUT
	#define TERRAIN_SURFACE_OUTPUT SurfaceOutput
#endif

void SplatmapFinalColor(Input IN, TERRAIN_SURFACE_OUTPUT o, inout fixed4 color)
{
	color *= o.Alpha;
	#ifdef TERRAIN_SPLAT_ADDPASS
		//UNITY_APPLY_FOG_COLOR(IN.fogCoord, color, fixed4(0,0,0,0));
		color.rgb = lerp(fixed3(0,0,0), (color).rgb, saturate(IN.fogCoord.x));
	#else
		//UNITY_APPLY_FOG(IN.fogCoord, color);
		fixed3 FarFog = lerp(unity_FogColor, color.rgb, IN.fogCoord.x);//Far Fog
		fixed3 HeiFog =lerp(unity_FogColor,FarFog,IN.fogCoord.y); //Height Fog
		fixed3 tempC =lerp(FarFog,HeiFog,_FogHeiDen);//Height Fog Density
		color.rgb = lerp(color.rgb,tempC,_FogVal);
	#endif
}

void SplatmapFinalPrepass(Input IN, TERRAIN_SURFACE_OUTPUT o, inout fixed4 normalSpec)
{
	normalSpec *= o.Alpha;
}

void SplatmapFinalGBuffer(Input IN, TERRAIN_SURFACE_OUTPUT o, inout half4 diffuse, inout half4 specSmoothness, inout half4 normal, inout half4 emission)
{
	diffuse.rgb *= o.Alpha;
	specSmoothness *= o.Alpha;
	normal.rgb *= o.Alpha;
	emission *= o.Alpha;
}

    inline fixed4 UnityBlinnPhongLight1 (SurfaceOutput s, half3 viewDir, UnityLight light)
        {
	        half3 h = normalize (light.dir + viewDir);
	
	        fixed diff = max (0, dot (s.Normal, light.dir));
	
	        float nh = max (0, dot (s.Normal, h));
	        float spec = pow (nh, s.Specular*128.0) * s.Gloss;
	
	        fixed4 c;
	        c.rgb = s.Albedo * light.color * diff + light.color * _SpecColor.rgb * spec;
	        c.a = s.Alpha;
    
            #ifndef LIGHTMAP_OFF  
                half3 h1 = normalize (_BakeLight.xyz + viewDir);
                half nh1 = max (0, dot (s.Normal, h1));

	            half spec1 = pow (nh1, s.Specular*16.0) * s.Gloss;

	            c.rgb +=  _SpecColor.rgb * spec1;
            #endif
	        return c;
        }

        inline fixed4 LightingBlinnPhong1 (SurfaceOutput s, half3 viewDir, UnityGI gi)
        {
	        fixed4 c;
	        c = UnityBlinnPhongLight1 (s, viewDir, gi.light);

	        #if defined(DIRLIGHTMAP_SEPARATE)
		        #ifdef LIGHTMAP_ON
			        c += UnityBlinnPhongLight1 (s, viewDir, gi.light2);
		        #endif
		        #ifdef DYNAMICLIGHTMAP_ON
			        c += UnityBlinnPhongLight1 (s, viewDir, gi.light3);
		        #endif
	        #endif

	        #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
		        c.rgb += s.Albedo * gi.indirect.diffuse;
	        #endif

	        return c;
        }

	      inline UnityGI UnityGlobalIlluminationLMS(UnityGIInput data, half occlusion, half3 normalWorld)
        {
	        UnityGI o_gi;
	        ResetUnityGI(o_gi);


	        #if !defined(LIGHTMAP_ON)
		        o_gi.light = data.light;
		        o_gi.light.color *= data.atten;
	        #endif

	        #if UNITY_SHOULD_SAMPLE_SH
		        o_gi.indirect.diffuse = ShadeSHPerPixel (normalWorld, data.ambient,data.worldPos);
	        #endif

	        #if defined(LIGHTMAP_ON)
		        // Baked lightmaps
		        fixed4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, data.lightmapUV.xy);
                
//==========================================================================
                /// 2nd LM!!!!!!!!!!!!!
                //float4 lmtex2 = UNITY_SAMPLE_TEX2D(_SecLMTex,data.lightmapUV.xy);
                //bakedColorTex =lerp(bakedColorTex,lmtex2,_LMLerp);        
//==========================================================================

		        half3 bakedColor = DecodeLightmap(bakedColorTex);
            

		        #ifdef DIRLIGHTMAP_OFF
			        o_gi.indirect.diffuse = bakedColor;
					
					///Add Normal / max(1e-4h, data.atten)
					///Attention: The value ( data.atten*0.5 +0.9) is not the real "rebalancing coefficient" as  dirTex.w,but a fake factor!!!!!
					///abs(_WorldSpaceLightPos0.xyz ) - 0.5 ----->(_WorldSpaceLightPos0.xyz+1)*0.5 -0.5 ---->_WorldSpaceLightPos0.xyz * 0.5
					//o_gi.indirect.diffuse =bakedColor *(dot(normalWorld, _WorldSpaceLightPos0.xyz * 0.5) + 0.5) *( data.atten*0.5+0.9);
					o_gi.indirect.diffuse =bakedColor *max(_MinShadowInt,dot(normalWorld, normalize(_BakeLight.xyz))) *_BakeLight.w;
					o_gi.indirect.diffuse  =lerp(o_gi.indirect.diffuse,bakedColor,_NorFactor);

			        #ifdef SHADOWS_SCREEN
				        o_gi.indirect.diffuse = MixLightmapWithRealtimeAttenuation (o_gi.indirect.diffuse, data.atten, bakedColorTex, normalWorld);
			        #endif // SHADOWS_SCREEN

		        #elif DIRLIGHTMAP_COMBINED
			        fixed4 bakedDirTex = UNITY_SAMPLE_TEX2D_SAMPLER (unity_LightmapInd, unity_Lightmap, data.lightmapUV.xy);
			        o_gi.indirect.diffuse = DecodeDirectionalLightmap (bakedColor, bakedDirTex, normalWorld);

			        #ifdef SHADOWS_SCREEN
				  //      o_gi.light.color = MixLightmapWithRealtimeAttenuation(o_gi.light.color, data.atten, bakedColorTex, normalWorld);
						//o_gi.light2.color = MixLightmapWithRealtimeAttenuation(o_gi.light2.color, data.atten, bakedColorTex, normalWorld);
						o_gi.indirect.diffuse = MixLightmapWithRealtimeAttenuation (o_gi.indirect.diffuse, data.atten, bakedColorTex, normalWorld);
			        #endif // SHADOWS_SCREEN

		        #elif DIRLIGHTMAP_SEPARATE
			        // Left halves of both intensity and direction lightmaps store direct light; right halves - indirect.

			        // Direct
			        fixed4 bakedDirTex = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd, unity_Lightmap, data.lightmapUV.xy);
			        o_gi.indirect.diffuse = DecodeDirectionalSpecularLightmap (bakedColor, bakedDirTex, normalWorld, false, 0, o_gi.light);

			        // Indirect
			        half2 uvIndirect = data.lightmapUV.xy + half2(0.5, 0);
			        bakedColor = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, uvIndirect));
			        bakedDirTex = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd, unity_Lightmap, uvIndirect);
			        o_gi.indirect.diffuse += DecodeDirectionalSpecularLightmap (bakedColor, bakedDirTex, normalWorld, false, 0, o_gi.light2);
		        #endif
	        #endif

	        #ifdef DYNAMICLIGHTMAP_ON
		        // Dynamic lightmaps
		        fixed4 realtimeColorTex = UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, data.lightmapUV.zw);
		        half3 realtimeColor = DecodeRealtimeLightmap (realtimeColorTex);

		        #ifdef DIRLIGHTMAP_OFF
			        o_gi.indirect.diffuse += realtimeColor;

		        #elif DIRLIGHTMAP_COMBINED
			        half4 realtimeDirTex = UNITY_SAMPLE_TEX2D_SAMPLER(unity_DynamicDirectionality, unity_DynamicLightmap, data.lightmapUV.zw);
			        o_gi.indirect.diffuse += DecodeDirectionalLightmap (realtimeColor, realtimeDirTex, normalWorld);

		        #elif DIRLIGHTMAP_SEPARATE
			        half4 realtimeDirTex = UNITY_SAMPLE_TEX2D_SAMPLER(unity_DynamicDirectionality, unity_DynamicLightmap, data.lightmapUV.zw);
			        half4 realtimeNormalTex = UNITY_SAMPLE_TEX2D_SAMPLER(unity_DynamicNormal, unity_DynamicLightmap, data.lightmapUV.zw);
			        o_gi.indirect.diffuse += DecodeDirectionalSpecularLightmap (realtimeColor, realtimeDirTex, normalWorld, true, realtimeNormalTex, o_gi.light3);
		        #endif
	        #endif

	        o_gi.indirect.diffuse *= occlusion;

	        return o_gi;
        }

        inline void LightingBlinnPhong1_GI (
	        SurfaceOutput s,
	        UnityGIInput data,
	        inout UnityGI gi)
        {
	        gi = UnityGlobalIlluminationLMS (data, 1.0, s.Normal);
        }

#endif // TERRAIN_SPLATMAP_COMMON_CGINC_INCLUDED
