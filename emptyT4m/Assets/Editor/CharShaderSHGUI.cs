using System;
using UnityEngine;

namespace UnityEditor
{
    public enum BlendMode2
    {
        Opaque,
        Cutout,
    }
    class CharShaderSHGUI : ShaderGUI
    {
        private static class Styles
        {
            public static GUIContent mainTexText = new GUIContent("Main Texture (RGB)", " Main Texture");
            public static GUIContent alphaTexText = new GUIContent("Alpha Texture (R)", " Alpha Texture");
            public static GUIContent flowTexText = new GUIContent("Flow Texture (RG)", "FlowMask(R) FlowTex(G),Note: 2nd UV Set");
            public static GUIContent specularMapText = new GUIContent("Specular (RGB)", "Specular Map");
            public static GUIContent normalMapText = new GUIContent("Normal Map(RGB)", "Normal Map");

            public static GUIContent cubeMapText = new GUIContent("Sky Box", "Cube Map");
            public static GUIContent refTexText = new GUIContent("Reflect Mask(B)", "ReflectionMask Map(B)");
            public static string renderingMode = "Rendering Mode";
            public static readonly string[] blendNames = Enum.GetNames(typeof(BlendMode2));
        }
        MaterialProperty blendMode = null;
        MaterialProperty selfColor = null;
        //MaterialProperty shColor = null;
        MaterialProperty mainMap = null;
        MaterialProperty alphaMap = null;
        MaterialProperty flowMap = null;
        MaterialProperty glowColor = null;
        MaterialProperty scrollX = null;
        MaterialProperty scrollY = null;
        MaterialProperty flowScale = null;

        MaterialProperty rimColor = null;
        MaterialProperty rimPower = null;
        MaterialProperty rimLevel = null;
        MaterialProperty rimDir = null;

        MaterialProperty specularMap = null;
        //MaterialProperty BRDFMap = null;
        MaterialProperty bumpScale = null;
        MaterialProperty bumpMap = null;

        MaterialProperty specularColor = null;
        MaterialProperty specularPower = null;
        MaterialProperty specularScale = null;

        MaterialProperty cubeMap = null;
        MaterialProperty reflectScale = null;
        MaterialProperty refMap = null;

        MaterialEditor m_MaterialEditor;

        bool m_FirstTimeApply = true;
        //static bool alphaOn = false;

        public void FindProperties(MaterialProperty[] props)
        {
            blendMode = FindProperty("_Mode", props);
            selfColor = FindProperty("_EmissionColor", props);
            //shColor = FindProperty("_SHlight", props);
            mainMap = FindProperty("_MainTex", props);
            alphaMap = FindProperty("_AlphaTex", props);

            bumpScale = FindProperty("_BumpScale", props);
            bumpMap = FindProperty("_BumpMap", props);

            flowMap = FindProperty("_FlowTex", props);
            glowColor = FindProperty("_GlowColor", props, false);
            scrollX = FindProperty("_ScrollX", props, false);
            scrollY = FindProperty("_ScrollY", props, false);
            flowScale = FindProperty("_FlowScale", props, false);

            rimColor = FindProperty("_RimColor", props);
            rimPower = FindProperty("_RimPower", props, false);
            rimLevel = FindProperty("_RimLevel", props, false);
            rimDir = FindProperty("_RimDir", props, false);

            specularMap = FindProperty("_SpecularMap", props);
            //BRDFMap = FindProperty("_BRDFTex", props);
            specularColor = FindProperty("_SpecColor", props, false);
            specularScale = FindProperty("_SpecScale", props, false);
            specularPower = FindProperty("_SpecPower", props, false);

            cubeMap = FindProperty("_Cubemap", props);
            refMap = FindProperty("_RefMask", props);
            reflectScale = FindProperty("_ReflectVal", props, false);
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            // Re-run this in case the new shader needs custom setup.
            m_FirstTimeApply = true;
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)

        {
            FindProperties(props);
            m_MaterialEditor = materialEditor;
            Material material = materialEditor.target as Material;
            ShaderPropertiesGUI(material);

            if (m_FirstTimeApply)
            {
                // Make sure we've updated this packed vector
                SetMaterialKeywords(material);
                m_FirstTimeApply = false;
                // Repaint all in case we modified how things render
                SceneView.RepaintAll();
            }
        }

