using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using HellishBattle.UI;
using HellishBattle.Campaign;
using HellishBattle.SaveSystem;


namespace HellishBattle
{
    public class SettingsManager : MonoBehaviour
    {
        public HorizontalSelector LanguageSelector;
        public HorizontalSelector DifficultySelector;
        public HorizontalSelector ResolutionSelector;
        public HorizontalSelector QualitySelector;
        public HorizontalSelector FullscreenSelector;
        public HorizontalSelector FPSLimitSelector;

        public List<SettingsOptionSelector> Selectors;
        public List<SettingsOptionSlider> Sliders;

        // Private
        public LocalizationManager _lm;
        private DifficultyManager _dm;
        private Resolution[] resolutions;

        private void Awake()
        {
            _dm = (DifficultyManager)Resources.Load("difficulty");

            SetupSettings();
        }

        public void ChangeLanguage(int index)
        {
            if (index < 0) { _lm.selectedLang = SystemLanguage.English; }
            else { _lm.selectedLang = _lm.lang[index]; }
            _lm.SaveLocalization(_lm.selectedLang);
        }

        public void SetupSettings()
        {
            // Localization
            LanguageSelector.KeyList.Clear();
            LanguageSelector.KeyList.Add("English");
            for (int i = 0; i < _lm.lang.Count; i++) { LanguageSelector.KeyList.Add(_lm.LocalizationLocalizationName(_lm.lang[i])); }
            for (int i = 0; i < LanguageSelector.KeyList.Count; i++) { if (_lm.LocalizationLocalizationName(_lm.selectedLang) == LanguageSelector.KeyList[i]) { LanguageSelector.startIndex = i; LanguageSelector.index = i; } }

            // Difficulty
            DifficultySelector.KeyList.Clear();
            for (int i = 0; i < _dm.DifficultySettingsList.Count; i++) { DifficultySelector.KeyList.Add(_dm.DifficultySettingsList[i].key); }
            if (!TinySaveSystem.HasKey("Selected Difficulty")) { DifficultySelector.startIndex = _dm.DefualtDifficulty; TinySaveSystem.SetInt("Selected Difficulty", _dm.DefualtDifficulty); }
            else { DifficultySelector.startIndex = TinySaveSystem.GetInt("Settings_Difficulty"); }
            DifficultySelector.OnChange.AddListener(delegate { TinySaveSystem.SetInt("Settings_Difficulty", DifficultySelector.index); ApplyChange(); });

            // Resolution
            ResolutionSelector.KeyList.Clear();
            resolutions = Screen.resolutions;
            for (int i = 0; i < resolutions.Length; i++) { ResolutionSelector.KeyList.Add($"{resolutions[i].width} x {resolutions[i].height}"); }
            if (!TinySaveSystem.HasKey("Game_Resolution")) { ResolutionSelector.startIndex = resolutions.Length - 1; TinySaveSystem.SetInt("Game_Resolution", resolutions.Length - 1); }
            else { ResolutionSelector.startIndex = TinySaveSystem.GetInt("Game_Resolution"); }
            ResolutionSelector.OnChange.AddListener(delegate { TinySaveSystem.SetInt("Game_Resolution", ResolutionSelector.index); ApplyChange(); });

            // Quality
            QualitySelector.KeyList.Clear();
            for (int i = 0; i < QualitySettings.names.Length; i++) { QualitySelector.KeyList.Add(QualitySettings.names[i]); }
            if (!TinySaveSystem.HasKey("Settings_Quality")) { QualitySelector.startIndex = QualitySettings.names.Length - 1; TinySaveSystem.SetInt("Settings_Quality", QualitySettings.names.Length - 1); }
            else { QualitySelector.startIndex = TinySaveSystem.GetInt("Settings_Quality"); }
            QualitySelector.OnChange.AddListener(delegate { TinySaveSystem.SetInt("Settings_Quality", QualitySelector.index); ApplyChange(); });

            // Fullscreen
            if (!TinySaveSystem.HasKey("Settings_Fullscreen")) { FullscreenSelector.startIndex = 0; TinySaveSystem.SetInt("Settings_Fullscreen", 0); }
            else { FullscreenSelector.startIndex = TinySaveSystem.GetInt("Settings_Fullscreen"); }
            FullscreenSelector.OnChange.AddListener(delegate { TinySaveSystem.SetInt("Settings_Fullscreen", FullscreenSelector.index); ApplyChange(); });

            // FPS Limit
            if (!TinySaveSystem.HasKey("Settings_FPSLimit")) { FPSLimitSelector.startIndex = FPSLimitSelector.KeyList.Count - 1; TinySaveSystem.SetInt("Settings_FPSLimit", FPSLimitSelector.KeyList.Count - 1); }
            else { FPSLimitSelector.startIndex = TinySaveSystem.GetInt("Settings_FPSLimit"); }
            FPSLimitSelector.OnChange.AddListener(delegate { TinySaveSystem.SetInt("Settings_FPSLimit", FPSLimitSelector.index); ApplyChange(); });


            for (int i = 0; i < Selectors.Count; i++)
            {
                // Setup Localization
                Selectors[i].selector.tryLocalizedText = Selectors[i].localizationEnable;
                if (Selectors[i].localizationEnable) { Selectors[i].selector.localization = Selectors[i].localization; }
                // Setup Option
                Selectors[i].selector.KeyList.Clear();
                for (int j = 0; j < Selectors[i].option.Count; j++) { Selectors[i].selector.KeyList.Add(Selectors[i].option[j]); }
                // Setup Save
                if (!TinySaveSystem.HasKey(Selectors[i].SaveName)) { TinySaveSystem.SetInt(Selectors[i].SaveName, Selectors[i].startIndex); }
                Selectors[i].selector.startIndex = TinySaveSystem.GetInt(Selectors[i].SaveName);
                // On Change
                int index = i;
                Selectors[index].selector.OnChange.AddListener(delegate { TinySaveSystem.SetInt(Selectors[index].SaveName, Selectors[index].selector.index); ApplyChange(); });
            }

            for (int i = 0; i < Sliders.Count; i++)
            {
                // Setup Option
                Sliders[i].slider.value = Sliders[i].startValue;
                Sliders[i].slider.minValue = Sliders[i].minValue;
                Sliders[i].slider.maxValue = Sliders[i].maxValue;
                // Setup Save
                if (!TinySaveSystem.HasKey(Sliders[i].SaveName)) { TinySaveSystem.SetInt(Sliders[i].SaveName, Sliders[i].startValue); }
                Sliders[i].slider.value = TinySaveSystem.GetInt(Sliders[i].SaveName);
                // On Change
                int index = i;
                Sliders[index].slider.onValueChanged.AddListener(delegate { TinySaveSystem.SetInt(Sliders[index].SaveName, (int)Sliders[index].slider.value); ApplyChange(); });
            }

            ApplyChange();
        }

