using System;
using UnityEngine;

namespace UnityEditor
{

    class PBSSSSGUI : ShaderGUI
{
    private enum WorkflowMode
    {
        Specular,
        Metallic,
        Dielectric
    }

	public enum BlendMode
	{
		Opaque,
		Cutout,
		Fade,		// Old school alpha-blending mode, fresnel does not affect amount of transparency
		Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
	}

	private static class Styles
	{
		public static GUIStyle optionsButton = "PaneOptions";

		public static string emptyTootip = "";
        public static GUIContent albedoText = new GUIContent("Albedo(RGBA)", "Albedo (RGBA) ");
        public static GUIContent BRDFText = new GUIContent("SSS Map(RGB)", "R for BRDF,G for Mask,B for Thickness ");
		public static GUIContent alphaCutoffText = new GUIContent("Alpha Cutoff", "Threshold for alpha cutoff");
        public static GUIContent specularMapText = new GUIContent("Specular (RGBA)", "RGB for Specular,A for smooth");

        //public static GUIContent rimOnText = new GUIContent("Rim Toggle", "Toggle Rim Color)");

		public static GUIContent metallicMapText = new GUIContent("Metallic", "Metallic (R) and Smoothness (A)");
		public static GUIContent smoothnessText = new GUIContent("Smoothness(R)", "");
		public static GUIContent normalMapText = new GUIContent("Normal Map", "Normal Map");
		public static GUIContent orthoNormalizeText = new GUIContent("Orthonormalize", "Orthonormalize tangent base");
        //public static GUIContent occlusionText = new GUIContent("Occlusion(RGB)", "Occlusion Map");
        //public static GUIContent emissionText = new GUIContent("Emission(RGB)", "Emission Map");

		public static string whiteSpaceString = " ";
		public static string primaryMapsText = "Main Maps";
        //public static string secondaryMapsText = "Secondary Maps";
		public static string renderingMode = "Rendering Mode";
		public static string cullingMode = "Culling Mode";
		public static GUIContent emissiveWarning = new GUIContent ("Emissive value is animated but the material has not been configured to support emissive. Please make sure the material itself has some amount of emissive.");

		public static readonly string[] blendNames = Enum.GetNames (typeof (BlendMode));
		public static readonly string[] cullingNames = Enum.GetNames (typeof (UnityEngine.Rendering.CullMode));
	}

	MaterialProperty blendMode = null;
	MaterialProperty cullMode = null;
	MaterialProperty albedoMap = null;
	MaterialProperty albedoColor = null;
	MaterialProperty alphaCutoff = null;
	MaterialProperty specularMap = null;
	MaterialProperty specularColor = null;

	MaterialProperty metallicMap = null;
	MaterialProperty metallic = null;

	MaterialProperty smoothness = null;
	MaterialProperty smoothnessTweak1 = null;
	MaterialProperty smoothnessTweak2 = null;
	MaterialProperty smoothnessTweaks = null;

	MaterialProperty specularMapColorTweak = null;
	MaterialProperty bumpScale = null;
	MaterialProperty bumpMap = null;
	MaterialProperty orthoNormalize = null;
    /// <summary>
    /// SSS property
    /// </summary>
    MaterialProperty subPower = null;
    MaterialProperty subDistortion = null;
    MaterialProperty BRDFMap = null;
    MaterialProperty subScale = null;
    MaterialProperty subColor = null;

    //MaterialProperty occlusionStrength = null;
    //MaterialProperty occlusionMap = null;

	MaterialProperty emissionColorForRendering = null;
    //MaterialProperty emissionMap = null;

    MaterialProperty rimColor = null;
    MaterialProperty rimPower = null;
    MaterialProperty rimLevel = null;
    MaterialProperty rimDir = null;

	MaterialEditor m_MaterialEditor;
	WorkflowMode m_WorkflowMode = WorkflowMode.Specular;
	ColorPickerHDRConfig m_ColorPickerHDRConfig = new ColorPickerHDRConfig(0f, 99f, 1/99f, 3f);

	bool m_FirstTimeApply = true;

