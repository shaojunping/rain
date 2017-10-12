using UnityEngine;
using System.Collections;

public class toogleWet : MonoBehaviour {
    public GameObject wetCon;
	// Use this for initialization
	void Start () {
	
	}
	
    public void changeWet(float value)
    {
        if (wetCon)
        {
            wetCon.GetComponent<EnvironmentSetup>().wetEffect = value;
            var tempAmb = wetCon.GetComponent<EnvironmentSetup>().ambientEffectScale * value;
            Shader.SetGlobalFloat("_AmbScale", tempAmb);
            Shader.SetGlobalFloat("_RefLerp", Mathf.GammaToLinearSpace(value));
        }
    }

    public void changeView(bool value)
    {
        if (wetCon)
        {
            wetCon.GetComponent<RotateAndPinch_2>().enabled = value;
        }
    }

	// Update is called once per frame
    //void Update () {
	
    //}
}
