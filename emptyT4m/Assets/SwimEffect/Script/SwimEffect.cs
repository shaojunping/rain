using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using UnityEngine.UI;

namespace TSHD.SwimEffect
{
    public class SwimEffect : MonoBehaviour
    {
        //Fog
        private FogMode _defFogMode;
        private float _defFogDensity;
        private Color _defFogColor;
        private bool _defFogEnabled;

        public float _fogStart = 0f;
        public float _fogEnd = 50;
        public Color _fogColor = new Color(0.2f, 0.65f, 0.75f, 0.5f);

        //AmbientLight
        private Color _defAmbientColor;
        public Color _underWaterAmbientColor = new Color(0, 1f, 1f, 1f);

        //HeightFog
        public float _fogHeightStart = 0;
        public float _fogHeightEnd = 40;
        public Color _fogHeightColor = new Color(0.2f, 0.65f, 0.75f, 0.5f);
        private float _heightFactor = 0f;
        public float _heightFogDensity = 0.5f;

        //Mat
        public GameObject _playerObj;
        //private SkinnedMeshRenderer _render;
        //private Material _material;
        //private Color _defMainColor;
        //private Color _defEmissionColor;

        //Blur
        private BlurPostEffect _blur;
        public float _blurSize = 0.5f;
        public int _blurIterations = 2;
        public int _downsample = 1;

        //underWaterLens
        private UnderWaterPostEffect _underWaterPostEffect;
        public Texture2D _distortionTexture;
        public float _distortionIntensity = 0.15f;
        public float _distortionSpeed = 3f;

        //wetLens
        private WetLensPostEffect _wetLensPostEffect;
        private float _outWaterTime = 1.5f;
        private float _wetTime = 0f;
        public float _wetLensDryTime = 2f;
        private Texture2D _normalTexture;
        public Texture2D[] _normalTextureArray;
        public int _wetLensFps = 6;
        private int _curTextureIndex = 0;
        private float _lastFrameTime = 0f;
        

        //Caustics
        public Projector _projector;
        public float _causticFps = 32f;
        public Texture2D[] _causticFrames;
        private int _frameIndex;
        private float _lastCausticTime = 0f;

        //Camera: is under water
        public Camera _camera;
        private bool _isCameraInWater = false;

        private MaterialPropertyBlock _materialPropertyBlcok;

        public bool IsUnderWater
        {
            set
            {
                _isCameraInWater = value;
                _underWaterPostEffect.enabled = _isCameraInWater;
                _wetLensPostEffect.enabled = !_isCameraInWater;
                _blur.enabled = _isCameraInWater;
                _projector.enabled = _isCameraInWater;

                if (_isCameraInWater)
                {
                    _outWaterTime = 0f;
                    this.enabled = true;
                    _curTextureIndex = 0;
                    _normalTexture = _normalTextureArray[_curTextureIndex];
                    _wetLensPostEffect._normalTexture = _normalTexture;
                    //StopAllCoroutines();
                    RenderSettings.fog = true;
                    RenderSettings.fogColor = _fogColor;
                    RenderSettings.fogMode = FogMode.Linear;
                    RenderSettings.fogStartDistance = _fogStart;
                    RenderSettings.fogEndDistance = _fogEnd;
                }
                else
                {
                    _outWaterTime = 0f;
                    RenderSettings.fogColor = _defFogColor;
                    RenderSettings.fogDensity = _defFogDensity;
                    RenderSettings.fog = _defFogEnabled;
                    RenderSettings.fogMode = _defFogMode;
                    //StartCoroutine(OutOfWater());
                }
            }
        }
        //public float _waterSurfacePosY = 0.0f;

        #region UIs
        public Toggle _blurToggle;
        public Toggle _waterLensToggle;
        public Toggle _airLensToggle;
        public Toggle _projectorToggle;
        public Toggle _switchToggle;
        public Toggle _underWaterToggle;
        #endregion

        public static SwimEffect _swimInstance;
        public static float HeightFoStart
        { get { return _swimInstance._fogHeightStart; } }

        public static float HeightFogEnd
        { get { return _swimInstance._fogHeightEnd; } }

        public static Color HeightFogColor
        { get { return _swimInstance._fogHeightColor; } }

        void Awake()
        {
            _swimInstance = this;
        }

