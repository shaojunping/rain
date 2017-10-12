using UnityEngine;
using System.Collections;
using UnityEditor;
//[CustomEditor(typeof(MeshRenderer))]
//[ExecuteInEditMode]

public class MatRenderQue : MonoBehaviour {

	public int RenderQue =0;


	void Start () {
		Debug.Log("Current Obj:" +gameObject.name+"render queue:"+gameObject.GetComponent<MeshRenderer>().material.renderQueue);
		gameObject.GetComponent<MeshRenderer>().material.renderQueue += RenderQue;
		Debug.Log("Current Mat render queue(After):"+ gameObject.GetComponent<MeshRenderer>().material.renderQueue);
	}
//	public override void OnInspectorGUI()
//	{
//		
//	}

}
