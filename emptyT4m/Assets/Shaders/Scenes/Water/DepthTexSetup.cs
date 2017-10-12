///Update 3.21: Only render DepthTex when visible
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[ExecuteInEditMode]
public class DepthTexSetup : MonoBehaviour
{
    public enum Quality
    {
        Low,		
        High,		
    }

    //在只有一个物体的情况下是不需要用list的，为了以防万一
    private static List<GameObject> RTobj = new List<GameObject>();//所有的RT物体,第0号索引物体要求渲染RT，其他直接读取RT

    RenderTexture tempDepthRt = null;
    Camera objDepthCam;
    GameObject myDepthCam;
    Shader depShader;

    private Quality quality = Quality.High;
    public LayerMask depthIncludeLayers = 0;//这个层里的物体会加入软边和折射运算

    void ClearWater()
    {
        RTobj.Remove(gameObject);
        if (tempDepthRt && myDepthCam)
        {
            if (RTobj.Count <= 0)
            {
                RenderTexture.ReleaseTemporary(tempDepthRt);
                tempDepthRt = null;
                myDepthCam = null;

                if (Camera.main.transform.FindChild("DepthCam"))
                {
                    Destroy(Camera.main.transform.FindChild("DepthCam").gameObject);
                }

            }
        }
    }

    void InitWater()
    {
        if (Shader.globalMaximumLOD > 300)
            Shader.globalMaximumLOD = 300;
        Camera cam = Camera.main;
        if (!cam)
        {
            enabled = false;
            return;
        }


        depShader = Shader.Find("Hidden/CustomDepth"); //这个shader必须放Graphic里渲染包含进打包!!

        if ( Camera.main.transform.FindChild("DepthCam") != null)
        {
            myDepthCam = Camera.main.transform.FindChild("DepthCam").gameObject;
            objDepthCam = myDepthCam.GetComponent<Camera>();
            tempDepthRt = objDepthCam.targetTexture;
            //RT特效数目加1，并给与当前RT特效索引
            RTobj.Add(gameObject);
        }
        else
        {
            //Debug.Log("CCCCCCCCCCCCCreate!!!" + useIndepDepth.ToString());
            CreateCam();
        }

        Shader.SetGlobalTexture("_LastCameraDepthTexture", tempDepthRt);
            
    }

    void OnBecameInvisible()
    {
        ClearWater();
        Shader.DisableKeyword("_SOFTPAR");
    }

    void OnEnable()
    {

    }

    void OnBecameVisible()
    {
        InitWater();
        Shader.EnableKeyword("_SOFTPAR");
    }

    void CreateCam()//建立渲染RT的摄像机，如果使用自定义深度模式，则这个摄像机也输出深度信息
    {

        myDepthCam = new GameObject("DepthCam", typeof(Camera));
        objDepthCam = myDepthCam.GetComponent<Camera>();
        objDepthCam.enabled = false;
        objDepthCam.CopyFrom(Camera.main);

        //建立一个临时的RenderTex文件，根据平台和屏幕分辨率设定大小，注意在IPone上开启rt抗锯齿可能会报错
        int rtWidth ;
        int rtHeight ;
        int rtAA = 2;

        if (objDepthCam.pixelHeight > 800)
        {
            rtWidth = (int)(objDepthCam.pixelWidth / 4);
            rtHeight = (int)(objDepthCam.pixelHeight / 4);
        }
        else
        {
            rtWidth = (int)(objDepthCam.pixelWidth / 2);
            rtHeight = (int)(objDepthCam.pixelHeight / 2);
        }

        tempDepthRt = RenderTexture.GetTemporary(rtWidth, rtHeight, 16,RenderTextureFormat.Depth);
        tempDepthRt.hideFlags = HideFlags.DontSave;
        tempDepthRt.filterMode = FilterMode.Bilinear;

        objDepthCam.targetTexture = tempDepthRt;
        objDepthCam.clearFlags = CameraClearFlags.SolidColor;
        objDepthCam.cullingMask = depthIncludeLayers;
        myDepthCam.transform.parent = Camera.main.transform;
        myDepthCam.transform.localPosition = Vector3.zero;
        myDepthCam.transform.localRotation = Quaternion.Euler(Vector3.zero);
        myDepthCam.transform.localScale = Vector3.one;
            
        //增加进队列
        RTobj.Add(gameObject);
        //Debug.Log("We have Created new Cam!Water OBJ counts:"+RTobj.Count);
    }

    void RenDepthRefraction()
    {

        if (tempDepthRt && objDepthCam)
            {
                //多个物体时，只有当前物体的索引是0号时渲染
            if (gameObject == RTobj[0] )
                objDepthCam.RenderWithShader(depShader, "RenderType");

                Shader.SetGlobalTexture("_LastCameraDepthTexture", tempDepthRt);
            }
    }
    //void Update()
    void OnWillRenderObject ()
    {
        //Debug.Log("sadfafsafdfsfsaf !!!!!!!!!!!!!!!!!!");
        //是否需要实时更改LOD？
        int sceneLOD = Shader.globalMaximumLOD;
        if (sceneLOD <= 200)
            quality = Quality.Low;
        else
                quality = Quality.High;

        switch (quality)
        {
            case Quality.High:
                RenDepthRefraction();
                break;

            case Quality.Low:
                break;

            default:
                RenDepthRefraction();
                break;
        }

    }
}
