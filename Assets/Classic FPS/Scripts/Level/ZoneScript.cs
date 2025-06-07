using HellishBattle.Weapon;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace HellishBattle.Level
{
    public class ZoneScript : MonoBehaviour
    {
        public ZoneType type;

        public bool ShowEnterPanel = true;
        public LocalizedString EnterZoneName;
        public LocalizedString EnterZoneDescription;

        public bool ShowExitPanel = true;
        public LocalizedString ExitZoneName;
        public LocalizedString ExitZoneDescription;

        public UnityEvent OnEnter, OnExit;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (ShowEnterPanel) GameManager.Instance.PanelSetup(EnterZoneName.GetLocalization(), EnterZoneDescription.GetLocalization());
                OnEnter.Invoke();

                if (type == ZoneType.SafeArea) Camera.main.transform.parent.Find("Weapons").GetComponent<WeaponSwitch>().SafeZone = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (ShowExitPanel) GameManager.Instance.PanelSetup(ExitZoneName.GetLocalization(), ExitZoneDescription.GetLocalization());
                OnExit.Invoke();

                if (type == ZoneType.SafeArea) Camera.main.transform.parent.Find("Weapons").GetComponent<WeaponSwitch>().SafeZone = false;
            }
        }
    }

    /// Custom Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(ZoneScript))]
    public class ZoneScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            ZoneScript zone = (ZoneScript)target;
            EditorGUI.indentLevel = 0;

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Zone Settings", EditorStyles.boldLabel);
            zone.type = (ZoneType)EditorGUILayout.EnumPopup(zone.type, EditorStyles.toolbarDropDown, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Enter Text", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ShowEnterPanel"), new GUIContent("Enable"));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            if (zone.ShowEnterPanel)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("EnterZoneName"), new GUIContent("Enable"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("EnterZoneDescription"), new GUIContent("Enable"));
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Exit Text", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ShowExitPanel"), new GUIContent("Enable"));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            if (zone.ShowExitPanel)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ExitZoneName"), new GUIContent("Enable"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ExitZoneDescription"), new GUIContent("Enable"));
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnEnter"), new GUIContent("OnEnter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnExit"), new GUIContent("OnExit"));


            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

    [System.Serializable]
    public enum ZoneType
    {
        SafeArea = 0, Custom = 1
    }
}
