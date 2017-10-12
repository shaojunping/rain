using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

public class TexFinder : EditorWindow  {

	private List<Object>  selectMat = new List<Object> ();
	private List<string>  selectShader = new List<string> ();
	static string[] MatFolder ={"Assets/Arts/Effects/Skills/Materials","Assets/Arts/Effects/Skills/Materials2",
	"Assets/Arts/Effects/Skills/Materials3","Assets/Arts/Effects/Skills/Materials4","Assets/Arts/Effects/Skills/Materials5"};
	Object source;
	Object source2;
	Object sourceMat;
	Object sourceCon;
	[MenuItem ("Artists Tools/Texture Finder")] 

	static void Init ()
	{   
		TexFinder window = EditorWindow.GetWindow(typeof (TexFinder)) as TexFinder;
		window.ShowPopup();
	}

	void OnGUI ()
	{

		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField ("Choose a texture:", EditorStyles.boldLabel);
		GUILayout.EndHorizontal ();	
		GUILayout.BeginHorizontal();
		GUILayout.Label("Texture:",GUILayout.Width(80));
		source = EditorGUILayout.ObjectField(source, typeof(Texture2D), true,GUILayout.MaxHeight(80));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Scan Materials via Texture!",GUILayout.Height(30)))
		{	
			selectMat.Clear();
			selectShader.Clear();
			if(source ==null)
			{
				//Debug.Log("No texture Selected!");
				return;
			}
		
			for (int j=0;j<5;j++)
			{
				//Debug.Log("Current Floder:"+MatFolder[j]);
				foreach (var file in Directory.GetFiles(MatFolder[j], "*.mat", SearchOption.AllDirectories)) 
				{

					var matTest = AssetDatabase.LoadMainAssetAtPath(file) as Material;
					Shader matShader = matTest.shader;

					for (int i = 0; i < ShaderUtil.GetPropertyCount(matShader); ++i)  
					{
						if (ShaderUtil.GetPropertyType(matShader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
						{						
							string propertyName = ShaderUtil.GetPropertyName(matShader, i); 
							Texture tex = matTest.GetTexture(propertyName);
							if (tex == null)
							{
								Debug.Log("No Tex!!");
							}
							else
							{
								if(tex.name == source.name)
								{						
									Object obj2 = AssetDatabase.LoadAssetAtPath(file, typeof(Object));								
									selectMat.Add(obj2);						
									selectShader.Add(matShader.name);
								}							
							}
						}
					}				
				}
			}
			Selection.objects =selectMat.ToArray();
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField ("Choose Shader:", EditorStyles.boldLabel);
		GUILayout.EndHorizontal ();	
		GUILayout.BeginHorizontal();
		GUILayout.Label("Shader:",GUILayout.Width(80));
		source2 = EditorGUILayout.ObjectField(source2, typeof(Object), true);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if (GUILayout.Button ("Scan Materials via Shader!", GUILayout.Height (30))) 
		{

			selectMat.Clear();
			selectShader.Clear();
			if(source2 ==null)
			{
				Debug.Log("No shader Selected!");
				return;
			}

			for (int j=0;j<5;j++)
			{
				foreach (var file in Directory.GetFiles(MatFolder[j], "*.mat", SearchOption.AllDirectories)) 
				{
					
					var matTest = AssetDatabase.LoadMainAssetAtPath(file) as Material;
					Shader matShader = matTest.shader;	
					if(matShader.name == source2.name)
						{						
					
							Object obj2 = AssetDatabase.LoadAssetAtPath(file, typeof(Object));								
							selectMat.Add(obj2);						
							selectShader.Add(matShader.name);

						}
								
				}
			}

		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField ("Choose Material:", EditorStyles.boldLabel);
		GUILayout.EndHorizontal ();	
		GUILayout.BeginHorizontal();
		GUILayout.Label("Material:",GUILayout.Width(80));
		sourceMat =EditorGUILayout.ObjectField(sourceMat, typeof(Material),true);
		GUILayout.EndHorizontal();
		
		if(GUILayout.Button("Change All Materilas!",GUILayout.Height(30)))
		{
			if(sourceMat ==null)
			{
				Debug.Log("No Material Selected!");
				return;
			}
			ApplyAll();
		}

		GUILayout.BeginHorizontal();
		GUILayout.Label("Controller:",GUILayout.Width(80));
		sourceCon =EditorGUILayout.ObjectField(sourceCon, typeof(object),true);
		GUILayout.EndHorizontal();
		
		if(GUILayout.Button("Change All Controllers!",GUILayout.Height(30)))
		{
			ChangeConAll();
		}


		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField ("MatCounts:     "+selectMat.Count, EditorStyles.boldLabel);
		GUILayout.EndHorizontal ();	
		for (int index =0; index<selectMat.Count; index++) 
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.ObjectField(selectMat[index], typeof(Object), true,GUILayout.Width(130));
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal();
			GUILayout.Label(selectShader[index].Replace("A-ONE workShader/",""));
			GUILayout.EndHorizontal ();
		}

	}

	void ChangeConAll()
	{
		if(Selection.activeGameObject != null)
		{
			foreach(GameObject g in Selection.gameObjects)
			{
				var ObjAnimator = g.GetComponentsInChildren<Animator>();
				foreach(var r in ObjAnimator)
				{
					if(r != null)
					{
						r.runtimeAnimatorController =(RuntimeAnimatorController)sourceCon;
					}
				}
			}
		}
	}

	void ApplyAll()
	{
		if(Selection.activeGameObject != null)
		{
			foreach(GameObject g in Selection.gameObjects)
			{
				Renderer[] SkinRenders = g.GetComponentsInChildren<SkinnedMeshRenderer>();
				foreach(Renderer r in SkinRenders)
				{
					if(r  !=  null)
					{
						r.material =(Material)sourceMat;	
						
					}
				}
				Renderer[] renders = g.GetComponentsInChildren<MeshRenderer>();
				foreach(Renderer r2 in renders)
				{
					if(r2  !=  null)
					{
						r2.material =(Material)sourceMat;	
						
					}
				}
			}
		}
	}

}
