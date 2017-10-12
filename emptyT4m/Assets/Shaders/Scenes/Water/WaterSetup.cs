/// update 12.5 :We dont update RefractTex when invisible.
/// update 17.2.4 render RT every 2 frames
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[ExecuteInEditMode]
public class WaterSetup : MonoBehaviour {
    public enum Quality
    {
        Low,		// 贴图软边，skybox反射，无折射
        Medium,		//贴图软边，skybox反射，折射
        High,		// 深度软边，skybox反射，折射
    }

    RenderTexture tempRt = null;
    Camera objCam; //渲染RT的摄像机的Camera组件
    GameObject myCam;//渲染RT的摄像机，用于提供折射和深度缓存
    //在只有一个物体的情况下是不需要用list的，为了以防万一
    private static List<GameObject> RTobj = new List<GameObject>();//所有的RT物体,第0号索引物体要求渲染RT，其他直接读取RT
    //private bool enableDepthDefault = false; //系统默认是否开启深度？
    private bool isHighMidMode = false;

    public bool useCustomDepth = true; //是否使用我们自己定义的camera生成深度贴图，5.3以后可以开启提高速度
    public Quality quality = Quality.High;
    public LayerMask refrIncludeLayers = 0;//这个层里的物体会加入软边和折射运算

    public bool enableSkipFrame = true;
    [Range(2,8)]
    [Tooltip("When frame couts larger than this value,skip the RT rendering")]
    public int renSkipFrames = 5;
    private int renFrame = 1;

    void ClearWater()
    {
        RTobj.Remove(gameObject);
        if (tempRt && myCam)
        {
            if (RTobj.Count <= 0)
            {
                RenderTexture.ReleaseTemporary(tempRt);
                //DestroyImmediate(tempRt);
                tempRt = null;
                myCam = null;
                if (Camera.main.transform.FindChild("GrabCam"))
                {
                    Destroy(Camera.main.transform.FindChild("GrabCam").gameObject);
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
            useCustomDepth = false;//如果本来就有深度模式，则不需要自定义深度贴图
        }

        //如果主摄像机下已经有GrabCam，则使用之，否则新建
        if (Camera.main.transform.FindChild("GrabCam") != null)
        {
            myCam = Camera.main.transform.FindChild("GrabCam").gameObject;
            objCam = myCam.GetComponent<Camera>();
            tempRt = objCam.targetTexture;
            //RT特效数目加1，并给与当前RT特效索引
            RTobj.Add(gameObject);
            //Debug.Log("Water OBJ counts:" + RTobj.Count);
        }
        else
        {
            CreateCam();
        }
        Shader.SetGlobalTexture("_RefractTex", tempRt);

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
        //建立新摄像机并作为主摄像机的子物体
        myCam = new GameObject("GrabCam", typeof(Camera));
        objCam = myCam.GetComponent<Camera>();
        objCam.enabled = false;
        objCam.CopyFrom(Camera.main);
        //建立一个临时的RenderTex文件，根据平台和屏幕分辨率设定大小，注意在IPone上开启rt抗锯齿可能会报错
        if (!tempRt)
        {
            int rtWidth;
            int rtHeight;
            int rtAA =2;

            if (objCam.pixelHeight > 800)
            {
                rtWidth = (int)(objCam.pixelWidth / 4);
                rtHeight = (int)(objCam.pixelHeight / 4);
            }
            else
            {
                rtWidth = (int)(objCam.pixelWidth / 2);
                rtHeight = (int)(objCam.pixelHeight / 2);
            }
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2)
            {
                rtAA = 1;
            }

            tempRt = RenderTexture.GetTemporary(rtWidth, rtHeight, 16, RenderTextureFormat.Default, RenderTextureReadWrite.Default, rtAA);
            tempRt.filterMode = FilterMode.Bilinear;
            tempRt.hideFlags = HideFlags.DontSave;
        }
        objCam.targetTexture = tempRt;
        objCam.clearFlags = CameraClearFlags.SolidColor;
        objCam.cullingMask = refrIncludeLayers;
        myCam.transform.parent = Camera.main.transform;
        myCam.transform.localPosition = Vector3.zero;
        myCam.transform.localRotation = Quaternion.Euler(Vector3.zero);
        myCam.transform.localScale = Vector3.one;
        //5.3之后才能正常使用自定义深度贴图,鉴于目前项目组使用情况，不再对5.3以下版本进行支持
        //#if UNITY_5_3_OR_NEWER
        //        useCustomDepth =useCustomDepth;
        //#else
        //        useCustomDepth = false;
        //#endif

        if (useCustomDepth)
            objCam.depthTextureMode = DepthTextureMode.Depth;
        else
            objCam.depthTextureMode = DepthTextureMode.None;
            
        //增加进队列
        RTobj.Add(gameObject);
        //Debug.Log("We have Created new Cam!Water OBJ counts:"+RTobj.Count);
    }

    void RenDepthRefraction()
    {
        //隔帧渲染
        if(enableSkipFrame)
            if (renFrame > renSkipFrames)
            {
                //Debug.Log(gameObject.name + " No ren RT:" + renFrame + "   Design rate:" + renSkipFrames);
                renFrame = 1;
                return;
            }

        if (tempRt &&objCam)
            {
                //多个物体时，只有当前物体的索引是0号时渲染
                if (gameObject == RTobj[0] && isHighMidMode)
                    objCam.Render();
                Shader.SetGlobalTexture("_RefractTex", tempRt);
                //Debug.Log(gameObject.name + " Ren RT:" + renFrame);
            }
        if (enableSkipFrame)
            renFrame++;
    }

    void OnWillRenderObject ()
    {
        //是否需要实时更改LOD？
        int sceneLOD = Shader.globalMaximumLOD;
        if (sceneLOD <= 200)
        {
            quality = Quality.Low;
            isHighMidMode = false;
        }
        else
            if (sceneLOD <= 250)
            {
                quality = Quality.Medium;
                isHighMidMode = true;
            }
            else
                if (sceneLOD <= 300)
                {
                    quality = Quality.High;
                    isHighMidMode = true;
                }

        switch (quality)
        {
            case Quality.High:
                if (Camera.main.depthTextureMode == DepthTextureMode.Depth)
                    objCam.depthTextureMode = DepthTextureMode.None;
                else
                    objCam.depthTextureMode = DepthTextureMode.Depth;

                RenDepthRefraction();
                break;

            case Quality.Medium:
                objCam.depthTextureMode = DepthTextureMode.None;
                RenDepthRefraction();
                break;

            case Quality.Low:
                objCam.depthTextureMode = DepthTextureMode.None;
                break;

            default:
                if (Camera.main.depthTextureMode == DepthTextureMode.Depth)
                    objCam.depthTextureMode = DepthTextureMode.None;
                else
                    objCam.depthTextureMode = DepthTextureMode.Depth;

                 RenDepthRefraction();
                break;
        }

    }
}
