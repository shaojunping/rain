using UnityEngine;
using System.Collections;

namespace TSHD.SwimEffect
{
    public class WetLensPostEffect : SwimPostEffectBase
    {
        public Shader _wetLensShader = null;
        private Material _wetLensMaterial = null;

        [HideInInspector]
        public Texture2D _normalTexture = null;

        [HideInInspector]public float _refraction = 0f;


        private float _lastFrameTime = 0f;


        void OnDisable()
        {
            //CancelInvoke("NextFrame");
            if (_wetLensMaterial)
                DestroyImmediate(_wetLensMaterial);
        }

        public override bool CheckResources()
        {
            CheckSupport(false);

            _wetLensMaterial = CheckShaderAndCreateMaterial(_wetLensShader, _wetLensMaterial);

            if (_wetLensMaterial != null && _normalTexture != null)
            {
                _wetLensMaterial.SetTexture("_Normal", _normalTexture);
            }

            if (!isSupported)
                ReportAutoDisable();
            return isSupported;
        }


        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (CheckResources() == false)
            {
                Graphics.Blit(source, destination);
                return;
            }
            _wetLensMaterial.SetFloat("_Refraction", _refraction);
            
            Graphics.Blit(source, destination, _wetLensMaterial);

            //RenderTexture.ReleaseTemporary(rt);
        }
    }

    

}