	public void FindProperties (MaterialProperty[] props)
	{
		blendMode = FindProperty ("_Mode", props);
		cullMode = FindProperty ("_CullMode", props, false);
		albedoMap = FindProperty ("_MainTex", props);
		albedoColor = FindProperty ("_Color", props);
		alphaCutoff = FindProperty ("_Cutoff", props);
		specularMap = FindProperty ("_SpecGlossMap", props, false);
        specularColor = FindProperty("_SpecColor", props, false);
        //smoothMap = FindProperty("_SmoothMap", props, false);
        subPower = FindProperty("_SubPower", props);
        subDistortion = FindProperty("_SubDistortion", props);
        BRDFMap = FindProperty("_BRDFTex", props, false);
        subScale = FindProperty("_SubScale", props);
        subColor = FindProperty("_SubColor", props, false);

        metallicMap = FindProperty("_MetallicGlossMap", props, false);
		metallic = FindProperty ("_Metallic", props, false);
		if (specularMap != null && specularColor != null)
			m_WorkflowMode = WorkflowMode.Specular;
		else if (metallicMap != null && metallic != null)
			m_WorkflowMode = WorkflowMode.Metallic;
		else
			m_WorkflowMode = WorkflowMode.Dielectric;
		smoothness = FindProperty ("_Glossiness", props);
		smoothnessTweak1 = FindProperty ("_SmoothnessTweak1", props, false);
		smoothnessTweak2 = FindProperty ("_SmoothnessTweak2", props, false);
		smoothnessTweaks = FindProperty ("_SmoothnessTweaks", props, false);
		specularMapColorTweak = FindProperty ("_SpecularMapColorTweak", props, false);

		bumpScale = FindProperty ("_BumpScale", props);
		bumpMap = FindProperty ("_BumpMap", props);
		orthoNormalize = FindProperty ("_Orthonormalize", props, false);
        //occlusionStrength = FindProperty("_OcclusionStrength", props);
        //occlusionMap = FindProperty("_OcclusionMap", props);

		emissionColorForRendering = FindProperty ("_EmissionColor", props);
        //emissionMap = FindProperty ("_EmissionMap", props);

        rimColor= FindProperty("_RimColor", props);
        rimPower = FindProperty("_RimPower", props, false);
        rimLevel = FindProperty("_RimLevel", props, false);
        rimDir = FindProperty("_RimDir", props, false);
	}

    //internal void DetermineWorkflow(MaterialProperty[] props)
    //{
    //    if (FindProperty("_SpecGlossMap", props, false) != null && FindProperty("_SpecColor", props, false) != null)
    //        m_WorkflowMode = WorkflowMode.Specular;
    //    else if (FindProperty("_MetallicGlossMap", props, false) != null && FindProperty("_Metallic", props, false) != null)
    //        m_WorkflowMode = WorkflowMode.Metallic;
    //    else
    //        m_WorkflowMode = WorkflowMode.Dielectric;
    //}

	public override void AssignNewShaderToMaterial (Material material, Shader oldShader, Shader newShader)
	{
        // _Emission property is lost after assigning Standard shader to the material
        // thus transfer it before assigning the new shader
        //if (material.HasProperty("_Emission"))
        //{
        //    material.SetColor("_EmissionColor", material.GetColor("_Emission"));
        //}

		base.AssignNewShaderToMaterial(material, oldShader, newShader);

        //DetermineWorkflow(MaterialEditor.GetMaterialProperties(new Material[] { material }));
        //MaterialChanged(material, m_WorkflowMode);

		// Re-run this in case the new shader needs custom setup.
        m_FirstTimeApply = true;
	}

