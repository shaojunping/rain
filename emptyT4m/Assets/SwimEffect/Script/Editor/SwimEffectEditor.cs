using UnityEngine;
using System.Collections;
using UnityEditor;

namespace TSHD.SwimEffect
{
    [CustomEditor(typeof(SwimEffect))]
    public class SwimEffectEditor : CustomEditorBase
    {
        SerializedObject _serObject;
        SwimEffect _swimEffect;

        #region SerializeProperties

        //fog
        SerializedProperty _fogStart;
        SerializedProperty _fogEnd;
        SerializedProperty _fogColor;

        //ambient
        SerializedProperty _underWaterAmbientColor;

        //height fog
        SerializedProperty _heightFogStart;
        SerializedProperty _heightFogEnd;
        SerializedProperty _heightFogColor;
        SerializedProperty _heightFogDensity;

        //player obj
        SerializedProperty _playerObj;

        //camera
        SerializedProperty _camera;

        //under water lens
        SerializedProperty _distortionTexture;
        SerializedProperty _distortionIntensity;
        SerializedProperty _distortionSpeed;

        //wet lens
        SerializedProperty _wetLensDryTime;
        SerializedProperty _normalTextureArray;
        SerializedProperty _wetLensFps;

        //blur
        SerializedProperty _downsample;
        SerializedProperty _blurSize;
        SerializedProperty _blurIterations;

        //caustic 
        SerializedProperty _projector;
        SerializedProperty _causticFps;
        SerializedProperty _causticFrames;

        //UIs
        SerializedProperty _blurToggle;
        SerializedProperty _waterLensToggle;
        SerializedProperty _airLensToggle;
        SerializedProperty _projectorToggle;
        SerializedProperty _switchToggle;
        SerializedProperty _underWaterToggle;

        #endregion

        #region foldouts
        bool _fogFoldout;
        bool _ambientColorFoldout;
        bool _heightFogFoldout;
        bool _playerObjFoldout;
        bool _cameraFoldout;
        bool _underWaterFoldout;
        bool _wetLensFoldout;
        bool _blurFoldout;
        bool _causticFoldout;
        bool _uiFoldout;
        #endregion


        void OnEnable()
        {
            _serObject = new SerializedObject(target);

            //fog
            _fogStart = _serObject.FindProperty("_fogStart");
            _fogEnd = _serObject.FindProperty("_fogEnd");
            _fogColor = _serObject.FindProperty("_fogColor");

            //ambient color
            _underWaterAmbientColor = _serObject.FindProperty("_underWaterAmbientColor");

            //height fog
            _heightFogStart = _serObject.FindProperty("_fogHeightStart");
            _heightFogEnd = _serObject.FindProperty("_fogHeightEnd");
            _heightFogColor = _serObject.FindProperty("_fogHeightColor");
            _heightFogDensity = _serObject.FindProperty("_heightFogDensity");

            //player obj
            _playerObj = _serObject.FindProperty("_playerObj");

            //camera
            _camera = _serObject.FindProperty("_camera");

            //under water lens
            _distortionTexture = _serObject.FindProperty("_distortionTexture");
            _distortionIntensity = _serObject.FindProperty("_distortionIntensity");
            _distortionSpeed = _serObject.FindProperty("_distortionSpeed");

            //wet lens
            _wetLensDryTime = _serObject.FindProperty("_wetLensDryTime");
            _wetLensFps = _serObject.FindProperty("_wetLensFps");
            _normalTextureArray = _serObject.FindProperty("_normalTextureArray");

            //blur
            _downsample = _serObject.FindProperty("_downsample");
            _blurSize = _serObject.FindProperty("_blurSize");
            _blurIterations = _serObject.FindProperty("_blurIterations");

            //caustic 
            _projector = _serObject.FindProperty("_projector");
            _causticFps = _serObject.FindProperty("_causticFps");
            _causticFrames = _serObject.FindProperty("_causticFrames");

            //uis
            _blurToggle = _serObject.FindProperty("_blurToggle");
            _waterLensToggle = _serObject.FindProperty("_waterLensToggle");
            _airLensToggle = _serObject.FindProperty("_airLensToggle");
            _projectorToggle = _serObject.FindProperty("_projectorToggle");
            _switchToggle = _serObject.FindProperty("_switchToggle");
            _underWaterToggle = _serObject.FindProperty("_underWaterToggle");
        }

