
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System;

namespace HellishBattle.SaveSystem
{
    public class TinySaveEditor : EditorWindow
    {
        static string fileName = "HellishBattle.tss";
        Vector2 scrollPosition;

        [MenuItem("Tiny Slime Studio/Save Editor")]

        public static void ShowWindow()
        {
            TinySaveEditor window = (TinySaveEditor)EditorWindow.GetWindow(typeof(TinySaveEditor));

            // Load the icon texture from the Assets folder
            Texture2D icon = EditorGUIUtility.IconContent("AnimatorStateTransition Icon").image as Texture2D;
            if (icon != null)
            {
                // Set the window's icon
                GUIContent titleContent = new GUIContent("Save Editor", icon);
                window.titleContent = titleContent;
            }

            window.Show();
        }

        void OnGUI()
        {
            TinySaveSystem.Initialize(fileName);

            /*EditorGUILayout.BeginHorizontal();

            // Info Box
            EditorGUILayout.BeginVertical("HelpBox", GUILayout.Width(150));
            EditorGUILayout.LabelField("Save Editor", EditorStyles.boldLabel);
            TinyGUI.Guideline("Jak u¿ywaæ Save Editor", "Ka¿de rêczne edytowanie pliku zapisu mo¿e spowodowaæ uszkodzenie pliku i nieoczekiwane zmiany");
            EditorGUILayout.EndVertical();

            // Save List
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("Save Editor");
            EditorGUILayout.EndVertical();


            EditorGUILayout.LabelField("Save Editor");
            EditorGUILayout.LabelField("Save Editor");
            EditorGUILayout.LabelField("Save Editor");
            EditorGUILayout.LabelField("Save Editor");

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();*/

            /*EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginVertical("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField("Loaded Record: " + TinySaveSystem.data.items.Count.ToString(), EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginHorizontal();
            fileName = EditorGUILayout.TextField("Save File Name", fileName);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();*/

            TinyGUI.InfoBox("Any manual editing of the save file may result in file corruption and unexpected changes", "console.warnicon");

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            /*for (int i = 0; i < TinySaveSystem.data.items.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));

                EditorGUILayout.LabelField(TinySaveSystem.data.items[i].Key, EditorStyles.boldLabel);
                if (GUILayout.Button("Delete", EditorStyles.toolbarButton, GUILayout.Width(75)))
                {
                    int index = i;
                    TinySaveSystem.data.items.Remove(TinySaveSystem.data.items[index]);
                    TinySaveSystem.SaveToDisk();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label("Value:", new GUIStyle("DD HeaderStyle"));
                GUILayout.Space(5);

                if (!DeCrypt(TinySaveSystem.data.items[i].Value).Contains("|"))
                {
                    GUILayout.Label(DeCrypt(TinySaveSystem.data.items[i].Value), new GUIStyle("DD HeaderStyle"));
                }
                else
                {
                    string[] parts = DeCrypt(TinySaveSystem.data.items[i].Value).Split('|');
                    for (int j = 0; j < parts.Length; j++) { GUILayout.Label(parts[j], new GUIStyle("DD HeaderStyle")); GUILayout.Space(5); };
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }*/

            for (int i = 0; i < TinySaveSystem.data.items.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                List<string> _tmpList = new List<string>();
                string[] values = new string[0];

                if (!DeCrypt(TinySaveSystem.data.items[i].Value).Contains("|"))
                {
                    _tmpList.Add(DeCrypt(TinySaveSystem.data.items[i].Value));
                }
                else
                {
                    string[] parts = DeCrypt(TinySaveSystem.data.items[i].Value).Split('|');
                    for (int j = 0; j < parts.Length; j++) { if (parts[j] != "") { _tmpList.Add(parts[j]); } else { _tmpList.Add("Empty Field"); } };
                }
                values = _tmpList.ToArray();

                Rect _tmpRect = TinyGUI.Guideline(TinySaveSystem.data.items[i].Key, values);

                if (TinyGUI.IconButton("P4_DeletedLocal", "", GUILayout.Height(50), GUILayout.Width(50)))
                {
                    bool userConfirmed = EditorUtility.DisplayDialog("Are you sure you want to delete this variable?", $"Confirm if you NEED to delete the variable: \n{TinySaveSystem.data.items[i].Key}", "OK", "Cancel");

                    if (userConfirmed)
                    {
                        int index = i;
                        TinySaveSystem.data.items.Remove(TinySaveSystem.data.items[index]);
                        TinySaveSystem.SaveToDisk();
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

        }

        static string DeCrypt(string text)
        {
            string result = string.Empty;
            foreach (char j in text) result += (char)((int)j! ^ 42);
            return result;
        }
    }
}

#endif