using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;


public class ChangeShaders : EditorWindow  {
	string shaderName;
    Object NewOpaqueShader1;
    Object NewAlphaTestShader1;
    Object OldOpaqueShader1, OldOpaqueShader2,OldOpaqueShader3;
    Object OldAlphaTestShader1, OldAlphaTestShader2, OldAlphaTestShader3;
    Object FogSource;
	[MenuItem ("Artists Tools/Change Shaders")] 
	
	static void Init ()
	{   
		ChangeShaders window = EditorWindow.GetWindow(typeof (ChangeShaders)) as ChangeShaders;
		window.ShowPopup();
	}
	
	void OnGUI ()
	{
        if (GUILayout.Button("Change BuiltIN Shaders to CFog Shader!", GUILayout.Height(30)))
        {
            Change2CfogShader();
        }
        if (GUILayout.Button("Change Cfog Shader to BuiltIN Shaders!", GUILayout.Height(30)))
        {
            shaderName = "Legacy Shaders/Diffuse";
            Change2BuildInShader();
        }
        GUILayout.Label("-----------------------------------------------------------------------------------");

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Old Shaders:", EditorStyles.boldLabel);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Opaque Shader:", GUILayout.Width(120));
        OldOpaqueShader1 = EditorGUILayout.ObjectField(OldOpaqueShader1, typeof(Object), true);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Opaque Shader2:", GUILayout.Width(120));
        OldOpaqueShader2 = EditorGUILayout.ObjectField(OldOpaqueShader2, typeof(Object), true);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Opaque Shader3:", GUILayout.Width(120));
        OldOpaqueShader3 = EditorGUILayout.ObjectField(OldOpaqueShader3, typeof(Object), true);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("AlphaTest Shader:", GUILayout.Width(120));
        OldAlphaTestShader1 = EditorGUILayout.ObjectField(OldAlphaTestShader1, typeof(Object), true);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("AlphaTest Shader2:", GUILayout.Width(120));
        OldAlphaTestShader2 = EditorGUILayout.ObjectField(OldAlphaTestShader2, typeof(Object), true);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("AlphaTest Shader3:", GUILayout.Width(120));
        OldAlphaTestShader3 = EditorGUILayout.ObjectField(OldAlphaTestShader3, typeof(Object), true);
        GUILayout.EndHorizontal();  

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("New Shaders:", EditorStyles.boldLabel);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Opaque Shader:", GUILayout.Width(120));
        NewOpaqueShader1 = EditorGUILayout.ObjectField(NewOpaqueShader1, typeof(Object), true);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("AlphaTest Shader:", GUILayout.Width(120));
        NewAlphaTestShader1 = EditorGUILayout.ObjectField(NewAlphaTestShader1, typeof(Object), true);
        GUILayout.EndHorizontal();  

 //修改shader 为指定

        if (GUILayout.Button("Change Old Shaders to New Shader!", GUILayout.Height(30)))
        {
            Change2NewShader();
        }

        if (GUILayout.Button("Change BuiltIN Shaders to New Shader!", GUILayout.Height(30)))
        {
            Change2SelShader();
        }

        if (GUILayout.Button("Change Old Shader  to BuiltIN Shaders !", GUILayout.Height(30)))
        {
            ChangeSel2BuildIn();
        }


