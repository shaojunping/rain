///use RGBA8 on mobile for low end mobile (HDR OFF!)
///down sample and up sample like UE bloom,and first 1/4 size filter on moblie
///Add Radial Blur,DOF
///Update 17.2.13:Ver1.0
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/ImageEffectsController")]

public class ImageEffectsController : MonoBehaviour
{
        #region Pra
        [SerializeField]
        [Tooltip("Filters out pixels under this level of brightness.")]
        public float threshold = 0.9f;

        public float thresholdGamma
        {
            set { threshold = value; }
            get { return Mathf.Max(0.0f, threshold); }
        }

        public float thresholdLinear
        {
            set { threshold = Mathf.LinearToGammaSpace(value); }
            get { return Mathf.GammaToLinearSpace(thresholdGamma); }
        }

        [SerializeField, Range(0, 1)]
        [Tooltip("Makes transition between under/over-threshold gradual.")]
        public float softKnee = 0.2f;

        [SerializeField, Range(2, 7)]
        [Tooltip("Changes extent of veiling effects in a screen resolution-independent fashion.")]
        public float radius = 3.0f;

        [SerializeField]
        [Tooltip("Blend factor of the result image.")]
        public float intensity = 0.5f;

        [SerializeField]
        [Range(0.0f, 2.0f)]
        [Tooltip("if over 0 ,stretch bloom")]
        public float stretchOffset = 0.0f;

        [SerializeField]
        [Tooltip("Controls filter quality and buffer resolution.")]
        public bool highQuality = false;

        [SerializeField]
        [Tooltip("Reduces flashing noise with an additional filter.")]
        public bool antiFlicker = false;

        [SerializeField]
        [Range(0.0f, 1.0f)]
        [Tooltip("Radial Sample Distance(0 means No Radial Blur)")]
        public float sampleDist = 0.4f;

        [SerializeField]
        [Range(0.0f, 5.0f)]
        [Tooltip("Radial Sample Strength(0 means No Radial Blur)")]
        public float sampleStrength = 0.0f;

        public bool enableDOF = false;
        [Range(0.0f, 15.0f)]
        public float focalPoint = 5.0f;
        [Range(0.1f, 15.0f)]
        public float smoothness = 4.0f;
        [Range(0.0f, 8.0f)]
        public float maxBlurSpread = 2.0f;
    #endregion

        #region prepare
        [SerializeField, HideInInspector]
        private Shader m_BloomShader;
        private Shader m_BlurShader;
        private Shader m_DOFBlurShader;
        private Shader m_DOFShader;

        public Shader bloomShader
        {
            get
            {
                if (m_BloomShader == null)
                {
                    const string shaderName = "Hidden/Image Effects/UEBloom";
                    m_BloomShader = Shader.Find(shaderName);
                }

                return m_BloomShader;
            }
        }
        public Shader blurShader
        {
            get
            {
                if (m_BlurShader == null)
                {
                    const string blurShaderName = "Hidden/Image Effects/RadialBlur";
                    m_BlurShader = Shader.Find(blurShaderName);
                }

                return m_BlurShader;
            }
        }
        public Shader dofBlurShader
        {
            get
            {
                if (m_DOFBlurShader == null)
                {
                    const string dofBlurShaderName = "Hidden/DofBlurSimple";
                    m_DOFBlurShader = Shader.Find(dofBlurShaderName);
                }

                return m_DOFBlurShader;
            }
        }
        public Shader dofShader
        {
            get
            {
                if (m_DOFShader == null)
                {
                    const string dofShaderName = "Hidden/DofSimple";
                    m_DOFShader = Shader.Find(dofShaderName);
                }

                return m_DOFShader;
            }
        }

         Material CheckShaderAndCreateMaterial(Shader s)
        {
            if (s == null || !s.isSupported)
                return null;

            var material = new Material(s);
            material.hideFlags = HideFlags.DontSave;
            return material;
        }
         public static bool IsSupported(Shader s,MonoBehaviour effect)
         {
#if UNITY_EDITOR
             // Don't check for shader compatibility while it's building as it would disable most effects
             // on build farms without good-enough gaming hardware.
             if (!BuildPipeline.isBuildingPlayer)
             {
#endif
                 if (s == null || !s.isSupported)
                 {
                     Debug.LogWarningFormat("Missing shader for image effect {0}", effect);
                     return false;
                 }

                 if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
                 {
                     Debug.LogWarningFormat("Image effects aren't supported on this device ({0})", effect);
                     return false;
                 }

#if UNITY_EDITOR
             }
#endif

             return true;
         }

        Material m_Material;
        Material m_BlurMaterial;

