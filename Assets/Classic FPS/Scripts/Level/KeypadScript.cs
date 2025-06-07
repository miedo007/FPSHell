using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEditor;

namespace HellishBattle.Level
{
    [RequireComponent(typeof(AudioSource))]
    public class KeypadScript : MonoBehaviour
    {
        public int Code;
        public TMP_Text KeypadText;
        public TMP_Text LeftTryText;
        public GameObject KeyPadUI;
        public UnityEvent OnUnlock;
        public UnityEvent OnToManyTrials;

        public int NumberOfTry;
        public AudioClip clickSound, clearSound, SuccessSound, FailSound;

        bool Unlocked = false;
        string keypadText;
        int tryCount = 0;
        AudioSource source;

        private void Awake()
        {
            KeyPadUI.SetActive(false);
            source = GetComponent<AudioSource>();
        }

        private void Update()
        {
            KeypadText.text = keypadText;
            if (NumberOfTry != 0)
            {
                LeftTryText.text = $"Left Try: {NumberOfTry - tryCount}";
            }
            else
            {
                LeftTryText.gameObject.SetActive(false);
            }
        }

        public void Open()
        {
            if (tryCount >= NumberOfTry && NumberOfTry != 0)
            {
                source.PlayOneShot(FailSound);
            }
            else
            {
                if (Unlocked)
                {
                    OnUnlock.Invoke();
                }
                else
                {

                    KeyPadUI.SetActive(true);
                    UnlockCursor();
                }
            }
        }

        public void TryEnter()
        {
            tryCount++;
            if (keypadText == Code.ToString())
            {
                KeyPadUI.SetActive(false);
                Unlocked = true;
                LockCursor();
                OnUnlock.Invoke();

                source.PlayOneShot(SuccessSound);
            }
            else
            {
                keypadText = "";
                if (NumberOfTry != 0)
                {
                    LeftTryText.text = $"Left Try: {NumberOfTry - tryCount}";
                }
                if (tryCount >= NumberOfTry && NumberOfTry != 0)
                {
                    KeyPadUI.SetActive(false);
                    LockCursor();
                    OnToManyTrials.Invoke();
                }
            }
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

        public void PressKey(int i)
        {
            keypadText += i;
            source.PlayOneShot(clickSound);
        }

        public void ClearCode()
        {
            keypadText = "";
            source.PlayOneShot(clearSound);
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(KeypadScript)), CanEditMultipleObjects]
    public class KeypadScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            KeypadScript safe = (KeypadScript)target;
            EditorGUI.indentLevel = 0;

            EditorGUILayout.BeginHorizontal("helpbox", GUILayout.Height(EditorGUIUtility.singleLineHeight + 6));
            EditorGUILayout.LabelField("Keypad Script", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.ObjectField(MonoScript.FromMonoBehaviour((KeypadScript)target), typeof(KeypadScript), false);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Keypad Settings", "Set Access Code, and number of attempts");
            if (safe.NumberOfTry == 0) TinyGUI.InfoBox("If the `Number Of Try` is 0, then there is an unlimited number of tries");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("NumberOfTry"), new GUIContent("Number Of Try"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Code"), new GUIContent("Code"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("UI Elements", "Elements belonging to Keypad UI");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("KeypadText"), new GUIContent("Keypad Text"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("LeftTryText"), new GUIContent("Left Try Text"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("KeyPadUI"), new GUIContent("Keypad UI"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Sounds", "Sounds made during interaction");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("clickSound"), new GUIContent("Click Sound"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("clearSound"), new GUIContent("Clear Sound"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SuccessSound"), new GUIContent("Success Sound"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("FailSound"), new GUIContent("Fail Sound"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Unity Events", "Add Action on OnUnlock or ToManyTrials");
            EditorGUILayout.EndVertical();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnUnlock"), new GUIContent("On Unlock"));

            if (safe.NumberOfTry == 0) { TinyGUI.InfoBox("If the `Number Of Try` is 0, ToManyTrials() will never execute"); GUI.enabled = false; }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnToManyTrials"), new GUIContent("To Many Trials"));
            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}