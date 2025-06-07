using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "String", menuName = "Localization/String")]
public class StringLocalization : ScriptableObject
{
    public LocalizationManager _manager;
    public List<StringLocalizationList> wordList = new List<StringLocalizationList>();
    public List<string> keyOption;

    public string GetString(string _key)
    {
        for (int i = 0; i < wordList.Count; i++)
        {
            if (_key == wordList[i].key) { return _manager.lang.IndexOf(_manager.selectedLang) >= 0 ? wordList[i].stringList[_manager.lang.IndexOf(_manager.selectedLang)] : wordList[i].englishText; }
        }
        return "UNAVAILABLE KEY";
    }

    public string GetValueDemo(string _key, int id)
    {
        for (int i = 0; i < wordList.Count; i++)
        {
            if (_key == wordList[i].key) { return _manager.lang.IndexOf((SystemLanguage)id) >= 0 ? wordList[i].stringList[_manager.lang.IndexOf((SystemLanguage)id)] : wordList[i].englishText; }
        }
        return "UNAVAILABLE KEY";
    }

    private void OnValidate()
    {
        GetKeyOption();
    }

    public void GetKeyOption()
    {

        keyOption = new List<string>();
        for(int i = 0; i<wordList.Count; i++)
        {
            keyOption.Add(wordList[i].key);
        }
    }

    public int GetKeyID(string key)
    {
        for (int i = 0; i < wordList.Count; i++)
        {
            if (key == wordList[i].key) return i;
        }
        Debug.Log("Not Found");
        return wordList.Count - 1;
    }
}

[System.Serializable]
public class LocalizedString
{
    public StringLocalization localization;
    public string key;

    public int _SelectedKey;
    public bool foldout = false;

    public string GetLocalization()
    {
        return localization.GetString(key);
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(LocalizedString))]
public class LocalizedStringDrawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        LocalizationManager _lm = (LocalizationManager)AssetDatabase.LoadAssetAtPath("Assets/Classic FPS/Localization/Languages.asset", typeof(LocalizationManager));
        LocalizedString _ls = fieldInfo.GetValue(property.serializedObject.targetObject) as LocalizedString;

        EditorGUI.BeginProperty(position, label, property);

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        if (_ls.localization != null)
        {
            EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, 60 + (25 * (_lm.lang.Count + 2))), "", MessageType.None);
            EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, 18), $" {label.text} - String Localization", MessageType.None);
            EditorGUI.PropertyField(new Rect(position.x + 5, position.y + 20, position.width - 10, 18), property.FindPropertyRelative("localization"));

            _ls.localization.GetKeyOption();

            for (int i = 0; i < _ls.localization.keyOption.Count; i++)
            {
                if (_ls.key == _ls.localization.keyOption[i])
                {
                    _ls._SelectedKey = i;
                }
            }

            _ls._SelectedKey = EditorGUI.Popup(new Rect(position.x + 5, position.y + 40, position.width - 10, 18), $"Key [{_ls._SelectedKey}]", _ls._SelectedKey, _ls.localization.keyOption.ToArray());
            _ls.key = _ls.localization.wordList[_ls._SelectedKey].key; 
            EditorUtility.SetDirty(property.serializedObject.targetObject);

            
            // Preview
            EditorGUI.HelpBox(new Rect(position.x, position.y + 60, position.width, (25 * (_lm.lang.Count + 2))), "", MessageType.None);

            EditorGUI.HelpBox(new Rect(position.x, position.y + 60, position.width, 20), " Available Localization", MessageType.None);


            GUIStyle style = new GUIStyle();
            style.richText = true;

            EditorGUI.LabelField(new Rect(position.x + 5, position.y + 85, EditorGUIUtility.labelWidth, 20), $"English: ");
            EditorGUI.ProgressBar(new Rect(position.x + 5 + EditorGUIUtility.labelWidth, position.y + 85, position.width - 10 - EditorGUIUtility.labelWidth, 20), 1, $"{_ls.localization.GetValueDemo(_ls.key, (int)SystemLanguage.English)}");

            
            for (int i = 0; i < _lm.lang.Count; i++)
            {
                //_ls = fieldInfo.GetValue(property.serializedObject.targetObject) as LocalizedString;

                EditorGUI.LabelField(new Rect(position.x + 5, position.y + 110 + (25 * i), EditorGUIUtility.labelWidth, 20), $"{_lm.lang[i]}");
                EditorGUI.ProgressBar(new Rect(position.x + 5 + EditorGUIUtility.labelWidth, position.y + 110 + (25 * i), position.width - 10 - EditorGUIUtility.labelWidth, 20), 1, $"{_ls.localization.GetValueDemo(_ls.key, (int)_lm.lang[i])}");
            }
        } else
        {
            EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, 40), "", MessageType.None);
            EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, 18), $" {label.text} - String Localization", MessageType.None);
            EditorGUI.PropertyField(new Rect(position.x + 5, position.y + 20, position.width - 10, 18), property.FindPropertyRelative("localization"));
        }
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        LocalizationManager _lm = (LocalizationManager)AssetDatabase.LoadAssetAtPath("Assets/Classic FPS/Localization/Languages.asset", typeof(LocalizationManager));
        LocalizedString _ls = fieldInfo.GetValue(property.serializedObject.targetObject) as LocalizedString;

        _ls.key += "";

        if(_ls.localization == null) { return 40; }
        return 60 + (25 * (_lm.lang.Count + 2));
    }
}

#endif