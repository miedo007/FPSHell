using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Sprite", menuName = "Localization/Sprite")]
public class SpriteLocalization : ScriptableObject
{
    public LocalizationManager _manager;
    public List<SpriteLocalizationList> wordList = new List<SpriteLocalizationList>();
    public List<string> keyOption;

    public Sprite GetSprite(string _key)
    {
        for (int i = 0; i < wordList.Count; i++)
        {
            if (_key == wordList[i].key) { return _manager.lang.IndexOf(_manager.selectedLang) >= 0 ? wordList[i].spriteList[_manager.lang.IndexOf(_manager.selectedLang)] : wordList[i].englishText; }
        }
        return null;
    }

    public Sprite GetValueDemo(string _key, int id)
    {
        for (int i = 0; i < wordList.Count; i++)
        {
            if (_key == wordList[i].key) { return _manager.lang.IndexOf((SystemLanguage)id) >= 0 ? wordList[i].spriteList[_manager.lang.IndexOf((SystemLanguage)id)] : wordList[i].englishText; }
        }

        return null;
    }

    private void OnValidate()
    {
        keyOption = new List<string>();
        for (int i = 0; i < wordList.Count; i++)
        {
            keyOption.Add(wordList[i].key);
        }
    }
}

[System.Serializable]
public class LocalizedSprite
{
    public SpriteLocalization localization;
    public string key;
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(LocalizedSprite))]
public class LocalizedSpriteDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        LocalizationManager _lm = (LocalizationManager)AssetDatabase.LoadAssetAtPath("Assets/Classic FPS/Localization/Languages.asset", typeof(LocalizationManager));
        LocalizedSprite _ls = fieldInfo.GetValue(property.serializedObject.targetObject) as LocalizedSprite;

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        if (_ls.localization != null)
        {
            EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, 60 + (25 * (_lm.lang.Count + 2))), "", MessageType.None);
            EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, 18), $" {label.text} - Sprite Localization", MessageType.None);
            EditorGUI.PropertyField(new Rect(position.x + 5, position.y + 20, position.width - 10, 18), property.FindPropertyRelative("localization"));

            EditorGUI.PropertyField(new Rect(position.x + 5, position.y + 40, position.width - 10, 18), property.FindPropertyRelative("key"));

            // Preview
            EditorGUI.HelpBox(new Rect(position.x, position.y + 60, position.width, (25 * (_lm.lang.Count + 2))), "", MessageType.None);

            EditorGUI.HelpBox(new Rect(position.x, position.y + 60, position.width, 20), " Available Localization", MessageType.None);


            GUIStyle style = new GUIStyle();
            style.richText = true;

            EditorGUI.LabelField(new Rect(position.x + 5, position.y + 85, EditorGUIUtility.labelWidth, 20), $"English: ");
            if (_ls.localization.GetValueDemo(_ls.key, (int)SystemLanguage.English) == null) { EditorGUI.ProgressBar(new Rect(position.x + 5 + EditorGUIUtility.labelWidth, position.y + 85, position.width - 10 - EditorGUIUtility.labelWidth, 20), 1, $"UNAVAILABLE KEY"); }
            else EditorGUI.ProgressBar(new Rect(position.x + 5 + EditorGUIUtility.labelWidth, position.y + 85, position.width - 10 - EditorGUIUtility.labelWidth, 20), 1, $"{_ls.localization.GetValueDemo(_ls.key, (int)SystemLanguage.English).name}");

            for (int i = 0; i < _lm.lang.Count; i++)
            {
                _ls = fieldInfo.GetValue(property.serializedObject.targetObject) as LocalizedSprite;

                EditorGUI.LabelField(new Rect(position.x + 5, position.y + 110 + (25 * i), EditorGUIUtility.labelWidth, 20), $"{_lm.lang[i]}");
                if (_ls.localization.GetValueDemo(_ls.key, (int)_lm.lang[i]) == null) { EditorGUI.ProgressBar(new Rect(position.x + 5 + EditorGUIUtility.labelWidth, position.y + 110 + (25 * i), position.width - 10 - EditorGUIUtility.labelWidth, 20), 1, $"UNAVAILABLE KEY"); }
                else EditorGUI.ProgressBar(new Rect(position.x + 5 + EditorGUIUtility.labelWidth, position.y + 110 + (25 * i), position.width - 10 - EditorGUIUtility.labelWidth, 20), 1, $"{_ls.localization.GetValueDemo(_ls.key, (int)_lm.lang[i]).name}");
            }
        }
        else
        {
            EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, 40), "", MessageType.None);
            EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, 18), $" {label.text} - Sprite Localization", MessageType.None);
            EditorGUI.PropertyField(new Rect(position.x + 5, position.y + 20, position.width - 10, 18), property.FindPropertyRelative("localization"));
        }

        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        LocalizationManager _lm = (LocalizationManager)AssetDatabase.LoadAssetAtPath("Assets/Classic FPS/Localization/Languages.asset", typeof(LocalizationManager));
        LocalizedSprite _ls = fieldInfo.GetValue(property.serializedObject.targetObject) as LocalizedSprite;

        if (_ls.localization == null) { return 40; }
        return 60 + (25 * (_lm.lang.Count + 2));
    }
}

#endif