        GUILayout.Label("-----------------------------------------------------------------------------------");
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Change Fog Texture!:", EditorStyles.boldLabel);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Texture:", GUILayout.Width(80));
        FogSource = EditorGUILayout.ObjectField(FogSource, typeof(Texture2D), true, GUILayout.MaxHeight(32));
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Change FogTex On Selected Objects!", GUILayout.Height(30)))
        {
            Shader.SetGlobalTexture("_FogTex", (Texture2D)FogSource);
        }
	}

    void Change2NewShader()
    {
        if (Selection.objects == null)
        {
            Debug.Log("Nothing Selected!");
            return;
        }

        foreach (GameObject g in Selection.objects)
        {
            Renderer[] MeshRenders = g.GetComponentsInChildren<MeshRenderer>();
            foreach (Renderer r in MeshRenders)
            {
                if (r != null)
                {
                    for (int i = 0; i < r.sharedMaterials.Length; i++)
                    {
                        string oldOpaqueName;
                        string oldAlphaName ;

                        if (OldOpaqueShader1 == null)
                            oldOpaqueName = "Legacy Shaders/Diffuse";
                        else
                            oldOpaqueName = OldOpaqueShader1.name;

                        if (OldAlphaTestShader1 == null)
                            oldAlphaName = "Legacy Shaders/Transparent/Cutout/Diffuse";
                        else
                            oldAlphaName = OldAlphaTestShader1.name;
                        

                        if (r.sharedMaterials[i].shader.name == oldOpaqueName)
                        {
                            r.sharedMaterials[i].shader = Shader.Find(NewOpaqueShader1.name);
                            Debug.Log("Current Shader:" + r.sharedMaterials[i].shader.name);
                        }

                        if (r.sharedMaterials[i].shader.name == oldAlphaName)
                        {
                            r.sharedMaterials[i].shader = Shader.Find(NewAlphaTestShader1.name);
                            Debug.Log("Current Shader:" + r.sharedMaterials[i].shader.name);
                        }

                    }
                }
            }

        }
    }

    void Change2CfogShader()
    {
        if (Selection.objects == null)
        {
            Debug.Log("Nothing Selected!");
            return;
        }
        foreach (GameObject g in Selection.objects)
        {
            Renderer[] MeshRenders = g.GetComponentsInChildren<MeshRenderer>();
            foreach (Renderer r in MeshRenders)
            {
                if (r != null)
                {
                    for (int i = 0; i < r.sharedMaterials.Length; i++)
                    {
                        if (r.sharedMaterials[i].shader.name == "Legacy Shaders/Diffuse")
                        {
                            r.sharedMaterials[i].shader = Shader.Find("TSHD/CustomeFog/Opaque_UnlitLM_cFog");
                            Debug.Log("Current Shader:" + r.sharedMaterials[i].shader.name);
                        }
                        if (r.sharedMaterials[i].shader.name == "Legacy Shaders/Transparent/Cutout/Diffuse")
                        {
                            r.sharedMaterials[i].shader = Shader.Find("TSHD/CustomeFog/AlphaTest_UnlitLM_cFog");
                            Debug.Log("Current Shader:" + r.sharedMaterials[i].shader.name);
                        }
                    }
                }

            }
        }

    }

    void Change2SelShader() //场景中gameObject批量修改shader
	{
		if (Selection.objects == null)
        {
            Debug.Log("Nothing Selected!");
            return;
        }
		foreach(GameObject g in Selection.objects)
		{
			Renderer[] MeshRenders = g.GetComponentsInChildren<MeshRenderer>();
//				Debug.Log("Current Prefab:"+g.name +"=======Find Renderer:"+MeshRenders.Length);
			foreach(Renderer r in MeshRenders)
			{
				if( r !=null)
				{
					for(int i =0;i<r.sharedMaterials.Length;i++)
					{
						if(r.sharedMaterials[i].shader.name =="Legacy Shaders/Diffuse")
						{
                                r.sharedMaterials[i].shader = Shader.Find(NewOpaqueShader1.name);
							Debug.Log("Current Shader:"+r.sharedMaterials[i].shader.name);
                        }
                        if (r.sharedMaterials[i].shader.name == "Legacy Shaders/Transparent/Cutout/Diffuse")
                        {
                            r.sharedMaterials[i].shader = Shader.Find(NewAlphaTestShader1.name);
                            Debug.Log("Current Shader:" + r.sharedMaterials[i].shader.name);
                        }
					}
				}

			}
		}

	}

    void ChangeSel2BuildIn() //场景中gameObject批量修改shader
    {
       if (Selection.objects == null)
        {
            Debug.Log("Nothing Selected!");
            return;
        }

        foreach (GameObject g in Selection.objects)
        {
            Renderer[] MeshRenders = g.GetComponentsInChildren<MeshRenderer>();
            //				Debug.Log("Current Prefab:"+g.name +"=======Find Renderer:"+MeshRenders.Length);
            foreach (Renderer r in MeshRenders)
            {
                if (r != null)
                {
                    for (int i = 0; i < r.sharedMaterials.Length; i++)
                    {
                        if (r.sharedMaterials[i].shader.name == OldOpaqueShader1.name || r.sharedMaterials[i].shader.name == OldOpaqueShader2.name || r.sharedMaterials[i].shader.name == OldOpaqueShader3.name)
                        {
                            r.sharedMaterials[i].shader = Shader.Find("Legacy Shaders/Diffuse");
                            Debug.Log("Current Shader:" + r.sharedMaterials[i].shader.name);
                        }
                        if (r.sharedMaterials[i].shader.name == OldAlphaTestShader1.name || r.sharedMaterials[i].shader.name == OldAlphaTestShader2.name || r.sharedMaterials[i].shader.name == OldAlphaTestShader3.name)
                        {
                            r.sharedMaterials[i].shader = Shader.Find("Legacy Shaders/Transparent/Cutout/Diffuse");
                            Debug.Log("Current Shader:" + r.sharedMaterials[i].shader.name);
                        }
                    }
                }

            }
        }

    }

    void Change2BuildInShader()
    {
         if (Selection.objects == null)
        {
            Debug.Log("Nothing Selected!");
            return;
        }
        foreach (GameObject g in Selection.objects)
        {
            Renderer[] MeshRenders = g.GetComponentsInChildren<MeshRenderer>();
            //				Debug.Log("Current Prefab:"+g.name +"=======Find Renderer:"+MeshRenders.Length);
            foreach (Renderer r in MeshRenders)
            {
                if (r != null)
                {
                    for (int i = 0; i < r.sharedMaterials.Length; i++)
                    {
                        if (r.sharedMaterials[i].shader.name == "TSHD/CustomeFog/Opaque_UnlitLM_cFog")
                        {
                            r.sharedMaterials[i].shader = Shader.Find("Legacy Shaders/Diffuse");
                            Debug.Log("Current Shader:" + r.sharedMaterials[i].shader.name);
                        }
                        if (r.sharedMaterials[i].shader.name == "TSHD/CustomeFog/AlphaTest_UnlitLM_cFog")
                        {
                            r.sharedMaterials[i].shader = Shader.Find("Legacy Shaders/Transparent/Cutout/Diffuse");
                            Debug.Log("Current Shader:" + r.sharedMaterials[i].shader.name);
                        }
                    }
                }

            }
        }

    }
	
}