        public override void OnInspectorGUI()
        {
            _serObject.Update();

            Separator(WhiteColor, 2);
            Text("Swim Effect", textTitleStyle, true);
            Separator(WhiteColor, 2);

            Fog();
            UnderWaterAmbientColor();
            HeightFog();
            PlayerObj();
            CameraObj();
            UnderWaterLens();
            WetLens();
            Blur();
            Caustic();
            UIs();

            _serObject.ApplyModifiedProperties();
        }

        void Fog()
        {
            _fogFoldout = EditorGUILayout.Foldout(_fogFoldout, "Fog");
            if (_fogFoldout)
            {
                Separator(WhiteColor, 2);
                Text("Fog", textTitleStyle, true);
                Separator(WhiteColor, 2);
                EditorGUILayout.PropertyField(_fogStart, new GUIContent("Fog Start"));
                EditorGUILayout.PropertyField(_fogEnd, new GUIContent("Fog End"));
                EditorGUILayout.PropertyField(_fogColor, new GUIContent("Fog Color"));
                Separator(WhiteColor, 2);
            }
        }

        void UnderWaterAmbientColor()
        {
            _ambientColorFoldout = EditorGUILayout.Foldout(_ambientColorFoldout, "Under Water Ambient Color");
            if (_ambientColorFoldout)
            {
                Separator(WhiteColor, 2);
                Text("UnderWater Ambient Color", textTitleStyle, true);
                Separator(WhiteColor, 2);

                EditorGUILayout.PropertyField(_underWaterAmbientColor, new GUIContent("Under Water Ambient Color"));

                Separator(WhiteColor, 2);

            }
        }

        void HeightFog()
        {
            _heightFogFoldout = EditorGUILayout.Foldout(_heightFogFoldout, "Height Fog");
            if (_heightFogFoldout)
            {
                Separator(WhiteColor, 2);
                Text("Height Fog", textTitleStyle, true);
                Separator(WhiteColor, 2);

                EditorGUILayout.PropertyField(_heightFogStart, new GUIContent("Height Fog Start"));
                EditorGUILayout.PropertyField(_heightFogEnd, new GUIContent("Height Fog End"));
                EditorGUILayout.PropertyField(_underWaterAmbientColor, new GUIContent("Under Water Ambient Color"));
                EditorGUILayout.PropertyField(_underWaterAmbientColor, new GUIContent("Under Water Ambient Color"));

                Separator(WhiteColor, 2);
            }
        }

        void PlayerObj()
        {
            _playerObjFoldout = EditorGUILayout.Foldout(_playerObjFoldout, "Player Object");
            if (_playerObjFoldout)
            {
                Separator(WhiteColor, 2);
                Text("Player Object", textTitleStyle, true);
                Separator(WhiteColor, 2);

                EditorGUILayout.PropertyField(_playerObj, new GUIContent("Player Object"));
                if (_playerObj.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("Please Assign Player Object", MessageType.Error);
                }
                Separator(WhiteColor, 2);
            }
        }

        void CameraObj()
        {
            _cameraFoldout = EditorGUILayout.Foldout(_cameraFoldout, "Camera Object");
            if (_cameraFoldout)
            {
                Separator(WhiteColor, 2);
                Text("Camera Object", textTitleStyle, true);
                Separator(WhiteColor, 2);

                EditorGUILayout.PropertyField(_camera, new GUIContent("Camera Object"));
                if (_camera.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("Please Assign Camera Object", MessageType.Error);
                }

                Separator(WhiteColor, 2);
            }

        }

        void UnderWaterLens()
        {
            _underWaterFoldout = EditorGUILayout.Foldout(_underWaterFoldout, "Under Water Lens");
            if (_underWaterFoldout)
            {
                Separator(WhiteColor, 2);
                Text("Under Water Lens", textTitleStyle, true);
                Separator(WhiteColor, 2);
                EditorGUILayout.PropertyField(_distortionTexture, new GUIContent("Distortion Texture"));
                EditorGUILayout.PropertyField(_distortionIntensity, new GUIContent("Distortion Intensity"));
                EditorGUILayout.PropertyField(_distortionSpeed, new GUIContent("Distortion Speed"));
                Separator(WhiteColor, 2);
            }
        }

