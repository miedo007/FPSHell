using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

namespace HellishBattle.Level
{
    public class SwitchScript : MonoBehaviour
    {
        public bool StartState = false;
        public AudioClip SwitchFX;
        public List<SwitchScript> SwitchFamily;
        public UnityEvent TurnOff;
        public UnityEvent TurnOn;

        AudioSource source;

        [HideInInspector] public bool isOpen = false;

        private void Start()
        {
            if (StartState) { TurnOn.Invoke(); }
            else { TurnOff.Invoke(); }

            isOpen = StartState;
        }

        public void Interaction()
        {
            isOpen = !isOpen;
            if (isOpen) { TurnOn.Invoke(); }
            else { TurnOff.Invoke(); }

            source = GetComponent<AudioSource>();
            if (source != null && SwitchFX != null) { source.PlayOneShot(SwitchFX); }

            // Family Switch
            for (int i = 0; i < SwitchFamily.Count; i++)
            {
                if (SwitchFamily[i] != null && SwitchFamily[i].isOpen && SwitchFamily[i] != this)
                {
                    SwitchFamily[i].isOpen = false;
                    SwitchFamily[i].TurnOff.Invoke();
                }
            }
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(SwitchScript)), CanEditMultipleObjects]
    public class SwitchScriptEditor : Editor
    {
        private bool FamilySwitch = true;

        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            SwitchScript safe = (SwitchScript)target;
            EditorGUI.indentLevel = 0;

            EditorGUILayout.BeginHorizontal("helpbox", GUILayout.Height(EditorGUIUtility.singleLineHeight + 6));
            EditorGUILayout.LabelField("In-Game Switch Script", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.ObjectField(MonoScript.FromMonoBehaviour((SwitchScript)target), typeof(SwitchScript), false);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Switch Settings", "Set Sounds and Defualt State");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("StartState"), new GUIContent("Start State"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SwitchFX"), new GUIContent("Switch Sound"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Switch Family Settings", "Add Action on TurnOn or TurnOff");
            TinyGUI.ShowArray(serializedObject, "SwitchFamily", "Switch Family List", ref FamilySwitch);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Unity Events", "Add Action on TurnOn or TurnOff");
            EditorGUILayout.EndVertical();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("TurnOn"), new GUIContent("On Turn On"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TurnOff"), new GUIContent("On Turn Off"));

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}