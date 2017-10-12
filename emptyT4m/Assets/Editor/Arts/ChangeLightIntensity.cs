using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;


public class ChangeLightIntensity : EditorWindow  {

    float LightIntenMulti =1.0f;
	[MenuItem ("Artists Tools/Change Light Intensity")] 
	
	static void Init ()
	{
        ChangeLightIntensity window = EditorWindow.GetWindow(typeof(ChangeLightIntensity)) as ChangeLightIntensity;
		window.ShowPopup();
	}
	
	void OnGUI ()
	{
		if(GUILayout.Button("Change Selected lights to 0.5X Intensity!",GUILayout.Height(30)))
		{
            LightIntenMulti = 0.5f;
            ChangeLightIn();

		}

        if (GUILayout.Button("Change Selected lights to 2X Intensity!", GUILayout.Height(30)))
		{
            LightIntenMulti = 2.0f;
            ChangeLightIn();
		}

        if (GUILayout.Button("Change Selected lights to 0.8X Intensity!", GUILayout.Height(30)))
        {
            LightIntenMulti = 0.8f;
            ChangeLightIn();
        }

        if (GUILayout.Button("Change Selected lights to 1.25X Intensity!", GUILayout.Height(30)))
        {
            LightIntenMulti = 1.25f;
            ChangeLightIn();
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label("Multiplier:", GUILayout.Width(90));
        LightIntenMulti = EditorGUILayout.FloatField(LightIntenMulti, GUILayout.Width(50));
        if (LightIntenMulti <= 0) LightIntenMulti = 0.1f;
        if (GUILayout.Button(new GUIContent("Scale Instensity", "Change Selected lights to Intensity Inputed!"), GUILayout.Width(150), GUILayout.Height(30)))
        {

            ChangeLightIn();
        }
        GUILayout.EndHorizontal();
	}
    void ChangeLightIn()
    {
        if (Selection.objects != null)
        {
            foreach (GameObject g in Selection.objects)
            {
                Light LightComp = g.GetComponent<Light>();
                LightComp.intensity *= LightIntenMulti;
            }
        }
    }

}