	public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] props)
	{
        // MaterialProperties can be animated so we do not cache them
        //but fetch them every event to ensure animated values are updated correctly
		FindProperties (props); 
		m_MaterialEditor = materialEditor;
		Material material = materialEditor.target as Material;
		ShaderPropertiesGUI (material);
		// Make sure that needed keywords are set up if we're switching some existing
		// material to a standard shader.
		if (m_FirstTimeApply)
		{
			// Make sure we've updated this packed vector
			if(smoothnessTweak1 != null && smoothnessTweak2 != null && smoothnessTweaks != null) {
				var w = new Vector4(smoothnessTweak1.floatValue, smoothnessTweak2.floatValue);
				if(smoothnessTweaks.vectorValue != w)
					smoothnessTweaks.vectorValue = w;
			}

			SetMaterialKeywords (material, m_WorkflowMode);
			m_FirstTimeApply = false;

			// Repaint all in case we modified how things render
			SceneView.RepaintAll();
		}
	}

	public void ShaderPropertiesGUI (Material material)
	{
		// Use default labelWidth
		EditorGUIUtility.labelWidth = 0f;

		// Detect any changes to the material
		EditorGUI.BeginChangeCheck();
		{
            //CullModePopup();
			BlendModePopup();
			OrthoNormalizeToggle();

			// Primary properties
			DoAlbedoArea(material);
            //SSS
            DoSSSArea(material);
            //高光
			DoSpecularMetallicArea();
            //外发光
            DoRimArea(material);
			m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapText, bumpMap, bumpMap.textureValue != null ? bumpScale : null);
            //m_MaterialEditor.TexturePropertySingleLine(Styles.occlusionText, occlusionMap, occlusionMap.textureValue != null ? occlusionStrength : null);
            DoEmissionArea(material);

			EditorGUI.BeginChangeCheck();
			m_MaterialEditor.TextureScaleOffsetProperty(albedoMap);
            // Apply the main texture scale and offset to the emission texture as well, for Enlighten's sake
            //if (EditorGUI.EndChangeCheck())
            //    emissionMap.textureScaleAndOffset = albedoMap.textureScaleAndOffset; 
  
			EditorGUILayout.Space();

		}
		if (EditorGUI.EndChangeCheck())
		{
			foreach (var obj in blendMode.targets)
				MaterialChanged((Material)obj, m_WorkflowMode);
		}
	}


    void ImmediateProperty(string name, MaterialEditor materialEditor, MaterialProperty[] props)
    {
        var p = FindProperty(name, props);
        if (p.type == MaterialProperty.PropType.Texture)
            materialEditor.TexturePropertySingleLine(new GUIContent(p.displayName), p);
        else
            materialEditor.ShaderProperty(p, p.displayName);
    }

	void CullModePopup()
	{
		if(cullMode == null)
			return;
			
		EditorGUI.showMixedValue = cullMode.hasMixedValue;
		var mode = (UnityEngine.Rendering.CullMode)Mathf.RoundToInt(cullMode.floatValue);

		EditorGUI.BeginChangeCheck();
		mode = (UnityEngine.Rendering.CullMode)EditorGUILayout.Popup(Styles.cullingMode, (int)mode, Styles.cullingNames);
		if (EditorGUI.EndChangeCheck())
		{
			m_MaterialEditor.RegisterPropertyChangeUndo("Culling Mode");
			cullMode.floatValue = (float)mode;
		}

		EditorGUI.showMixedValue = false;

	}
	
	void OrthoNormalizeToggle()
	{
		if(orthoNormalize == null)
			return;
		
		EditorGUI.showMixedValue = orthoNormalize.hasMixedValue;
		var on = Mathf.RoundToInt(orthoNormalize.floatValue);
		
		EditorGUI.BeginChangeCheck();
			on = EditorGUILayout.Toggle(Styles.orthoNormalizeText, on == 1) ? 1 : 0;
		if (EditorGUI.EndChangeCheck())
		{
			m_MaterialEditor.RegisterPropertyChangeUndo("Orthonormalize");
			orthoNormalize.floatValue = (float)on;
		}
		
		EditorGUI.showMixedValue = false;
	}

	void BlendModePopup()
	{
		EditorGUI.showMixedValue = blendMode.hasMixedValue;
		var mode = (BlendMode)blendMode.floatValue;

		EditorGUI.BeginChangeCheck();
		mode = (BlendMode)EditorGUILayout.Popup(Styles.renderingMode, (int)mode, Styles.blendNames);
		if (EditorGUI.EndChangeCheck())
		{
			m_MaterialEditor.RegisterPropertyChangeUndo("Rendering Mode");
			blendMode.floatValue = (float)mode;
		}

		EditorGUI.showMixedValue = false;
	}

	void DoAlbedoArea(Material material)
	{
		m_MaterialEditor.TexturePropertySingleLine(Styles.albedoText, albedoMap, albedoColor);
        //if (((BlendMode)material.GetFloat("_Mode") == BlendMode.Cutout))
        //{
        //    m_MaterialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel+1);
        //}
	}

    void DoSSSArea(Material material)
    {
        m_MaterialEditor.TexturePropertySingleLine(Styles.BRDFText, BRDFMap, subColor);
        if (BRDFMap.textureValue != null)
        {
            m_MaterialEditor.ShaderProperty(subPower, subPower.displayName);
            m_MaterialEditor.ShaderProperty(subScale, subScale.displayName);
            m_MaterialEditor.ShaderProperty(subDistortion, subDistortion.displayName);
        }

    }

    void DoRimArea(Material material)
    {
  
        m_MaterialEditor.ColorProperty(rimColor, rimColor.displayName);

        if (rimPower != null && rimLevel != null)
        {
            m_MaterialEditor.ShaderProperty(rimPower, rimPower.displayName);
            m_MaterialEditor.ShaderProperty(rimLevel, rimLevel.displayName);
            m_MaterialEditor.ShaderProperty(rimDir, "Rim Direction(W>0 Direction Rim,or esle Full Rim)");
        }
   
    }

	void DoEmissionArea(Material material)
	{
        //float brightness = emissionColorForRendering.colorValue.maxColorComponent;
        //bool showHelpBox = !HasValidEmissiveKeyword(material);
        //bool showEmissionColorAndGIControls = brightness > 0.0f;
		
        //bool hadEmissionTexture = emissionMap.textureValue != null;

		// Texture and HDR color controls
        //m_MaterialEditor.TexturePropertyWithHDRColor(Styles.emissionText, emissionMap, emissionColorForRendering, m_ColorPickerHDRConfig, false);
        m_MaterialEditor.ColorProperty(emissionColorForRendering, emissionColorForRendering.displayName);
		// If texture was assigned and color was black set color to white
        //if (emissionMap.textureValue != null && !hadEmissionTexture && brightness <= 0f)
        //    emissionColorForRendering.colorValue = Color.white;

		// Dynamic Lightmapping mode
        //if (showEmissionColorAndGIControls)
        //{
        //    bool shouldEmissionBeEnabled = ShouldEmissionBeEnabled(emissionColorForRendering.colorValue);
        //    EditorGUI.BeginDisabledGroup(!shouldEmissionBeEnabled);

        //    m_MaterialEditor.LightmapEmissionProperty (MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1);

        //    EditorGUI.EndDisabledGroup();
        //}

        //if (showHelpBox)
        //{
        //    EditorGUILayout.HelpBox(Styles.emissiveWarning.text, MessageType.Warning);
        //}
	}

	void DoSpecularMetallicArea()
	{
		if (m_WorkflowMode == WorkflowMode.Specular)
		{
			if (specularMap.textureValue == null) // 不加入spe贴图，直接读取smooth参数
            {

                m_MaterialEditor.TexturePropertyTwoLines(Styles.specularMapText, specularMap, specularColor, Styles.smoothnessText, smoothness);

			}
            else
            {
                //有spe贴图，分别读取spe和smooth，增加调节选项
				m_MaterialEditor.TexturePropertySingleLine(Styles.specularMapText, specularMap);
                //m_MaterialEditor.TexturePropertySingleLine(Styles.smoothMapText, smoothMap);
				
				if(specularMapColorTweak != null)
					m_MaterialEditor.ColorProperty(specularMapColorTweak, specularMapColorTweak.displayName);

				if(smoothnessTweak1 != null && smoothnessTweak2 != null) {
					m_MaterialEditor.ShaderProperty(smoothnessTweak1, smoothnessTweak1.displayName);
					m_MaterialEditor.ShaderProperty(smoothnessTweak2, smoothnessTweak2.displayName);
					
                    //把spe的倍增值和哑光值放到smoothnewssTweaks的x和y值中
					if(GUI.changed && smoothnessTweaks != null)
						smoothnessTweaks.vectorValue = new Vector4(smoothnessTweak1.floatValue, smoothnessTweak2.floatValue);
				}
			}
		}
		else if (m_WorkflowMode == WorkflowMode.Metallic)
		{
			if (metallicMap.textureValue == null)
				m_MaterialEditor.TexturePropertyTwoLines(Styles.metallicMapText, metallicMap, metallic, Styles.smoothnessText, smoothness);
			else
				m_MaterialEditor.TexturePropertySingleLine(Styles.metallicMapText, metallicMap);
		}
	}

	public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
	{
		switch (blendMode)
		{
			case BlendMode.Opaque:
				material.SetOverrideTag("RenderType", "");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt("_ZWrite", 1);
				material.DisableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = -1;
				break;
			case BlendMode.Cutout:
				material.SetOverrideTag("RenderType", "TransparentCutout");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt("_ZWrite", 1);
				material.EnableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 2450;
				break;
			case BlendMode.Fade:
				material.SetOverrideTag("RenderType", "Transparent");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.SetInt("_ZWrite", 0);
				material.DisableKeyword("_ALPHATEST_ON");
				material.EnableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 3000;
				break;
			case BlendMode.Transparent:
				material.SetOverrideTag("RenderType", "Transparent");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.SetInt("_ZWrite", 0);
				material.DisableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 3000;
				break;
		}
	}
	
	static bool ShouldEmissionBeEnabled (Color color)
	{
		return color.maxColorComponent > (0.1f / 255.0f);
	}

	static void SetMaterialKeywords(Material material, WorkflowMode workflowMode)
	{
		// Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
		// (MaterialProperty value might come from renderer material property block)
		SetKeyword (material, "_NORMALMAP", material.GetTexture ("_BumpMap") );
		SetKeyword (material, "ORTHONORMALIZE_TANGENT_BASE", material.HasProperty("__orthonormalize") && material.GetFloat("__orthonormalize") > 0.5f);
        SetKeyword(material, "_BRDF", material.GetTexture("_BRDFTex"));
		if (workflowMode == WorkflowMode.Specular)
			SetKeyword (material, "_SPECGLOSSMAP", material.GetTexture ("_SpecGlossMap"));
		else if (workflowMode == WorkflowMode.Metallic)
			SetKeyword (material, "_METALLICGLOSSMAP", material.GetTexture ("_MetallicGlossMap"));

        //SetKeyword(material, "_RIM", rimOn);
        SetKeyword(material, "_RIM", material.GetFloat("_RimLevel") > 0 || material.GetColor("_RimColor").maxColorComponent > (0.1f / 255.0f));

        bool shouldEmissionBeEnabled = ShouldEmissionBeEnabled(material.GetColor("_EmissionColor"));
        //SetKeyword (material, "_EMISSION", shouldEmissionBeEnabled);

		// Setup lightmap emissive flags
		MaterialGlobalIlluminationFlags flags = material.globalIlluminationFlags;
		if ((flags & (MaterialGlobalIlluminationFlags.BakedEmissive | MaterialGlobalIlluminationFlags.RealtimeEmissive)) != 0)
		{
			flags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
			if (!shouldEmissionBeEnabled)
				flags |= MaterialGlobalIlluminationFlags.EmissiveIsBlack;

			material.globalIlluminationFlags = flags;
		}
	}

	bool HasValidEmissiveKeyword (Material material)
	{
		// Material animation might be out of sync with the material keyword.
		// So if the emission support is disabled on the material, but the property blocks have a value that requires it, then we need to show a warning.
		// (note: (Renderer MaterialPropertyBlock applies its values to emissionColorForRendering))
        //bool hasEmissionKeyword = material.IsKeywordEnabled ("_EMISSION");
        //if (!hasEmissionKeyword && ShouldEmissionBeEnabled (emissionColorForRendering.colorValue))
        if ( ShouldEmissionBeEnabled(emissionColorForRendering.colorValue))
			return false;
		else
			return true;
	}

	static void MaterialChanged(Material material, WorkflowMode workflowMode)
	{
		SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));

		SetMaterialKeywords(material, workflowMode);
	}

	static void SetKeyword(Material m, string keyword, bool state)
	{
        if (state)
        {
            m.EnableKeyword(keyword);
            m.DisableKeyword(keyword + "_OFF");
        }
        else
        {
            m.DisableKeyword(keyword);
            m.EnableKeyword(keyword + "_OFF");
        }
	}
}

} // namespace UnityEditor