        Material m_DOFBlurMaterial;
        Material m_DOFMaterial;
        public Material material
        {
            get
            {
                if (m_Material == null)
                    m_Material = CheckShaderAndCreateMaterial(bloomShader);

                return m_Material;
            }
        }
        public Material blurMaterial
        {
            get
            {
                if (m_BlurMaterial == null)
                    m_BlurMaterial = CheckShaderAndCreateMaterial(blurShader);

                return m_BlurMaterial;
            }
        }
        public Material dofBlurMaterial
        {
            get
            {
                if (m_DOFBlurMaterial == null)
                    m_DOFBlurMaterial = CheckShaderAndCreateMaterial(dofBlurShader);

                return m_DOFBlurMaterial;
            }
        }
        public Material dofMaterial
        {
            get
            {
                if (m_DOFMaterial == null)
                    m_DOFMaterial = CheckShaderAndCreateMaterial(dofShader);

                return m_DOFMaterial;
            }
        }

        #endregion

        #region ImageFX Pipeline

        //const int kMaxIterations = 16;
        const int kMaxIterations = 8;
        RenderTexture[] m_blurBuffer1 = new RenderTexture[kMaxIterations];
        RenderTexture[] m_blurBuffer2 = new RenderTexture[kMaxIterations];

        private Camera _camera;
        private bool hasDepth;
        private float focalStartCurve = 2.0f;
        private float focalEndCurve = 2.0f;
        private float focalDistance01 = 0.1f;
        private float widthOverHeight = 1.25f;
        private float oneOverBaseSize = 1.0f / 512.0f;

        float FocalDistance01(float worldDist)
        {
            return _camera.WorldToViewportPoint((worldDist - _camera.nearClipPlane) * _camera.transform.forward + _camera.transform.position).z / (_camera.farClipPlane - _camera.nearClipPlane);
        }

        private void OnEnable()
        {
            if (!IsSupported(bloomShader, this) || !IsSupported(blurShader, this) || !IsSupported(dofBlurShader, this) || !IsSupported(dofShader, this))
            //if (!IsSupported(bloomShader, this) || !IsSupported(blurShader, this) )
                enabled = false;

            _camera = GetComponent<Camera>();
            if (_camera.depthTextureMode == DepthTextureMode.Depth)
                hasDepth = true;

        }

        private void OnDisable()
        {
            if (m_Material != null)
                DestroyImmediate(m_Material);
            m_Material = null;

            if (m_BlurMaterial != null)
                DestroyImmediate(m_BlurMaterial);
            m_BlurMaterial = null;

            if (m_DOFBlurMaterial != null)
                DestroyImmediate(m_DOFBlurMaterial);
            m_DOFBlurMaterial = null;

            if (m_DOFMaterial != null)
                DestroyImmediate(m_DOFMaterial);
            m_DOFMaterial = null;

            //return state of camera
            if (hasDepth)
                _camera.depthTextureMode = DepthTextureMode.Depth;
            else
                _camera.depthTextureMode = DepthTextureMode.None;
        }

         void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            var useRGBM = Application.isMobilePlatform;

            // source texture size
            var tw = source.width;
            var th = source.height;

            // resize  on mobile
            if (useRGBM)
            {
                tw /= 2;
                th /= 2;
            }

            // halve the texture size for the low quality mode
            if (!highQuality)
            {
                tw /= 2;
                th /= 2;
            }

            // blur buffer format
            var rtFormat = useRGBM ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR;

            // determine the iteration count
            var logh = Mathf.Log(th, 2) + radius - 8;
            var logh_i = (int)logh;
            var iterations = Mathf.Clamp(logh_i, 2, kMaxIterations);
            if (useRGBM &&iterations > 4)
                iterations = 4;

            // update the shader properties
            var threshold = thresholdLinear;
            material.SetFloat("_Threshold", threshold);

            var knee = threshold * softKnee + 1e-2f;
            var curve = new Vector3(threshold - knee, knee * 2, 0.25f / knee);
            material.SetVector("_Curve", curve);

            var pfo = !highQuality && antiFlicker;
            material.SetFloat("_PrefilterOffs", pfo ? -0.5f : 0.0f);

            material.SetFloat("_SampleScale", 0.5f + logh - logh_i);
            material.SetFloat("_Intensity", Mathf.Max(0.0f, intensity));
            //material.SetFloat("_Intensity", Mathf.Max(0.0f, useRGBM ? settings.intensity * 0.5f : settings.intensity));

            // prefilter pass
            var prefiltered = RenderTexture.GetTemporary(tw, th, 0, rtFormat);

            //Down Sample
            RenderTexture quaterRez = RenderTexture.GetTemporary(tw, th, 0);

            if (stretchOffset > 0)
            {
                //X  Guass Blur
                Graphics.Blit(source, quaterRez, material, antiFlicker ? 1 : 0);
                material.SetVector("_Offset", new Vector2(stretchOffset * oneOverBaseSize, 0.0f));
                Graphics.Blit(quaterRez, prefiltered, material, 9);
            }
            else
            {
                Graphics.Blit(source, prefiltered, material, antiFlicker ? 1 : 0);
            }

