///Update 17.2.14:Add amb col controller
using System;
using UnityEditor;
using UnityEngine;

  [CustomEditor(typeof(EnvironmentSetup))]
public class HeightFogEditor : CustomEditorBase
{

    private SerializedObject serObj;

    #region SerializedProperties

    //fog
    private SerializedProperty fogHeightStart;
    private SerializedProperty fogHeightEnd;
    private SerializedProperty fogEnable;

    //ambient weather color
    private SerializedProperty ambScale;
    private SerializedProperty ambCol;
    private SerializedProperty ambEnable;

    //wet
    private SerializedProperty wetVal;
    private SerializedProperty wetEnable;

    //snow
    private SerializedProperty snowLevel;
    private SerializedProperty snowTex;
    private SerializedProperty snowEnable;

    //day and night change
    //private SerializedProperty LMpro;
    //private SerializedProperty LMtex;

    //rain
    private SerializedProperty RainDisturbTex;
    private SerializedProperty enableRainDisturb;
    private SerializedProperty intervalTime;
    private SerializedProperty disturbFactor;
    #endregion SerializedProperty

    #region foldouts
    bool _fogFoldout;
    bool _ambientFoldout;
    bool _wetFoldout;
    bool _snowFoldout;
    bool _rainFoldout;
    #endregion foldouts

    EnvironmentSetup cls;

    void OnEnable()
      {
          serObj = new SerializedObject(target);
          cls = (EnvironmentSetup)target;

          //fog
          fogHeightStart = serObj.FindProperty("FogHeightStart");
          fogHeightEnd = serObj.FindProperty("FogHeightEnd");
          fogEnable = serObj.FindProperty("enableFogHeight");

        //ambient
        ambScale = serObj.FindProperty("ambientEffectScale");
        ambCol = serObj.FindProperty("newAmbCol");
        ambEnable = serObj.FindProperty("enableAmbient");

        //wet
        wetVal = serObj.FindProperty("wetEffect");
        wetEnable = serObj.FindProperty("enableWet");

          //snow
          snowLevel = serObj.FindProperty("SnowLevel");
          snowTex = serObj.FindProperty("SnowTex");
        snowEnable = serObj.FindProperty("enableSnow");

          //rain
          RainDisturbTex = serObj.FindProperty("rainDisturbTex");
          enableRainDisturb = serObj.FindProperty("enableRainDisturb");
          intervalTime = serObj.FindProperty("intervalTime");
          disturbFactor = serObj.FindProperty("disturbFactor");
      }

    
    private void OnFogGUI()
    {
        _fogFoldout = EditorGUILayout.Foldout(_fogFoldout, "Height Fog");
        if (_fogFoldout)
        {
            EditorGUILayout.PropertyField(fogEnable, new GUIContent("Enable Fog Height"));
            EditorGUILayout.PropertyField(fogHeightStart, new GUIContent("FogHeightStart"));
            EditorGUILayout.PropertyField(fogHeightEnd, new GUIContent("FogHeightEnd"));
            if (cls.FogHeightStart > cls.FogHeightEnd)
                cls.FogHeightStart = cls.FogHeightEnd;

            
        }
     }

    private void OnAmbientGUI()
    {
        _ambientFoldout = EditorGUILayout.Foldout(_ambientFoldout, "Ambient");
        if (_ambientFoldout)
        {
            EditorGUILayout.PropertyField(ambEnable, new GUIContent("Amb Changed by Weather"));
            EditorGUILayout.PropertyField(ambScale, new GUIContent("Ambient Effect Scale"));
            EditorGUILayout.PropertyField(ambCol, new GUIContent("Ambient Sky Color"));
        }
    }

    private void OnWetGUI()
    {
        _wetFoldout = EditorGUILayout.Foldout(_wetFoldout, "Wet");
        if (_wetFoldout)
        {
            EditorGUILayout.PropertyField(wetEnable, new GUIContent("Wet Enable"));
            EditorGUILayout.PropertyField(wetVal, new GUIContent("Wet Effect"));
        }
    }

    private void OnSnowGUI()
    {
        _snowFoldout = EditorGUILayout.Foldout(_snowFoldout, "Snow");
        if(_snowFoldout)
        {
            EditorGUILayout.PropertyField(snowEnable, new GUIContent("Snow Enable"));

            EditorGUILayout.PropertyField(snowLevel, new GUIContent("Snow Effect"));
            EditorGUILayout.PropertyField(snowTex, new GUIContent("Snow Texture"));
        }
    }

    private void OnRainGUI()
    {
        _rainFoldout = EditorGUILayout.Foldout(_rainFoldout, "Rain");
        if(_rainFoldout)
        {
            EditorGUILayout.PropertyField(enableRainDisturb, new GUIContent("Enable Rain Disturb"));
            EditorGUILayout.PropertyField(intervalTime, new GUIContent("Disturb interval Time"));
            EditorGUILayout.PropertyField(disturbFactor, new GUIContent("Disturb disturb factor"));
            EditorGUILayout.PropertyField(RainDisturbTex, true);
        }
    }

      public override void OnInspectorGUI()
      {
          serObj.Update();
          EditorGUI.BeginChangeCheck();
          OnFogGUI();
          OnAmbientGUI();
          OnWetGUI();
          OnSnowGUI();
          OnRainGUI();

          serObj.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
        {
            float FogHeiParaZ = 1 / (cls.FogHeightEnd - cls.FogHeightStart);
            float FogHeiParaW = -cls.FogHeightStart / (cls.FogHeightEnd - cls.FogHeightStart);
            cls.SetHeightFog(FogHeiParaZ, FogHeiParaW);
            cls.OpenHeightFog(cls.enableFogHeight);
            cls.OpenAmbientEffect(cls.enableAmbient);
            cls.OpenRainDisturb(cls.enableRainDisturb);
            cls.OpenWetEffect(cls.enableWet);
            cls.OpenSnowEffect(cls.enableSnow);
        }
    }
}
