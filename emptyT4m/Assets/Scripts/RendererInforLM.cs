using UnityEngine;
using System.Collections;

public class RendererInforLM : MonoBehaviour
{
    public int lightmapindex = 65535;
    public Vector4 lightmapScaleOffset = new Vector4(1,1,0,0);
    public GameObject LMcon;

    private int orLMIndex;
    private Vector4 orLMScaleOffset;
    //public Texture2D[] SecondLMTex;

    void Awake()
    { 
        var renderer = this.transform.GetComponent<Renderer>();

        //Debug.Log("LMindex:" + renderer.lightmapIndex + "   LMScaleOffset:" + renderer.lightmapScaleOffset);
        lightmapindex = renderer.lightmapIndex;
        lightmapScaleOffset = renderer.lightmapScaleOffset;
        
        //var SecLMTex = gameObject.GetComponent<LightMapSwitcher>().NightFar;
        var SecLMTex = LMcon.GetComponent<EnvironmentSetup>().SecondLMTex;
        MaterialPropertyBlock block =new MaterialPropertyBlock();
        renderer.GetPropertyBlock(block);
        block.SetTexture("_SecLMTex", SecLMTex[renderer.lightmapIndex]);
        renderer.SetPropertyBlock(block);
    }

    #region Publics
    public void GetLM()
    {
        var renderer = this.transform.GetComponent<Renderer>();
        orLMIndex = renderer.lightmapIndex;
        orLMScaleOffset = renderer.lightmapScaleOffset;
    }

    public void SetLM()
    {
        var renderer = this.transform.GetComponent<Renderer>();
        renderer.lightmapIndex = lightmapindex;
        renderer.lightmapScaleOffset = lightmapScaleOffset;
    }

    #endregion


    #region Debug
    [ContextMenu("Get LM")]
    void Debug00()
    {
        GetLM();
    }

    [ContextMenu("Set LM")]
    void Debug01()
    {
        SetLM();
    }

    #endregion

    void OnEnable()
    {
    }
}
