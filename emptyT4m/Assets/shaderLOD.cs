using UnityEngine;
using System.Collections;

public class shaderLOD : MonoBehaviour {
    //public GameObject waterObj;
    //public GameObject waterObj2;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void OnGUI ()
    {
        if (GUI.Button(new Rect(60, 100, 200, 60), "Toggle Hight Quality Shader!"))
        {
            Shader.globalMaximumLOD = 300;

            //waterObj.GetComponent<WaterSetup>().quality = WaterSetup.Quality.High;
            //waterObj2.GetComponent<WaterSetup>().quality = WaterSetup.Quality.High;
        }

        //if (GUI.Button(new Rect(60, 200, 200, 60), "Toggle Med Quality Shader!"))
        //{
        //    Shader.globalMaximumLOD = 250;
        //}

        if (GUI.Button(new Rect(60, 300, 200, 60), "Toggle Low Quality Shader!"))
        {
            Shader.globalMaximumLOD = 200;
        }
	}
}
