using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
//using UnityEditor.UIElements;
using UnityEngine.SceneManagement;
using System;
using System.Reflection;

namespace HellishBattle
{
    namespace Campaign
    {
        public class CampaignManager : ScriptableObject
        {
            public const string k_CampaingPath = "Assets/Classic FPS/Scene/Campaing.asset";

            private static CampaignManager _instance;
            public static CampaignManager Instance
            {
                get { return _instance; }
            }

            [SerializeField]
            public StringLocalization localization;
            public bool ShowLockedLevelInStoryMode = false;
            [SerializeField]
            public List<Chapter> storymode = new List<Chapter>();
            [SerializeField]
            public List<Chapter> chapter = new List<Chapter>();
            public List<GamemodeLevel> GameModes = new List<GamemodeLevel>();

            // OLD TO REMOVE
            //[SerializeField]  public List<Chapter> gamemodes = new List<Chapter>();

            public string FindKeyFromSceneName(string sceneName)
            {
                foreach (Chapter chapter in chapter)
                {
                    foreach (Level level in chapter.Levels)
                    {
                        if (level.sceneName == sceneName) { return level.key; }
                    }
                }
                foreach (GamemodeLevel gamemode in GameModes)
                {
                    foreach (Chapter chapter in gamemode.Chapters)
                    {
                        foreach (Level level in chapter.Levels)
                        {
                            if (level.sceneName == sceneName) { return level.key; }
                        }
                    }
                }
                foreach (Chapter storymode in chapter)
                {
                    foreach (Level level in storymode.Levels)
                    {
                        if (level.sceneName == sceneName) { return level.key; }
                    }
                }
                TinyDebug.Error("Invalid Scene Name");
                return "Error";
            }

            public string FindKeyFromScene()
            {
                foreach (Chapter chapters in storymode)
                {
                    foreach (Level level in chapters.Levels)
                    {
                        if (level.sceneName == SceneManager.GetActiveScene().name) { return level.key; }
                    }
                }
                foreach (Chapter chapters in chapter)
                {
                    foreach (Level level in chapters.Levels)
                    {
                        if (level.sceneName == SceneManager.GetActiveScene().name) { return level.key; }
                    }
                }
                foreach (GamemodeLevel gamemode in GameModes)
                {
                    foreach (Chapter chapters in gamemode.Chapters)
                    {
                        foreach (Level level in chapters.Levels)
                        {
                            if (level.sceneName == SceneManager.GetActiveScene().name) { return level.key; }
                        }
                    }
                }
                TinyDebug.Error("Invalid Scene Name");
                return "Error";
            }

            public string CheckNextLevel()
            {
                //string er = "";
                for (int i = 0; i < chapter.Count; i++)
                {
                    for (int j = 0; j < chapter[i].Levels.Count; j++)
                    {
                        if (chapter[i].Levels[j].sceneName == SceneManager.GetActiveScene().name)
                        {
                            if (j + 1 < chapter[i].Levels.Count)
                            {
                                return chapter[i].Levels[j + 1].sceneName;
                            }
                            else
                            {
                                if (chapter.Count != i + 1)
                                {
                                    return chapter[i + 1].Levels[0].sceneName;
                                }
                                return "Main Scene";
                            }
                        }
                    }
                }

                for (int i = 0; i < storymode.Count; i++)
                {
                    for (int j = 0; j < storymode[i].Levels.Count; j++)
                    {
                        if (storymode[i].Levels[j].sceneName == SceneManager.GetActiveScene().name)
                        {
                            if (j + 1 < storymode[i].Levels.Count)
                            {
                                return storymode[i].Levels[j + 1].sceneName;
                            }
                            else
                            {
                                if (storymode.Count != i + 1)
                                {
                                    return storymode[i + 1].Levels[0].sceneName;
                                }
                                return "Main Scene";
                            }
                        }

                    }

                }
                return "Main Scene";
            }
            private static string NameFromIndex(int BuildIndex)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(BuildIndex);
                int slash = path.LastIndexOf('/');
                string name = path.Substring(slash + 1);
                int dot = name.LastIndexOf('.');
                return name.Substring(0, dot);
            }
            public int sceneIndexFromName(string sceneName)
            {
                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    string testedScreen = NameFromIndex(i);
                    if (testedScreen == sceneName)
                        return i;
                }
                return 0;
            }

#if UNITY_EDITOR
            internal static CampaignManager GetOrCreateSettings()
            {
                var settings = AssetDatabase.LoadAssetAtPath<CampaignManager>(k_CampaingPath);
                if (settings == null)
                {
                    settings = ScriptableObject.CreateInstance<CampaignManager>();


                    AssetDatabase.CreateAsset(settings, k_CampaingPath);
                    AssetDatabase.SaveAssets();
                }
                return settings;
            }
            internal static SerializedObject GetSerializedSettings()
            {
                return new SerializedObject(GetOrCreateSettings());
            }
#endif
        }