        public void ApplyChange()
        {
            // Resolution & Fullscreen
            if (Application.isMobilePlatform == false && (Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.LinuxEditor && Application.platform != RuntimePlatform.OSXEditor))
            {
                Screen.SetResolution(resolutions[TinySaveSystem.GetInt("Game_Resolution")].width, resolutions[TinySaveSystem.GetInt("Game_Resolution")].height, TinySaveSystem.GetInt("Settings_Fullscreen") == 0 ? true : false);
                Screen.fullScreen = TinySaveSystem.GetInt("Settings_Fullscreen") == 0 ? true : false;
            }

            // Quality
            QualitySettings.SetQualityLevel(TinySaveSystem.GetInt("Settings_Quality"));

            // FPS Limit
            Application.targetFrameRate = GetFPSLimit(TinySaveSystem.GetInt("Settings_FPSLimit"));
        }

        int GetFPSLimit(int i)
        {
            switch (i)
            {
                case 0:
                    return 30;
                case 1:
                    return 60;
                case 2:
                    return 120;
                case 3:
                    return 144;
                case 4:
                    return 165;
                case 5:
                    return 240;
                case 6:
                    return 360;
                default:
                    return 999;
            }
        }
    }

    [System.Serializable]
    public class SettingsOptionSelector
    {
        public bool localizationEnable;
        public StringLocalization localization;
        public HorizontalSelector selector;
        public List<string> option;
        public int startIndex;
        public string SaveName = "Settings_";
    }

