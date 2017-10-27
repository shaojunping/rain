using UnityEngine;
using System.Collections;

public class testImageResize : MonoBehaviour {

    public Texture2D SnowTex;
    // Use this for initialization
    void Start () {
	    if(SnowTex != null)
        {
            SnowTex.Resize(16, 16);
            Debug.Log("resize successfully!");
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
