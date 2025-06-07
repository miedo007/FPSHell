using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;

public class TMPTextStringLocalization : MonoBehaviour
{
    public LocalizedString ShowText;

    public TMP_Text text_tmp;

    private void Update() { if (ShowText.localization != null && ShowText.key != "") text_tmp.text = ShowText.localization.GetString(ShowText.key); }
    private void OnEnable() { if (ShowText.localization != null && ShowText.key != "") text_tmp.text = ShowText.localization.GetString(ShowText.key); }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TMPTextStringLocalization))]
public class TMPTextStringLocalizationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorUtility.SetDirty(target);
        TMPTextStringLocalization manager = (TMPTextStringLocalization)target;
        TinyGUI.DrawScript<TMPTextStringLocalization>("TMP Text Spring Localization", target);

        TinyGUI.LocalizedString(manager.ShowText, serializedObject.FindProperty("ShowText"));

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("text_tmp"), new GUIContent("Text Field"));
        EditorGUILayout.EndHorizontal();
    }
}
#endif