        // Use this for initialization
        void Start()
        {
            //_materialPropertyBlcok = new MaterialPropertyBlock();

            //_render = _playerObj.GetComponent<SkinnedMeshRenderer>();
            //if (_render == null)
            //{
            //    _render = _playerObj.GetComponentInChildren<SkinnedMeshRenderer>();

            //}
            //if (_render != null)
            //{
            //    _material = _render.sharedMaterial;
            //    _defMainColor = _material.GetColor("_Color");
            //    _defEmissionColor = _material.GetColor("_EmissionColor");

            //    _render.GetPropertyBlock(_materialPropertyBlcok);
            //    //_defMainColor = _materialPropertyBlcok.GetVector("_Color");
            //    //_defEmissionColor = _materialPropertyBlcok.GetVector("_EmissionColor");
            //}

            //fog
            _defFogMode = RenderSettings.fogMode;
            _defFogDensity = RenderSettings.fogDensity;
            _defFogColor = RenderSettings.fogColor;
            _defFogEnabled = RenderSettings.fog;

            //ambientColor
            _defAmbientColor = RenderSettings.ambientSkyColor;

            //material
            //_material = _render.material;

            //_defMainColor = _material.GetColor("_Color");
            //_defEmissionColor = _material.GetColor("_EmissionColor");

            //InvokeRepeating("CausticsNextFrame", 1 / _projectorFps, 1 / _projectorFps);

            //wet lens
            _wetLensPostEffect = _camera.GetComponent<WetLensPostEffect>();
            _curTextureIndex = 0;
            _normalTexture = _normalTextureArray[_curTextureIndex];

            //under water lens
            _underWaterPostEffect = _camera.GetComponent<UnderWaterPostEffect>();

            //blur
            _blur = _camera.GetComponent<BlurPostEffect>();
        }

        // Update is called once per frame
        void Update()
        {
            //general
            if (!CheckIsInited())
            {
                Debug.LogError("Swim.Update: some gameobject or component not set, please check it!");
                return;
            }

            CalcHeightFactor();
            
            SetAmbColor();
            
            SetHeightFog();
            
            //SetMaterialValues();

            SetUnderWaterLens();
          
            SetWetLens();

            SetBlur();

            SetCaustics();


        }

        void OnDisable()
        {
            //_material.SetColor("_Color", _defMainColor);
            //_material.SetColor("_EmissionColor", _defEmissionColor);
        }

        #region general
        private bool CheckIsInited()
        {
            bool isInited = true;
            if (_playerObj == null || _blur == null || _underWaterPostEffect == null ||
                _wetLensPostEffect == null || _projector == null || _camera == null)
            {
                isInited = false;
            }
            return isInited;
        }

        private void CalcHeightFactor()
        {
            float curPosY = _playerObj.transform.position.y;
            if (curPosY > _fogHeightEnd)
            {
                _heightFactor = 1f;
            }
            else if (curPosY < _fogHeightStart)
            {
                _heightFactor = 0f;
            }
            else
            {
                if (_fogHeightStart > _fogHeightEnd)
                {
                    _fogHeightStart = _fogHeightEnd - 0.1f;
                }
                _heightFactor = (curPosY - _fogHeightStart) / (_fogHeightEnd - _fogHeightStart);
            }
        }

        private IEnumerator OutOfWater()
        {
            _outWaterTime = 0;
            yield return new WaitForSeconds(_wetLensDryTime);
            _wetLensPostEffect.enabled = false;
            this.enabled = false;
        }

        #endregion

        #region UIs
        public void SwitchToggleChanged(bool isOn)
        {
            _blurToggle.gameObject.SetActive(isOn);
            _waterLensToggle.gameObject.SetActive(isOn);
            _airLensToggle.gameObject.SetActive(isOn);
            _projectorToggle.gameObject.SetActive(isOn);
            _underWaterToggle.gameObject.SetActive(!isOn);

            _blur.enabled = false;
            _underWaterPostEffect.enabled = false;
            _wetLensPostEffect.enabled = false;
            _projector.enabled = false;

            _blurToggle.isOn = false;
            _waterLensToggle.isOn = false;
            _airLensToggle.isOn = false;
            _projectorToggle.isOn = false;
            _underWaterToggle.isOn = false;
        }

        public void BlurToggleChanged(bool isOn)
        {
            _blur.enabled = isOn;
        }

        public void WaterLensToggleChanged(bool isOn)
        {
            Debug.Log("SwimEffect WaterLensToggleChanged isOn: " + isOn);
            _underWaterPostEffect.enabled = isOn;
        }

        public void AirLensToggleChanged(bool isOn)
        {
            _outWaterTime = 0f;
            _wetLensPostEffect.enabled = isOn;
        }

        public void ProjectorToggleChanged(bool isOn)
        {
            _projector.enabled = isOn;
        }

