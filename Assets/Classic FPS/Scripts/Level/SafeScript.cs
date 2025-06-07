using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using TMPro;
using HellishBattle.UI;

namespace HellishBattle.Level
{
    [RequireComponent(typeof(AudioSource))]
    public class SafeScript : MonoBehaviour
    {
        public SafeValueCount safeValueCount;
        public SafeValueSize safeValueSize;
        public int Value1, Value2, Value3, Value4;
        public HorizontalSelector Value1Text, Value2Text, Value3Text, Value4Text;

        public AudioClip ChangeClick, ChangeGoodClick, UnlockSound, FailSound;

        public UnityEvent OnSuccess, OnFailure;
        AudioSource source;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
        }

        public void Start()
        {
            int ValueSize = 10;
            if (safeValueSize == SafeValueSize.size0To99) ValueSize = 100;

            // Clear
            Value1Text.KeyList.Clear();
            Value2Text.KeyList.Clear();
            Value3Text.KeyList.Clear();
            Value4Text.KeyList.Clear();

            for (int i = 0; i < ValueSize; i++) { Value1Text.KeyList.Add(i.ToString()); }
            for (int i = 0; i < ValueSize; i++) { Value2Text.KeyList.Add(i.ToString()); }
            for (int i = 0; i < ValueSize; i++) { Value3Text.KeyList.Add(i.ToString()); }

            if (safeValueCount == SafeValueCount.Three)
            {
                Value4Text.transform.gameObject.SetActive(false);
            }
            else
            {
                for (int i = 0; i < ValueSize; i++) { Value4Text.KeyList.Add(i.ToString()); }
            }
        }

        public void Check()
        {
            bool good = false;
            if (Value1Text.KeyList[Value1Text.index] == Value1.ToString())
            {
                if (Value2Text.KeyList[Value2Text.index] == Value2.ToString())
                {
                    if (Value3Text.KeyList[Value3Text.index] == Value3.ToString())
                    {
                        if (safeValueCount == SafeValueCount.Four && Value4Text.KeyList[Value4Text.index] == Value4.ToString())
                        {
                            good = true;
                        }
                        else if (safeValueCount != SafeValueCount.Four)
                        {
                            good = true;
                        }
                    }
                }
            }

            if (good)
            {
                OnSuccess.Invoke();
                source.PlayOneShot(UnlockSound);
            }
            else
            {
                OnFailure.Invoke();
                source.PlayOneShot(FailSound);
            }
        }

        public void PlaySound(int slot)
        {
            if (slot == 1) { if (Value1Text.KeyList[Value1Text.index] == Value1.ToString()) { source.PlayOneShot(ChangeGoodClick); } else { source.PlayOneShot(ChangeClick); } }
            if (slot == 2) { if (Value2Text.KeyList[Value2Text.index] == Value2.ToString()) { source.PlayOneShot(ChangeGoodClick); } else { source.PlayOneShot(ChangeClick); } }
            if (slot == 3) { if (Value3Text.KeyList[Value3Text.index] == Value3.ToString()) { source.PlayOneShot(ChangeGoodClick); } else { source.PlayOneShot(ChangeClick); } }
            if (slot == 4) { if (Value4Text.KeyList[Value4Text.index] == Value4.ToString()) { source.PlayOneShot(ChangeGoodClick); } else { source.PlayOneShot(ChangeClick); } }
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

    public enum SafeValueCount
    {
        Three = 0, Four = 1
    }

    public enum SafeValueSize
    {
        size0To9, size0To99
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(SafeScript)), CanEditMultipleObjects]
    public class SafeScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            SafeScript safe = (SafeScript)target;
            EditorGUI.indentLevel = 0;

            EditorGUILayout.BeginHorizontal("helpbox", GUILayout.Height(EditorGUIUtility.singleLineHeight + 6));
            EditorGUILayout.LabelField("Safe Script", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.ObjectField(MonoScript.FromMonoBehaviour((SafeScript)target), typeof(SafeScript), false);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Code Settings", "Change Slot Count and Number Size");
            TinyGUI.Guideline("Code Combinations:", "3 Slots |  0-9 Size  | 1 000 Combinations", "4 Slots |  0-9 Size  | 10 000 Combinations", "3 Slots | 0-99 Size | 1 000 000 Combinations", "4 Slots | 0-99 Size | 100 000 000 Combinations");


            EditorGUILayout.PropertyField(serializedObject.FindProperty("safeValueCount"), new GUIContent("Slot Count"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("safeValueSize"), new GUIContent("Value Size"));

            int maxValue = safe.safeValueSize == SafeValueSize.size0To99 ? 99 : 9;
            // To Many
            if (safe.Value1 > maxValue || safe.Value2 > maxValue || safe.Value3 > maxValue)
            {
                if (safe.safeValueCount == SafeValueCount.Four || safe.Value4 > maxValue)
                {
                    TinyGUI.InfoBox("Warning!\nAt least one half of the Code exceeds the maximum value!", "console.warnicon");
                }
                else
                {
                    TinyGUI.InfoBox("Warning!\nAt least one half of the Code exceeds the maximum value!", "console.warnicon");
                }
            }
            if (safe.Value1 < 0 || safe.Value2 < 0 || safe.Value3 < 0)
            {
                if (safe.safeValueCount == SafeValueCount.Four || safe.Value4 < 0)
                {
                    TinyGUI.InfoBox("Warning!\nAt least one half of the Code has a negative value", "console.warnicon");
                }
                else
                {
                    TinyGUI.InfoBox("Warning!\nAt least one half of the Code has a negative value", "console.warnicon");
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Code", GUILayout.Width(EditorGUIUtility.labelWidth));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Value1"), GUIContent.none);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Value2"), GUIContent.none);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Value3"), GUIContent.none);
            if (safe.safeValueCount == SafeValueCount.Four) EditorGUILayout.PropertyField(serializedObject.FindProperty("Value4"), GUIContent.none);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Horizontal Selector", "Link Horizontal Selector From UI");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Value1Text"), new GUIContent("Slot 1"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Value2Text"), new GUIContent("Slot 2"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Value3Text"), new GUIContent("Slot 3"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Value4Text"), new GUIContent("Slot 4"));
            EditorGUILayout.EndVertical();

            TinyGUI.BeginBoxGroup("Sound Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ChangeClick"), new GUIContent("Change Sound"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ChangeGoodClick"), new GUIContent("Good Change Sound"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("UnlockSound"), new GUIContent("Unlock Sound"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("FailSound"), new GUIContent("Fail Sound"));
            TinyGUI.EndBoxGroup();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Unity Events", "Add Action on Unlock or Failure");
            EditorGUILayout.EndVertical();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnSuccess"), new GUIContent("On Success"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnFailure"), new GUIContent("On Failure"));

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}