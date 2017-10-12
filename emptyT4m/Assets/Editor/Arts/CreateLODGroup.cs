using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;


public class CreateLODGroup : EditorWindow
{
    public float cullPercent = 0.1f;

    [MenuItem("Artists Tools/CreateLODGroup")]

    static void Init()
    {
        CreateLODGroup window = EditorWindow.GetWindow(typeof(CreateLODGroup)) as CreateLODGroup;
        window.ShowPopup();
    }

    void OnGUI()
    {
        GUILayout.Label("Culling Percents(0~1.0):", GUILayout.Width(90));
        cullPercent = EditorGUILayout.FloatField(cullPercent, GUILayout.Width(50));

        if (GUILayout.Button("Add LODs!!", GUILayout.Height(30)))
        {
            AddLODs();
        }

        if (GUILayout.Button("Set LOD Culling Percents!!", GUILayout.Height(30)))
        {
            SetCullingPercents();
        }
    }
    void AddLODs()
    {
        if (cullPercent > 1.0f)
            cullPercent = 1.0f;
        if (cullPercent <0.0f)
            cullPercent = 0.0f;

        if (Selection.objects == null )
        {
            Debug.Log("Nothing Selected!");
            return;
        }

        foreach (GameObject g in Selection.objects)
        {

            if (g.GetComponent<LODGroup>() == null)
                g.AddComponent<LODGroup>();
            else
                return;
            LODGroup group = g.GetComponent<LODGroup>();
            LOD[] lods = new LOD[1];

            //Renderer[] renderers = new Renderer[1];
            //renderers[0] = g.transform.GetComponent<Renderer>();
            Renderer[] renderers = g.transform.GetComponentsInChildren<Renderer>();

            lods[0] = new LOD(cullPercent, renderers);
            group.SetLODs(lods);
            group.RecalculateBounds();
        }

    }

    void SetCullingPercents()
    {
        if (cullPercent > 1.0f)
            cullPercent = 1.0f;
        if (cullPercent < 0.0f)
            cullPercent = 0.0f;

        if (Selection.objects == null)
        {
            Debug.Log("Nothing Selected!");
            return;
        }

        foreach (GameObject g in Selection.objects)
        {

            if (g.GetComponent<LODGroup>() == null)
                return;
            LODGroup group = g.GetComponent<LODGroup>();

            LOD[] lods = group.GetLODs();
            Renderer[] renderers = lods[0].renderers;
            lods[0] = new LOD(cullPercent, renderers);
            group.SetLODs(lods);
            group.RecalculateBounds();
        }
    }

}
