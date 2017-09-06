//using UnityEditor;
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class EnvironmentSetup : MonoBehaviour
{
    public float FogHeightStart = 0;
    public float FogHeightEnd = 40;
    //if true,amb will be controlled by weather completely!
    public bool weatherAmbient = true;

    [Range(0.0f,1.0f)]
    public float ambientEffectScale = 0.0f;
    public Color newAmbCol = Color.white;

    [Range(0.0f, 1.0f)]
    public float ambEffectOverall = 0.0f;

    [Range(0.0f, 1.0f)]
    public float wetEffect= 0.0f;

    [Range(0.0f, 0.8f)]
    public float SnowLevel = 0.0f;
    public Texture2D SnowTex;

    public Texture2D[] SecondLMTex;
    [Range(0.0f, 1.0f)]
    public float LMLerp;

    public bool enableRainDisturb =false;
    private int frameIndex =0;
    public float intervalTime = 0.1f;
    [Range(0.0f,1.0f)]
    public float disturbFactor = 0.3f;
    public Texture2D[] rainDisturbTex;
    float countTime = 0;

    //public Texture2D FogTexture;
    //public Color GradientFogColor;
    // Use this for initialization
    void OnEnable()
    {
        //Shader.SetGlobalFloat("_RefLerp", Mathf.GammaToLinearSpace(wetEffect));
        //Shader.SetGlobalTexture("_FogTex", FogTexture);
        //Shader.SetGlobalColor("_GradientFogColor", GradientFogColor);
    }

    void Update()
    {
        SetRainDisturb();
    }

    public void SetHeightFog()
    {
        float FogHeiParaZ = 1 / (FogHeightEnd - FogHeightStart);
        float FogHeiParaW = -FogHeightStart / (FogHeightEnd - FogHeightStart);
        if (FogHeightStart > FogHeightEnd)
            FogHeightStart = FogHeightEnd;
        Shader.SetGlobalFloat("_FogHeiParaZ", FogHeiParaZ);
        Shader.SetGlobalFloat("_FogHeiParaW", FogHeiParaW);
    }

    public void SetAmbEffect()
    {
        RenderSettings.ambientSkyColor = newAmbCol;
        if (!weatherAmbient) 
            Shader.SetGlobalFloat("_AmbScale", ambientEffectScale);
    }

    public void SetWetEffect()
    {
        Shader.SetGlobalFloat("_RefLerp", wetEffect);
        RenderSettings.ambientSkyColor = newAmbCol;
        if(weatherAmbient)
            if (wetEffect > 0.0f && SnowLevel == 0.0f)
                Shader.SetGlobalFloat("_AmbScale", ambientEffectScale * wetEffect);
            else
                if (SnowLevel > 0.0f && wetEffect == 0.0f)
                    Shader.SetGlobalFloat("_AmbScale", ambientEffectScale * SnowLevel);
    }

    public void SetSnowEffect()
    {
        Shader.SetGlobalFloat("_SnowLevel", Mathf.GammaToLinearSpace(SnowLevel));
        Shader.SetGlobalTexture("_SnowTex", SnowTex);
        RenderSettings.ambientSkyColor = newAmbCol;
        if (weatherAmbient)
            if (wetEffect > 0.0f && SnowLevel == 0.0f)
                Shader.SetGlobalFloat("_AmbScale", ambientEffectScale * wetEffect);
            else
                if (SnowLevel > 0.0f && wetEffect == 0.0f)
                    Shader.SetGlobalFloat("_AmbScale", ambientEffectScale * SnowLevel);
    }

    public void SetLMLerp()
    {
        Shader.SetGlobalFloat("_LMLerp", LMLerp);
    }

    public void SetRainDisturb()
    {
        if (!enableRainDisturb)
        {
            Shader.SetGlobalFloat("_DisturbMapFactor", 0.0f);
            return;
        }
        //Add Lpy 2017.9.4 这个_reflerp的赋值在编辑类中，不能放在编辑类里
        Shader.SetGlobalFloat("_RefLerp", wetEffect);
        Shader.SetGlobalTexture("_DisturbMap", rainDisturbTex[frameIndex]);
        Shader.SetGlobalFloat("_DisturbMapFactor", disturbFactor);
        countTime += Time.deltaTime;
        if (countTime >= intervalTime)
        {
            countTime = 0;
            frameIndex++;
            if (frameIndex >=rainDisturbTex.Length)
                frameIndex = 0;
        }

    }
}

