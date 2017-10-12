using UnityEngine;
using System.Collections;

public class ToogleSnow : MonoBehaviour {
    public Texture2D SnowTex;
    GameObject tarObj = null;
    public float snowSoakTime = 5.0f;
    public float snowDisplayTime = 5.0f;
    [Range(0.0f, 1.0f)]
    public float snowHighLevel = 0.25f;
    [Range(0.0f, 1.0f)]
    public float snowLowLevel = 0.0f;
    float changeTime1, changeTime2;
    bool enterSnow,outSnow;
    public GameObject snowObj;
    ParticleSystem snowPar1,snowPar2;
    float parOrRate1, parOrRate2;
    public float frozenDelay = 0.5f;
	// Use this for initialization
	void Start () {
        Shader.SetGlobalTexture("_SnowTex", SnowTex);
        changeTime1 = 0.0f;
        changeTime2 = 0.0f;
        if (snowObj != null)
        {
            snowPar1 = snowObj.transform.Find("Snow").GetComponent<ParticleSystem>();
            snowPar2 = snowObj.transform.Find("BigSnow").GetComponent<ParticleSystem>();
            parOrRate1 = snowPar1.emissionRate;
            parOrRate2 = snowPar2.emissionRate;
        }
	}

    void dry2Snow()
    {
        if (changeTime1 < 1.0)
        {
            changeTime1 += Time.deltaTime / snowSoakTime;

            float currentSnowLerp = Mathf.Lerp(snowLowLevel, snowHighLevel, changeTime1);
            Shader.SetGlobalFloat("_SnowLevel", currentSnowLerp);

            if (changeTime1 >= 1)
            {
                enterSnow = false;
                changeTime1 = 0.0f;
            }

            //Debug.Log("Current Snow:" + currentSnowLerp + "ChangTime:" + changeTime1);
        }
    }

    void snow2dry()
    {
        if (changeTime2 < 1.0)
        {
            changeTime2 += Time.deltaTime / snowDisplayTime;

            float currentSnowLerp = Mathf.Lerp(snowHighLevel, snowLowLevel, changeTime2);
            Shader.SetGlobalFloat("_SnowLevel", currentSnowLerp);

            if (snowObj != null)
            {
                snowPar1.emissionRate = Mathf.Lerp(parOrRate1, 0.0f, (changeTime2 +frozenDelay)*3);
                snowPar2.emissionRate = Mathf.Lerp(parOrRate2, 0.0f, (changeTime2 + frozenDelay)*3);
            }

            if (changeTime2 >= 1)
            {
                outSnow = false;
                changeTime2 = 0.0f;
                snowObj.SetActive(false);
            }

            //Debug.Log("Current Snow:" + currentSnowLerp + "ChangTime:" + changeTime2);
        }
    }

    public void enableSnow()
    {
        //if we are in process of Stoping Snow
        if (enterSnow || outSnow)
            return;

        //delay after snow par 
        changeTime1 -= frozenDelay;

        if (snowObj != null)
        {
            snowObj.SetActive(true);
            enterSnow = true;
            outSnow = false;
            snowPar1.emissionRate = parOrRate1;
            snowPar2.emissionRate = parOrRate2;
        }
        else
        {
            //Debug.Log("To Process of Snow!");
            enterSnow = true;
            outSnow = false;
        }


    }

    public void enableDry()
    {
        if (enterSnow || outSnow)
            return;

        changeTime2 -= frozenDelay;

        //Debug.Log("To Process of Clean!");
        enterSnow = false;
        outSnow = true;
    }

	void Update () {
        if (enterSnow)
            dry2Snow();
        else
            if (outSnow)
                snow2dry();

	}
}
