using HellishBattle.SaveSystem;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#if MEET_AND_TALK

#endif

public class LocalizationManager : ScriptableObject
{
    public const string k_LocalizationManagerPath = "Assets/Classic FPS/Resources/Languages.asset";

    private static LocalizationManager _instance;
    public static LocalizationManager Instance
    {
        get { return _instance; }
    }

    [SerializeField]
    public List<SystemLanguage> lang = new List<SystemLanguage>();
    [SerializeField]
    public SystemLanguage selectedLang = SystemLanguage.English;

#if UNITY_EDITOR
    internal static LocalizationManager GetOrCreateSettings()
    {
        var settings = AssetDatabase.LoadAssetAtPath<LocalizationManager>(k_LocalizationManagerPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<LocalizationManager>();

            settings.lang = new List<SystemLanguage>() { SystemLanguage.Polish, SystemLanguage.Spanish };
            settings.selectedLang = SystemLanguage.English;

            AssetDatabase.CreateAsset(settings, k_LocalizationManagerPath);
            AssetDatabase.SaveAssets();
        }
        return settings;
    }
    internal static SerializedObject GetSerializedSettings()
    {
        return new SerializedObject(GetOrCreateSettings());
    }
#endif

    public void SaveLocalization(SystemLanguage lang)
    {
        selectedLang = lang;
        TinySaveSystem.SetInt("SelectedLocalization", (int)lang);
    }
    public void LoadLocalization()
    {
        int _lang = TinySaveSystem.GetInt("SelectedLocalization");
        selectedLang = (SystemLanguage)_lang;
    }

    public string LocalizationLocalizationName(SystemLanguage lang)
    {
        switch (lang)
        {
            case SystemLanguage.English:
                return "English";
            case SystemLanguage.Japanese:
                return "日本語";
            case SystemLanguage.ChineseSimplified:
                return "中国语 简体字";
            case SystemLanguage.ChineseTraditional:
                return "中国语 繁體字";
            case SystemLanguage.Chinese:
                return "中国语";
            case SystemLanguage.Afrikaans:
                return "Afrikane";
            case SystemLanguage.Arabic:
                return "عربى";
            case SystemLanguage.Basque:
                return "Euskara";
            case SystemLanguage.Belarusian:
                return "Беларуская";
            case SystemLanguage.Bulgarian:
                return "български";
            case SystemLanguage.Catalan:
                return "Català";
            case SystemLanguage.Czech:
                return "Český jazyk";
            case SystemLanguage.Danish:
                return "Dansk";
            case SystemLanguage.Dutch:
                return "Nederlands";
            case SystemLanguage.Estonian:
                return "Eestlane";
            case SystemLanguage.Faroese:
                return "Føroyskt";
            case SystemLanguage.Finnish:
                return "Suomi";
            case SystemLanguage.French:
                return "Français";
            case SystemLanguage.German:
                return "Deutsch";
            case SystemLanguage.Greek:
                return "Ελληνικά";
            case SystemLanguage.Hebrew:
                return "עברית";
            case SystemLanguage.Hungarian:
                return "Magyar nyelv";
            case SystemLanguage.Icelandic:
                return "íslenska";
            case SystemLanguage.Indonesian:
                return "Bahasa Indonesia";
            case SystemLanguage.Italian:
                return "Italiano";
            case SystemLanguage.Korean:
                return "조선말";
            case SystemLanguage.Latvian:
                return "Latviešu";
            case SystemLanguage.Lithuanian:
                return "lietuvių kalba";
            case SystemLanguage.Norwegian:
                return "Norsk";
            case SystemLanguage.Polish:
                return "Polski";
            case SystemLanguage.Portuguese:
                return "Português";
            case SystemLanguage.Romanian:
                return "Limba română";
            case SystemLanguage.Russian:
                return "Pусский язык";
            case SystemLanguage.SerboCroatian:
                return "Cрпскохрватски";
            case SystemLanguage.Slovak:
                return "Slovenčina";
            case SystemLanguage.Slovenian:
                return "Slovenščina";
            case SystemLanguage.Spanish:
                return "Español";
            case SystemLanguage.Swedish:
                return "Svenska";
            case SystemLanguage.Thai:
                return "ภาษาไทย";
            case SystemLanguage.Turkish:
                return "Türkçe";
            case SystemLanguage.Ukrainian:
                return "Yкраїнська мова";
            case SystemLanguage.Vietnamese:
                return "Tiếng Việt";
            case SystemLanguage.Unknown:
            default:
                return lang.ToString();
        }
    }


    public static string GetIsoLanguageCode(string languageName)
    {
        switch (languageName)
        {
            case "English":
                return "en";
            case "Japanese":
                return "ja";
            case "ChineseSimplified":
                return "zh-Hans";
            case "ChineseTraditional":
                return "zh-Hant";
            case "Chinese":
                return "zh";
            case "Afrikaans":
                return "af";
            case "Arabic":
                return "ar";
            case "Basque":
                return "eu";
            case "Belarusian":
                return "be";
            case "Bulgarian":
                return "bg";
            case "Catalan":
                return "ca";
            case "Czech":
                return "cs";
            case "Danish":
                return "da";
            case "Dutch":
                return "nl";
            case "Estonian":
                return "et";
            case "Faroese":
                return "fo";
            case "Finnish":
                return "fi";
            case "French":
                return "fr";
            case "German":
                return "de";
            case "Greek":
                return "el";
            case "Hebrew":
                return "he";
            case "Hungarian":
                return "hu";
            case "Icelandic":
                return "is";
            case "Indonesian":
                return "id";
            case "Italian":
                return "it";
            case "Korean":
                return "ko";
            case "Latvian":
                return "lv";
            case "Lithuanian":
                return "lt";
            case "Norwegian":
                return "no";
            case "Polish":
                return "pl";
            case "Portuguese":
                return "pt";
            case "Romanian":
                return "ro";
            case "Russian":
                return "ru";
            case "SerboCroatian":
                return "sh";
            case "Slovak":
                return "sk";
            case "Slovenian":
                return "sl";
            case "Spanish":
                return "es";
            case "Swedish":
                return "sv";
            case "Thai":
                return "th";
            case "Turkish":
                return "tr";
            case "Ukrainian":
                return "uk";
            case "Vietnamese":
                return "vi";
            case "Unknown":
            default:
                return string.Empty;
        }
    }


