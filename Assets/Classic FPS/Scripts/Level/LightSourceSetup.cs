using UnityEngine;
using UnityEditor;

namespace HellishBattle.Level
{
    public class LightSourceSetup : MonoBehaviour
    {
        public Light lightSource;
        public SpriteRenderer spriteRenderer;

        public LightMode lightMode;

        public Color lightColor;
        [ColorUsage(false, true)] public Color spriteColor;

        public Texture2D EmissionTexture;
        public bool ScaleTo32Res = true;
        public float ManualScale = 1;

        // Base
        public float lightRange;
        public float lightIntensity;
        // Wave
        public float lightWaveMaxIntensity;
        public float lightWaveMinIntensity;
        public float lightWaveTime;
        // Flicker
        public float flickerSpeed = 0.1f;
        public float flickerIntensity = 0.2f;


        bool _endWave = false;
        float _timer = 0;
        [HideInInspector] public float scale = 1;


        void Start()
        {
            Material mymat = spriteRenderer.material;
            mymat.SetColor("_EmissionColor", spriteColor);

            if (!ScaleTo32Res) { scale = ManualScale; }

            if (EmissionTexture != null)
            {
                mymat.SetTexture("_EmissionMap", EmissionTexture);
            }
            if (EmissionTexture != null) spriteRenderer.sprite = Sprite.Create(EmissionTexture, new Rect(0.0f, 0.0f, EmissionTexture.width, EmissionTexture.height), new Vector2(0.5f, 0.0f), 32 * scale);

            lightSource.color = lightColor;
            lightSource.intensity = lightIntensity;
            lightSource.range = lightRange;

            if (lightMode == LightMode.Wave) { lightSource.intensity = lightWaveMinIntensity; }
        }

        private void OnValidate()
        {
            lightSource.color = lightColor;
            lightSource.range = lightRange;

            if (lightMode == LightMode.Static) lightSource.intensity = lightIntensity;
            if (lightMode == LightMode.Wave) lightSource.intensity = (lightWaveMinIntensity + lightWaveMaxIntensity) / 2;
        }

        private void Update()
        {
            if (lightMode == LightMode.Wave)
            {
                float lightWaveValueIntensity = lightWaveMaxIntensity - lightWaveMinIntensity;

                _timer += Time.deltaTime;

                if (_timer > lightWaveTime) { _timer = 0; _endWave = !_endWave; }

                if (_endWave) { lightSource.intensity -= lightWaveValueIntensity * (Time.deltaTime / lightWaveTime); }
                else { lightSource.intensity += lightWaveValueIntensity * (Time.deltaTime / lightWaveTime); }
            }
            if (lightMode == LightMode.Flicker)
            {
                float flickerAmount = Random.Range(0.0f, flickerIntensity);
                lightSource.intensity = lightIntensity - flickerAmount;
                Invoke("ResetLight", flickerSpeed);
            }
        }
        void ResetLight() { lightSource.intensity = lightIntensity; }
    }

    /// Custom Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(LightSourceSetup))]
    [CanEditMultipleObjects]
    public class LightSourceSetupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            LightSourceSetup tiles = (LightSourceSetup)target;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Light Source Setup", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("lightSource"), new GUIContent("Light Source"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteRenderer"), new GUIContent("Sprite Renderer"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("EmissionTexture"), new GUIContent("Emission Texture"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ScaleTo32Res"), new GUIContent("Scale To 32px Res"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ManualScale"), new GUIContent("Manual Scale"));

            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Light Source Setup", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("lightColor"), new GUIContent("Light Color"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteColor"), new GUIContent("Glow Color"));

            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Light Source Setup", EditorStyles.boldLabel);
            tiles.lightMode = (LightMode)EditorGUILayout.EnumPopup(tiles.lightMode, EditorStyles.toolbarDropDown, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("lightRange"), new GUIContent("Light Range"));
            if (tiles.lightMode == LightMode.Static)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lightIntensity"), new GUIContent("Light Intensity"));
            }
            if (tiles.lightMode == LightMode.Wave)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lightWaveMinIntensity"), new GUIContent("Light Intensity [Minimum]"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lightWaveMaxIntensity"), new GUIContent("Light Intensity [Maximum]"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lightWaveTime"), new GUIContent("Wave Light Time"));
            }
            if (tiles.lightMode == LightMode.Flicker)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lightIntensity"), new GUIContent("Base Light Intensity"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("flickerSpeed"), new GUIContent("Flicker Light Speed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("flickerIntensity"), new GUIContent("Flicker Light Intensity"));
            }

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

    public enum LightMode
    {
        Static = 0, Wave = 1, Flicker = 2
    }
}