        void WetLens()
        {
            _wetLensFoldout = EditorGUILayout.Foldout(_wetLensFoldout, "Wet Lens");
            if (_wetLensFoldout)
            {
                Separator(WhiteColor, 2);
                Text("Wet Lens", textTitleStyle, true);
                Separator(WhiteColor, 2);

                EditorGUILayout.PropertyField(_wetLensDryTime, new GUIContent("Wet Lens Dry Time"));
                EditorGUILayout.PropertyField(_wetLensFps, new GUIContent("Wet Lens FPS"));
                EditorGUILayout.PropertyField(_normalTextureArray, new GUIContent("Wet Lens Textures"), true);

                Separator(WhiteColor, 2);
            }
        }

        void Blur()
        {
            _blurFoldout = EditorGUILayout.Foldout(_blurFoldout, "Blur");
            if (_blurFoldout)
            {
                Separator(WhiteColor, 2);
                Text("Blur", textTitleStyle, true);
                Separator(WhiteColor, 2);

                EditorGUILayout.PropertyField(_downsample, new GUIContent("DownSample"));
                EditorGUILayout.PropertyField(_blurSize, new GUIContent("Blur Size"));
                EditorGUILayout.PropertyField(_blurIterations, new GUIContent("Blue Iteration"), true);

                Separator(WhiteColor, 2);
            }
        }

        void Caustic()
        {
            _causticFoldout = EditorGUILayout.Foldout(_causticFoldout, "Caustic");
            if (_causticFoldout)
            {
                Separator(WhiteColor, 2);
                Text("Caustic", textTitleStyle, true);
                Separator(WhiteColor, 2);

                EditorGUILayout.PropertyField(_projector, new GUIContent("Projector"));
                if (_projector == null)
                {
                    EditorGUILayout.HelpBox("Please Assign Projector Object", MessageType.Error);
                }

                EditorGUILayout.PropertyField(_causticFps, new GUIContent("Caustic FPS"));
                //ListIterator("_causticFrames", ref _listVisibility);

                EditorGUILayout.PropertyField(_causticFrames, new GUIContent("Caustic Textures"), true);

                Separator(WhiteColor, 2);
            }
        }

        void UIs()
        {
            _uiFoldout = EditorGUILayout.Foldout(_uiFoldout, "UIs (For Test)");
            if (_uiFoldout)
            {
                Separator(WhiteColor, 2);
                Text("UIs (For Test)", textTitleStyle, true);
                Separator(WhiteColor, 2);

                EditorGUILayout.PropertyField(_blurToggle, new GUIContent("Blur Toggle"));
                EditorGUILayout.PropertyField(_waterLensToggle, new GUIContent("Water Lens Toggle"));
                EditorGUILayout.PropertyField(_airLensToggle, new GUIContent("Wet Lens Toggle"));
                EditorGUILayout.PropertyField(_projectorToggle, new GUIContent("Projector Toggle"));
                EditorGUILayout.PropertyField(_switchToggle, new GUIContent("Switch Toggle"));
                EditorGUILayout.PropertyField(_underWaterToggle, new GUIContent(" Is Under Water Toggle"));

                Separator(WhiteColor, 2);
            }
        }

        GUIStyle textTitleStyle
        {

            get
            {

                GUIStyle style = new GUIStyle(EditorStyles.label);
                style.fontStyle = FontStyle.Bold;
                style.fontSize = 12;

                return style;
            }
        }

        public void ListIterator(string propertyPath, ref bool visible)
        {
            SerializedProperty listProperty = serializedObject.FindProperty(propertyPath);
            visible = EditorGUILayout.Foldout(visible, listProperty.name);
            if (visible)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < listProperty.arraySize; i++)
                {
                    SerializedProperty elementProperty = listProperty.GetArrayElementAtIndex(i);
                    Rect drawZone = GUILayoutUtility.GetRect(0f, 16f);
                    bool showChildren = EditorGUI.PropertyField(drawZone, elementProperty);
                }
                EditorGUI.indentLevel--;
            }
        }

    }
}

