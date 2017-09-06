///Update 17.2.14:Add amb col controller
using System;
using UnityEditor;
using UnityEngine;

  [CustomEditor(typeof(EnvironmentSetup))]
public class HeightFogEditor : Editor
{
    private SerializedObject serObj;
    private SerializedProperty fogHeiStart;
    private SerializedProperty fogHeiEnd;
    private SerializedProperty fogTexture;
    //private SerializedProperty fogGradientColor;
    private SerializedProperty ambScale;
    private SerializedProperty ambCol;
    private SerializedProperty ambWeather;
    private SerializedProperty wetVal;

    private SerializedProperty snowLevel;
    private SerializedProperty snowTex;

    private SerializedProperty LMpro;
    private SerializedProperty LMtex;

    private SerializedProperty RDtex;
    private SerializedProperty enableRD;
    private SerializedProperty intervalTime;
    private SerializedProperty disturbFactor;

    EnvironmentSetup cls;

    void OnEnable()
      {
          serObj = new SerializedObject(target);
          cls = (EnvironmentSetup)target;

          fogHeiStart = serObj.FindProperty("FogHeightStart");
          fogHeiEnd = serObj.FindProperty("FogHeightEnd");
          fogTexture = serObj.FindProperty("FogTexture");
          //fogGradientColor = serObj.FindProperty("GradientFogColor");
          ambScale = serObj.FindProperty("ambientEffectScale");
          ambCol = serObj.FindProperty("newAmbCol");
          ambWeather= serObj.FindProperty("weatherAmbient");
          wetVal = serObj.FindProperty("wetEffect");
          snowLevel = serObj.FindProperty("SnowLevel");
          snowTex = serObj.FindProperty("SnowTex");

          LMpro = serObj.FindProperty("LMLerp");
          LMtex = serObj.FindProperty("SecondLMTex");

          RDtex = serObj.FindProperty("rainDisturbTex");
          enableRD = serObj.FindProperty("enableRainDisturb");
          intervalTime = serObj.FindProperty("intervalTime");
          disturbFactor = serObj.FindProperty("disturbFactor");
      }
      public override void OnInspectorGUI()
      {
          serObj.Update();
          EditorGUI.BeginChangeCheck();

          EditorGUILayout.PropertyField(fogHeiStart, new GUIContent("FogHeightStart"));
          EditorGUILayout.PropertyField(fogHeiEnd, new GUIContent("FogHeightEnd"));
         
          //EditorGUILayout.PropertyField(fogGradientColor, new GUIContent("GradientFogColor"));
          //if(cls.GradientFogColor.maxColorComponent>0.01f)
          //    EditorGUILayout.PropertyField(fogTexture, new GUIContent("GradientFogTexture"));
          EditorGUILayout.PropertyField(ambWeather, new GUIContent("Amb Changed by Weather"));
          EditorGUILayout.PropertyField(ambScale, new GUIContent("Ambient Effect Scale"));
          EditorGUILayout.PropertyField(ambCol, new GUIContent("Ambient Sky Color"));

          EditorGUILayout.PropertyField(wetVal, new GUIContent("Wet Effect"));

          EditorGUILayout.PropertyField(snowLevel, new GUIContent("Snow Effect"));
          EditorGUILayout.PropertyField(snowTex, new GUIContent("Snow Texture"));

          EditorGUILayout.PropertyField(LMpro, new GUIContent("Lightmap Lerp"));
          EditorGUILayout.PropertyField(LMtex, true);

          EditorGUILayout.PropertyField(enableRD, new GUIContent("Enable Rain Disturb"));
          EditorGUILayout.PropertyField(intervalTime, new GUIContent("Disturb interval Time"));
          EditorGUILayout.PropertyField(disturbFactor, new GUIContent("Disturb disturb factor"));
          EditorGUILayout.PropertyField(RDtex, true);

          if (cls.FogHeightStart > cls.FogHeightEnd)
              cls.FogHeightStart = cls.FogHeightEnd;

          if (EditorGUI.EndChangeCheck())
          {
              float FogHeiParaZ = 1 / (cls.FogHeightEnd - cls.FogHeightStart);
              float FogHeiParaW = -cls.FogHeightStart / (cls.FogHeightEnd - cls.FogHeightStart);
              Shader.SetGlobalFloat("_FogHeiParaZ", FogHeiParaZ);
              Shader.SetGlobalFloat("_FogHeiParaW", FogHeiParaW);
              //Shader.SetGlobalTexture("_FogTex", cls.FogTexture);
              //Shader.SetGlobalColor("_GradientFogColor", cls.GradientFogColor);

              //Important:LinearSpace!!
              //Shader.SetGlobalFloat("_RefLerp", Mathf.GammaToLinearSpace(cls.wetArea));
              Shader.SetGlobalFloat("_RefLerp", cls.wetEffect);
            
              RenderSettings.ambientSkyColor = cls.newAmbCol;

              Shader.SetGlobalFloat("_SnowLevel", Mathf.GammaToLinearSpace(cls.SnowLevel));
              Shader.SetGlobalTexture("_FogTex", cls.SnowTex);
              if (cls.weatherAmbient)
              {
                  if (cls.wetEffect > 0.0f && cls.SnowLevel == 0.0f)
                      Shader.SetGlobalFloat("_AmbScale", cls.ambientEffectScale * cls.wetEffect);
                  else
                      if (cls.SnowLevel > 0.0f && cls.wetEffect == 0.0f)
                          Shader.SetGlobalFloat("_AmbScale", cls.ambientEffectScale * cls.SnowLevel);
              }
                else
                {
                    Shader.SetGlobalFloat("_AmbScale", cls.ambientEffectScale);
                }

            cls.count = 0;
            //cls.
              //if (cls.LMLerp >= 0.01f && cls.LMLerp <= 0.99f)
              //{
              //    Shader.EnableKeyword("_BOTHLM");
              //    Shader.DisableKeyword("_DAYLM");
              //    Shader.DisableKeyword("_NIGHTLM");
              //}
              //if (cls.LMLerp < 0.01f)
              //{
              //    Shader.EnableKeyword("_DAYLM");
              //    Shader.DisableKeyword("_BOTHLM");
              //    Shader.DisableKeyword("_NIGHTLM");
              //}
              //if (cls.LMLerp > 0.99f)
              //{
              //    Shader.EnableKeyword("_NIGHTLM");
              //    Shader.DisableKeyword("_BOTHLM");
              //    Shader.DisableKeyword("_DAYLM");
              //}
              Shader.SetGlobalFloat("_LMLerp", cls.LMLerp);
          }

          serObj.ApplyModifiedProperties();
      }
}