    [System.Serializable]
    public class SettingsOptionSlider
    {
        public Slider slider;
        public int startValue;
        public int minValue;
        public int maxValue;
        public string SaveName = "Settings_";
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(SettingsOptionSelector))]
    public class SettingsOptionSelectorDrawer : PropertyDrawer
    {
        SerializedProperty optionProp;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty localizationEnableProp = property.FindPropertyRelative("localizationEnable");
            SerializedProperty localizationProp = property.FindPropertyRelative("localization");
            SerializedProperty selectorProp = property.FindPropertyRelative("selector");
            optionProp = property.FindPropertyRelative("option");
            SerializedProperty startIndexProp = property.FindPropertyRelative("startIndex");
            SerializedProperty saveNameProp = property.FindPropertyRelative("SaveName");

            Rect boxPosition = EditorGUI.IndentedRect(position);
            boxPosition.height = GetPropertyHeight(property, label) - 8;
            EditorGUI.HelpBox(boxPosition, "", MessageType.None);

            Rect localizationEnableRect = new Rect(position.x + 3, position.y, 150, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(localizationEnableRect, localizationEnableProp, new GUIContent("Enable Localization"));

            if (localizationEnableProp.boolValue)
            {
                Rect localizationRect = new Rect(position.x + 3 + 160, position.y, position.width - 160 - 6, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(localizationRect, localizationProp, GUIContent.none);
            }

            Rect selectorRect = new Rect(position.x + 3, position.y + ((EditorGUIUtility.singleLineHeight + 2) * 1), position.width - 6, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(selectorRect, selectorProp);

            Rect startIndexRect = new Rect(position.x + 3, position.y + ((EditorGUIUtility.singleLineHeight + 2) * 2), position.width - 6, EditorGUIUtility.singleLineHeight);
            startIndexProp.intValue = EditorGUI.IntSlider(startIndexRect, new GUIContent($"Option {startIndexProp.intValue}: {(optionProp.GetArrayElementAtIndex(startIndexProp.intValue).stringValue).ToUpperInvariant()}"), startIndexProp.intValue, 0, optionProp.arraySize - 1);

            Rect saveNameRect = new Rect(position.x + 3, position.y + ((EditorGUIUtility.singleLineHeight + 2) * 3), position.width - 6, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(saveNameRect, saveNameProp);

            Rect optionRect = new Rect(position.x + 18, position.y + ((EditorGUIUtility.singleLineHeight + 2) * 4), position.width - 24, EditorGUI.GetPropertyHeight(optionProp, true));
            EditorGUI.PropertyField(optionRect, optionProp, true);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int height = (int)(EditorGUIUtility.singleLineHeight + 2) * 5;
            height += (int)(EditorGUI.GetPropertyHeight(property.FindPropertyRelative("option"), true) - EditorGUIUtility.singleLineHeight) + 10;
            return height;
        }
    }

    [CustomPropertyDrawer(typeof(SettingsOptionSlider))]
    public class SettingsOptionSliderDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty selectorProp = property.FindPropertyRelative("slider");
            SerializedProperty startValueProp = property.FindPropertyRelative("startValue");
            SerializedProperty minValueProp = property.FindPropertyRelative("minValue");
            SerializedProperty maxValueProp = property.FindPropertyRelative("maxValue");
            SerializedProperty saveNameProp = property.FindPropertyRelative("SaveName");

            Rect boxPosition = EditorGUI.IndentedRect(position);
            boxPosition.height = (EditorGUIUtility.singleLineHeight + 2) * 4;
            EditorGUI.HelpBox(boxPosition, "", MessageType.None);

            Rect selectorRect = new Rect(position.x + 3, position.y, position.width - 6, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(selectorRect, selectorProp);

            Rect startIndexRect = new Rect(position.x + 3, position.y + ((EditorGUIUtility.singleLineHeight + 2) * 1), position.width - 6, EditorGUIUtility.singleLineHeight);
            startValueProp.intValue = EditorGUI.IntSlider(startIndexRect, new GUIContent($"Start Value [{startValueProp.intValue}]"), startValueProp.intValue, minValueProp.intValue, maxValueProp.intValue);

            Rect minRect = new Rect(position.x + 3, position.y + ((EditorGUIUtility.singleLineHeight + 2) * 2), position.width / 2 - 3 - 6, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(minRect, minValueProp);

            Rect maxRect = new Rect(position.x + 3 + (position.width / 2) + 4, position.y + ((EditorGUIUtility.singleLineHeight + 2) * 2), position.width / 2 - 4 - 6, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(maxRect, maxValueProp);

            Rect saveNameRect = new Rect(position.x + 3, position.y + ((EditorGUIUtility.singleLineHeight + 2) * 3), position.width - 6, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(saveNameRect, saveNameProp);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int height = (int)(EditorGUIUtility.singleLineHeight + 4) * 4;
            return height;
        }
    }

#endif
}


