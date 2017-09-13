//using UnityEditor;
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class EnvironmentSetup : MonoBehaviour
{
    //fog height
    public bool enableFogHeight = false;
    public float FogHeightStart = 0;
    public float FogHeightEnd = 40;

    public float FogHeiParaZ = 0;
    public float FogHeiParaW = 0;

    //ambient
    //if true,amb will be controlled by weather completely!
    public bool enableAmbient = true;

    [Range(0.0f, 1.0f)]
    public float ambientEffectScale = 0.0f;
    public Color newAmbCol = Color.white;

    //wet
    [Range(0.0f, 1.0f)]
    public float wetEffect= 1.0f;
    public bool enableWet = true;

    //snow
    [Range(0.0f, 0.8f)]
    public float SnowLevel = 0.0f;
    public Texture2D SnowTex;
    public bool enableSnow;

    //rain
    public bool enableRainDisturb = false;
    private int frameIndex =0;
    public float intervalTime = 0.1f;
    [Range(0.0f,1.0f)]
    public float disturbFactor = 0.3f;
    public Texture2D[] rainDisturbTex;
    float countTime;

    // Use this for initialization
    void OnEnable()
    {
    }

    //note： if rain effect is opened, it should be updated each frame
    void Update()
    {
        UpdateRainDisturb();
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(150, 100, 200, 20), "开启下雨"))
        {
            OpenRainDisturb(true);
            Debug.Log("开启下雨");
        }
        if (GUI.Button(new Rect(350, 100, 200, 20), "关闭下雨"))
        {
            OpenRainDisturb(false);
            Debug.Log("关闭下雨");
        }
        if (GUI.Button(new Rect(150, 200, 200, 20), "开启高度雾"))
        {
            OpenHeightFog(true);
            Debug.Log("开启高度雾");
        }
        if (GUI.Button(new Rect(350, 200, 200, 20), "关闭高度雾"))
        {
            OpenHeightFog(false);
            Debug.Log("关闭高度雾");
        }
        if (GUI.Button(new Rect(150, 300, 200, 20), "开启环境光"))
        {
            OpenAmbientEffect(true);
            Debug.Log("开启环境光");
        }
        if (GUI.Button(new Rect(350, 300, 200, 20), "关闭环境光"))
        {
            OpenAmbientEffect(false);
            Debug.Log("关闭环境光");
        }
        if (GUI.Button(new Rect(150, 400, 200, 20), "开启wet"))
        {
            OpenWetEffect(true);
            Debug.Log("开启wet");
        }
        if (GUI.Button(new Rect(350, 400, 200, 20), "关闭wet"))
        {
            OpenWetEffect(false);
            Debug.Log("关闭wet");
        }
        if (GUI.Button(new Rect(150, 500, 200, 20), "开启snow"))
        {
            OpenSnowEffect(true);
            Debug.Log("开启snow");
        }
        if (GUI.Button(new Rect(350, 500, 200, 20), "关闭snow"))
        {
            OpenSnowEffect(false);
            Debug.Log("关闭snow");
        }
    }

    #region heightFog
    public void OpenHeightFog(bool open)
    {
        enableFogHeight = open;
        if(open)
        {
            Shader.SetGlobalFloat("_FogHeiParaZ", FogHeiParaZ);
            Shader.SetGlobalFloat("_FogHeiParaW", FogHeiParaW);
        }
        else
        {
            Shader.SetGlobalFloat("_FogHeiParaZ", 0.0f);
            Shader.SetGlobalFloat("_FogHeiParaW", 1.0f);
        }
    }

    public void SetHeightFog(float fFogHeiParaZ = 0.0f, float fFogHeiParaW = 0.0f)
    {
        FogHeiParaW = fFogHeiParaW;
        FogHeiParaZ = fFogHeiParaZ;
    }
    #endregion

    #region ambientEffect
    public void OpenAmbientEffect(bool open)
    {
        enableAmbient = open;
        if(open)
        {
            Shader.SetGlobalFloat("_AmbScale", ambientEffectScale);
            RenderSettings.ambientSkyColor = newAmbCol;
        }
        else
        {
            Shader.SetGlobalFloat("_AmbScale", 0.0f);
        }
    }
    #endregion

#region wetEffect
    public void OpenWetEffect(bool open)
    {
        if(open)
        {
            OpenAmbientEffect(true);
            OpenSnowEffect(false);
            Shader.SetGlobalFloat("_RefLerp", wetEffect);
            Shader.SetGlobalFloat("_AmbScale", wetEffect * ambientEffectScale);
        }
        else
        {
            Shader.SetGlobalFloat("_RefLerp", 0.0f);
        }
    }

    #endregion
    #region snow
    public void OpenSnowEffect(bool open)
    {
        enableSnow = open;
        if (!open)
        {
            Shader.SetGlobalFloat("_SnowLevel", 0);
        }
        else
        {
            OpenWetEffect(false);
            OpenAmbientEffect(true);
            RenderSettings.ambientSkyColor = newAmbCol;
            Shader.SetGlobalFloat("_SnowLevel", Mathf.GammaToLinearSpace(SnowLevel));
            Shader.SetGlobalTexture("_SnowTex", SnowTex);
            Shader.SetGlobalFloat("_AmbScale", SnowLevel * ambientEffectScale);
        }
        return;
    }
    
    #endregion

#region rain

    public void OpenRainDisturb(bool open)
    {
        enableRainDisturb = open;

        if (!open)
        {
            enableRainDisturb = false;
            Shader.SetGlobalFloat("_DisturbMapFactor", 0.0f);
            Shader.SetGlobalFloat("_RefLerp", 0);
        }
    }
    public void UpdateRainDisturb()
    {
        if (!enableRainDisturb)
            return;
        if(rainDisturbTex != null && rainDisturbTex.Length > frameIndex)
        {
            OpenWetEffect(true);
            Shader.SetGlobalTexture("_DisturbMap", rainDisturbTex[frameIndex]);
            Shader.SetGlobalFloat("_DisturbMapFactor", disturbFactor);
            countTime += Time.deltaTime;
            if (countTime >= intervalTime)
            {
                countTime = 0;
                frameIndex++;
                if (frameIndex >= rainDisturbTex.Length)
                    frameIndex = 0;
            }
        }
    }
#endregion
}

