using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using TMPro;

namespace HellishBattle.Level
{
    public class MultiSwitchScript : MonoBehaviour
    {
        public int startIndex;
        public TMP_Text DisplayText;
        public bool ActivateOnChange = true;
        public AudioClip SwitchFX;

        public List<string> OptionsName;
        public List<LocalizedString> OptionsLocalizedName;
        public List<UnityEvent> OptionOnEvent;
        public List<UnityEvent> OptionOffEvent;

        [HideInInspector] public int index;
        AudioSource source;

        private void Start()
        {
            index = startIndex;
            ChangeOption();
        }

        public void Next()
        {
            index++;
            if (index == OptionsName.Count) { index = 0; }
            ChangeOption();
        }

        public void Previous()
        {
            index--;
            if (index < 0) { index = OptionsName.Count - 1; }
            ChangeOption();
        }

        public void ChangeOption()
        {
            source = GetComponent<AudioSource>();
            if (source != null && SwitchFX != null) { source.PlayOneShot(SwitchFX); }

            if (OptionsLocalizedName[index].localization == null && OptionsName[index] != "") DisplayText.text = OptionsName[index].Replace("\\n", "\n");
            else if (OptionsName[index] != "") { DisplayText.text = OptionsLocalizedName[index].GetLocalization(); }
            else { DisplayText.text = $"Option: ID {index}"; }

            if (ActivateOnChange)
            {
                for (int i = 0; i < OptionsName.Count; i++)
                {
                    if (i != index) { OptionOffEvent[i].Invoke(); }
                }

                OptionOnEvent[index].Invoke();
            }
        }

        public void Activate()
        {
            for (int i = 0; i < OptionsName.Count; i++)
            {
                if (i != index) { OptionOffEvent[i].Invoke(); }
            }

            OptionOnEvent[index].Invoke();
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(MultiSwitchScript)), CanEditMultipleObjects]
    public class MultiSwitchEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            MultiSwitchScript mSwitch = (MultiSwitchScript)target;
            EditorGUI.indentLevel = 0;

            EditorGUILayout.BeginHorizontal("helpbox", GUILayout.Height(EditorGUIUtility.singleLineHeight + 6));
            EditorGUILayout.LabelField("In-Game Multi-Switch Script", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.ObjectField(MonoScript.FromMonoBehaviour((MultiSwitchScript)target), typeof(MultiSwitchScript), false);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Base Settings", "Change Base Multi-Switch Settings");

            if (mSwitch.OptionsName == null) { mSwitch.OptionsName = new List<string>(); }
            if (mSwitch.OptionsName.Count > 1) mSwitch.startIndex = EditorGUILayout.IntSlider("Start Index", mSwitch.startIndex, 0, mSwitch.OptionsName.Count - 1);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DisplayText"), new GUIContent("Display Text"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SwitchFX"), new GUIContent("Switch Sound"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ActivateOnChange"), new GUIContent("Activate On Change"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle($"Options [{mSwitch.OptionsName.Count}]", "Desc");

            //EditorGUILayout.BeginVertical("HelpBox");
            for (int i = 0; i < mSwitch.OptionsName.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");

                int index = i;

                string title = "";
                string desc = "";

                // LOcalized and Normal Empty
                if (mSwitch.OptionsName[i] == "" && mSwitch.OptionsLocalizedName[i].localization == null)
                {
                    title = $"Option Nr. {index + 1}";
                    desc = $"Empty Option";
                }
                // Selected Localization
                else if (mSwitch.OptionsName[i] == "")
                {
                    title = $"Option Nr. {index + 1} (Localized String)";
                    desc = $"English Text: {mSwitch.OptionsLocalizedName[i].localization.GetValueDemo(mSwitch.OptionsLocalizedName[i].key, (int)SystemLanguage.English)}";
                }
                // Selected Normal Text
                else
                {
                    title = $"Option Nr. {index + 1} (String)";
                    desc = $"Text: {mSwitch.OptionsName[i]}";
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space(2);
                TinyGUI.EditorTitle(title, desc);
                EditorGUILayout.EndVertical();
                if (TinyGUI.IconButton("d_P4_DeletedLocal", "", $"Delete Option Nr. {index + 1}", GUILayout.Width(40), GUILayout.Height(40)))
                {
                    mSwitch.OptionsName.RemoveAt(index);
                    mSwitch.OptionsLocalizedName.RemoveAt(index);
                    mSwitch.OptionOnEvent.RemoveAt(index);
                    mSwitch.OptionOffEvent.RemoveAt(index);

                    serializedObject.ApplyModifiedProperties();
                    break;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical();
                if (mSwitch.OptionsName[i] == "") { TinyGUI.LocalizedString(mSwitch.OptionsLocalizedName[i], serializedObject.FindProperty("OptionsLocalizedName").GetArrayElementAtIndex(index)); }
                if (mSwitch.OptionsLocalizedName[i].localization == null) { EditorGUILayout.PropertyField(serializedObject.FindProperty("OptionsName").GetArrayElementAtIndex(index), new GUIContent("Option Name")); }
                EditorGUILayout.EndVertical();

                //EditorGUILayout.BeginVertical("box");
                EditorGUILayout.Space(2);
                TinyGUI.EditorTitle("Unity Events", "Add Action on OptionOn or OptionOff");
                //EditorGUILayout.EndVertical();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("OptionOnEvent").GetArrayElementAtIndex(index), new GUIContent("Option ON"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OptionOffEvent").GetArrayElementAtIndex(index), new GUIContent("Option OFF"));


                EditorGUILayout.EndVertical();
            }
            //EditorGUILayout.EndVertical();

            if (TinyGUI.IconButton("Toolbar Plus", $"Add New Option"))
            {
                //EditorGUILayout.BeginVertical("helpbox");
                mSwitch.OptionsName.Add("");
                mSwitch.OptionsLocalizedName.Add(new LocalizedString());
                mSwitch.OptionOnEvent.Add(new UnityEvent());
                mSwitch.OptionOffEvent.Add(new UnityEvent());

                serializedObject.ApplyModifiedProperties();
                //EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}
