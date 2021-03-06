using UnityEngine;
using System.Collections;


public enum ProjectorShaderProperties
{
    _MainAlpha,
    _BurnAmount,
    _XOffset,
    _YOffset
};


public class ProjectorShaderFloatCurve : MonoBehaviour {

    public ProjectorShaderProperties m_projectorFloatProperty = ProjectorShaderProperties._BurnAmount;
    public AnimationCurve m_floatCurve = AnimationCurve.EaseInOut(0, -1, 1, 1);
    public float m_graphTimeMultiplier = 6;
    public float m_graphIntensityMultiplier = 1;
    public bool m_isLoop = false;
    public bool m_useSharedMaterial = true;

    private bool m_canUpdate;
    private float m_startTime;
    private Material m_mat;
    private float m_startFloat;
    private int m_propertyID;
    private string m_shaderProperty;
    private bool m_isInitialized;

    private void Awake()
    {
        var rend = GetComponent<Renderer>();
        if (rend == null)
        {
            var projector = GetComponent<Projector>();
            if (projector != null)
            {
                if (!m_useSharedMaterial)
                {
                    if (!projector.material.name.EndsWith("(Instance)"))
                        projector.material = new Material(projector.material) { name = projector.material.name + " (Instance)" };
                    m_mat = projector.material;
                }
                else
                {
                    m_mat = projector.material;
                }
            }
        }
        else
        {
            if (!m_useSharedMaterial) m_mat = rend.material;
            else m_mat = rend.sharedMaterial;
        }

        m_shaderProperty = m_projectorFloatProperty.ToString();
        if (m_mat.HasProperty(m_shaderProperty)) m_propertyID = Shader.PropertyToID(m_shaderProperty);
        m_startFloat = m_mat.GetFloat(m_propertyID);
        var eval = m_floatCurve.Evaluate(0) * m_graphIntensityMultiplier;
        m_mat.SetFloat(m_propertyID, eval);
        m_isInitialized = true;
    }

    private void OnEnable()
    {
        m_startTime = Time.time;
        m_canUpdate = true;
        if (!m_isInitialized)
        {
            var eval = m_floatCurve.Evaluate(0)* m_graphIntensityMultiplier;
            m_mat.SetFloat(m_propertyID, eval);
        }
    }

    private void Update()
    {
        var time = Time.time - m_startTime;
        if (m_canUpdate)
        {
            var eval = m_floatCurve.Evaluate(time / m_graphTimeMultiplier) * m_graphIntensityMultiplier;
            m_mat.SetFloat(m_propertyID, eval);
        }
        if (time >= m_graphTimeMultiplier)
        {
            if (m_isLoop) m_startTime = Time.time;
            else m_canUpdate = false;
        }
    }
   
    void OnDisable()
    {
        if (m_mat == null)
            return;
        if(m_useSharedMaterial) m_mat.SetFloat(m_propertyID, m_startFloat);
    }

    void OnDestroy()
    {
        if (!m_useSharedMaterial)
        {
            if (m_mat != null)
                DestroyImmediate(m_mat);
            m_mat = null;
        }
    }
}
