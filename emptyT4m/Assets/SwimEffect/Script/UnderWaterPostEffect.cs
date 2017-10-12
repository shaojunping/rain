using UnityEngine;
using System.Collections;


namespace TSHD.SwimEffect
{
    public class UnderWaterPostEffect : SwimPostEffectBase
    {
        public Shader _underWaterLensShader = null;
        private Material _underWaterLensMaterial = null;

        [HideInInspector]public Texture2D _distortionTexture = null;

        [HideInInspector]
        public float _distortionIntensity = 0.15f;
        [HideInInspector]
        public float _distortionSpeed = 3f;

        public override bool CheckResources()
        {
            CheckSupport(false);

            _underWaterLensMaterial = CheckShaderAndCreateMaterial(_underWaterLensShader, _underWaterLensMaterial);

            if (_underWaterLensMaterial != null && _distortionTexture != null)
            {
                _underWaterLensMaterial.SetTexture("_DistortionTexture", _distortionTexture);
            }

            if (!isSupported)
                ReportAutoDisable();
            return isSupported;
        }

        public void OnDisable()
        {
            if (_underWaterLensMaterial)
                DestroyImmediate(_underWaterLensMaterial);
        }

        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (CheckResources() == false)
            {
                Graphics.Blit(source, destination);
                return;
            }
            _underWaterLensMaterial.SetFloat("_DistortionIntensity", _distortionIntensity);
            _underWaterLensMaterial.SetFloat("_DistortionSpeed", _distortionSpeed);

            Graphics.Blit(source, destination, _underWaterLensMaterial);
        }

    }
}



