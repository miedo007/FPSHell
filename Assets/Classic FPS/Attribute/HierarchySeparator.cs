using UnityEngine;
using UnityEditor;

public class HierarchySeparator : MonoBehaviour
{
    public string title = "Header";
    [Range(16, 48)] public int MaxCharacter = 24;
    public char CharacterBefore = '<';
    public char CharacterAfter = '>';

    public SeparatorPrefixType type;

    public static GameObject AudioPrefab;
    public GameObject LevelPrefab;
    public GameObject PlayerPrefab;
    public GameObject EventPrefab;
    public GameObject LoadingPrefab;

#if UNITY_EDITOR

    private void OnValidate()
    {
        this.name = Title();
        transform.position = Vector3.zero;
        transform.gameObject.SetActive(false);
    }

    public string Title()
    {
        switch (type)
        {
            case SeparatorPrefixType.Comment:
                // Prefix
                string start_line = ""; string end_line = "";
                for (int i = 0; i < Mathf.RoundToInt((MaxCharacter - title.Length - 2) / 2); i++) { start_line += " "; end_line += " "; }
                // Title
                return $"/* {start_line}" + title.ToUpperInvariant() + $"{end_line} */";

            case SeparatorPrefixType.Equal:
                // Prefix
                string start_Equal = ""; string end_Equal = "";
                for (int i = 0; i < Mathf.RoundToInt((MaxCharacter - title.Length - 2) / 2); i++) { start_Equal += "="; end_Equal += "="; }
                // Title
                return $"{start_Equal} " + title.ToUpperInvariant() + $" {end_Equal}";

            case SeparatorPrefixType.Custom:
                // Prefix
                string start_custom = ""; string end_custom = "";
                for (int i = 0; i < Mathf.RoundToInt((MaxCharacter - title.Length - 2) / 2); i++) { start_custom += CharacterBefore; end_custom += CharacterAfter; }
                // Title
                return $"{start_custom} " + title.ToUpperInvariant() + $" {end_custom}";

            default:
                // Prefix
                string start = ""; string end = "";
                for (int i = 0; i < Mathf.RoundToInt((MaxCharacter - title.Length - 2) / 2); i++) { start += "━"; end += "━"; }
                // Title
                return $"{start} {title.ToUpperInvariant()} {end}";
        }
    }

    [MenuItem("GameObject/Add Separator", false, 0)]
    private static void CreateHeader()
    {
        var header = new GameObject();
        header.tag = "EditorOnly";
        header.AddComponent<HierarchySeparator>();
        header.transform.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector;
        Undo.RegisterCreatedObjectUndo(header, "Add Hierarchy Separator");
        Selection.activeGameObject = header;
    }

#endif
}

/// Custom Editor
#if UNITY_EDITOR

[CustomEditor(typeof(HierarchySeparator))]
public class HierarchySeparatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorUtility.SetDirty(target);
        HierarchySeparator tiles = (HierarchySeparator)target;

        TinyGUI.DrawScript<HierarchySeparator>("Hierarchy Separotor Script", target);

        TinyGUI.BeginBoxGroup("Separator Settings", "Dodać Coś");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("type"), new GUIContent("Separator Style"));
        if (tiles.type != SeparatorPrefixType.Custom)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("title"), new GUIContent("Separator Title"));
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Separator Title");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterBefore"), GUIContent.none, GUILayout.Width(30));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("title"), GUIContent.none);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CharacterAfter"), GUIContent.none, GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxCharacter"), new GUIContent("Characters In Separator"));
        TinyGUI.EndBoxGroup();

        tiles.name = tiles.Title();

        serializedObject.ApplyModifiedProperties();
    }
}

#endif


public enum SeparatorPrefixType
{
    Line, Comment, Equal, Custom
}