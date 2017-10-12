using UnityEngine;
using System.Collections;

public class ToogleWet : MonoBehaviour {
    public GameObject wetObj;
    GameObject tarObj = null;
    public float tranTime = 3.0f;  //Time of Transfroming
    //public bool useTriggerReflection = true; //tirgger to toggle realtime relfection or not
    ReflectionSetup objRef;

    float changeTime1, changeTime2,getWetTime,getDryTime;// dry2wet,wet2dry,dry2wet Delay,wet2dry Delay
    bool enterWet, outWet, inRain, outRain; //all states
    Renderer myRenderer;
    Material originalMaterial;
    float oriRefLerp;
    float currentWetVal =1.0f; //when wet2dry or dry2wet was interrupted,the value of "_RefLerp"
	// Use this for initialization
	void Start () {
        if (wetObj )
        {
            myRenderer =wetObj.GetComponent<Renderer>();
            originalMaterial = myRenderer.sharedMaterial;
            objRef = wetObj.GetComponent<ReflectionSetup>();
            //if (useTriggerReflection)
                objRef.enabled = false;

            getWetTime = 0;
            getDryTime = 0;
        }
        else
        {
            //Debug.Log("No enough obj!");
            enabled = false;
        }
        
	}

    void OnTriggerEnter(Collider other)
    {
        oriRefLerp = myRenderer.material.GetFloat("_RefLerp");
        objRef.enabled = true;
        changeTime1 = 0;
        changeTime2 = 0;
        enterWet = true;
        outWet = false;
        inRain = false;
        outRain = false;
        getDryTime = 0;
        currentWetVal =  myRenderer.material.GetFloat("_RefLerp");
        tarObj = other.gameObject;
    }

    void ExitEvents()
    {
        tarObj = null;
        changeTime1 = 0;
        changeTime2 = 0;
        enterWet = false;//enter wet area
        outWet = true;//exit wet area
        inRain = false;//stay in wet area or not
        outRain = false;//stay in dry area or not
        getWetTime = 0;
        currentWetVal =  myRenderer.material.GetFloat("_RefLerp");

    }
    void OnTriggerExit(Collider other)
    {
        ExitEvents();
    }

    void dry2wet()
    {
        if (changeTime1 < 1.0)
        {
            changeTime1 += Time.deltaTime / tranTime;
            if (changeTime1 >= 1)
                inRain = true;

            float currentRefLerp = Mathf.Lerp(currentWetVal, 0.0f, changeTime1);
             myRenderer.material.SetFloat("_RefLerp", currentRefLerp);
            //Debug.Log("Current wet float:" + currentRefLerp);
        }
    }

    void wet2dry()
    {
        if (changeTime2 < 1.0)
        {
            
            changeTime2 += Time.deltaTime / tranTime;

            float currentRefLerp = Mathf.Lerp(currentWetVal, oriRefLerp, changeTime2);
             myRenderer.material.SetFloat("_RefLerp", currentRefLerp);

             if (changeTime2 >= 1.0)
             {
                 outRain = true;
                 objRef.enabled = false;
                 DestroyImmediate(myRenderer.material);
                 myRenderer.sharedMaterial = originalMaterial;
             }

        }
    }

	// Update is called once per frame
	void Update () {
        if (enterWet && !inRain)
        {
            if (getWetTime >= 1.0f)
                dry2wet();
            else
                getWetTime += Time.deltaTime;
        }
        else
        {
            if (outWet && !outRain)
            {
                if (getDryTime >= 1.0f)
                    wet2dry();
                else
                    getDryTime += Time.deltaTime;
                //Debug.Log("Out!");
            }
        }

        if (tarObj)
            if (!tarObj.active)
                ExitEvents();

	}
}
