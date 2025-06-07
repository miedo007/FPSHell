using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TinyGUIPreviews : MonoBehaviour
{
    public string Test;
    public bool fold1, fold2, fold3;
}

#if UNITY_EDITOR

[CustomEditor(typeof(TinyGUIPreviews))]
[CanEditMultipleObjects]
public class TinyGUIPreviewsEditor : Editor
{
    public int TabID = 0;
    public override void OnInspectorGUI()
    {
        EditorUtility.SetDirty(target);
        EditorGUI.indentLevel = 0;

        // Show Script
        TinyGUIPreviews interaction = (TinyGUIPreviews)target;

        /// Draw Script
        TinyGUI.DrawScript<TinyGUIPreviews>("Test Tiny Editor", target);

        /// Title
        TinyGUI.Title("Test");
        TinyGUI.Title("Test", "Desc");


        TabID = TinyGUI.TabList(TabID, "Test", "Test 2", "Test 3", "Very Long Name");
        if(TabID == 0) {  EditorGUILayout.BeginHorizontal("helpbox"); EditorGUILayout.LabelField($"Tab ID: {TabID} Opened"); EditorGUILayout.EndHorizontal(); }
        if(TabID == 1) {  EditorGUILayout.BeginHorizontal("helpbox"); EditorGUILayout.LabelField($"Tab ID: {TabID} Opened"); EditorGUILayout.EndHorizontal(); }
        if(TabID == 2) {  EditorGUILayout.BeginHorizontal("helpbox"); EditorGUILayout.LabelField($"Tab ID: {TabID} Opened"); EditorGUILayout.EndHorizontal(); }
        if(TabID == 3) {  EditorGUILayout.BeginHorizontal("helpbox"); EditorGUILayout.LabelField($"Tab ID: {TabID} Opened"); EditorGUILayout.EndHorizontal(); }

        TinyGUI.Title("Test");
        TinyGUI.Title("Test", "Desc");

        TinyGUI.InfoBox("1", "CollabExclude Icon");
        TinyGUI.InfoBox("1\n2", "CollabExclude Icon");
        TinyGUI.InfoBox("1\n2\n3", "CollabExclude Icon");
        TinyGUI.InfoBox("1\n2\n3\n4", "CollabExclude Icon");
        TinyGUI.InfoBox("1\n2\n3\n4\n5", "CollabExclude Icon");

        EditorGUILayout.Space(10);

        TinyGUI.IconButton("InputField Icon", "Test Button");
        TinyGUI.IconButton("InputField Icon", "Test Button with Tooltip", "Test Tooltip");

        EditorGUILayout.BeginVertical("box");
        TinyGUI.EditorTitle("Test");
        TinyGUI.EditorTitle("Test", "Desc");
        EditorGUILayout.EndVertical();

        TinyGUI.BeginBoxGroup("Test Group", "Test Desc");
        TinyGUI.InfoBox("1\n2\n3", "CollabExclude Icon");
        TinyGUI.EndBoxGroup();

        interaction.fold1 = TinyGUI.FoldoutGroup(" Foldout Tab with Icon", "UnityLogo", interaction.fold1);
        if (interaction.fold1)
        {
            EditorGUILayout.BeginVertical("helpbox");
            TinyGUI.Title("Foldout Tab with Icon", "Demo Content");
            EditorGUILayout.EndVertical();
        }
        interaction.fold2 = TinyGUI.FoldoutGroup(" Foldout Tab", interaction.fold2);
        if (interaction.fold2)
        {
            EditorGUILayout.BeginVertical("helpbox");
            TinyGUI.Title("Foldout Tab", "Demo Content");
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(10);
        TinyGUI.Guideline("Test Guildeline", "1", "1\n2", "1\n2\n3", "1\n2\n3\n4", "1\n2\n3\n4\n5");

        EditorGUILayout.Space(10);
        TinyGUI.ProgressBar(0.7f, 1, "Normal Bar");
        TinyGUI.ProgressBar(0.7f, 1, "Normal Bar", Color.cyan);
        TinyGUI.ProgressBar(0.7f, 1, "Normal Bar", 50);
        TinyGUI.ProgressBar(0.7f, 1, "Normal Bar", 50, Color.yellow);
    }
}


#endif