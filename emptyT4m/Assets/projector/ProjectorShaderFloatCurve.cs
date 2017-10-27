using UnityEngine;
using System.Collections;


public enum ProjectorShaderProperties
{
    _Color,
    _BurnAmount,
    _XOffset,
    _YOffset
};


public class ProjectorShaderFloatCurve : MonoBehaviour {

    public ProjectorShaderProperties ShaderFloatProperty = ProjectorShaderProperties._BurnAmount;
    public AnimationCurve FloatCurve = AnimationCurve.EaseInOut(0, -1, 1, 1);
    public float GraphTimeMultiplier = 6, GraphIntensityMultiplier = 1;
    public bool IsLoop = false;
    public bool UseSharedMaterial = false;

    private bool canUpdate;
    private float startTime;
    private Material mat;
    private float startFloat;
    private int propertyID;
    private string shaderProperty;
    private bool isInitialized;

    private void Awake()
    {
        var rend = GetComponent<Renderer>();
        if (rend == null)
        {
            var projector = GetComponent<Projector>();
            if (projector != null)
            {
                if (!UseSharedMaterial)
                {
                    if (!projector.material.name.EndsWith("(Instance)"))
                        projector.material = new Material(projector.material) { name = projector.material.name + " (Instance)" };
                    mat = projector.material;
                }
                else
                {
                    mat = projector.material;
                }
            }
        }
        else
        {
            if (!UseSharedMaterial) mat = rend.material;
            else mat = rend.sharedMaterial;
        }

        shaderProperty = ShaderFloatProperty.ToString();
        if (mat.HasProperty(shaderProperty)) propertyID = Shader.PropertyToID(shaderProperty);
        startFloat = mat.GetFloat(propertyID);
        var eval = FloatCurve.Evaluate(0) * GraphIntensityMultiplier;
        mat.SetFloat(propertyID, eval);
        isInitialized = true;
    }

    private void OnEnable()
    {
        startTime = Time.time;
        canUpdate = true;
        if (isInitialized)
        {
            var eval = FloatCurve.Evaluate(0)*GraphIntensityMultiplier;
            mat.SetFloat(propertyID, eval);
        }
    }

    private void Update()
    {
        var time = Time.time - startTime;
        if (canUpdate)
        {
            var eval = FloatCurve.Evaluate(time / GraphTimeMultiplier) * GraphIntensityMultiplier;
            mat.SetFloat(propertyID, eval);
        }
        if (time >= GraphTimeMultiplier)
        {
            if (IsLoop) startTime = Time.time;
            else canUpdate = false;
        }
    }
   
    void OnDisable()
    {
        if(UseSharedMaterial) mat.SetFloat(propertyID, startFloat);
    }

    void OnDestroy()
    {
        if (!UseSharedMaterial)
        {
            if (mat != null)
                DestroyImmediate(mat);
            mat = null;
        }
    }
}
