using UnityEngine;
using System.Collections;

public class UpdateFrame : MonoBehaviour
{
	public int targetFrameRate = 60;	

	void Awake ()
	{
		Application.targetFrameRate = targetFrameRate;
	}
	
}
