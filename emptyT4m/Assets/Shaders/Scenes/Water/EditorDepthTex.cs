using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class EditorDepthTex : MonoBehaviour
{
    public bool EnableDepTex = true;

    void OnEnable()
    {

#if UNITY_EDITOR
        if (EnableDepTex)
            if (UnityEditor.SceneView.currentDrawingSceneView != null && UnityEditor.SceneView.currentDrawingSceneView.camera != null)
                UnityEditor.SceneView.currentDrawingSceneView.camera.depthTextureMode = DepthTextureMode.Depth;
#endif
        Camera cam = Camera.main;
        if (!cam)
        {
            return;
        }
        if (EnableDepTex)
            cam.depthTextureMode = DepthTextureMode.Depth;
        else
            cam.depthTextureMode = DepthTextureMode.None;
    }
    void OnDisable()
    {

#if UNITY_EDITOR
        if (UnityEditor.SceneView.currentDrawingSceneView != null && UnityEditor.SceneView.currentDrawingSceneView.camera != null)
            UnityEditor.SceneView.currentDrawingSceneView.camera.depthTextureMode = DepthTextureMode.None;
#endif

        Camera cam = Camera.main;
        if (!cam)
        {
            return;
        }
        cam.depthTextureMode = DepthTextureMode.None;
    }


}
