using UnityEngine;
using System.Collections;

public class showFPS : MonoBehaviour {
	public  float updateInterval = 0.5F;
		
	private float accum   = 0; // FPS accumulated over the interval
	private int frames  = 0; // Frames drawn over the interval
	private float timeleft; // Left time for current interval
	private string FPS;

	void Start()
	{
		timeleft = updateInterval;  
	}

	void OnGUI(){

		GUIStyle style = new GUIStyle();
		style.normal.textColor = new Color( 1, 1, 1);   
		style.fontSize = 40;
		GUI.skin.label.alignment = TextAnchor.UpperCenter;
		GUI.Label(new Rect(0,0,200,60),FPS,style);
	}

	
	void Update()
	{
		timeleft -= Time.deltaTime;
		accum += Time.timeScale/Time.deltaTime;
		++frames;
		
		// Interval ended - update GUI text and start new interval
		if( timeleft <= 0.0 )
		{
			float fps = accum/frames;
			FPS ="FPS:"+ System.String.Format("{0:F2}",fps);		

			timeleft = updateInterval;
			accum = 0.0F;
			frames = 0;
		}
	}
}
