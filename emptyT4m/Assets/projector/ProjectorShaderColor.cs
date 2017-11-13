using UnityEngine;
using System.Collections;

public enum ProjectorShaderColorProperties
{
    _MainColor
};

public class ProjectorShaderColor : MonoBehaviour {
    public ProjectorShaderColorProperties m_colorProperty = ProjectorShaderColorProperties._MainColor;
    public Gradient m_mainColorGradient = new Gradient()
    {
        colorKeys = new GradientColorKey[]
        {
                new GradientColorKey(new Color(1f, .639f, .482f, 1f), 0f),
                new GradientColorKey(new Color(1f, .725f, .482f, 1f), .10f),
                new GradientColorKey(new Color(1f, .851f, .722f, 1f), .50f),
                new GradientColorKey(new Color(1f, .725f, .482f, 1f), .90f),
                new GradientColorKey(new Color(1f, .639f, .482f, 1f), 1f)
        }
    }
    ;

    public float m_graphTimeMultiplier = 6;
    public bool m_isLoop = false;
    public bool m_useSharedMaterial = false;

    private bool m_canUpdate;
    private float m_startTime;
    private Material m_mat;
    private string m_shaderProperty;
    private int m_propertyID;
    private bool m_isInitialized;
    private Color m_startColor;

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

        m_shaderProperty = m_colorProperty.ToString();
        if (m_mat.HasProperty(m_shaderProperty)) m_propertyID = Shader.PropertyToID(m_shaderProperty);
        m_startColor = m_mat.GetColor(m_propertyID);
        var eval = m_mainColorGradient.Evaluate(0);
        m_mat.SetColor(m_propertyID, eval);
        m_isInitialized = true;
    }

    private void OnEnable()
    {
        m_startTime = Time.time;
        m_canUpdate = true;
        if (!m_isInitialized)
        {
            var eval = m_mainColorGradient.Evaluate(0);
            m_mat.SetColor(m_propertyID, eval);
        }
    }

    // Use this for initialization
    void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
        var time = Time.time - m_startTime;
        if (m_canUpdate)
        {
            var eval = m_mainColorGradient.Evaluate(time / m_graphTimeMultiplier);
            m_mat.SetColor(m_propertyID, eval);
        }
        if (time >= m_graphTimeMultiplier)
        {
            if (m_isLoop) m_startTime = Time.time;
            else m_canUpdate = false;
        }
    }

    void OnDisable()
    {
        if(m_mat == null)
        {
            return;
        }
        m_mat.SetColor(m_propertyID, m_startColor);
    }
}
