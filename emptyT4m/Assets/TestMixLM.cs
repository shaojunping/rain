using UnityEngine;
using System.Collections;

public class TestMixLM : MonoBehaviour {
    public GameObject skyObj;
    private MaterialPropertyBlock matBlock;
	// Use this for initialization
	 public void changeLM(float value)
     {
         if (value >= 0.01f && value <= 0.99f)
         {
             Shader.EnableKeyword("_BOTHLM");
             Shader.DisableKeyword("_DAYLM");
             Shader.DisableKeyword("_NIGHTLM");
         }

         if (value < 0.01f)
         {
             Shader.EnableKeyword("_DAYLM");
             Shader.DisableKeyword("_BOTHLM");
             Shader.DisableKeyword("_NIGHTLM");
         }

         if (value > 0.99f)
         {
             Shader.EnableKeyword("_NIGHTLM");
             Shader.DisableKeyword("_BOTHLM");
             Shader.DisableKeyword("_DAYLM");
         }

         Shader.SetGlobalFloat("_LMLerp", value);
         matBlock.SetFloat("_SkyLerp", value);
         skyObj.GetComponent<Renderer>().SetPropertyBlock(matBlock);
	}
	
	// Update is called once per frame
	void Start () {
        matBlock = new MaterialPropertyBlock();
        skyObj.GetComponent<Renderer>().GetPropertyBlock(matBlock);
 
	}
}
