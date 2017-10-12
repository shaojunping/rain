using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;


public class ChangeMats : EditorWindow
{
    string shaderName;
    public Object SelMat;
    public Object ReplaceMat;

    [MenuItem("Artists Tools/Change Materials")]

    static void Init()
    {
        ChangeMats window = EditorWindow.GetWindow(typeof(ChangeMats)) as ChangeMats;
        window.ShowPopup();
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Select Materials:", EditorStyles.boldLabel);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("New Material:", GUILayout.Width(120));
        SelMat = EditorGUILayout.ObjectField(SelMat, typeof(Object), true);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Old Material:", GUILayout.Width(120));
        ReplaceMat = EditorGUILayout.ObjectField(ReplaceMat, typeof(Object), true);
        GUILayout.EndHorizontal();
        //修改shader 为指定
        if (GUILayout.Button("Change Select Objs to Selected Material!", GUILayout.Height(30)))
        {
            ChangeMateril();
        }
        if (GUILayout.Button("Toggle LightProbes!", GUILayout.Height(30)))
        {
            ToggleProbes();
        }
    }
    void ChangeMateril()
    {
        if (Selection.objects != null )
        {
            foreach (GameObject g in Selection.objects)
            {
             Renderer[] MeshRenders = g.GetComponentsInChildren<MeshRenderer>();
                foreach (Renderer r in MeshRenders)
                {
                    Debug.Log("Change:"+g.name+"Selected Mat:"+SelMat.name);
                    if (r != null)
                    {

                            if (r.sharedMaterial == (Material)ReplaceMat)
                            {
                                r.sharedMaterial= (Material)SelMat;
                                //r.sharedMaterials[i] = (Material)SelMat;
                                Debug.Log("Change Mat!" + r.sharedMaterial.name);
                            }
                        
                    }
                }
            }
        }
    }

    void ToggleProbes()
    {
        if (Selection.objects != null)
        {
            foreach (GameObject g in Selection.objects)
            {
                Renderer[] MeshRenders = g.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (Renderer r in MeshRenders)
                {
                    Debug.Log("Change:" + g.name + "Selected Mat:" + SelMat.name);
                    if (r != null)
                    {

                        if (r.useLightProbes == true)
                        {
                            r.useLightProbes = false;
                        }
                        else
                        {
                            r.useLightProbes = true;
                        }
                           
                        Debug.Log("ToggleLightPorbes!" );

                    }
                }
            }
        }
    }

}
