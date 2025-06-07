using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System;
using UnityEditor.PackageManager;
using System.Linq;
using System.IO;

public class RequirementAssetEditor : EditorWindow
{
    [MenuItem("Tiny Slime Studio/Hellish Battle Requirement Asset")]
    public static void ShowWindow()
    {
        RequirementAssetEditor window = (RequirementAssetEditor)EditorWindow.GetWindow(typeof(RequirementAssetEditor));

        // Load the icon texture from the Assets folder
        Texture2D icon = EditorGUIUtility.IconContent("PrefabModel Icon").image as Texture2D;
        if (icon != null)
        {
            // Set the window's icon
            GUIContent titleContent = new GUIContent("Asset Requirement", icon);
            window.titleContent = titleContent;
        }

        window.position = new Rect(710, 290, 500, 510);
        window.minSize = new Vector2(300, 100);

        window.Show();
    }

    Vector2 scrollPos;

    public void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.LabelField("Requirement Assets", EditorStyles.boldLabel);
        DrawAsset("Input System", "Release", "1.4.4", "com.unity.inputsystem");
        DrawAsset("Post Processing", "Release", "3.2.2", "com.unity.postprocessing");
        DrawAsset("TextMeshPro", "Release", "3.0.6", "com.unity.textmeshpro");
        DrawAsset("ProBuilder", "Release", "5.0.3", "com.unity.probuilder");
        DrawAsset("ProGrids", "Experience", "3.0.1-preview", "com.unity.progrids");
        EditorGUILayout.LabelField("Intergrated Assets", EditorStyles.boldLabel);
        DrawIntergratedAsset("Loot Table", "2.0.0a");

        EditorGUILayout.BeginVertical("box");

        // Toolbar
        EditorGUILayout.BeginVertical("Toolbar", GUILayout.Height(30));
        EditorGUILayout.LabelField("Meet and Talk - Dialogue System", EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();

        EditorGUILayout.LabelField($"Package Type: Paid / Free");
        EditorGUILayout.LabelField($"Requirement Version: 1.6.0a or higher");
#if MEET_AND_TALK
        GUIStyle myStyle = new GUIStyle();
        myStyle.normal.textColor = Color.green;
        EditorGUILayout.LabelField("Installed", myStyle);
#endif
#if !MEET_AND_TALK
        GUIStyle myStyle = new GUIStyle();
        myStyle.normal.textColor = Color.red;
        EditorGUILayout.LabelField("Install from Assets Store", myStyle);
#endif
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();
    }

    public void DrawAsset(string Name, string Type, string Version, string manifestName)
    {
        EditorGUILayout.BeginVertical("box"); 

        // Toolbar
        EditorGUILayout.BeginVertical("Toolbar", GUILayout.Height(30));
        EditorGUILayout.LabelField(Name, EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.LabelField($"Package Cost: {Type}");
        EditorGUILayout.LabelField($"Requirement Version: {Version} or higher");
        string jsonText = File.ReadAllText("Packages/manifest.json");

        if (!jsonText.Contains(manifestName))
        {
            GUIStyle myStyle = new GUIStyle();
            myStyle.normal.textColor = Color.red;
            EditorGUILayout.LabelField("Not Installed", myStyle);
            if (GUILayout.Button("Install From Package Manager"))
            {
                UnityEditor.PackageManager.UI.Window.Open(manifestName);
            }
        }
        else
        {
            GUIStyle myStyle = new GUIStyle();
            myStyle.normal.textColor = Color.green;
            EditorGUILayout.LabelField("Installed", myStyle);
        }

        EditorGUILayout.EndVertical();
    }

    public void DrawIntergratedAsset(string Name, string Version)
    {
        EditorGUILayout.BeginVertical("box");

        // Toolbar
        EditorGUILayout.BeginVertical("Toolbar", GUILayout.Height(30));
        EditorGUILayout.LabelField(Name, EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();

        EditorGUILayout.LabelField($"Asset Version: {Version}");

            GUIStyle myStyle = new GUIStyle();
            myStyle.normal.textColor = Color.green;
            EditorGUILayout.LabelField("Intergrated", myStyle);
        

        EditorGUILayout.EndVertical();
    }

    public void DrawCustomAsset(string Name, string Type, string Version, string manifestName)
    {
        EditorGUILayout.BeginVertical("box");

        // Toolbar
        EditorGUILayout.BeginVertical("Toolbar", GUILayout.Height(30));
        EditorGUILayout.LabelField(Name, EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();

        EditorGUILayout.LabelField($"Package Type: {Type}");
        EditorGUILayout.LabelField($"Requirement Version: {Version} or higher");
        string jsonText = File.ReadAllText("Packages/manifest.json");

        if (!jsonText.Contains(manifestName))
        {
            GUIStyle myStyle = new GUIStyle();
            myStyle.normal.textColor = Color.red;
            EditorGUILayout.LabelField("Not Installed", myStyle);
            if (GUILayout.Button("Install From Package Manager"))
            {
                //UnityEditor.PackageManager.UI.Window.Open(manifestName);
                Application.OpenURL("http://unity3d.com/");
            }
        }
        else
        {
            GUIStyle myStyle = new GUIStyle();
            myStyle.normal.textColor = Color.green;
            EditorGUILayout.LabelField("Installed", myStyle);
        }

        EditorGUILayout.EndVertical();
    }
}