#if UNITY_EDITOR
        static class CampaignManagerIMGUIRegister
        {

            [SettingsProvider]
            public static SettingsProvider CampaignManagerProvider()
            {
                var provider = new SettingsProvider("Project/Hellish Battle/Campaign", SettingsScope.Project)
                {
                    label = "Campaing",
                    guiHandler = (searchContext) =>
                    {
                        // Load Campaign File
                        var settings = CampaignManager.GetSerializedSettings();
                        CampaignManager CampaingFile = (CampaignManager)AssetDatabase.LoadAssetAtPath("Assets/Classic FPS/Scene/Campaing.asset", typeof(CampaignManager));

                        // Load Scene Names
                        string[] sceneNames = new string[SceneManager.sceneCountInBuildSettings];
                        for (int scene = 0; scene < SceneManager.sceneCountInBuildSettings; scene++)
                        {
                            string scenePath = SceneUtility.GetScenePathByBuildIndex(scene);
                            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                            sceneNames[scene] = sceneName;
                        }

                        // Save Defualt Color
                        Color defualtButtonColor = GUI.backgroundColor;

                        // Localization
                        EditorGUILayout.ObjectField("Localization File", CampaingFile.localization, typeof(StringLocalization), false);
                        EditorGUILayout.PropertyField(settings.FindProperty("ShowLockedLevelInStoryMode"), new GUIContent("Show Locked Level"));
                        EditorGUILayout.Space();

                        if (CampaingFile.localization != null)
                        {
                            DrawChapter(CampaingFile.storymode, settings.FindProperty("storymode"),true,"Story Mode");
                            DrawChapter(CampaingFile.chapter, settings.FindProperty("chapter"),true, "Campaign");

                            for (int mode = 0; mode < CampaingFile.GameModes.Count; mode++)
                            {
                                EditorGUILayout.Space(16);
                                Rect t = EditorGUILayout.GetControlRect(GUILayout.Height(52));
                                EditorGUI.HelpBox(t, "", MessageType.None);
                                EditorGUI.LabelField(new Rect(t.x + 60, t.y + 2, t.width - 8, 30), $"{CampaingFile.localization.GetValueDemo(CampaingFile.GameModes[mode].key, (int)SystemLanguage.English)} [{settings.FindProperty("GameModes").GetArrayElementAtIndex(mode).FindPropertyRelative("Chapters").arraySize}]", new GUIStyle("AM MixerHeader"));


                                CampaingFile.GameModes[mode]._keyID = 0;
                                for (int x = 0; x < CampaingFile.localization.keyOption.Count; x++)
                                {
                                    if (CampaingFile.GameModes[mode].key == CampaingFile.localization.keyOption[x])
                                    {
                                        CampaingFile.GameModes[mode]._keyID = x;
                                        break;
                                    }
                                }
                                int newKeyID = EditorGUI.Popup(new Rect(t.x + 60, t.y + 28, t.width - 12 - 52 - 125, EditorGUIUtility.singleLineHeight), "Gamemode Name Key", CampaingFile.GameModes[mode]._keyID, CampaingFile.localization.keyOption.ToArray());
                                if (newKeyID != CampaingFile.GameModes[mode]._keyID)
                                {
                                    CampaingFile.GameModes[mode]._keyID = newKeyID;
                                    CampaingFile.GameModes[mode].key = CampaingFile.localization.wordList[newKeyID].key;
                                    EditorUtility.SetDirty(CampaingFile);
                                }

                                settings.ApplyModifiedPropertiesWithoutUndo();

                                GUI.backgroundColor = new Color(.6f, 1f, .6f, 1);
                                if (GUI.Button(new Rect(t.x + t.width - 125, t.y, 125, t.height), new GUIContent(" Add Chapter", EditorGUIUtility.IconContent("Toolbar Plus").image)))
                                {
                                    settings.FindProperty("GameModes").GetArrayElementAtIndex(mode).FindPropertyRelative("Chapters").InsertArrayElementAtIndex(settings.FindProperty("GameModes").GetArrayElementAtIndex(mode).FindPropertyRelative("Chapters").arraySize); settings.ApplyModifiedPropertiesWithoutUndo();
                                }
                                GUI.backgroundColor = defualtButtonColor;
                                EditorGUILayout.Space(4);


                                    DrawChapter(CampaingFile.GameModes[mode].Chapters, settings.FindProperty("GameModes").GetArrayElementAtIndex(mode).FindPropertyRelative("Chapters"),false, CampaingFile.localization.GetValueDemo(CampaingFile.GameModes[mode].key, (int)SystemLanguage.English));

                                GUI.backgroundColor = new Color(1f, .6f, .6f, 1);
                                if (GUI.Button(new Rect(t.x, t.y, 52, 52), "X"))
                                {
                                    settings.FindProperty("GameModes").DeleteArrayElementAtIndex(mode);
                                    settings.ApplyModifiedPropertiesWithoutUndo();
                                }
                                GUI.backgroundColor = defualtButtonColor;

                            }
                        }
                        // Chapter List End

                        else
                        {
                            TinyGUI.InfoBox("First Setup \"Localization File\"");
                        }

                        settings.ApplyModifiedPropertiesWithoutUndo();
                        //EditorUtility.SetDirty(CampaingFile);
                        //AssetDatabase.SaveAssets();

                        /*

                        // To This
                        EditorGUILayout.ObjectField("Localization File", Campaing.localization, typeof(StringLocalization), false);
                        EditorGUILayout.PropertyField(settings.FindProperty("ShowLockedLevelInStoryMode"), new GUIContent("Show Locked Level"));
                        EditorGUILayout.Space();

                        // Story Mode
                        Rect t = EditorGUILayout.GetControlRect(GUILayout.Height(36));
                        EditorGUI.HelpBox(t, "", MessageType.None);
                        EditorGUI.LabelField(new Rect(t.x + 8, t.y, t.width - 8, t.height), $"Story Mode [{settings.FindProperty("storymode").arraySize}]", new GUIStyle("AM MixerHeader"));
                        GUI.backgroundColor = new Color(.6f, 1f, .6f, 1);
                        if (GUI.Button(new Rect(t.x + t.width - 125, t.y, 125, t.height), new GUIContent(" Add Chapter", EditorGUIUtility.IconContent("Toolbar Plus").image)))
                        {
                            settings.FindProperty("storymode").InsertArrayElementAtIndex(settings.FindProperty("storymode").arraySize); settings.ApplyModifiedPropertiesWithoutUndo();
                        }
                        GUI.backgroundColor = defualtButtonColor;
                        EditorGUILayout.Space(4);

                        // Story Mode List
                        for (int i = 0; i < settings.FindProperty("storymode").arraySize; i++)
                        {
                            if (i < settings.FindProperty("storymode").arraySize)
                            {
                                int index = i;
                                EditorGUILayout.BeginVertical("HelpBox");

                                // Title
                                Rect title = EditorGUILayout.GetControlRect(GUILayout.Height(36));
                                EditorGUI.HelpBox(new Rect(title.x - 4, title.y - 3, title.width + 8, title.height), "", MessageType.None);
                                EditorGUI.LabelField(new Rect(title.x + 6, title.y - 3, title.width - 2 - 125, title.height), $"[E{i + 1}] Story Mode - Chapter {(i + 1).ToString("00")}", EditorStyles.boldLabel);

                                // Settings
                                Rect baseSett = EditorGUILayout.GetControlRect(GUILayout.Height(44));
                                EditorGUI.HelpBox(baseSett, "", MessageType.None);
                                Campaing.storymode[i]._keyID = 0;
                                for (int x = 0; x < Campaing.localization.keyOption.Count; x++)
                                {
                                    if (Campaing.storymode[i].key == Campaing.localization.keyOption[x]) { Campaing.storymode[i]._keyID = x; }
                                }
                                Campaing.storymode[i]._keyID = EditorGUI.Popup(new Rect(baseSett.x + 4, baseSett.y + 4, baseSett.width - 8, EditorGUIUtility.singleLineHeight), "Chapter Name Key", Campaing.storymode[i]._keyID, Campaing.localization.keyOption.ToArray());
                                Campaing.storymode[i].key = Campaing.localization.wordList[Campaing.storymode[i]._keyID].key;

                                // TMP
                                EditorGUI.LabelField(new Rect(baseSett.x + 4, baseSett.y + 6 + EditorGUIUtility.singleLineHeight, 42, EditorGUIUtility.singleLineHeight), "ID", EditorStyles.centeredGreyMiniLabel);
                                EditorGUI.LabelField(new Rect(baseSett.x + 50, baseSett.y + 6 + EditorGUIUtility.singleLineHeight, (baseSett.width - 129) / 2 - 4, EditorGUIUtility.singleLineHeight), "Scene Name", EditorStyles.centeredGreyMiniLabel);
                                EditorGUI.LabelField(new Rect(baseSett.x + 50 + (baseSett.width - 129) / 2 - 4, baseSett.y + 6 + EditorGUIUtility.singleLineHeight, (baseSett.width - 129) / 2, EditorGUIUtility.singleLineHeight), "Localization Name", EditorStyles.centeredGreyMiniLabel);

                                // Levels List
                                for (int x = 0; x < settings.FindProperty("storymode").GetArrayElementAtIndex(i).FindPropertyRelative("Levels").arraySize; x++)
                                {
                                    Rect level = EditorGUILayout.GetControlRect(GUILayout.Height(26));
                                    EditorGUI.HelpBox(level, "", MessageType.None);

                                    EditorGUI.LabelField(new Rect(level.x + 4, level.y, 42, level.height), $"E{i + 1}M{x + 1}", EditorStyles.centeredGreyMiniLabel);
                                    Campaing.storymode[i].Levels[x].sceneName = sceneNames[EditorGUI.Popup(new Rect(level.x + 50, level.y + 4, (level.width - 129) / 2 - 4, level.height), "", Campaing.sceneIndexFromName(Campaing.storymode[i].Levels[x].sceneName), sceneNames)];
                                    Campaing.storymode[i].Levels[x].key = Campaing.localization.keyOption[EditorGUI.Popup(new Rect(level.x + 50 + (level.width - 129) / 2, level.y + 4, (level.width - 129) / 2, level.height), "", Campaing.localization.GetKeyID(Campaing.storymode[i].Levels[x].key), Campaing.localization.keyOption.ToArray())];



                                    // Delete
                                    GUI.backgroundColor = new Color(1f, .5f, .5f, 1);
                                    if (GUI.Button(new Rect(level.x + level.width - 75, level.y, 75, level.height), "Delete"))
                                    {
                                        settings.FindProperty("storymode").GetArrayElementAtIndex(i).FindPropertyRelative("Levels").DeleteArrayElementAtIndex(x); settings.ApplyModifiedPropertiesWithoutUndo();
                                    }
                                    GUI.backgroundColor = defualtButtonColor;
                                }

                                // Button
                                GUI.backgroundColor = new Color(.5f, 1f, .5f, 1);
                                Rect addNew = EditorGUILayout.GetControlRect(GUILayout.Height(33));
                                if (GUI.Button(new Rect(addNew.x - 4, addNew.y, addNew.width + 8, addNew.height + 3), new GUIContent("Add New Level", EditorGUIUtility.IconContent("Toolbar Plus").image)))
                                {
                                    settings.FindProperty("storymode").GetArrayElementAtIndex(i).FindPropertyRelative("Levels").InsertArrayElementAtIndex(settings.FindProperty("storymode").GetArrayElementAtIndex(i).FindPropertyRelative("Levels").arraySize); settings.ApplyModifiedPropertiesWithoutUndo();
                                }
                                GUI.backgroundColor = defualtButtonColor;

                                // Delete Chapter
                                GUI.backgroundColor = new Color(1f, .6f, .6f, 1);
                                if (GUI.Button(new Rect(title.x + title.width - 125 + 4, title.y - 3, 125, title.height), "Remove Chapter"))
                                {
                                    settings.FindProperty("storymode").DeleteArrayElementAtIndex(index);
                                    settings.ApplyModifiedPropertiesWithoutUndo();
                                }
                                GUI.backgroundColor = defualtButtonColor;

                                EditorGUILayout.EndVertical();

                                EditorGUILayout.Space();
                            }
                        }



                        // Chapters
                        EditorGUILayout.Space(32);
                        t = EditorGUILayout.GetControlRect(GUILayout.Height(36));
                        EditorGUI.HelpBox(t, "", MessageType.None);
                        EditorGUI.LabelField(new Rect(t.x + 8, t.y, t.width - 8, t.height), $"Campaign [{settings.FindProperty("chapter").arraySize}]", new GUIStyle("AM MixerHeader"));
                        GUI.backgroundColor = new Color(.6f, 1f, .6f, 1);
                        if (GUI.Button(new Rect(t.x + t.width - 125, t.y, 125, t.height), new GUIContent(" Add Chapter", EditorGUIUtility.IconContent("Toolbar Plus").image)))
                        {
                            settings.FindProperty("chapter").InsertArrayElementAtIndex(settings.FindProperty("chapter").arraySize); settings.ApplyModifiedPropertiesWithoutUndo();
                        }
                        GUI.backgroundColor = defualtButtonColor;
                        EditorGUILayout.Space(4);
                        for (int i = 0; i < settings.FindProperty("chapter").arraySize; i++)
                        {
                            if (i < settings.FindProperty("chapter").arraySize)
                            {
                                int index = i;
                                EditorGUILayout.BeginVertical("HelpBox");

                                // Title
                                Rect title = EditorGUILayout.GetControlRect(GUILayout.Height(36));
                                EditorGUI.HelpBox(new Rect(title.x - 4, title.y - 3, title.width + 8, title.height), "", MessageType.None);
                                EditorGUI.LabelField(new Rect(title.x + 6, title.y - 3, title.width - 2 - 125, title.height), $"[E{i + 1}] Campaign - Chapter {(i + 1).ToString("00")}", EditorStyles.boldLabel);

                                // Settings
                                Rect baseSett = EditorGUILayout.GetControlRect(GUILayout.Height(44));
                                EditorGUI.HelpBox(baseSett, "", MessageType.None);
                                Campaing.chapter[i]._keyID = 0;
                                for (int x = 0; x < Campaing.localization.keyOption.Count; x++)
                                {
                                    if (Campaing.chapter[i].key == Campaing.localization.keyOption[x]) { Campaing.chapter[i]._keyID = x; }
                                }
                                Campaing.chapter[i]._keyID = EditorGUI.Popup(new Rect(baseSett.x + 4, baseSett.y + 4, baseSett.width - 8, EditorGUIUtility.singleLineHeight), "Chapter Name Key", Campaing.chapter[i]._keyID, Campaing.localization.keyOption.ToArray());
                                Campaing.chapter[i].key = Campaing.localization.wordList[Campaing.chapter[i]._keyID].key;

                                // TMP
                                EditorGUI.LabelField(new Rect(baseSett.x + 4, baseSett.y + 6 + EditorGUIUtility.singleLineHeight, 42, EditorGUIUtility.singleLineHeight), "ID", EditorStyles.centeredGreyMiniLabel);
                                EditorGUI.LabelField(new Rect(baseSett.x + 50, baseSett.y + 6 + EditorGUIUtility.singleLineHeight, (baseSett.width - 129) / 2 - 4, EditorGUIUtility.singleLineHeight), "Scene Name", EditorStyles.centeredGreyMiniLabel);
                                EditorGUI.LabelField(new Rect(baseSett.x + 50 + (baseSett.width - 129) / 2 - 4, baseSett.y + 6 + EditorGUIUtility.singleLineHeight, (baseSett.width - 129) / 2, EditorGUIUtility.singleLineHeight), "Localization Name", EditorStyles.centeredGreyMiniLabel);

                                // Levels List
                                for (int x = 0; x < settings.FindProperty("chapter").GetArrayElementAtIndex(i).FindPropertyRelative("Levels").arraySize; x++)
                                {
                                    Rect level = EditorGUILayout.GetControlRect(GUILayout.Height(26));
                                    EditorGUI.HelpBox(level, "", MessageType.None);

                                    EditorGUI.LabelField(new Rect(level.x + 4, level.y, 42, level.height), $"E{i + 1}M{x + 1}", EditorStyles.centeredGreyMiniLabel);
                                    
                                    Campaing.chapter[i].Levels[x].sceneName = sceneNames[EditorGUI.Popup(new Rect(level.x + 50, level.y + 4, (level.width - 129) / 2 - 4, level.height), "", Campaing.sceneIndexFromName(Campaing.chapter[i].Levels[x].sceneName), sceneNames)];
                                    Campaing.chapter[i].Levels[x].key = Campaing.localization.keyOption[EditorGUI.Popup(new Rect(level.x + 50 + (level.width - 129) / 2, level.y + 4, (level.width - 129) / 2, level.height), "", Campaing.localization.GetKeyID(Campaing.chapter[i].Levels[x].key), Campaing.localization.keyOption.ToArray())];


                                    // Delete
                                    GUI.backgroundColor = new Color(1f, .5f, .5f, 1);
                                    if (GUI.Button(new Rect(level.x + level.width - 75, level.y, 75, level.height), "Delete"))
                                    {
                                        settings.FindProperty("chapter").GetArrayElementAtIndex(i).FindPropertyRelative("Levels").DeleteArrayElementAtIndex(x); settings.ApplyModifiedPropertiesWithoutUndo();
                                    }
                                    GUI.backgroundColor = defualtButtonColor;
                                }

                                // Button
                                GUI.backgroundColor = new Color(.5f, 1f, .5f, 1);
                                Rect addNew = EditorGUILayout.GetControlRect(GUILayout.Height(33));
                                if (GUI.Button(new Rect(addNew.x - 4, addNew.y, addNew.width + 8, addNew.height + 3), new GUIContent("Add New Level", EditorGUIUtility.IconContent("Toolbar Plus").image)))
                                {
                                    settings.FindProperty("chapter").GetArrayElementAtIndex(i).FindPropertyRelative("Levels").InsertArrayElementAtIndex(settings.FindProperty("chapter").GetArrayElementAtIndex(i).FindPropertyRelative("Levels").arraySize); settings.ApplyModifiedPropertiesWithoutUndo();
                                }
                                GUI.backgroundColor = defualtButtonColor;

                                // Delete Chapter
                                GUI.backgroundColor = new Color(1f, .6f, .6f, 1);
                                if (GUI.Button(new Rect(title.x + title.width - 125 + 4, title.y - 3, 125, title.height), "Remove Chapter"))
                                {
                                    settings.FindProperty("chapter").DeleteArrayElementAtIndex(index);
                                    settings.ApplyModifiedPropertiesWithoutUndo();
                                }
                                GUI.backgroundColor = defualtButtonColor;

                                EditorGUILayout.EndVertical();

                                EditorGUILayout.Space();
                            }
                        }



                        EditorGUILayout.Space(32);
                        t = EditorGUILayout.GetControlRect(GUILayout.Height(36));
                        EditorGUI.HelpBox(t, "", MessageType.None);
                        EditorGUI.LabelField(new Rect(t.x + 8, t.y, t.width - 8, t.height), $"Gamemodes [{settings.FindProperty("GameModes").arraySize}]", new GUIStyle("AM MixerHeader"));
                        GUI.backgroundColor = new Color(.6f, 1f, .6f, 1);
                        if (GUI.Button(new Rect(t.x + t.width - 125, t.y, 125, t.height), new GUIContent(" Add Gamemode", EditorGUIUtility.IconContent("Toolbar Plus").image)))
                        {
                            settings.FindProperty("GameModes").InsertArrayElementAtIndex(settings.FindProperty("GameModes").arraySize); settings.ApplyModifiedPropertiesWithoutUndo();
                        }
                        GUI.backgroundColor = defualtButtonColor;
                        // Gamemode List
                        for (int mode = 0; mode < settings.FindProperty("GameModes").arraySize; mode++)
                        {
                            EditorGUILayout.Space(16);
                            t = EditorGUILayout.GetControlRect(GUILayout.Height(52));
                            EditorGUI.HelpBox(t, "", MessageType.None);
                            EditorGUI.LabelField(new Rect(t.x + 60, t.y + 2, t.width - 8, 30), $"{Campaing.GameModes[mode].key} [{settings.FindProperty("GameModes").GetArrayElementAtIndex(mode).FindPropertyRelative("Chapters").arraySize}]", new GUIStyle("AM MixerHeader"));

                            Campaing.GameModes[mode]._keyID = 0;
                            for (int x = 0; x < Campaing.localization.keyOption.Count; x++)
                            {
                                if (Campaing.GameModes[mode].key == Campaing.localization.keyOption[x]) { Campaing.GameModes[mode]._keyID = x; }
                            }
                            Campaing.GameModes[mode]._keyID = EditorGUI.Popup(new Rect(t.x + 60, t.y + 28, t.width - 12 - 52 - 125, EditorGUIUtility.singleLineHeight), "Gamemode Name Key", Campaing.GameModes[mode]._keyID, Campaing.localization.keyOption.ToArray());
                            Campaing.GameModes[mode].key = Campaing.localization.wordList[Campaing.GameModes[mode]._keyID].key;

                            settings.ApplyModifiedPropertiesWithoutUndo();

                            GUI.backgroundColor = new Color(.6f, 1f, .6f, 1);
                            if (GUI.Button(new Rect(t.x + t.width - 125, t.y, 125, t.height), new GUIContent(" Add Chapter", EditorGUIUtility.IconContent("Toolbar Plus").image)))
                            {
                                settings.FindProperty("GameModes").GetArrayElementAtIndex(mode).FindPropertyRelative("Chapters").InsertArrayElementAtIndex(settings.FindProperty("GameModes").GetArrayElementAtIndex(mode).FindPropertyRelative("Chapters").arraySize); settings.ApplyModifiedPropertiesWithoutUndo();
                            }
                            GUI.backgroundColor = defualtButtonColor;
                            EditorGUILayout.Space(4);
                            for (int i = 0; i < settings.FindProperty("GameModes").GetArrayElementAtIndex(mode).FindPropertyRelative("Chapters").arraySize; i++)
                            {
                                if (i < settings.FindProperty("GameModes").GetArrayElementAtIndex(mode).FindPropertyRelative("Chapters").arraySize)
                                {
                                    int index = i;
                                    EditorGUILayout.BeginVertical("HelpBox");

                                    // Title
                                    Rect title = EditorGUILayout.GetControlRect(GUILayout.Height(36));
                                    EditorGUI.HelpBox(new Rect(title.x - 4, title.y - 3, title.width + 8, title.height), "", MessageType.None);
                                    EditorGUI.LabelField(new Rect(title.x + 6, title.y - 3, title.width - 2 - 125, title.height), $"[E{i + 1}] {Campaing.GameModes[mode].key} - Chapter {(i + 1).ToString("00")}", EditorStyles.boldLabel);

                                    // Settings
                                    Rect baseSett = EditorGUILayout.GetControlRect(GUILayout.Height(44));
                                    EditorGUI.HelpBox(baseSett, "", MessageType.None);
                                    Campaing.GameModes[mode].Chapters[i]._keyID = 0;
                                    for (int x = 0; x < Campaing.localization.keyOption.Count; x++)
                                    {
                                        if (Campaing.GameModes[mode].Chapters[i].key == Campaing.localization.keyOption[x]) { Campaing.GameModes[mode].Chapters[i]._keyID = x; }
                                    }
                                    Campaing.GameModes[mode].Chapters[i]._keyID = EditorGUI.Popup(new Rect(baseSett.x + 4, baseSett.y + 4, baseSett.width - 8, EditorGUIUtility.singleLineHeight), "Chapter Name Key", Campaing.GameModes[mode].Chapters[i]._keyID, Campaing.localization.keyOption.ToArray());
                                    Campaing.GameModes[mode].Chapters[i].key = Campaing.localization.wordList[Campaing.GameModes[mode].Chapters[i]._keyID].key;

                                    // TMP
                                    EditorGUI.LabelField(new Rect(baseSett.x + 4, baseSett.y + 6 + EditorGUIUtility.singleLineHeight, 42, EditorGUIUtility.singleLineHeight), "ID", EditorStyles.centeredGreyMiniLabel);
                                    EditorGUI.LabelField(new Rect(baseSett.x + 50, baseSett.y + 6 + EditorGUIUtility.singleLineHeight, (baseSett.width - 129) / 2 - 4, EditorGUIUtility.singleLineHeight), "Scene Name", EditorStyles.centeredGreyMiniLabel);
                                    EditorGUI.LabelField(new Rect(baseSett.x + 50 + (baseSett.width - 129) / 2 - 4, baseSett.y + 6 + EditorGUIUtility.singleLineHeight, (baseSett.width - 129) / 2, EditorGUIUtility.singleLineHeight), "Localization Name", EditorStyles.centeredGreyMiniLabel);

                                    // Levels List
                                    for (int x = 0; x < settings.FindProperty("GameModes").GetArrayElementAtIndex(mode).FindPropertyRelative("Chapters").GetArrayElementAtIndex(i).FindPropertyRelative("Levels").arraySize; x++)
                                    {
                                        Rect level = EditorGUILayout.GetControlRect(GUILayout.Height(26));
                                        EditorGUI.HelpBox(level, "", MessageType.None);

                                        EditorGUI.LabelField(new Rect(level.x + 4, level.y, 42, level.height), $"E{i + 1}M{x + 1}", EditorStyles.centeredGreyMiniLabel);
                                        
                                        Campaing.GameModes[mode].Chapters[i].Levels[x].sceneName = sceneNames[EditorGUI.Popup(new Rect(level.x + 50, level.y + 4, (level.width - 129) / 2 - 4, level.height), "", Campaing.sceneIndexFromName(Campaing.GameModes[mode].Chapters[i].Levels[x].sceneName), sceneNames)];
                                        Campaing.GameModes[mode].Chapters[i].Levels[x].key = Campaing.localization.keyOption[EditorGUI.Popup(new Rect(level.x + 50 + (level.width - 129) / 2, level.y + 4, (level.width - 129) / 2, level.height), "", Campaing.localization.GetKeyID(Campaing.GameModes[mode].Chapters[i].Levels[x].key), Campaing.localization.keyOption.ToArray())];

                                        settings.ApplyModifiedPropertiesWithoutUndo();

                                        // Delete
                                        GUI.backgroundColor = new Color(1f, .5f, .5f, 1);
                                        if (GUI.Button(new Rect(level.x + level.width - 75, level.y, 75, level.height), "Delete"))
                                        {
                                            settings.FindProperty("GameModes").GetArrayElementAtIndex(mode).FindPropertyRelative("Chapters").GetArrayElementAtIndex(i).FindPropertyRelative("Levels").DeleteArrayElementAtIndex(x); settings.ApplyModifiedPropertiesWithoutUndo();
                                        }
                                        GUI.backgroundColor = defualtButtonColor;
                                    }

                                    // Button
                                    GUI.backgroundColor = new Color(.5f, 1f, .5f, 1);
                                    Rect addNew = EditorGUILayout.GetControlRect(GUILayout.Height(33));
                                    if (GUI.Button(new Rect(addNew.x - 4, addNew.y, addNew.width + 8, addNew.height + 3), new GUIContent("Add New Level", EditorGUIUtility.IconContent("Toolbar Plus").image)))
                                    {
                                        settings.FindProperty("GameModes").GetArrayElementAtIndex(mode).FindPropertyRelative("Chapters").GetArrayElementAtIndex(i).FindPropertyRelative("Levels").InsertArrayElementAtIndex(settings.FindProperty("GameModes").GetArrayElementAtIndex(mode).FindPropertyRelative("Chapters").GetArrayElementAtIndex(i).FindPropertyRelative("Levels").arraySize); settings.ApplyModifiedPropertiesWithoutUndo();
                                    }
                                    GUI.backgroundColor = defualtButtonColor;

                                    // Delete Chapter
                                    GUI.backgroundColor = new Color(1f, .6f, .6f, 1);
                                    if (GUI.Button(new Rect(title.x + title.width - 125 + 4, title.y - 3, 125, title.height), "Remove Chapter"))
                                    {
                                        settings.FindProperty("GameModes").GetArrayElementAtIndex(mode).FindPropertyRelative("Chapters").DeleteArrayElementAtIndex(index);
                                        settings.ApplyModifiedPropertiesWithoutUndo();
                                    }
                                    GUI.backgroundColor = defualtButtonColor;

                                    EditorGUILayout.EndVertical();

                                    EditorGUILayout.Space();
                                }
                            }
                            GUI.backgroundColor = new Color(1f, .6f, .6f, 1);
                            if (GUI.Button(new Rect(t.x, t.y, 52, 52), "X"))
                            {
                                settings.FindProperty("GameModes").DeleteArrayElementAtIndex(mode);
                                settings.ApplyModifiedPropertiesWithoutUndo();
                            }
                            GUI.backgroundColor = defualtButtonColor;
                        }

                        settings.ApplyModifiedPropertiesWithoutUndo();
                        */

                        void DrawChapter(List<Chapter> chapterList, SerializedProperty chapterProperty, bool DrawHeader, string Header)
                        {
                            // Title Draw Start

                            if (DrawHeader)
                            {
                                Rect t = EditorGUILayout.GetControlRect(GUILayout.Height(36));
                                EditorGUI.HelpBox(t, "", MessageType.None);
                                EditorGUI.LabelField(new Rect(t.x + 8, t.y, t.width - 8, t.height), $"{Header} [{chapterProperty.arraySize}]", new GUIStyle("AM MixerHeader"));
                                GUI.backgroundColor = new Color(.6f, 1f, .6f, 1);
                                if (GUI.Button(new Rect(t.x + t.width - 125, t.y, 125, t.height), new GUIContent(" Add Chapter", EditorGUIUtility.IconContent("Toolbar Plus").image)))
                                {
                                    chapterProperty.InsertArrayElementAtIndex(chapterProperty.arraySize); settings.ApplyModifiedPropertiesWithoutUndo();
                                }
                                GUI.backgroundColor = defualtButtonColor;
                                EditorGUILayout.Space(4);
                            }
                            // Title Draw End

                            for (int chapterID = 0; chapterID < chapterList.Count; chapterID++)
                            {
                                Chapter chapter = chapterList[chapterID];

                                // Show Title
                                Rect title = EditorGUILayout.GetControlRect(GUILayout.Height(36));
                                EditorGUI.HelpBox(new Rect(title.x + 4, title.y - 3, title.width-8, title.height), "", MessageType.None);
                                EditorGUI.LabelField(new Rect(title.x + 14, title.y - 3, title.width - 2 - 125, title.height), $"[E{chapterID + 1}] {Header} - Chapter {(chapterID + 1).ToString("00")}", EditorStyles.boldLabel);

                                // Character Localization
                                Rect baseSett = EditorGUILayout.GetControlRect(GUILayout.Height(44));
                                EditorGUI.HelpBox(new Rect(baseSett.x+4,baseSett.y,baseSett.width-8,baseSett.height), "", MessageType.None);

                                // Show Pupup
                                chapter._keyID = 0;
                                for (int x = 0; x < CampaingFile.localization.keyOption.Count; x++)
                                {
                                    if (chapter.key == CampaingFile.localization.keyOption[x])
                                    {
                                        chapter._keyID = x;
                                        break;
                                    }
                                }
                                int newKeyID = EditorGUI.Popup(new Rect(baseSett.x + 8, baseSett.y + 4, baseSett.width - 16, EditorGUIUtility.singleLineHeight), "Chapter Name Key", chapter._keyID, CampaingFile.localization.keyOption.ToArray());
                                if (newKeyID != chapter._keyID)
                                {
                                    chapter._keyID = newKeyID;
                                    chapter.key = CampaingFile.localization.wordList[newKeyID].key;
                                    EditorUtility.SetDirty(CampaingFile);
                                }
                                //CampaingFile.storymode[chapter].key = CampaingFile.localization.keyOption[EditorGUI.Popup(new Rect(baseSett.x + 4, baseSett.y + 4, baseSett.width - 8, EditorGUIUtility.singleLineHeight), "", CampaingFile.localization.GetKeyID(CampaingFile.storymode[chapter].key), CampaingFile.localization.keyOption.ToArray())];

                                // Labels
                                EditorGUI.LabelField(new Rect(baseSett.x + 8, baseSett.y + 6 + EditorGUIUtility.singleLineHeight, 42, EditorGUIUtility.singleLineHeight), "ID", EditorStyles.centeredGreyMiniLabel);
                                EditorGUI.LabelField(new Rect(baseSett.x + 54, baseSett.y + 6 + EditorGUIUtility.singleLineHeight, (baseSett.width - 129) / 2 - 4, EditorGUIUtility.singleLineHeight), "Scene Name", EditorStyles.centeredGreyMiniLabel);
                                EditorGUI.LabelField(new Rect(baseSett.x + 54 + (baseSett.width - 129) / 2 - 4, baseSett.y + 6 + EditorGUIUtility.singleLineHeight, (baseSett.width - 129) / 2, EditorGUIUtility.singleLineHeight), "Localization Name", EditorStyles.centeredGreyMiniLabel);

                                // Levels List
                                for (int x = 0; x < chapter.Levels.Count; x++)
                                {
                                    Rect level = EditorGUILayout.GetControlRect(GUILayout.Height(26));
                                    EditorGUI.HelpBox(new Rect(level.x+4,level.y,level.width-8,level.height), "", MessageType.None);

                                    EditorGUI.LabelField(new Rect(level.x + 8, level.y, 42, level.height), $"E{chapterID + 1}M{x + 1}", EditorStyles.centeredGreyMiniLabel);


                                    chapter.Levels[x].sceneName = sceneNames[EditorGUI.Popup(new Rect(level.x + 54, level.y + 4, (level.width - 129) / 2 - 8, level.height), "", CampaingFile.sceneIndexFromName(chapter.Levels[x].sceneName), sceneNames)];

                                    //chapter.Levels[x].key = CampaingFile.localization.keyOption[EditorGUI.Popup(new Rect(level.x + 50 + (level.width - 129) / 2, level.y + 4, (level.width - 129) / 2, level.height), "", CampaingFile.localization.GetKeyID(chapter.Levels[x].key), CampaingFile.localization.keyOption.ToArray())];
                                    chapter.Levels[x]._keyID = 0;
                                    for (int keys = 0; keys < CampaingFile.localization.keyOption.Count; keys++)
                                    {
                                        if (chapter.Levels[x].key == CampaingFile.localization.keyOption[keys])
                                        {
                                            chapter.Levels[x]._keyID = keys;
                                            break;
                                        }
                                    }
                                    newKeyID = EditorGUI.Popup(new Rect(level.x + 50 + (level.width - 129) / 2, level.y + 4, (level.width - 129) / 2 - 4, level.height), "", chapter.Levels[x]._keyID, CampaingFile.localization.keyOption.ToArray());
                                    if (newKeyID != chapter._keyID)
                                    {
                                        chapter.Levels[x]._keyID = newKeyID;
                                        chapter.Levels[x].key = CampaingFile.localization.wordList[newKeyID].key;
                                        EditorUtility.SetDirty(CampaingFile);
                                    }

                                    // Delete
                                    GUI.backgroundColor = new Color(1f, .5f, .5f, 1);
                                    if (GUI.Button(new Rect(level.x + level.width - 75-4, level.y, 75, level.height), "Delete"))
                                    {
                                        chapterProperty.GetArrayElementAtIndex(chapterID).FindPropertyRelative("Levels").DeleteArrayElementAtIndex(x); settings.ApplyModifiedPropertiesWithoutUndo();
                                    }
                                    GUI.backgroundColor = defualtButtonColor;
                                }


                                // Add Button
                                GUI.backgroundColor = new Color(.5f, 1f, .5f, 1);
                                Rect addNew = EditorGUILayout.GetControlRect(GUILayout.Height(26));
                                if (GUI.Button(new Rect(addNew.x + 4, addNew.y, addNew.width - 8, addNew.height), new GUIContent("Add New Level", EditorGUIUtility.IconContent("Toolbar Plus").image)))
                                {
                                    chapterProperty.GetArrayElementAtIndex(chapterID).FindPropertyRelative("Levels").InsertArrayElementAtIndex(chapterProperty.GetArrayElementAtIndex(chapterID).FindPropertyRelative("Levels").arraySize); settings.ApplyModifiedPropertiesWithoutUndo();
                                }
                                GUI.backgroundColor = defualtButtonColor;
                                EditorGUILayout.Space();

                                // Delete Chapter
                                GUI.backgroundColor = new Color(1f, .6f, .6f, 1);
                                if (GUI.Button(new Rect(title.x + title.width - 125 - 4, title.y - 3, 125, title.height), "Remove Chapter"))
                                {
                                    chapterProperty.DeleteArrayElementAtIndex(chapterID);
                                    settings.ApplyModifiedPropertiesWithoutUndo();
                                }
                                GUI.backgroundColor = defualtButtonColor;
                            }


                            EditorGUILayout.Space();
                            EditorGUILayout.Space();
                            EditorGUILayout.Space();
                        }

                    }


                };



                return provider;
            }

        }



        public class CampaignManagerProvider : SettingsProvider
        {
            private SerializedObject m_CustomSettings;

            class Styles
            {
                public static GUIContent chapter = new GUIContent("chapter");
                public static GUIContent localization = new GUIContent("localization");
            }

            const string k_CampaingPath = "Assets/Classic FPS/Scene/Campaing.asset";
            public CampaignManagerProvider(string path, SettingsScope scope = SettingsScope.User)
                : base(path, scope) { }

            public static bool IsSettingsAvailable()
            {
                return File.Exists(k_CampaingPath);
            }

            public override void OnActivate(string searchContext, VisualElement rootElement)
            {
                m_CustomSettings = CampaignManager.GetSerializedSettings();
            }

            public override void OnGUI(string searchContext)
            {
                EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("chapter"), Styles.chapter);
                EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("localization"), Styles.localization);
            }
        }

