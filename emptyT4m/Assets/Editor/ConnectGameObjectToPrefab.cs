using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;

/// <summary>
/// Check prefabs.
/// 解决地图场景由于拖了Prefab导致子对象Prefab关联丢失的问题（以遮天的场景测试的代码）
/// 检查Meshrenderer的materail如果一样则关联
/// </summary>
public class ConnectGameObjectToPrefab : MonoBehaviour {

	private static Dictionary<string,Dictionary<string,GameObject>> _dicAllPrefabs = new Dictionary<string, Dictionary<string, GameObject>>();


	[MenuItem("Tools/UGE——场景地图GO关联Prefab（相同MeshRenderer即关联）")]
	public static void Check_Map_Prefab()
	{

		#region 遍历指定目录的Prefab并保存相关信息为关联场景GO做准备

		//指定美术Prefab存放目录
		string[] filePaths = Directory.GetFiles(Application.dataPath + "/BaseResources/map/compprefab/","*.prefab",SearchOption.AllDirectories);

		foreach(var str in filePaths)
		{

			string tmp =  str.Remove(0, str.LastIndexOf("/Assets")+1);
//			Debug.LogError(tmp);
			var go = AssetDatabase.LoadAssetAtPath<GameObject>(tmp);
			if(go != null)
			{

				var len = go.GetComponentsInChildren<MeshFilter>().Length;
				if(len > 0 )
				{

					if(!_dicAllPrefabs.ContainsKey(len + "_" + go.transform.childCount))
					{
						_dicAllPrefabs.Add(len+ "_" + go.transform.childCount,new Dictionary<string, GameObject>());
					}

					Dictionary<string,GameObject> tmpDic = _dicAllPrefabs[ len + "_" + go.transform.childCount];
					MeshFilter[] tmpMfArr = go.GetComponentsInChildren<MeshFilter>();

					string keyStr = "";
					foreach(var mf in tmpMfArr)
					{
						if(mf.GetComponent<MeshRenderer>() != null && mf.GetComponent<MeshRenderer>().sharedMaterial == null)
						{
							Debug.LogError(mf.gameObject.name + "=========" + mf.name);
							Debug.LogError(go.name + "------------");
						}
						else
						{
							if(mf.sharedMesh != null)
							{
								if(mf.GetComponent<MeshRenderer>() != null && mf.GetComponent<MeshRenderer>().sharedMaterial != null)
								{
									keyStr += mf.GetComponent<MeshRenderer>().sharedMaterial.name;
								}
								keyStr += mf.sharedMesh.name;
							}

						}

					}
					if(keyStr != "" && !tmpDic.ContainsKey(keyStr))
					{
						tmpDic.Add(keyStr,go);
					}

				}
			}
		}
		Debug.Log("遍历等待关联的Prefab完成>>>>>");
		#endregion


		#region 遍历场景中的物体并关联到上面指定目录下的的Prefab
		GameObject[] arrSceneObj = GameObject.FindObjectsOfType<GameObject>();
		foreach(var go in arrSceneObj)
		{
			if(go == null)
			{
				continue;
			}

			MeshFilter[] tmpMfArr = go.GetComponentsInChildren<MeshFilter>();

			if(tmpMfArr != null && _dicAllPrefabs.ContainsKey(tmpMfArr.Length+ "_" + go.transform.childCount))
			{

				var tmpDic = _dicAllPrefabs[tmpMfArr.Length+ "_" + go.transform.childCount];
				string keyStr = "";

				foreach(var mf in tmpMfArr)
				{
					if( mf.GetComponent<MeshRenderer>() == null || (mf.GetComponent<MeshRenderer>() != null && mf.GetComponent<MeshRenderer>().sharedMaterial == null))
					{
						Debug.LogError(mf.gameObject.name);
					}
					else
					{
						if(mf.sharedMesh != null)
						{
							if(mf.GetComponent<MeshRenderer>() != null && mf.GetComponent<MeshRenderer>().sharedMaterial != null)
							{
								keyStr += mf.GetComponent<MeshRenderer>().sharedMaterial.name;
							}
							keyStr += mf.sharedMesh.name;
						}
					}
					
				}

				//关联Prefab并保留场景中GO的Transform信息
				if(keyStr != "" && tmpDic.ContainsKey(keyStr))
				{
						List<Vector3> p = new List<Vector3>();
						List<Quaternion> r = new List<Quaternion>();
						List<Vector3> s = new List<Vector3>();

						for(int i = 0;i<go.transform.childCount;i++)
						{
							var ts = go.transform.GetChild(i);
							p.Add(ts.localPosition);
							r.Add(ts.localRotation);
							s.Add(ts.localScale);
						}
						var v1 = go.transform.localPosition;
						var v2 = go.transform.localRotation;
						var v3 = go.transform.localScale;
						var newGo = PrefabUtility.ConnectGameObjectToPrefab(go,tmpDic[keyStr]);
						newGo.transform.localPosition = v1;
						newGo.transform.localRotation = v2;
						newGo.transform.localScale = v3;

						for(int i = 0;i<newGo.transform.childCount;i++)
						{
							var ts = newGo.transform.GetChild(i);
							ts.localPosition = p[i];
							ts.localRotation = r[i];
							ts.localScale = s[i];

						}

				}

			}

		}

		#endregion
		Debug.Log("Done ! Connect GameObject To Prefab Finished");

	}

}