        public void UnderWaterToggleChanged(bool isOn)
        {
            IsUnderWater = isOn;
        }

        #endregion

        #region ambientColor
        public void SetAmbColor()
        {
            RenderSettings.ambientSkyColor = _isCameraInWater ?
                Color.Lerp(_defAmbientColor, _underWaterAmbientColor, 1 - _heightFactor) : _defAmbientColor;
        }
        #endregion

        #region heightFog
        public void SetHeightFog()
        {
            //如果不在水里，则关闭高度雾：
            if (!_isCameraInWater)
            {
                Shader.SetGlobalFloat("_FogHeiDen", 0f);
            }
            else
            {
                Shader.SetGlobalFloat("_FogHeiDen", _heightFogDensity);
            }

            if (_fogHeightStart > _fogHeightEnd)
            {
                _fogHeightStart = _fogHeightEnd - 0.1f;
            }
            float fogHeiParaZ = 1 / (_fogHeightEnd - _fogHeightStart);
            float fogHeiParaW = -_fogHeightStart / (_fogHeightEnd - _fogHeightStart);
            Shader.SetGlobalFloat("_FogHeiParaZ", fogHeiParaZ);
            Shader.SetGlobalFloat("_FogHeiParaW", fogHeiParaW);
            Shader.SetGlobalColor("_HeightFogColor", _fogHeightColor);
        }
        #endregion

        #region material
        //private void SetMaterialValues()
        //{
        //    if (_isCameraInWater)
        //    {
        //        _materialPropertyBlcok.SetColor("_Color", Color.Lerp(_defMainColor, _fogHeightColor, 1 - _heightFactor));
        //        _materialPropertyBlcok.SetColor("_EmissionColor", Color.Lerp(_defEmissionColor, _fogHeightColor, 1 - _heightFactor));
        //    }
        //    else
        //    {
        //        _materialPropertyBlcok.SetColor("_Color", _defMainColor);
        //        _materialPropertyBlcok.SetColor("_EmissionColor", _defEmissionColor);
        //    }
        //    _render.SetPropertyBlock(_materialPropertyBlcok);
        //}
        #endregion

        #region UnderWaterLens

        private void SetUnderWaterLens()
        {
            _underWaterPostEffect._distortionTexture = _distortionTexture;
            _underWaterPostEffect._distortionIntensity = _distortionIntensity;
            _underWaterPostEffect._distortionSpeed = _distortionSpeed;
        }

        #endregion

        #region airLens
        private void SetWetLens()
        {
            if (_wetLensPostEffect.enabled)
            {
                _outWaterTime += Time.deltaTime;
                _wetLensPostEffect._refraction = Mathf.Lerp(1, 0, _outWaterTime / _wetLensDryTime);

                if (_outWaterTime >= _wetLensDryTime)
                {
                    _wetLensPostEffect.enabled = false;
                    this.enabled = false;
                }

                float interTime = 1f / _wetLensFps;

                if (Time.time - _lastFrameTime < interTime)
                {
                    return;
                }

                _curTextureIndex++;
                //如果已经播放完了，则返回
                if (_curTextureIndex > _normalTextureArray.Length - 1)
                {
                    return;
                }
                _lastFrameTime = Time.time;
                _normalTexture = _normalTextureArray[_curTextureIndex];
                _wetLensPostEffect._normalTexture = _normalTexture;


                //if (_outWaterTime <= _wetTime)
                //{
                //    _wetLensPostEffect._refraction = 1;
                //    _wetLensPostEffect._transparency = 0.01f;
                //}
                //else
                //{
                //    _wetLensPostEffect._refraction = Mathf.Lerp(1, 0, (_outWaterTime - _wetTime) / _dryTime);
                //    _wetLensPostEffect._transparency = Mathf.Lerp(0.01f, 0, (_outWaterTime - _wetTime) / _dryTime);
                //}
            }
        }
        #endregion


        #region blur

        private void SetBlur()
        {
            if (_blur.enabled)
            {
                _blur.downsample = _downsample;
                _blur.blurSize = _blurSize;
                _blur.blurIterations = _blurIterations;
            }
        }

        #endregion

        #region Caustics
        void SetCaustics()
        {
            if (!_projector.enabled)
            {
                return;
            }
            float causticInterTime = 1f / _causticFps;
            if (Time.time - _lastCausticTime < causticInterTime)
            {
                return;
            }
            _lastCausticTime = Time.time;
            _projector.material.SetTexture("_Texture", _causticFrames[_frameIndex]);
            _frameIndex = (_frameIndex + 1) % _causticFrames.Length;
        }

        #endregion
    }
}



