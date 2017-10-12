using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;


public class ChangeLODPara : EditorWindow
{
    float LOD1Per = 0.5f;
    float CulledPer= 0.2f;
	[MenuItem ("Artists Tools/Change LOD Prameter")] 
	
	static void Init ()
	{
        ChangeLODPara window = EditorWindow.GetWindow(typeof(ChangeLODPara)) as ChangeLODPara;
		window.ShowPopup();
	}
	
	void OnGUI ()
	{
        GUILayout.BeginHorizontal();
        GUILayout.Label("LOD1 Percents:", GUILayout.Width(120));
        LOD1Per = EditorGUILayout.FloatField(LOD1Per, GUILayout.Width(80));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Culled  Percents:", GUILayout.Width(120));
        CulledPer = EditorGUILayout.FloatField(CulledPer, GUILayout.Width(80));
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Change Selected LOD Prameters!", GUILayout.Height(30)))
        {
            ChangeLOD();
        }

	}

    void ChangeLOD()
    {
        if(Selection.objects ==null)
            return;
        foreach (GameObject g in Selection.objects)
        {
            var LODGroups = g.GetComponentsInChildren<LODGroup>();
            foreach (var r in LODGroups)
            {
                if (r != null)
                {
                    Debug.Log("Current LODGourps is on:" + g.name+"     LOD1 Percents:"+LOD1Per);
                    LOD[] LG = r.GetLODs();
                    LOD[] lods = new LOD[2];
                    if (LG != null)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            Renderer[] rens;
                            if (LG[i].renderers.Length == 1)
                            {
                               rens = new Renderer[1];
                                rens[0] = LG[i].renderers[0];
                            }
                            else
                            {
                                rens = new Renderer[2];
                                rens[0] = LG[i].renderers[0];
                                rens[1] = LG[i].renderers[1];
                            }
     
                            float relHei = 1.0f ;
                            switch (i)
                            {
                                case 0:
                                    relHei = LOD1Per;
                                    break;
                                case 1:
                                    relHei = CulledPer;
                                    break;
                            }

                            lods[i] = new LOD(relHei, rens);
                            Debug.Log("Change LOD percents！" + LG[i].renderers[0].name +"  " +LG[i].screenRelativeTransitionHeight);
                        }
                        r.SetLODs(lods);
                        r.RecalculateBounds();
                        //LG[1].screenRelativeTransitionHeight = LOD1Per;

                        Debug.Log("Finish LOD Construct!:");
                       
                    }
                
                }
            }
        }

    }
}
