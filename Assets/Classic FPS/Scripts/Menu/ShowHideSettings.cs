using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using HellishBattle.SaveSystem;


namespace HellishBattle
{
    public class ShowHideSettings : MonoBehaviour
    {
        public string SettingsName;
        public ShowHideWhen when;
        public int value;

        public List<GameObject> gameObjects;

        void Update()
        {
            bool enable = false;
            if (when == ShowHideWhen.Equal && TinySaveSystem.GetInt(SettingsName) == value) { enable = true; }
            if (when == ShowHideWhen.NoEqual && TinySaveSystem.GetInt(SettingsName) != value) { enable = true; }

            for (int i = 0; i < gameObjects.Count; i++) { gameObjects[i].SetActive(enable); }
        }
    }

    [System.Serializable]
    public enum ShowHideWhen
    {
        Equal = 0, NoEqual = 1, Less = 2, More = 3, LessOrEqual = 4, MoreOrEqual = 5
    }


#if UNITY_EDITOR

    [CustomEditor(typeof(ShowHideSettings))]
    [CanEditMultipleObjects]
    public class ShowHideSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            ShowHideSettings Control = (ShowHideSettings)target;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Show / Hide Elements", EditorStyles.boldLabel);
            Control.when = (ShowHideWhen)EditorGUILayout.EnumPopup(Control.when, EditorStyles.toolbarDropDown, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            int ID = 0;
            TinySaveSystem.Initialize("HellishBattle.tss");
            string[] tmp = new string[TinySaveSystem.data.items.Count];

            for (int i = 0; i < TinySaveSystem.data.items.Count; i++)
            {
                tmp[i] = TinySaveSystem.data.items[i].Key;
                if (TinySaveSystem.data.items[i].Key == Control.SettingsName)
                {
                    ID = i;
                }
            }

            ID = EditorGUILayout.Popup("Settings Key", ID, tmp);
            Control.SettingsName = tmp[ID];
            Control.value = EditorGUILayout.IntField("Value", Control.value);
            EditorGUILayout.HelpBox($"When {Control.SettingsName} is {Control.when} from {Control.value} then Show Elements", MessageType.Info);

            EditorGUILayout.Space(5);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gameObjects"));
            EditorGUI.indentLevel = 0;

            EditorGUILayout.EndVertical();

            //EditorGUILayout.HelpBox($"Base Volume: {Control.GetComponent<AudioSource>().volume * 100}%\nMaster Volume: {TinySaveSystem.GetInt("Settings_Master_Volume")}%, {Control.KeyName} Volume: {TinySaveSystem.GetInt(Control.KeyName)}%\nIn Game Volume: {Control.GetComponent<AudioSource>().volume * ((float)TinySaveSystem.GetInt("Settings_Master_Volume") / 100) * ((float)TinySaveSystem.GetInt(Control.KeyName) / 100) * 100}%", MessageType.None);
        }
    }

#endif
}
