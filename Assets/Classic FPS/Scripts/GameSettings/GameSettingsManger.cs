#define HellishBattle
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace HellishBattle
{
    public class GameSettingsManger : ScriptableObject
    {
        public const string k_CampaingPath = "Assets/Classic FPS/Resources/Game_Settings.asset";

        private static GameSettingsManger _instance;
        public static GameSettingsManger Instance
        {
            get { return _instance; }
        }

        //* Enemy *//
        [SerializeField] public bool EnemyHealthBar = true;
        [SerializeField] public bool EnemyName = true;
        [SerializeField] public bool EnemyDamageFlash = false;
        public Color DamageFlashEnemy;
        public Color HealFlashEnemy;

        //* Player *//
        [SerializeField] public bool EnablePlayerAbility = true;
        [SerializeField] public bool EnablePlayerSprint = true;
        [SerializeField] public bool EnablePlayerCrouch = true;

        [SerializeField] public bool EnablePlayerDamageOverlay = true;
        [SerializeField] public bool EnablePlayerBonusOverlay = true;
        [SerializeField] public bool EnablePlayerDamageIndicator = true;

        //* Player *//
        [SerializeField] public float WeaponSpreadWalk = 10;
        [SerializeField] public float WeaponSpreadRun = 25;
        //[SerializeField] public bool WeaponSpreadCrouch = true;
        [SerializeField] public float WeaponSpreadJump = 50;

        //* Inventory *//
        public InventoryType inventoryType;
        public int PrimaryWeaponCount;
        public int SecoundaryWeaponCount;
        public int MelleWeaponCount;
        public int ThrowableWeaponCount;

        //* Other *//
        [SerializeField] public int NoneTrophyPoints = 0;
        [SerializeField] public int BronzeTrophyPoints = 1;
        [SerializeField] public int SilverTrophyPoints = 2;
        [SerializeField] public int GoldTrophyPoints = 3;
        [SerializeField] public int DiamondTrophyPoints = 4;
        [SerializeField] public int PlatiniumTrophyPoints = 5;

#if UNITY_EDITOR
        internal static GameSettingsManger GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<GameSettingsManger>(k_CampaingPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<GameSettingsManger>();


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
    static class GameSettingsIMGUIRegister
    {
        [SettingsProvider]
        public static SettingsProvider GameSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Hellish Battle/Asset Settings", SettingsScope.Project)
            {
                label = "Asset Settings",
                guiHandler = (searchContext) =>
                {

                    EditorGUILayout.HelpBox("Here you can fine-tune game settings that are not available from the user level, so that each developer can choose what they want and don't want in their game. Text with no fields visible next to them shows what is likely to appear in the future].", MessageType.Info, true);
                    EditorGUILayout.Space();
                    var settings = GameSettingsManger.GetSerializedSettings();

                    EditorGUILayout.LabelField($"Enemy Settings", EditorStyles.largeLabel);
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.BeginVertical("Toolbar", GUILayout.Height(30));
                    EditorGUILayout.LabelField("Boss & Enemy Settings", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.PropertyField(settings.FindProperty("EnemyHealthBar"), new GUIContent("Enemy Healthbar"));
                    EditorGUILayout.PropertyField(settings.FindProperty("EnemyName"), new GUIContent("Enemy Name"));
                    EditorGUILayout.PropertyField(settings.FindProperty("EnemyDamageFlash"), new GUIContent("Enemy Damage Flash"));
                    EditorGUILayout.PropertyField(settings.FindProperty("DamageFlashEnemy"), new GUIContent("Enemy"));
                    EditorGUILayout.PropertyField(settings.FindProperty("HealFlashEnemy"), new GUIContent("Enemy"));
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField($"Player Settings", EditorStyles.largeLabel);
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.BeginVertical("Toolbar", GUILayout.Height(30));
                    EditorGUILayout.LabelField("Player's Mechanics", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.PropertyField(settings.FindProperty("EnablePlayerSprint"), new GUIContent("Enable Sprint"));
                    GUI.enabled = false;
                    EditorGUILayout.LabelField("Enable Double Jump");
                    EditorGUILayout.LabelField("Enable Crouch");
                    EditorGUILayout.LabelField("Enable Dash");
                    GUI.enabled = true;
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.BeginVertical("Toolbar", GUILayout.Height(30));
                    EditorGUILayout.LabelField("Player Ability", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.PropertyField(settings.FindProperty("EnablePlayerAbility"), new GUIContent("Enable Player Ability"));
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.BeginVertical("Toolbar", GUILayout.Height(30));
                    EditorGUILayout.LabelField("Inventory Slot Count", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();

                    settings.FindProperty("inventoryType").enumValueIndex = (int)EditorGUILayout.Popup("Inventory Type", settings.FindProperty("inventoryType").enumValueIndex, settings.FindProperty("inventoryType").enumNames);
                    if (settings.FindProperty("inventoryType").enumValueIndex == 0)
                    {
                        EditorGUILayout.HelpBox($"Here You can set the maximum number of slots for each weapon slot\nPrimary Slot: {settings.FindProperty("PrimaryWeaponCount").intValue}   |   Secoundary Slot: {settings.FindProperty("SecoundaryWeaponCount").intValue}   |   Melle Slot: {settings.FindProperty("MelleWeaponCount").intValue}   |   Throwable Slot {settings.FindProperty("ThrowableWeaponCount").intValue}", MessageType.Info);
                        settings.FindProperty("PrimaryWeaponCount").intValue = EditorGUILayout.IntSlider("Primary Weapon Slot", settings.FindProperty("PrimaryWeaponCount").intValue, 0, 6);
                        settings.FindProperty("SecoundaryWeaponCount").intValue = EditorGUILayout.IntSlider("Secoundary Weapon Slot", settings.FindProperty("SecoundaryWeaponCount").intValue, 0, 6);
                        settings.FindProperty("MelleWeaponCount").intValue = EditorGUILayout.IntSlider("Melle Weapon Slot", settings.FindProperty("MelleWeaponCount").intValue, 0, 6);
                        settings.FindProperty("ThrowableWeaponCount").intValue = EditorGUILayout.IntSlider("Throwable Weapon Slot", settings.FindProperty("ThrowableWeaponCount").intValue, 0, 6);
                    }


                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField($"Inventory Settings", EditorStyles.largeLabel);
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.BeginVertical("Toolbar", GUILayout.Height(30));
                    EditorGUILayout.LabelField("Player UI", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.PropertyField(settings.FindProperty("EnablePlayerBonusOverlay"), new GUIContent("Enable Bonus Overlay"));
                    EditorGUILayout.PropertyField(settings.FindProperty("EnablePlayerDamageOverlay"), new GUIContent("Enable Damage Overlay"));
                    EditorGUILayout.PropertyField(settings.FindProperty("EnablePlayerDamageIndicator"), new GUIContent("Enable Damage Indicator"));
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField($"Weapon Settings", EditorStyles.largeLabel);
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.BeginVertical("Toolbar", GUILayout.Height(30));
                    EditorGUILayout.LabelField("Weapon Spread", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.PropertyField(settings.FindProperty("WeaponSpreadWalk"), new GUIContent("Walk Spread"));
                    EditorGUILayout.PropertyField(settings.FindProperty("WeaponSpreadRun"), new GUIContent("Run Spread"));
                    //EditorGUILayout.PropertyField(settings.FindProperty("WeaponSpreadCrouch"), new GUIContent("Crouch Spread"));
                    EditorGUILayout.PropertyField(settings.FindProperty("WeaponSpreadJump"), new GUIContent("Jump Spread"));
                    GUI.enabled = false;
                    EditorGUILayout.LabelField("Crouch Spread");
                    GUI.enabled = true;
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField($"Other Settings", EditorStyles.largeLabel);
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.BeginVertical("Toolbar", GUILayout.Height(30));
                    EditorGUILayout.LabelField("Points Settings", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.PropertyField(settings.FindProperty("NoneTrophyPoints"), new GUIContent("None Trophy Points"));
                    EditorGUILayout.PropertyField(settings.FindProperty("BronzeTrophyPoints"), new GUIContent("Bronze Trophy Points"));
                    EditorGUILayout.PropertyField(settings.FindProperty("SilverTrophyPoints"), new GUIContent("Silver Trophy Points"));
                    EditorGUILayout.PropertyField(settings.FindProperty("GoldTrophyPoints"), new GUIContent("Gold Trophy Points"));
                    EditorGUILayout.PropertyField(settings.FindProperty("DiamondTrophyPoints"), new GUIContent("Diamond Trophy Points"));
                    EditorGUILayout.PropertyField(settings.FindProperty("PlatiniumTrophyPoints"), new GUIContent("Platinium Trophy Points"));
                    EditorGUILayout.EndVertical();

                    GUI.enabled = false;
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.BeginVertical("Toolbar", GUILayout.Height(30));
                    EditorGUILayout.LabelField("Achievements", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.LabelField("Enable In-Game Notification");
                    EditorGUILayout.EndVertical();
                    GUI.enabled = true;

                    //if(settings.FindProperty("inventoryType").enumValueIndex == 0)

                    settings.ApplyModifiedPropertiesWithoutUndo();
                }
            };

            return provider;
        }
    }

    class GameSettingsProvider : SettingsProvider
    {
        private SerializedObject m_CustomSettings;

        class Styles
        {
            public static GUIContent chapter = new GUIContent("EnemyHealthBar");
        }

        const string k_CampaingPath = "Assets/Classic FPS/Resources/Game_Settings.asset";
        public GameSettingsProvider(string path, SettingsScope scope = SettingsScope.User)
            : base(path, scope) { }

        public static bool IsSettingsAvailable()
        {
            return File.Exists(k_CampaingPath);
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            m_CustomSettings = GameSettingsManger.GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("EnemyHealthBar"), Styles.chapter);
        }
    }

#endif

    public enum InventoryType
    {
        Normal = 0, Classic = 1
    }
}