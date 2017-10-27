using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//public enum ProjectorShaderProperties
//{
//    _Color,
//    _BurnAmount,
//    _XSpeed,
//    _YSpeed
//    //_MaskPow,
//    //_Speed,
//    //_BumpAmt
//};
public enum LineRendererShaderProperties
{
    _TilingX,
    _TilingY
};

public class LineRendererUvAnim : MonoBehaviour
{
    public LineRendererShaderProperties ShaderFloatProperty;
    public bool mUseSharedMaterial;
    private LineRenderer mLineRenderer;

    private bool mCanUpdate;
    private float mStartTime;
    Texture tex = new Texture();
    private Material mMat;
    private float mStartFloat;
    private int mPropertyID;
    private string mShaderProperty;

    public Vector3 mStartPosition;
    public Vector3 mEndPosition;
    public float mMinDistance;

    private void Awake()
    {
        //mat = GetComponent<Renderer>().material;
        var rend = GetComponent<Renderer>();
        if (rend == null)
        {
            var lineRenderer = GetComponent<LineRenderer>();
            //mStartPosition = lineRenderer.
            if (lineRenderer != null)
            {
                if (!mUseSharedMaterial)
                {
                    if (!lineRenderer.material.name.EndsWith("(Instance)"))
                        lineRenderer.material = new Material(lineRenderer.material) { name = lineRenderer.material.name + " (Instance)" };
                    mMat = lineRenderer.material;
                }
                else
                {
                    mMat = lineRenderer.material;
                }
            }
        }
        else
        {
            if (!mUseSharedMaterial) mMat = rend.material;
            else mMat = rend.sharedMaterial;
        }

        //mShaderProperty = ShaderFloatProperty.ToString();
        //if (mMat.HasProperty(mShaderProperty)) mPropertyID = Shader.PropertyToID(mShaderProperty);
        //mStartFloat = mMat.GetFloat(mPropertyID);
        //mMat.SetFloat(mPropertyID, mStartFloat);
        //mIsInitialized = true;
        UpdateTilings();
    }

    private void UpdateTilings()
    {
        var distance = (mStartPosition - mEndPosition).magnitude;
        var count = (int)(distance / mMinDistance);
        List<Vector3> positions = new List<Vector3>();
        for(int i = 0; i <= count - 1; i++)
        {
            var interpolatedPoint = mStartPosition + (mEndPosition - mStartPosition) * i * 1.0f / count;
            positions.Add(interpolatedPoint);
        }
        mLineRenderer.SetPositions(positions.ToArray());
    }

    private void OnEnable()
    {
        //startTime = Time.time;
        //canUpdate = true;
        //if (isInitialized)
        //{
        //    var eval = FloatCurve.Evaluate(0) * GraphIntensityMultiplier;
        //    mat.SetFloat(propertyID, eval);
        //}
    }

    private void Update()
    {
        
    }

    void OnDisable()
    {
        if (mUseSharedMaterial) mMat.SetFloat(mPropertyID, mStartFloat);
    }

    void OnDestroy()
    {
        if (!mUseSharedMaterial)
        {
            if (mMat != null)
                DestroyImmediate(mMat);
            mMat = null;
        }
    }
}