        public void ShaderPropertiesGUI(Material material)
        {
            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;
            // Detect any changes to the material
            EditorGUI.BeginChangeCheck();
            {
                BlendModePopup();
                // Main Tex
                DoMainArea(material);
                // Normal Tex
                m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapText, bumpMap, bumpMap.textureValue != null ? bumpScale : null);
                //Flow Tex
                DoFlowArea(material);
                //Rim
                DoRimArea(material);
                //Spe
                DoSpeArea(material);
                //Reflect
                DoRefArea(material);

                EditorGUILayout.Space();
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in blendMode.targets)
                {
                    //Debug.Log("Current Mat:" + obj.name);
                    MaterialChanged((Material)obj);
                }
            }
        }

        void BlendModePopup()
        {
            EditorGUI.showMixedValue = blendMode.hasMixedValue;
            var mode = (BlendMode2)blendMode.floatValue;

            EditorGUI.BeginChangeCheck();
            mode = (BlendMode2)EditorGUILayout.Popup(Styles.renderingMode, (int)mode, Styles.blendNames);
            if (EditorGUI.EndChangeCheck())
            {
                m_MaterialEditor.RegisterPropertyChangeUndo("Rendering Mode");
                blendMode.floatValue = (float)mode;
            }

            EditorGUI.showMixedValue = false;
        }
        void DoMainArea(Material material)
        {
            m_MaterialEditor.ColorProperty(selfColor, selfColor.displayName);
            //m_MaterialEditor.ColorProperty(shColor, shColor.displayName);
            m_MaterialEditor.TexturePropertySingleLine(Styles.mainTexText, mainMap);
            if (((BlendMode2)material.GetFloat("_Mode") == BlendMode2.Cutout))
            {
                m_MaterialEditor.TexturePropertySingleLine(Styles.alphaTexText, alphaMap);
            }
            m_MaterialEditor.TextureScaleOffsetProperty(mainMap);
        }

        void DoFlowArea(Material material)
        {
            m_MaterialEditor.TexturePropertySingleLine(Styles.flowTexText, flowMap);
            if (flowMap.textureValue != null)
            {
                m_MaterialEditor.ColorProperty(glowColor, glowColor.displayName);
                m_MaterialEditor.ShaderProperty(scrollX, scrollX.displayName);
                m_MaterialEditor.ShaderProperty(scrollY, scrollY.displayName);
                m_MaterialEditor.ShaderProperty(flowScale, flowScale.displayName);
                m_MaterialEditor.TextureScaleOffsetProperty(flowMap);
            }
        }
        void DoSpeArea(Material material)
        {
            m_MaterialEditor.TexturePropertySingleLine(Styles.specularMapText, specularMap);
            //m_MaterialEditor.TexturePropertySingleLine(Styles.BRDFMapText, BRDFMap);
            m_MaterialEditor.ColorProperty(specularColor, specularColor.displayName);
            m_MaterialEditor.ShaderProperty(specularScale, specularScale.displayName);
            m_MaterialEditor.ShaderProperty(specularPower, specularPower.displayName);
        }
        void DoRefArea(Material material)
        {
            m_MaterialEditor.TexturePropertySingleLine(Styles.cubeMapText, cubeMap);
            if (cubeMap.textureValue != null)
            {
                m_MaterialEditor.TexturePropertySingleLine(Styles.refTexText, refMap);
                m_MaterialEditor.ShaderProperty(reflectScale, reflectScale.displayName);
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

        static void SetMaterialKeywords(Material material)
        {
            // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
            // (MaterialProperty value might come from renderer material property block)
            SetKeyword(material, "_ALPHA", material.GetTag("RenderType", false, "Opaque") != "Opaque");
            //SetKeyword(material, "_SHLIGHT", material.GetColor("_SHlight").maxColorComponent > (0.1f / 255.0f));
            SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap"));
            //if (material.GetTexture("_BumpMap"))
            //{
            //    Debug.Log("asdfasdfadfas ds dsa df BUMP!");
            //    material.DisableKeyword("_NORMALMAP_OFF");
            //}
            SetKeyword(material, "_FLOWMAP", material.GetTexture("_FlowTex"));
            SetKeyword(material, "_RIM", material.GetFloat("_RimLevel")>0 || material.GetColor("_RimColor").maxColorComponent>(0.1f/255.0f));
            SetKeyword(material, "_SPEMAP", material.GetColor("_SpecColor").maxColorComponent > (0.1f / 255.0f));
            SetKeyword(material, "_REFMAP", material.GetTexture("_Cubemap"));
        }

        static void MaterialChanged(Material material)
        {
            SetupMaterialWithBlendMode(material, (BlendMode2)material.GetFloat("_Mode"));
            SetMaterialKeywords(material);
        }

        static void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }
        public static void SetupMaterialWithBlendMode(Material material, BlendMode2 blendMode)
        {
            switch (blendMode)
            {
                case BlendMode2.Opaque:
                    material.SetOverrideTag("RenderType", "");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = -1;
                    break;
                case BlendMode2.Cutout:
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 2450;
                    break;
            }
        }
    }

} // namespace UnityEditor
