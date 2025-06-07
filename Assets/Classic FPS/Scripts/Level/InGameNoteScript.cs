using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEditor;

namespace HellishBattle.Level
{
    public class InGameNoteScript : MonoBehaviour
    {
        public LocalizedString NoteTitle;
        public LocalizedString NoteText;

        public GameObject UI;
        public TMP_Text TitleTextField, NodeTextField;

        public UnityEvent OnEnter, OnClose;

        private void Awake()
        {
            TitleTextField.text = NoteTitle.GetLocalization();
            NodeTextField.text = NoteText.GetLocalization();
            UI.SetActive(false);
        }

        public void OpenNode()
        {
            UI.SetActive(true);
            UnlockCursor();
            OnEnter.Invoke();
        }

        public void CloseNode()
        {
            UI.SetActive(false);
            LockCursor();
            OnClose.Invoke();
        }

        public void LockCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            GameManager.Instance.paused = false;
        }

        public void UnlockCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            GameManager.Instance.paused = true;
        }
    }

    /// Custom Editor/
#if UNITY_EDITOR

    [CustomEditor(typeof(InGameNoteScript))]
    public class InGameNoteScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            InGameNoteScript zone = (InGameNoteScript)target;

            EditorGUILayout.BeginHorizontal("helpbox", GUILayout.Height(EditorGUIUtility.singleLineHeight + 6));
            EditorGUILayout.LabelField("In-Game Note Script", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.ObjectField(MonoScript.FromMonoBehaviour((InGameNoteScript)target), typeof(InGameNoteScript), false);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Node Text", "Assign Title and Note Content here");
            TinyGUI.LocalizedString(zone.NoteTitle, serializedObject.FindProperty("NoteTitle"));
            TinyGUI.LocalizedString(zone.NoteText, serializedObject.FindProperty("NoteText"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("UI Elements", "Elements belonging to In_Game Note UI");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("UI"), new GUIContent("Damage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TitleTextField"), new GUIContent("Title Text Field"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("NodeTextField"), new GUIContent("Node Text Field"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Unity Events", "Add Action on OnEnter or OnClose");
            EditorGUILayout.EndVertical();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnEnter"), new GUIContent("On Enter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnClose"), new GUIContent("On Close"));


            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}