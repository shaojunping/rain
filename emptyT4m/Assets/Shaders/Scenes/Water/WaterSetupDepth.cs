/// update 12.5 :We dont update RefractTex when invisible.
/// update 12.21:We could use standalone cam to generate depth tex
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[ExecuteInEditMode]
public class WaterSetupDepth : MonoBehaviour {
    public enum Quality
    {
        Low,		// 贴图软边，skybox反射，无折射
        Medium,		//贴图软边，skybox反射，折射
        High,		// 深度软边，skybox反射，折射
    }

    //在只有一个物体的情况下是不需要用list的，为了以防万一
    private static List<GameObject> RTobj = new List<GameObject>();//所有的RT物体,第0号索引物体要求渲染RT，其他直接读取RT
    //private bool enableDepthDefault = false; //系统默认是否开启深度？
    private bool isHighMidMode = false;

    public bool useIndepDepth = false; // 使用完全独立的摄像机去生成深度贴图，不与折射的rt共用camera
    RenderTexture tempDepthRt = null;
    Camera objDepthCam;
    GameObject myDepthCam;
    Shader depShader;

    public Quality quality = Quality.High;
    public LayerMask refrIncludeLayers = 0;//这个层里的物体会加入软边和折射运算

    public bool enableSkipFrame = true;
    [Range(2, 8)]
    [Tooltip("When frame couts larger than this value,skip the RT rendering")]
    public int renSkipFrames = 3;
    private int renFrame = 1;

    void ClearWater()
    {
        RTobj.Remove(gameObject);
        if (tempDepthRt && myDepthCam)
        {
            if (RTobj.Count <= 0)
            {
                RenderTexture.ReleaseTemporary(tempDepthRt);
                //DestroyImmediate(tempRt);
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
        renFrame = 1;
        #if UNITY_EDITOR
                enableSkipFrame = false;
        #endif

        if (Shader.globalMaximumLOD > 300)
            Shader.globalMaximumLOD = 300;
        Camera cam = Camera.main;
        if (!cam)
        {
            enabled = false;
            return;
        }
        if (cam.depthTextureMode == DepthTextureMode.Depth)
        {
            useIndepDepth = false;
            Debug.Log("Default Depth!!!" + useIndepDepth.ToString());
        }

        if (useIndepDepth)
        {
            depShader = Shader.Find("Hidden/CustomDepth"); //这个shader必须放Graphic里渲染包含进打包!!
            //Debug.Log("Use IndepDepth Shader:    " + depShader.name);
        }

        if (useIndepDepth && Camera.main.transform.FindChild("DepthCam") != null)
        {
            myDepthCam = Camera.main.transform.FindChild("DepthCam").gameObject;
            objDepthCam = myDepthCam.GetComponent<Camera>();
            tempDepthRt = objDepthCam.targetTexture;
            RTobj.Add(gameObject);
        }
        else
        {
            //Debug.Log("CCCCCCCCCCCCCreate!!!" + useIndepDepth.ToString());
            CreateCam();
        }

        if (useIndepDepth)
            Shader.SetGlobalTexture("_LastCameraDepthTexture", tempDepthRt);

        //只在初始化时确认是否使用自定义深度
        //if (useCustomDepth)
        if ( useIndepDepth)
        {
            Shader.DisableKeyword("_CUSTOMDEPTH_OFF");
            Shader.EnableKeyword("_CUSTOMDEPTH");
        }
            
    }

    void OnBecameInvisible()
    {
        ClearWater();
        //Debug.Log("We have Clear Water:" + gameObject.name);
    }

    void OnEnable()
    {

    }

    void OnBecameVisible()
    {
        InitWater();
        //Debug.Log(" Insit Water:" + gameObject.name);
    }

    void CreateCam()//建立渲染RT的摄像机，如果使用自定义深度模式，则这个摄像机也输出深度信息
    {

        if (useIndepDepth)
        {
            myDepthCam = new GameObject("DepthCam", typeof(Camera));
            objDepthCam = myDepthCam.GetComponent<Camera>();
            objDepthCam.enabled = false;
            objDepthCam.CopyFrom(Camera.main);
        }

        //建立一个临时的RenderTex文件，根据平台和屏幕分辨率设定大小，注意在IPone上开启rt抗锯齿可能会报错
        int rtWidth ;
        int rtHeight ;
        int rtAA = 2;

        if (Camera.main.pixelHeight > 800)
        {
            rtWidth = (int)(Camera.main.pixelWidth / 4);
            rtHeight = (int)(Camera.main.pixelHeight / 4);
        }
        else
        {
            rtWidth = (int)(Camera.main.pixelWidth / 2);
            rtHeight = (int)(Camera.main.pixelHeight / 2);
        }

        if (useIndepDepth)
        {

            tempDepthRt = RenderTexture.GetTemporary(rtWidth, rtHeight, 24,RenderTextureFormat.Depth);
            tempDepthRt.hideFlags = HideFlags.DontSave;

            objDepthCam.targetTexture = tempDepthRt;
            objDepthCam.clearFlags = CameraClearFlags.SolidColor;
            objDepthCam.cullingMask = refrIncludeLayers;
            myDepthCam.transform.parent = Camera.main.transform;
            myDepthCam.transform.localPosition = Vector3.zero;
            myDepthCam.transform.localRotation = Quaternion.Euler(Vector3.zero);
            myDepthCam.transform.localScale = Vector3.one;

            //Debug.Log("WE CREATE INDEDEPTH TEX!!!!!!!!!!!!!!!!!!!");
        }
            
        //增加进队列
        RTobj.Add(gameObject);
        //Debug.Log("We have Created new Cam!Water OBJ counts:"+RTobj.Count);
    }

    void RenDepthRefraction()
    {
        //隔帧渲染
        if (enableSkipFrame)
            if (renFrame > renSkipFrames)
            {
                //Debug.Log(gameObject.name + " No ren RT:" + renFrame + "   Design rate:" + renSkipFrames);
                renFrame = 1;
                return;
            }


        //if (tempRt &&objCam)
        if (tempDepthRt && objDepthCam)
            {
                //多个物体时，只有当前物体的索引是0号时渲染
                if (gameObject == RTobj[0] && isHighMidMode)
                {
                     objDepthCam.RenderWithShader(depShader, "RenderType");
                }
                if (useIndepDepth)
                    Shader.SetGlobalTexture("_LastCameraDepthTexture", tempDepthRt);
            }
    }
    //void Update()
    void OnWillRenderObject ()
    {
        //是否需要实时更改LOD？
        int sceneLOD = Shader.globalMaximumLOD;
        if (sceneLOD <= 200)
            quality = Quality.Low;
        else
            if (sceneLOD <= 250)
                quality = Quality.Medium;

        if(quality ==Quality.Low)
            isHighMidMode = false;
        else
            isHighMidMode = true;
        switch (quality)
        {
            case Quality.High:
                //Shader.globalMaximumLOD = 300;
                RenDepthRefraction();
                break;

            case Quality.Medium:
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
