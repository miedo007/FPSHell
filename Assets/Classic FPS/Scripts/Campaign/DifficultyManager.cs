using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System;
using HellishBattle.SaveSystem;

namespace HellishBattle
{
    namespace Campaign
    {
        public class DifficultyManager : ScriptableObject
        {
            public const string k_CampaingPath = "Assets/Classic FPS/Resources/Difficulty.asset";

            private static DifficultyManager _instance;
            public static DifficultyManager Instance
            {
                get { return _instance; }
            }


            public StringLocalization localization;
            public List<string> DifficultyList;
            public List<DifficultyLevel> DifficultySettingsList;
            public int selectedDifficulty;
            public int DefualtDifficulty;

            [HideInInspector] public Vector2 EditorScrollPosition;


#if UNITY_EDITOR
            internal static DifficultyManager GetOrCreateSettings()
            {
                var settings = AssetDatabase.LoadAssetAtPath<DifficultyManager>(k_CampaingPath);
                if (settings == null)
                {
                    settings = ScriptableObject.CreateInstance<DifficultyManager>();


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

            public DifficultyLevel CurrentDifficultyLevel()
            {
                return DifficultySettingsList[TinySaveSystem.GetInt("Settings_Difficulty")];
            }
        }

        [System.Serializable]
        public class DifficultyLevel
        {
            public string key;
            public LocalizedString Name;

            public float damageMultiplier = 1;
            public float healthMultiplier = 1;
            public float speedMultiplier = 1;

            public float bossDamageMultiplier = 1;
            public float bossHealthMultiplier = 1;
            public float bossSpeedMultiplier = 1;

            public float minionDamageMultiplier = 1;
            public float minionHealthMultiplier = 1;
            public float minionSpeedMultiplier = 1;

            public bool enemyAbility = true;

            public float lootDropChange = 1;

            public float areaDamageMultiplier = 1;
            public float pointsMultiplier = 1;

            public int _keyID;
        }

#if UNITY_EDITOR
        static class DifficultyIMGUIRegister
        {
            static List<bool> foldout = new List<bool>(16);

            [SettingsProvider]
            public static SettingsProvider DifficultyProvider()
            {
                var provider = new SettingsProvider("Project/Hellish Battle/Difficulty", SettingsScope.Project)
                {
                    label = "Difficulty Level",
                    guiHandler = (searchContext) =>
                    {
                        var settings = DifficultyManager.GetSerializedSettings();
                        DifficultyManager DifficultyObject = (DifficultyManager)Resources.Load("Difficulty");

                        EditorGUILayout.BeginHorizontal();

                        float widthLabel = EditorGUIUtility.labelWidth;
                        EditorGUIUtility.labelWidth = 75;

                        // Settings
                        EditorGUILayout.BeginVertical(GUILayout.Width(250));
                        TinyGUI.BeginBoxGroup("Basic Settings", "desc");
                        EditorGUILayout.PropertyField(settings.FindProperty("localization"), new GUIContent("Localization"));

                        TinyGUI.Title("Defualt Difficulty", "Desc");
                        settings.FindProperty("DefualtDifficulty").intValue = EditorGUILayout.Popup(settings.FindProperty("DefualtDifficulty").intValue, DifficultyObject.DifficultyList.ToArray());


                        TinyGUI.BeginBoxGroup("Difficulty Names");
                        for (int i = 0; i < settings.FindProperty("DifficultyList").arraySize; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(settings.FindProperty("DifficultyList").GetArrayElementAtIndex(i), new GUIContent($"Difficulty {i + 1}"));
                            if (TinyGUI.IconButton("d_P4_DeletedLocal", "", GUILayout.Width(EditorGUIUtility.singleLineHeight * 2)))
                            {
                                settings.FindProperty("DifficultyList").DeleteArrayElementAtIndex(i);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        if (TinyGUI.IconButton("Toolbar Plus", " Add New Difficulty", GUILayout.Height(40)))
                        {
                            settings.FindProperty("DifficultyList").InsertArrayElementAtIndex(settings.FindProperty("DifficultyList").arraySize);
                        }
                        TinyGUI.EndBoxGroup();

                        TinyGUI.EndBoxGroup();
                        EditorGUILayout.EndVertical();
                        EditorGUIUtility.labelWidth = widthLabel;


                        // Create
                        EditorGUILayout.BeginVertical();
                        TinyGUI.BeginBoxGroup("Difficulty Settings", "desc");

                        for (int i = 0; i < DifficultyObject.DifficultyList.Count; i++)
                        {
                            if (foldout.Count < DifficultyObject.DifficultyList.Count) { foldout.Add(true); }

                            Rect rect = EditorGUILayout.BeginVertical("HelpBox");
                            GUIContent foldoutContent = new GUIContent($"Difficulty \"{DifficultyObject.DifficultyList[i]}\"");
                            foldout[i] = EditorGUILayout.Foldout(foldout[i], foldoutContent, true);
                            EditorGUILayout.EndVertical();

                            if (foldout[i])
                            {
                                EditorGUILayout.BeginVertical("helpbox");
                                if (i == DifficultyObject.DefualtDifficulty)
                                {
                                    EditorGUILayout.LabelField(DifficultyObject.DifficultyList[i] + " [Defualt]", EditorStyles.boldLabel);
                                }
                                else
                                {
                                    EditorGUILayout.LabelField(DifficultyObject.DifficultyList[i], EditorStyles.boldLabel);
                                }


                                if (DifficultyObject.localization == null) { EditorGUILayout.LabelField("Localization Key", "Localization doesn't setup !!!"); }
                                else
                                {
                                    DifficultyObject.DifficultySettingsList[i]._keyID = 0;
                                    for (int x = 0; x < DifficultyObject.localization.keyOption.Count; x++)
                                    {
                                        if (DifficultyObject.DifficultySettingsList[i].key == DifficultyObject.localization.keyOption[x]) { DifficultyObject.DifficultySettingsList[i]._keyID = x; }
                                    }

                                    DifficultyObject.DifficultySettingsList[i]._keyID = EditorGUILayout.Popup("Localization Key", DifficultyObject.DifficultySettingsList[i]._keyID, DifficultyObject.localization.keyOption.ToArray());
                                    DifficultyObject.DifficultySettingsList[i].key = DifficultyObject.localization.wordList[DifficultyObject.DifficultySettingsList[i]._keyID].key;

                                    //DifficultyObject.DifficultySettingsList[i].key = EditorGUILayout.TextField("Localization Key", DifficultyObject.DifficultySettingsList[i].key);
                                }

                                EditorGUILayout.Space(5);

                                EditorGUILayout.LabelField("Enemy Settings", EditorStyles.boldLabel);
                                DifficultyObject.DifficultySettingsList[i].damageMultiplier = EditorGUILayout.Slider("Damage Multiplier", DifficultyObject.DifficultySettingsList[i].damageMultiplier, 0.2f, 2);
                                DifficultyObject.DifficultySettingsList[i].healthMultiplier = EditorGUILayout.Slider("Health Multiplier", DifficultyObject.DifficultySettingsList[i].healthMultiplier, 0.2f, 2);
                                DifficultyObject.DifficultySettingsList[i].speedMultiplier = EditorGUILayout.Slider("Speed Multiplier", DifficultyObject.DifficultySettingsList[i].speedMultiplier, 0.2f, 2);
                                DifficultyObject.DifficultySettingsList[i].enemyAbility = EditorGUILayout.Toggle("Speed Multiplier", DifficultyObject.DifficultySettingsList[i].enemyAbility);
                                EditorGUILayout.Space(5);

                                EditorGUILayout.LabelField("Boss Settings", EditorStyles.boldLabel);
                                DifficultyObject.DifficultySettingsList[i].bossDamageMultiplier = EditorGUILayout.Slider("Damage Multiplier", DifficultyObject.DifficultySettingsList[i].bossDamageMultiplier, 0.2f, 2);
                                DifficultyObject.DifficultySettingsList[i].bossHealthMultiplier = EditorGUILayout.Slider("Health Multiplier", DifficultyObject.DifficultySettingsList[i].bossHealthMultiplier, 0.2f, 2);
                                DifficultyObject.DifficultySettingsList[i].bossSpeedMultiplier = EditorGUILayout.Slider("Speed Multiplier", DifficultyObject.DifficultySettingsList[i].bossSpeedMultiplier, 0.2f, 2);
                                EditorGUILayout.Space(5);

                                EditorGUILayout.LabelField("Loot Drop Settings", EditorStyles.boldLabel);
                                DifficultyObject.DifficultySettingsList[i].lootDropChange = EditorGUILayout.Slider("Loot Drop Change", DifficultyObject.DifficultySettingsList[i].lootDropChange, 0, 1f);
                                EditorGUILayout.Space(5);

                                EditorGUILayout.LabelField("Other Settings", EditorStyles.boldLabel);
                                DifficultyObject.DifficultySettingsList[i].areaDamageMultiplier = EditorGUILayout.Slider("Area Damage Multiplier", DifficultyObject.DifficultySettingsList[i].areaDamageMultiplier, 0.2f, 2);
                                DifficultyObject.DifficultySettingsList[i].pointsMultiplier = EditorGUILayout.Slider("Points Multiplier", DifficultyObject.DifficultySettingsList[i].pointsMultiplier, 0.5f, 4);
                                GUI.enabled = false;
                                GameSettingsManger gsm = Resources.Load("Game_Settings") as GameSettingsManger;
                                EditorGUILayout.LabelField($"Points: {(int)(gsm.NoneTrophyPoints * DifficultyObject.DifficultySettingsList[i].pointsMultiplier)} / {(int)(gsm.BronzeTrophyPoints * DifficultyObject.DifficultySettingsList[i].pointsMultiplier)} / {(int)(gsm.SilverTrophyPoints * DifficultyObject.DifficultySettingsList[i].pointsMultiplier)} / {(int)(gsm.GoldTrophyPoints * DifficultyObject.DifficultySettingsList[i].pointsMultiplier)} / {(int)(gsm.DiamondTrophyPoints * DifficultyObject.DifficultySettingsList[i].pointsMultiplier)} / {(int)(gsm.PlatiniumTrophyPoints * DifficultyObject.DifficultySettingsList[i].pointsMultiplier)}");
                                GUI.enabled = true;

                                EditorGUILayout.EndVertical();
                            }
                        }

                        TinyGUI.EndBoxGroup();
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.EndHorizontal();
                        /*
                        var settings = DifficultyManager.GetSerializedSettings();
                        DifficultyManager DifficultyObject = (DifficultyManager)Resources.Load("Difficulty");

                        for (int i = 0; i < DifficultyObject.DifficultyList.Count; i++)
                        {
                            if (i >= DifficultyObject.DifficultySettingsList.Count)
                            {
                                DifficultyObject.DifficultySettingsList.Add(new DifficultyLevel());
                            }
                        }

                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.BeginVertical("Toolbar", GUILayout.Height(30));
                        EditorGUILayout.LabelField("String Localization to Difficutly Name", EditorStyles.boldLabel);
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.PropertyField(settings.FindProperty("localization"), new GUIContent("Localization"));

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(5);

                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.BeginVertical("Toolbar", GUILayout.Height(30));
                        EditorGUILayout.LabelField("Localization List", EditorStyles.boldLabel);
                        EditorGUILayout.EndVertical();
                        for(int i = 0; i < settings.FindProperty("DifficultyList").arraySize; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(settings.FindProperty("DifficultyList").GetArrayElementAtIndex(i), new GUIContent($"Difficulty {i}"));
                            if(GUILayout.Button("Delete", GUILayout.Width(75)))
                            {
                                settings.FindProperty("DifficultyList").DeleteArrayElementAtIndex(i);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        if(GUILayout.Button("+ Add New Difficulty",GUILayout.Height(40)))
                        {
                            settings.FindProperty("DifficultyList").InsertArrayElementAtIndex(settings.FindProperty("DifficultyList").arraySize);
                        }

                        EditorGUILayout.Space(5);
                        settings.FindProperty("DefualtDifficulty").intValue = EditorGUILayout.Popup($"Defualt Difficulty", settings.FindProperty("DefualtDifficulty").intValue, DifficultyObject.DifficultyList.ToArray());

                        //EditorGUILayout.PropertyField(settings.FindProperty("DifficultyList"), new GUIContent("Difficulty List"));
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(5);

                        DifficultyObject.EditorScrollPosition = EditorGUILayout.BeginScrollView(DifficultyObject.EditorScrollPosition);
                        EditorGUILayout.BeginHorizontal();
                        for (int i = 0; i < DifficultyObject.DifficultyList.Count; i++)
                        {
                            EditorGUILayout.BeginVertical("box");
                            EditorGUILayout.BeginVertical("Toolbar", GUILayout.Height(30));
                            if (i == DifficultyObject.DefualtDifficulty)
                            {
                                EditorGUILayout.LabelField(DifficultyObject.DifficultyList[i] + " [Defualt]", EditorStyles.boldLabel);
                            }
                            else
                            {
                                EditorGUILayout.LabelField(DifficultyObject.DifficultyList[i], EditorStyles.boldLabel);
                            }
                            EditorGUILayout.EndVertical();


                            if (DifficultyObject.localization == null) { EditorGUILayout.LabelField("Localization Key", "Localization doesn't setup !!!"); }
                            else
                            {
                                DifficultyObject.DifficultySettingsList[i]._keyID = 0;
                                for (int x = 0; x < DifficultyObject.localization.keyOption.Count; x++)
                                {
                                    if (DifficultyObject.DifficultySettingsList[i].key == DifficultyObject.localization.keyOption[x]) { DifficultyObject.DifficultySettingsList[i]._keyID = x; }
                                }

                                DifficultyObject.DifficultySettingsList[i]._keyID = EditorGUILayout.Popup("Localization Key", DifficultyObject.DifficultySettingsList[i]._keyID, DifficultyObject.localization.keyOption.ToArray());
                                DifficultyObject.DifficultySettingsList[i].key = DifficultyObject.localization.wordList[DifficultyObject.DifficultySettingsList[i]._keyID].key;

                                //DifficultyObject.DifficultySettingsList[i].key = EditorGUILayout.TextField("Localization Key", DifficultyObject.DifficultySettingsList[i].key);
                            }

                            EditorGUILayout.Space(5);

                            EditorGUILayout.LabelField("Enemy Settings", EditorStyles.boldLabel);
                            DifficultyObject.DifficultySettingsList[i].damageMultiplier = EditorGUILayout.Slider("Damage Multiplier", DifficultyObject.DifficultySettingsList[i].damageMultiplier,0.2f,2);
                            DifficultyObject.DifficultySettingsList[i].healthMultiplier = EditorGUILayout.Slider("Health Multiplier", DifficultyObject.DifficultySettingsList[i].healthMultiplier, 0.2f, 2);
                            DifficultyObject.DifficultySettingsList[i].speedMultiplier = EditorGUILayout.Slider("Speed Multiplier", DifficultyObject.DifficultySettingsList[i].speedMultiplier, 0.2f, 2);
                            DifficultyObject.DifficultySettingsList[i].enemyAbility = EditorGUILayout.Toggle("Speed Multiplier", DifficultyObject.DifficultySettingsList[i].enemyAbility);
                            EditorGUILayout.Space(5);

                            EditorGUILayout.LabelField("Boss Settings", EditorStyles.boldLabel);
                            DifficultyObject.DifficultySettingsList[i].bossDamageMultiplier = EditorGUILayout.Slider("Damage Multiplier", DifficultyObject.DifficultySettingsList[i].bossDamageMultiplier, 0.2f, 2);
                            DifficultyObject.DifficultySettingsList[i].bossHealthMultiplier = EditorGUILayout.Slider("Health Multiplier", DifficultyObject.DifficultySettingsList[i].bossHealthMultiplier, 0.2f, 2);
                            DifficultyObject.DifficultySettingsList[i].bossSpeedMultiplier = EditorGUILayout.Slider("Speed Multiplier", DifficultyObject.DifficultySettingsList[i].bossSpeedMultiplier, 0.2f, 2);
                            EditorGUILayout.Space(5);

                            EditorGUILayout.LabelField("Loot Drop Settings", EditorStyles.boldLabel);
                            DifficultyObject.DifficultySettingsList[i].lootDropChange = EditorGUILayout.Slider("Loot Drop Change", DifficultyObject.DifficultySettingsList[i].lootDropChange, 0, 1f);
                            EditorGUILayout.Space(5);

                            EditorGUILayout.LabelField("Other Settings", EditorStyles.boldLabel);
                            DifficultyObject.DifficultySettingsList[i].areaDamageMultiplier = EditorGUILayout.Slider("Area Damage Multiplier", DifficultyObject.DifficultySettingsList[i].areaDamageMultiplier, 0.2f, 2);
                            DifficultyObject.DifficultySettingsList[i].pointsMultiplier = EditorGUILayout.Slider("Points Multiplier", DifficultyObject.DifficultySettingsList[i].pointsMultiplier, 0.5f, 4);
                            GUI.enabled = false;
                            GameSettingsManger gsm = Resources.Load("Game_Settings") as GameSettingsManger;
                            EditorGUILayout.LabelField($"Points: {(int)(gsm.NoneTrophyPoints * DifficultyObject.DifficultySettingsList[i].pointsMultiplier)} / {(int)(gsm.BronzeTrophyPoints * DifficultyObject.DifficultySettingsList[i].pointsMultiplier)} / {(int)(gsm.SilverTrophyPoints * DifficultyObject.DifficultySettingsList[i].pointsMultiplier)} / {(int)(gsm.GoldTrophyPoints * DifficultyObject.DifficultySettingsList[i].pointsMultiplier)} / {(int)(gsm.DiamondTrophyPoints * DifficultyObject.DifficultySettingsList[i].pointsMultiplier)} / {(int)(gsm.PlatiniumTrophyPoints * DifficultyObject.DifficultySettingsList[i].pointsMultiplier)}");
                            GUI.enabled = true;

                            EditorGUILayout.EndVertical();
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndScrollView();

                        settings.ApplyModifiedPropertiesWithoutUndo();
                        */
                    }
                };

                return provider;
            }
        }

        class DifficultyProvider : SettingsProvider
        {
            private SerializedObject m_CustomSettings;

            const string k_CampaingPath = "Assets/Classic FPS/Resources/Difficulty.asset";
            public DifficultyProvider(string path, SettingsScope scope = SettingsScope.User)
                : base(path, scope) { }

            public static bool IsSettingsAvailable()
            {
                return File.Exists(k_CampaingPath);
            }

            public override void OnActivate(string searchContext, VisualElement rootElement)
            {
                m_CustomSettings = DifficultyManager.GetSerializedSettings();
            }

        }

#endif

    }
}