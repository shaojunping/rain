	using UnityEngine;
	using System.Collections;
    using System.Collections.Generic;
    //using System.Runtime.Remoting.Messaging;

public class Heat_frame : MonoBehaviour
	{
	    Material m;
	    RenderTexture tempRt = null;
	    Camera objCam; //渲染RT的摄像机的Camera组件
        GameObject myCam;//渲染RT的摄像机
        //private static int useCount = 0;//使用RT的特效数目索引，0表示1个
        private static List<GameObject> RTobj = new List<GameObject>();//所有的RT物体,第0号索引物体要求渲染RT，其他直接读取RT

	    int lastLayer;//存放特效的原始层级
	    public int VFXLayerIndex = 8;//使用RT的特效物体会放进这个层
        public LayerMask excludeLayers = 0;//渲染RT包括层，一般来说VFXLayerIndex表示的那个层要去掉

	void CreatCam ()//建立渲染RT的摄像机
	{
        //建立新摄像机并作为主摄像机的子物体
		myCam = new GameObject ("GrabCam", typeof(Camera));
		objCam = myCam.GetComponent<Camera> ();
		objCam.enabled = false;
		objCam.CopyFrom (Camera.main);
		myCam.transform.parent = Camera.main.transform;
		myCam.transform.localPosition = Vector3.zero;
		myCam.transform.localRotation = Quaternion.Euler (Vector3.zero);
		myCam.transform.localScale = Vector3.one;
		//建立一个临时的RenderTex文件，根据屏幕分辨率设定大小
		if (!tempRt) {
			if(objCam.pixelHeight > 800)
			{
				tempRt = RenderTexture.GetTemporary (Mathf.FloorToInt(objCam.pixelWidth /4),Mathf.FloorToInt(objCam.pixelHeight /4), 16);
			}
			else
			{
				tempRt = RenderTexture.GetTemporary (Mathf.FloorToInt(objCam.pixelWidth /2),Mathf.FloorToInt(objCam.pixelHeight /2), 16);
			}

			tempRt.hideFlags = HideFlags.DontSave;		
		}
		objCam.targetTexture = tempRt;
        //设置 摄影机 渲染的layer ,5层是UI,VFXLayerIndex层是专门做了个分层给使用这个材质的物体
        //objCam.cullingMask = ~((1 << VFXLayerIndex) | (1 << 5));
        objCam.cullingMask = excludeLayers ;

        //增加进队列
        RTobj.Add(gameObject);

        //Debug.Log("Count:" + useCount + "        currentOBJ:" + gameObject.name );
	}

	void OnEnable ()
	{
      //判断是否有需要的Render组件
		if (GetComponent<MeshRenderer> ()) 
		{
			m = GetComponent<MeshRenderer> ().material;
		} else 
			{
				if (GetComponent<ParticleSystem> ())
				{
					m = GetComponent<ParticleSystem> ().GetComponent<Renderer> ().material;
				} else
					{
					if (GetComponent<SkinnedMeshRenderer>())
						{
						m = GetComponent<SkinnedMeshRenderer> ().material;
						}else
						{
							return;
						}
					}

			}
        //如果主摄像机下已经有GrabCam，则使用之，否则新建
			if (Camera.main.transform.FindChild("GrabCam") != null) {
				myCam = Camera.main.transform.FindChild("GrabCam").gameObject;
				objCam =myCam.GetComponent<Camera> ();
				tempRt = objCam.targetTexture;

        //RT特效数目加1，并给与当前RT特效索引
                //useCount++;
                RTobj.Add(gameObject);

                //Debug.Log("RTobj counts:"+RTobj.Count);
                //Debug.Log("Count:" + useCount + "        currentOBJ:" + gameObject.name );
			} else {
				CreatCam ();
			}
		//设置 物体的layer，这个层级可以根据项目进行修订
			lastLayer = gameObject.layer;
            gameObject.layer = VFXLayerIndex;
			
		}

	    void DestoryRTCam()
	    {

	        RTobj.Remove(gameObject);
            //useCount--;
            //Debug.Log("RTobj counts:"+RTobj.Count);
            //Debug.Log("Count:" + useCount + "        currentOBJ:" + gameObject.name);
            
			gameObject.layer = lastLayer;
            //如果是最后一个RT物体，则销毁对应的camra和RT贴图
			if (tempRt &&myCam) {

				if (RTobj.Count <= 0)
				{
					RenderTexture.ReleaseTemporary (tempRt);
					tempRt = null;
					myCam = null;
					if(Camera.main.transform.FindChild("GrabCam"))
					{
						Destroy(Camera.main.transform.FindChild("GrabCam").gameObject);
					}
					
				}
			}
	    }
		void OnDestory ()
		{
            DestoryRTCam();
		}
		void OnDisable ()
		{
            DestoryRTCam();
		}
	// Update is called once per frame
		void Update ()
		{
			if (tempRt )
            {
				if(objCam)
				{
                    //只有当前物体的索引是0号时渲染
                    if(gameObject ==RTobj[0])
				    {
                        objCam.Render();
				    }

					m.SetTexture ("_MainTex", tempRt);
				}
				
			}
    
		}
	}
			