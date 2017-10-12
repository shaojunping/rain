using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;
//using UnityStandardAssets.CinematicEffects;

public class BloomTest : MonoBehaviour {
    private string CurrentBloom = "Bloom Optimized";
    private string CurrentHDR= "   HDR ON!";
	// Use this for initialization
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = new Color(0, 1, 1);
        style.fontSize = 40;
        GUI.skin.label.alignment = TextAnchor.UpperCenter;
        GUI.Label(new Rect(0, 50, 200, 60), "Now :" + CurrentBloom + CurrentHDR, style);

        if (GUI.Button(new Rect(100, 100, 200, 60), "Bloom Optimized"))
        {
            gameObject.GetComponent<BloomOptimized>().enabled = true;
            gameObject.GetComponent<Bloom>().enabled = false;
            gameObject.GetComponent<ImageEffectsController>().enabled = false;
            CurrentBloom = "Bloom Optimized";
        }
        if (GUI.Button(new Rect(100, 200, 200, 60), "Bloom Default"))
        {
            gameObject.GetComponent<BloomOptimized>().enabled = false;
            gameObject.GetComponent<Bloom>().enabled = true;
            gameObject.GetComponent<ImageEffectsController>().enabled = false;
            CurrentBloom = "Bloom Default";
        }
        if (GUI.Button(new Rect(100, 300, 200, 60), "UEBloom"))
        {
            gameObject.GetComponent<BloomOptimized>().enabled = false;
            gameObject.GetComponent<Bloom>().enabled = false;
            gameObject.GetComponent<ImageEffectsController>().enabled = true;
            CurrentBloom = "CinematicBloom";
        }
        if (GUI.Button(new Rect(100, 400, 200, 60), "Toogle HDR"))
        {
            if (gameObject.GetComponent<Camera>().hdr)
            {
                gameObject.GetComponent<Camera>().hdr = false;
                CurrentHDR = "   HDR False!";
            }
            else
            {
                gameObject.GetComponent<Camera>().hdr = true;
                CurrentHDR = "   HDR ON!";
            }
        }
        if (GUI.Button(new Rect(100, 500, 200, 60), "Bloom OFF"))
        {
            gameObject.GetComponent<BloomOptimized>().enabled = false;
            gameObject.GetComponent<Bloom>().enabled = false;
            gameObject.GetComponent<ImageEffectsController>().enabled = false;
            CurrentBloom = "Bloom OFF";

        }
    }
	// Update is called once per frame
	void Update () {
	
	}
}
