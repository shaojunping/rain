using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
public class InfoToReflMaterial : MonoBehaviour
{
    // The proxy volume used for local reflection calculations.
    public GameObject RefProbe;
    public Vector3 RefineSize;
    void OnEnable()
    {
        if (RefProbe == null)
            return;
        //Vector3 bboxLenght = boundingBox.transform.localScale;
        Vector3 bboxLenght = RefProbe.GetComponent<ReflectionProbe>().size;
        bboxLenght += RefineSize;
        //Debug.Log("sdfsafdsa :" + bboxLenght);
        Vector3 centerBBox = RefProbe.transform.position;
        // Min and max BBox points in world coordinates
        Vector3 BMin = centerBBox - bboxLenght / 2;
        Vector3 BMax = centerBBox + bboxLenght / 2;
        // Pass the values to the material.
#if UNITY_EDIOR || UNITY_ANDROID || UNITY_IOS
        //#if UNITY_EDIOR 
        gameObject.GetComponent<Renderer>().sharedMaterial.SetVector("_BBoxMin", BMin);
        gameObject.GetComponent<Renderer>().sharedMaterial.SetVector("_BBoxMax", BMax);
        gameObject.GetComponent<Renderer>().sharedMaterial.SetVector("_EnviCubeMapPos", centerBBox);
#else
        gameObject.GetComponent<Renderer>().material.SetVector("_BBoxMin", BMin);
        gameObject.GetComponent<Renderer>().material.SetVector("_BBoxMax", BMax);
        gameObject.GetComponent<Renderer>().material.SetVector("_EnviCubeMapPos", centerBBox);
#endif

    }
}