half _NorFactor,_MinShadowInt;
half4 _BakeLight;
   
   inline fixed4 UnityBlinnPhongLight1 (SurfaceOutput s, half3 viewDir, UnityLight light)
        {
	        half3 h = normalize (light.dir + viewDir);
	
	        half diff = max (0, dot (s.Normal, light.dir));
	
	        half nh = max (0, dot (s.Normal, h));
	        half spec = pow (nh, s.Specular*128.0) * s.Gloss;
	
	        half4 c;
	        c.rgb = s.Albedo * light.color * diff + light.color * _SpecColor.rgb * spec;
	        c.a = s.Alpha;
    
            #ifndef LIGHTMAP_OFF  
                half3 h1 = normalize (_BakeLight.xyz + viewDir);
                half nh1 = max (0, dot (s.Normal, h1));

	            half spec1 = pow (nh1, s.Specular*16.0) * s.Gloss;
				 c.rgb +=  _SpecColor.rgb * spec1;
	            //c.rgb += _LightColor0.rgb  * _SpecColor.rgb * spec1;
            #endif
	        return c;
        }

        inline fixed4 LightingBlinnPhong1 (SurfaceOutput s, half3 viewDir, UnityGI gi)
        {
	        half4 c;
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
					///_MinShadowInt: the darker part of normal erea
					o_gi.indirect.diffuse =bakedColor *max(_MinShadowInt,dot(normalWorld, normalize(_BakeLight.xyz))) *_BakeLight.w;
					o_gi.indirect.diffuse  =lerp(o_gi.indirect.diffuse,bakedColor,_NorFactor);

					//o_gi.indirect.diffuse  =lerp(o_gi.indirect.diffuse,bakedColor,_NorFactor);

			        #ifdef SHADOWS_SCREEN
				        o_gi.indirect.diffuse = MixLightmapWithRealtimeAttenuation (o_gi.indirect.diffuse, data.atten, bakedColorTex, normalWorld);
			        #endif // SHADOWS_SCREEN

		        #elif DIRLIGHTMAP_COMBINED
			        half4 bakedDirTex = UNITY_SAMPLE_TEX2D_SAMPLER (unity_LightmapInd, unity_Lightmap, data.lightmapUV.xy);
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
		        half4 realtimeColorTex = UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, data.lightmapUV.zw);
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