#endif

        [System.Serializable]
        public class GamemodeLevel
        {
            public string key = "";
            public List<Chapter> Chapters;

            [HideInInspector] public int _keyID;
        }


        [System.Serializable]
        public class Chapter
        {
            public string key = "";
            public List<Level> Levels;

            [HideInInspector] public int _keyID;
        }


        [System.Serializable]
        public class Level
        {
            public string key = "";
            public string sceneName;

            [HideInInspector] public int _keyID;
        }

#if UNITY_EDITOR
        /*
        [CustomPropertyDrawer(typeof(Level))]
        public class LevelPropertyDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                SerializedProperty nameType = property.FindPropertyRelative("sceneName");
                SerializedProperty keyType = property.FindPropertyRelative("key");

                int indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                float widthSize = position.width / 2;

                Rect pos1 = new Rect(position.x, position.y, widthSize, position.height);
                Rect pos2 = new Rect(position.x + widthSize + 5, position.y, widthSize - 5, position.height);

                EditorGUI.PropertyField(pos1, nameType, GUIContent.none);
                EditorGUI.PropertyField(pos2, keyType, GUIContent.none);

                EditorGUI.indentLevel = indent;

                EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return 40;
            }
        }

        [CustomPropertyDrawer(typeof(Chapter))]
        public class ChapterPropertyDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);
                SerializedProperty keyType = property.FindPropertyRelative("key");

                Rect labelPosition = new Rect(9, position.y, position.width/2, EditorGUIUtility.singleLineHeight);
                Rect keyLabelPosition = new Rect(position.x + (position.width / 2), position.y, position.width/2, EditorGUIUtility.singleLineHeight);

                int indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                position = EditorGUI.PrefixLabel(labelPosition, EditorGUIUtility.GetControlID(FocusType.Passive), new GUIContent(keyType.stringValue != "" ? keyType.stringValue : "Empty Key"));
                 Rect pos1 = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                 EditorGUI.PropertyField(keyLabelPosition, keyType, GUIContent.none);

                // Line 2
                GUILayout.BeginHorizontal();
                GUILayout.Label(""); GUILayout.Label(""); GUILayout.Label(""); GUILayout.Label("");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                GUILayout.Label("Scene Name");
                GUILayout.Label("Localization Key");
                GUILayout.Label("",GUILayout.Width(50));

                GUILayout.EndHorizontal();

                // List
                for (int i = 0; i < property.FindPropertyRelative("Levels").arraySize; i++)
                {
                    //Rect r = EditorGUILayout.BeginHorizontal("window", GUILayout.Height(10));
                    Rect r = EditorGUILayout.BeginHorizontal("box");
                    EditorGUI.PropertyField(new Rect(r.x, r.y, r.width - 80, r.height), property.FindPropertyRelative("Levels").GetArrayElementAtIndex(i));
                    GUILayout.FlexibleSpace();
                    var _oldColor = GUI.backgroundColor; GUI.backgroundColor = Color.red; 
                    if (GUILayout.Button("Delete", GUILayout.Width(75), GUILayout.Height(EditorGUIUtility.singleLineHeight) )) { property.FindPropertyRelative("Levels").DeleteArrayElementAtIndex(i); }
                    GUI.backgroundColor = _oldColor;

                    EditorGUILayout.EndHorizontal();
                }

                Rect s = EditorGUILayout.BeginHorizontal("box");
                var oldColor = GUI.backgroundColor; GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Add New Level", GUILayout.Height(35))) { property.FindPropertyRelative("Levels").InsertArrayElementAtIndex(property.FindPropertyRelative("Levels").arraySize); }
                GUI.backgroundColor = oldColor;
                EditorGUILayout.EndHorizontal();


                EditorGUI.indentLevel = indent;
                EditorGUI.EndProperty();
            }
        }
        */
#endif
    }
}