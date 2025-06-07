using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;

[CustomEditor(typeof(StringLocalization))]
public class StringLocalizationEditor : Editor
{
    SerializedProperty wordlist;
    ReorderableList list;
    StringLocalization ld;

    /*private void OnEnable()
    {
        wordlist = serializedObject.FindProperty("wordList");
        list = new ReorderableList(serializedObject, wordlist, true, true, true, true);

        list.drawElementCallback = DrawListItems;
        list.drawHeaderCallback = DrawHeader;
        ld = target as StringLocalization;
    }

    void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);

        list.elementHeight = (EditorGUIUtility.singleLineHeight + 2) * (2 + ld._manager.lang.Count) + 5;

        EditorGUI.LabelField(new Rect(rect.x, rect.y, 250, EditorGUIUtility.singleLineHeight), "Key");
        EditorGUI.PropertyField(new Rect(rect.x + 250, rect.y, rect.width - 250, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("key"), GUIContent.none);
        EditorGUI.LabelField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 2, 125, EditorGUIUtility.singleLineHeight), "English (Defualt)");
        EditorGUI.PropertyField(new Rect(rect.x + 125, rect.y + EditorGUIUtility.singleLineHeight + 2, rect.width - 125, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("englishText"), GUIContent.none);

        for (int i = 0; i < ld._manager.lang.Count; i++)
        {
            while (ld.wordList[index].stringList.Count < ld._manager.lang.Count) { ld.wordList[index].stringList.Add(""); }
            EditorGUI.LabelField(new Rect(rect.x, rect.y + ((EditorGUIUtility.singleLineHeight + 2) * (2 + i)), 125, EditorGUIUtility.singleLineHeight), ld._manager.lang[i].ToString());
            EditorGUI.PropertyField(new Rect(rect.x + 125, rect.y + ((EditorGUIUtility.singleLineHeight + 2) * (2 + i)), rect.width - 125, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("stringList").GetArrayElementAtIndex(i), GUIContent.none);
        }


    }
    void DrawHeader(Rect rect)
    {
        string name = $"Translation List";
        EditorGUI.LabelField(rect, name);
    }

    bool foldout = true;*/

    public override void OnInspectorGUI()
    {
        EditorUtility.SetDirty(target);
        StringLocalization localization = (StringLocalization)target;

        serializedObject.Update();

        LocalizationManager _lm = (LocalizationManager)AssetDatabase.LoadAssetAtPath("Assets/Classic FPS/Localization/Languages.asset", typeof(LocalizationManager));

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        TinyGUI.Title($"{localization.name} - String Table Localization", $"Assigned {localization.wordList.Count} translations to {localization.name}");
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space(5);
        if (TinyGUI.IconButton("AlphabeticalSorting", " Sort", GUILayout.Height(30)))
        {
            localization.wordList = localization.wordList.OrderBy(ch => ch.key).ToList();
            serializedObject.ApplyModifiedProperties();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginVertical("HelpBox");
        int count = localization.wordList.Count;
        EditorGUILayout.BeginVertical("HelpBox");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.EndHorizontal();

        // List
        for (int i = 0; i < count; i++)
        {
            int index = i;
            EditorGUILayout.BeginHorizontal();

            // Key Node
            Rect tex = EditorGUILayout.BeginVertical();
            //if(localization.wordList[i].key == "") TinyGUI.InfoBox("\"Key Text\" is empty, \nwithout it you will not be able to refer to the translation!", "console.erroricon");
            localization.wordList[i].key = EditorGUILayout.TextField("Key Text", localization.wordList[i].key);

            // English
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("English"), GUILayout.Width(75));
            float textAreaHeight = EditorStyles.textArea.CalcHeight(new GUIContent(localization.wordList[i].englishText), EditorGUIUtility.currentViewWidth - 158);
            localization.wordList[i].englishText = EditorGUILayout.TextArea(localization.wordList[i].englishText, EditorStyles.textArea, GUILayout.Height(textAreaHeight));
            EditorGUILayout.EndHorizontal();

            // Additional Language
            for(int w = 0; w < _lm.lang.Count; w++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent($"{_lm.lang[w]}"), GUILayout.Width(75));
                float textAreaHeight2 = EditorStyles.textArea.CalcHeight(new GUIContent(localization.wordList[i].stringList[w]), EditorGUIUtility.currentViewWidth - 158);
                localization.wordList[i].stringList[w] = EditorGUILayout.TextArea(localization.wordList[i].stringList[w], EditorStyles.textArea, GUILayout.Height(textAreaHeight2));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            // This Button
            if (TinyGUI.IconButton("d_P4_DeletedLocal", "", GUILayout.Width(25)))
            {
                serializedObject.FindProperty("wordList").DeleteArrayElementAtIndex(index);
                serializedObject.ApplyModifiedProperties();
                break;
            }
            EditorGUILayout.EndHorizontal();

            if(i < count - 1)
            {
                EditorGUILayout.Space(5);
            }
        }
        EditorGUILayout.EndVertical();

        if (TinyGUI.IconButton("Toolbar Plus", $"Add New Key"))
        {
            StringLocalizationList tmp = new StringLocalizationList();
            for (int i = 0; i < _lm.lang.Count; i++) { tmp.stringList.Add(""); }
            localization.wordList.Add(tmp);

            serializedObject.ApplyModifiedProperties();
        }
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