    public LocalizationEnum SelectedLang()
    {
        for (int i = 0; i < lang.Count; i++)
        {
            if (lang[i].ToString() == selectedLang.ToString())
            {
                return (LocalizationEnum)(i + 1);
            }
        }
        return (LocalizationEnum)0;
    }
}

#if UNITY_EDITOR
static class LocalizationManagerIMGUIRegister
{
    public static bool foldout;
    [SettingsProvider]
    public static SettingsProvider LocalizationManagerProvider()
    {
        var provider = new SettingsProvider("Project/Hellish Battle/Localization", SettingsScope.Project)
        {
            label = "Localization",
            guiHandler = (searchContext) =>
            {
                TinyGUI.InfoBox("\nEnglish is the primary language and you don't need to add it as an additional language\n", "console.infoicon");
                var settings = LocalizationManager.GetSerializedSettings();


                EditorGUILayout.PropertyField(settings.FindProperty("selectedLang"), new GUIContent("Base Language"));

                TinyGUI.BeginBoxGroup("Available Language", "desc");
                LocalizationManager _lm = (LocalizationManager)AssetDatabase.LoadAssetAtPath("Assets/Classic FPS/Localization/Languages.asset", typeof(LocalizationManager));
                EditorGUILayout.BeginVertical("helpbox");
                for (int i = 0; i < settings.FindProperty("lang").arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(settings.FindProperty("lang").GetArrayElementAtIndex(i));
                    if (TinyGUI.IconButton("d_P4_DeletedLocal", "", GUILayout.Width(EditorGUIUtility.singleLineHeight*2)))
                    {
                        int index = i;
                        settings.FindProperty("lang").DeleteArrayElementAtIndex(index);
                        settings.ApplyModifiedProperties();
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (TinyGUI.IconButton("Toolbar Plus", $"Add New Localization", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
                {
                    //_lm.lang.Add(new SystemLanguage());
                    settings.FindProperty("lang").arraySize++;
                    settings.ApplyModifiedProperties();
                }
                EditorGUILayout.EndVertical();

                TinyGUI.EndBoxGroup();

                /// Generate Localization Enum
                EditorGUILayout.Space();
                if (TinyGUI.IconButton("Toolbar Plus", "Generate C# Enum", GUILayout.Height(EditorGUIUtility.singleLineHeight * 2)))
                {
                    List<string> enumEntries = new List<string>();
                    enumEntries.Add(SystemLanguage.English.ToString());
                    LocalizationManager tmp = Resources.Load<LocalizationManager>("Languages");
                    for (int i = 0; i < tmp.lang.Count; i++)
                    {
                        enumEntries.Add(tmp.lang[i].ToString());
                    }
                    string filePathAndName = "Assets/Classic FPS/Resources/LocalizationEnum.cs";

                    using (StreamWriter streamWriter = new StreamWriter(filePathAndName))
                    {
                        streamWriter.WriteLine("public enum LocalizationEnum");
                        streamWriter.WriteLine("{");
                        for (int i = 0; i < enumEntries.Count; i++)
                        {
                            streamWriter.WriteLine("\t" + enumEntries[i] + ",");
                        }
                        streamWriter.WriteLine("}");
                    }
                    AssetDatabase.Refresh();
                }

                settings.ApplyModifiedPropertiesWithoutUndo();
            },
            keywords = new HashSet<string>(new[] { "Language" })
        };

        return provider;
    }
}
class LocalizationManagerProvider : SettingsProvider
{
    private SerializedObject m_CustomSettings;

    class Styles
    {
        public static GUIContent lang = new GUIContent("lang");
        public static GUIContent selectedLang = new GUIContent("selectedLang");
    }

    const string k_LocalizationManagerPath = "Assets/Classic FPS/Resources/Languages.asset";
    public LocalizationManagerProvider(string path, SettingsScope scope = SettingsScope.User)
        : base(path, scope) { }

    public static bool IsSettingsAvailable()
    {
        return File.Exists(k_LocalizationManagerPath);
    }

    public override void OnActivate(string searchContext, VisualElement rootElement)
    {
        m_CustomSettings = LocalizationManager.GetSerializedSettings();
    }

    public override void OnGUI(string searchContext)
    {
        EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("lang"), Styles.lang);
        EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("selectedLang"), Styles.selectedLang);
    }
}
#endif

[System.Serializable]
public class StringLocalizationList
{
    public string key = "";
    public string englishText = "";
    public List<string> stringList = new List<string>();
}

[System.Serializable]
public class AudioLocalizationList
{
    public string key = "";
    public AudioClip englishText;
    public List<AudioClip> audioList = new List<AudioClip>();
}

[System.Serializable]
public class TextureLocalizationList
{
    public string key = "";
    public Texture2D englishText;
    public List<Texture2D> textureList = new List<Texture2D>();
}

[System.Serializable]
public class SpriteLocalizationList
{
    public string key = "";
    public Sprite englishText;
    public List<Sprite> spriteList = new List<Sprite>();
}

#if UNITY_EDITOR

#endif