            // construct a mip pyramid
            var last = prefiltered;
            for (var level = 0; level < iterations; level++)
            {
                m_blurBuffer1[level] = RenderTexture.GetTemporary(last.width / 2, last.height / 2, 0, rtFormat);
                Graphics.Blit(last, m_blurBuffer1[level], material, level == 0 ? (antiFlicker ? 3 : 2) : 4);
                last = m_blurBuffer1[level];
            }

            // upsample and combine loop
            //for (var level = iterations - 2; level >= 0; level--)
            for (var level = iterations - 2; level >= 0; level--)
            {
                var basetex = m_blurBuffer1[level];
                material.SetTexture("_BaseTex", basetex);
                m_blurBuffer2[level] = RenderTexture.GetTemporary(basetex.width, basetex.height, 0, rtFormat);
                Graphics.Blit(last, m_blurBuffer2[level], material, highQuality ? 6 : 5);
                last = m_blurBuffer2[level];
            }
            ///to Prefillter
            prefiltered.DiscardContents();
            Graphics.Blit(last, prefiltered, material, highQuality ? 6 : 5);
            // finish process
            material.SetTexture("_BaseTex", source);

              #region DOF
                //IF DOF
                if (enableDOF)
                {
                    _camera.depthTextureMode |= DepthTextureMode.Depth;

                    focalDistance01 = FocalDistance01(focalPoint);
                    focalStartCurve = focalDistance01 * smoothness;
                    focalEndCurve = focalStartCurve;

                    widthOverHeight = tw / th;
                    dofMaterial.SetVector("_CurveParams", new Vector4(1.0f / focalStartCurve, 1.0f / focalEndCurve, 0.0f, focalDistance01));
                    dofMaterial.SetVector("_InvRenderTargetSize", new Vector4(1.0f / tw, 1.0f / th, 0.0f, 0.0f));

                    //DownSample && write COC to Alpha
                    RenderTexture quaterRez1 = RenderTexture.GetTemporary(tw, th, 0);

                    // WRITE COC to alpha channel
                    //Graphics.Blit(source, quaterRez1, dofMaterial, 3);
                    Graphics.Blit(source, quaterRez1, dofMaterial, 6);

                    RenderTexture tmp = RenderTexture.GetTemporary(tw, th);
                    dofBlurMaterial.SetVector("offsets", new Vector4(0.0f, maxBlurSpread * oneOverBaseSize, 0.0f, 0.0f));
                    Graphics.Blit(quaterRez1, tmp, dofBlurMaterial, 1);
                    dofBlurMaterial.SetVector("offsets", new Vector4(maxBlurSpread / widthOverHeight * oneOverBaseSize, 0.0f, 0.0f, 0.0f));
                    quaterRez1.DiscardContents();
                    Graphics.Blit(tmp, quaterRez1, dofBlurMaterial, 1);
                    RenderTexture.ReleaseTemporary(tmp);

                    //FINAL DEFOCUS (background),source + quaterRez1 +prefiltered--->dest
                    material.SetTexture("_TapLowBackground", quaterRez1);
                    material.SetTexture("_PreTex", prefiltered);
                    Graphics.Blit(source, destination, material, 10);
                    //dofMaterial.SetTexture("_TapLowBackground", quaterRez1);

                    RenderTexture.ReleaseTemporary(quaterRez1);
                }
                #endregion
            else
            {
                #region RadialBlur2
                //Radial Blur
                if (sampleDist != 0 && sampleStrength != 0)
                {
                    Graphics.Blit(prefiltered, quaterRez, material, highQuality ? 8 : 7);

                    blurMaterial.SetFloat("_SampleDist", sampleDist);
                    blurMaterial.SetFloat("_SampleStrength", sampleStrength);

                    RenderTexture rtTempB = RenderTexture.GetTemporary(tw, th, 0, RenderTextureFormat.Default);
                    rtTempB.filterMode = FilterMode.Bilinear;
                    // RadialBlurMaterial.SetTexture ("_MainTex", rtTempA);
                    Graphics.Blit(quaterRez, rtTempB, blurMaterial, 0);

                    blurMaterial.SetTexture("_BlurTex", rtTempB);
                    Graphics.Blit(source, destination, blurMaterial, 1);
                    RenderTexture.ReleaseTemporary(rtTempB);
                }
                #endregion
                else
                {
                    if (hasDepth)
                        _camera.depthTextureMode = DepthTextureMode.Depth;
                    else
                        _camera.depthTextureMode = DepthTextureMode.None;

                    Graphics.Blit(prefiltered, destination, material, highQuality ? 8 : 7);
                }

            }

            RenderTexture.ReleaseTemporary(quaterRez);

            // release the temporary buffers
            for (var i = 0; i < kMaxIterations; i++)
            {
                if (m_blurBuffer1[i] != null) RenderTexture.ReleaseTemporary(m_blurBuffer1[i]);
                if (m_blurBuffer2[i] != null) RenderTexture.ReleaseTemporary(m_blurBuffer2[i]);
                m_blurBuffer1[i] = null;
                m_blurBuffer2[i] = null;
            }

            RenderTexture.ReleaseTemporary(prefiltered);
        }

        #endregion
    }

