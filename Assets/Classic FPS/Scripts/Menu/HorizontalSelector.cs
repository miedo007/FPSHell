using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using TMPro;

namespace HellishBattle.UI
{
    public class HorizontalSelector : MonoBehaviour
    {
        public bool tryLocalizedText = false;
        public StringLocalization localization;

        public List<string> KeyList;
        public TMP_Text Text;
        public int startIndex;
        public UnityEvent OnChange;
        public UnityEvent<int> OnChangeInt;

        public int index;

        private void Start()
        {
            index = startIndex;
            OnChange.Invoke();
        }

        public void Update()
        {
            if (tryLocalizedText) { Text.text = localization.GetString(KeyList[index]); }
            else { Text.text = KeyList[index]; }
        }

        public void Previous()
        {
            index -= 1;
            if (index < 0) { index = KeyList.Count - 1; }
            OnChange.Invoke();
            OnChangeInt.Invoke(index);
        }

        public void Next()
        {
            index += 1;
            if (index > KeyList.Count - 1) { index = 0; }
            OnChange.Invoke();
            OnChangeInt.Invoke(index);
        }

        public void ChangeLanguage(SettingsManager sm)
        {
            sm.ChangeLanguage(index - 1);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(HorizontalSelector)), CanEditMultipleObjects]
    public class HorizontalSelectorEditor : Editor
    {
        //private string[] _Tabs = { "Static Text", "Localized String" };
        //private int _TabSelected = 1;

        SerializedProperty guaranteedList;
        ReorderableList reorderableGuaranteed;
        HorizontalSelector ld;

        private void OnEnable()
        {
            /* GUARANTEED */
            guaranteedList = serializedObject.FindProperty("KeyList");
            reorderableGuaranteed = new ReorderableList(serializedObject, guaranteedList, true, true, true, true);
            // Functions
            reorderableGuaranteed.drawElementCallback = DrawChangeListItems;
            reorderableGuaranteed.drawHeaderCallback = DrawHeaderGuaranteed;

            ld = target as HorizontalSelector;
        }

        void DrawHeaderGuaranteed(Rect rect) { EditorGUI.LabelField(rect, "Guaranteed Loot Table"); }

        void DrawChangeListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            //reorderableGuaranteed.elementHeight = 42;

            SerializedProperty element = reorderableGuaranteed.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, rect.height - 2), element, GUIContent.none);
        }

        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            HorizontalSelector loot = (HorizontalSelector)target;

            serializedObject.Update();
            Rect r = EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginVertical("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Horizontal Selector - [{loot.KeyList.Count}]", EditorStyles.boldLabel);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tryLocalizedText"), new GUIContent("Localized Text"), GUILayout.Width(225));
            GUI.enabled = loot.tryLocalizedText;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("localization"), GUIContent.none);
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Text"));
            if (loot.startIndex >= loot.KeyList.Count) loot.startIndex = 0;
            if (loot.KeyList.Count > 1) loot.startIndex = EditorGUILayout.IntSlider(new GUIContent($"Start Index - [{loot.KeyList[loot.startIndex]}]"), loot.startIndex, 0, loot.KeyList.Count - 1);
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("startIndex"));

            EditorGUILayout.EndVertical();


            reorderableGuaranteed.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnChange"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnChangeInt"));

            serializedObject.ApplyModifiedProperties();

            //_TabSelected = GUILayout.Toolbar(_TabSelected, _Tabs);
        }
    }
#endif
}
