using System;
using UnityEngine;

[ExecuteInEditMode]
public class Water : MonoBehaviour
{
    public bool allowCamraDepth =true;
    void OnEnable()
    {
        var cam = Camera.main;
        if (cam != null)
        {
            if(allowCamraDepth)

                cam.depthTextureMode = DepthTextureMode.Depth;
            else
                cam.depthTextureMode = DepthTextureMode.None;
        }
    }


}
