using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

public static class TinyGUI
{
#if UNITY_EDITOR

    public static void DrawScript<T>(string name, UnityEngine.Object target)
    {
        EditorGUILayout.BeginHorizontal("helpbox", GUILayout.Height(EditorGUIUtility.singleLineHeight + 6));
        EditorGUILayout.LabelField(name, EditorStyles.boldLabel);

        GUI.enabled = false;

        if (target is MonoBehaviour monoBehaviourTarget)
        {
            MonoScript monoScript = MonoScript.FromMonoBehaviour(monoBehaviourTarget);
            EditorGUILayout.ObjectField(monoScript, typeof(T), false);
        }
        else
        {
            EditorGUILayout.ObjectField(target, typeof(T), false);
        }

        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }



    public static bool Title(string title, string desc)
    {
        bool tmp = false;

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField(new GUIContent(title), EditorStyles.boldLabel);
        if (desc != "") { EditorGUILayout.Space(-8); EditorGUILayout.LabelField(new GUIContent(desc), EditorStyles.wordWrappedMiniLabel); EditorGUILayout.Space(2); }
        EditorGUILayout.EndVertical();

        //EditorGUILayout.BeginVertical(GUILayout.Width(160));
        //EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        //EditorGUILayout.Space(5);
        //EditorGUILayout.LabelField(new GUIContent(title), EditorStyles.boldLabel);
        //if (desc != "") { EditorGUILayout.Space(-8); EditorGUILayout.LabelField(new GUIContent(desc), EditorStyles.wordWrappedMiniLabel); EditorGUILayout.Space(2); }
        return tmp;
    }

    public static bool Title(string title) { return Title(title, ""); }
    //public static bool Title(string title, string desc) { return Title(title, desc, ""); }



    public static void InfoBox(string text, string icon)
    {
        float textHeight = EditorStyles.wordWrappedMiniLabel.CalcHeight(new GUIContent(text), EditorGUIUtility.currentViewWidth - 30);
        textHeight = EditorStyles.wordWrappedMiniLabel.CalcHeight(new GUIContent(text), EditorGUIUtility.currentViewWidth - textHeight - 6);

        Texture2D originalIconTexture = EditorGUIUtility.IconContent(icon).image as Texture2D;
        int iconSize = Mathf.RoundToInt(textHeight) + 6;

        if (iconSize > EditorGUIUtility.singleLineHeight * 5) { iconSize = Mathf.RoundToInt(EditorGUIUtility.singleLineHeight * 5); }

        Texture2D scaledIconTexture = ScaleTexture(originalIconTexture, iconSize, iconSize);

        GUIContent iconContent = new GUIContent(scaledIconTexture);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginHorizontal("HelpBox", GUILayout.Width(iconSize), GUILayout.Height(iconSize));
        EditorGUILayout.LabelField(iconContent, GUILayout.Width(iconSize - 8), GUILayout.Height(iconSize - 8));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal("HelpBox", GUILayout.Height(textHeight));
        EditorGUILayout.LabelField(text, EditorStyles.wordWrappedMiniLabel, GUILayout.Height(textHeight));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndHorizontal();
    }

    public static void InfoBox(string text)
    {
        InfoBox(text, "console.infoicon");
    }

    public static bool IconButton(string iconName, string text, string tooltip, params GUILayoutOption[] options)
    {
        Texture icon = EditorGUIUtility.IconContent(iconName).image;
        GUIContent content = new GUIContent(text, icon, tooltip);

        return GUILayout.Button(content, options);
    }

    public static bool IconButton(string iconName, string text, params GUILayoutOption[] options)
    {
        Texture icon = EditorGUIUtility.IconContent(iconName).image;
        GUIContent content = new GUIContent(text, icon);

        return GUILayout.Button(content, options);
    }



    public static void EditorTitle(string Title, string Desc)
    {
        EditorGUILayout.BeginVertical("toolbar", GUILayout.Height(EditorGUIUtility.singleLineHeight));
        EditorGUILayout.LabelField(Title, EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();

        if (Desc != "")
        {
            EditorGUILayout.Space(-2);
            EditorGUILayout.BeginVertical("toolbar", GUILayout.Height(EditorGUIUtility.singleLineHeight));
            EditorGUILayout.LabelField(Desc, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
        }


        EditorGUILayout.Space(4);
    }

    public static void EditorTitle(string Title)
    {
        EditorTitle(Title, "");
    }



    public static void BeginBoxGroup(string Title, string Desc)
    {
        EditorGUILayout.BeginVertical("box");
        EditorTitle(Title, Desc);
    }

    public static void BeginBoxGroup(string Title)
    {
        BeginBoxGroup(Title, "");
    }

    public static void EndBoxGroup() { EditorGUILayout.EndVertical(); }



    /// <summary>
    /// This method assists in displaying serialized arrays in a foldable manner. It provides an interface for adding, removing, and editing array elements.
    /// </summary>
    /// <param name="serializedObject"></param>
    /// <param name="PropertyName"></param>
    /// <param name="objectName"></param>
    /// <param name="foldout"></param>
    /// <returns></returns>
    public static bool ShowArray(SerializedObject serializedObject, string PropertyName, string objectName, ref bool foldout)
    {
        EditorGUILayout.BeginVertical("HelpBox");

        SerializedProperty property = serializedObject.FindProperty(PropertyName);
        int count = property.arraySize;

        // Foldout
        Rect rect = EditorGUILayout.BeginVertical("HelpBox");
        EditorGUI.indentLevel++;
        GUIContent foldoutContent = new GUIContent($"{objectName} [{count}]");
        foldout = EditorGUILayout.Foldout(foldout, foldoutContent, true);
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        if (foldout)
        {
            EditorGUILayout.BeginVertical("HelpBox");

            // List
            for (int i = 0; i < count; i++)
            {
                int index = i;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i), GUIContent.none);
                if (IconButton("d_P4_DeletedLocal", "", GUILayout.Width(25), GUILayout.Height(EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(index)))))
                {
                    property.DeleteArrayElementAtIndex(index);
                    serializedObject.ApplyModifiedProperties();
                    break;
                }
                EditorGUILayout.EndHorizontal();
                    serializedObject.ApplyModifiedProperties();
            }


            EditorGUILayout.EndVertical(); // End of List Vertical

            // Button
            if (IconButton("Toolbar Plus", $"Add New {objectName}", GUILayout.Height(EditorGUIUtility.singleLineHeight*2)))
            {
                property.arraySize++;
                serializedObject.ApplyModifiedProperties();
            }

        }

        EditorGUILayout.EndVertical(); // End of Main Vertical

        return true;
    }

    public static bool FoldoutGroup(string Name, bool Bool)
    {
        EditorGUILayout.BeginVertical("HelpBox");
        EditorGUI.indentLevel++;
        GUIContent foldoutContent = new GUIContent(Name);
        Bool = EditorGUILayout.Foldout(Bool, foldoutContent, true);
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        return Bool;
    }

    public static bool FoldoutGroup(string Name, string Icon, bool Bool)
    {
        EditorGUILayout.BeginVertical("HelpBox");
        EditorGUI.indentLevel++;
        GUIContent foldoutContent = new GUIContent(Name, EditorGUIUtility.IconContent(Icon).image);
        Bool = EditorGUILayout.Foldout(Bool, foldoutContent, true);
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        return Bool;
    }



    public static void ProgressBar(float value, float max, string Name, int BarHeight, Color color)
    {
        // Base 36
        EditorGUILayout.BeginVertical("helpbox", GUILayout.Height(BarHeight));

        // Calculate Base 
        Rect BaseBarSize = EditorGUILayout.GetControlRect();
        BaseBarSize.y += 2;
        BaseBarSize.height = BarHeight - 10;

        // Draw Base Bar
        GUIContent iconContent = EditorGUIUtility.IconContent("IN BigTitle Post@2x");
        Texture2D iconTexture = iconContent.image as Texture2D;
        GUI.DrawTexture(BaseBarSize, iconTexture, ScaleMode.StretchToFill);
        Color tmp = EditorGUIUtility.isProSkin ? new Color(0.06f, 0.06f, 0.06f, .8f) : new Color(0.76f, 0.76f, 0.76f, 1f);
        EditorGUI.DrawRect(BaseBarSize, tmp);

        // Calculate Fill
        Rect FilledBarSize = BaseBarSize;
        float width = BaseBarSize.width * (value / max);
        FilledBarSize.width = width;

        // Fill Bar

        Color color1 = new Color(color.r, color.g, color.b, .35f);
        GUI.color = color1;
        GUI.DrawTexture(FilledBarSize, iconTexture, ScaleMode.StretchToFill);
        GUI.color = Color.white;

        // Draw Text
        GUIStyle centeredBoldLabel = new GUIStyle(EditorStyles.boldLabel);
        centeredBoldLabel.alignment = TextAnchor.MiddleCenter;
        centeredBoldLabel.normal.textColor = Color.white;
        //centeredBoldLabel.f
        EditorGUI.LabelField(BaseBarSize, new GUIContent($"{Name} [{value}/{max}] {value/max*100}%"), centeredBoldLabel);

        EditorGUILayout.EndVertical();
    }

    public static void ProgressBar(float value, float max, string Name, int BarHeight)
    {
        ProgressBar(value, max, Name, BarHeight, new Color(.04f, .3f, .57f));
    }

    public static void ProgressBar(float value, float max, string Name, Color color)
    {
        ProgressBar(value, max, Name, 36, color);
    }

    public static void ProgressBar(float value, float max, string Name)
    {
        ProgressBar(value, max, Name, 36, new Color(.04f, .3f, .57f));
    }

    public static int TabList(int ID, params string[] message)
    {
        EditorGUILayout.BeginHorizontal("helpbox");
        for(int i = 0; i < message.Length; i++)
        {
            int index = i;
            if(GUILayout.Button(message[i], EditorStyles.miniButton))
            {
                return index;
            }
        }
        EditorGUILayout.EndHorizontal();

        return ID;
    }


    public static Rect LocalizedString(LocalizedString obj, SerializedProperty property)
    {
        /* Load Icon */
        Texture2D iconTexture = EditorGUIUtility.Load("Assets/Classic FPS/Scripts/_Gizmo/LocalizationStringGizmo.png") as Texture2D;
        Texture2D scaledIconTexture = ScaleTexture(iconTexture, 50, 50);


        EditorGUILayout.BeginVertical("HelpBox");
        EditorGUILayout.BeginHorizontal();

        // Icon
        EditorGUILayout.BeginHorizontal("HelpBox", GUILayout.Width(44), GUILayout.Height(44));
        EditorGUILayout.LabelField(new GUIContent(scaledIconTexture), GUILayout.Width(44 - 8), GUILayout.Height(44 - 8));
        EditorGUILayout.EndHorizontal();

        //
        Rect rect = EditorGUILayout.BeginVertical("HelpBox");

        EditorGUILayout.PropertyField(property.FindPropertyRelative("localization"));

        if (obj.localization != null)
        {
            obj.localization.GetKeyOption();
            obj._SelectedKey = 0;
            for (int x = 0; x < obj.localization.keyOption.Count; x++)
            {
                if (obj.key == obj.localization.keyOption[x]) { obj._SelectedKey = x; }
            }
            EditorGUILayout.BeginHorizontal();
            obj._SelectedKey = EditorGUILayout.Popup($"{obj.localization.name} Key", obj._SelectedKey, obj.localization.keyOption.ToArray());
            EditorGUILayout.EndHorizontal();
            obj.key = obj.localization.wordList[obj._SelectedKey].key;
        }
        else
        {
            EditorGUILayout.LabelField("Setup Localization First!", EditorStyles.centeredGreyMiniLabel);
        }

        EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

        if (obj.localization != null)
        {
            EditorGUILayout.BeginVertical("HelpBox");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("English:",GUILayout.Width(100));
            EditorGUILayout.LabelField(obj.localization.GetValueDemo(obj.key, (int)SystemLanguage.English), EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.EndHorizontal();

            LocalizationManager _lm = (LocalizationManager)AssetDatabase.LoadAssetAtPath("Assets/Classic FPS/Localization/Languages.asset", typeof(LocalizationManager));
            for (int i = 0; i < _lm.lang.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{_lm.lang[i]}:", GUILayout.Width(100));
                EditorGUILayout.LabelField(obj.localization.GetValueDemo(obj.key, (int)_lm.lang[i]), EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }


        EditorGUILayout.EndVertical();
        return rect;
    }

    public static Rect Guideline(string Title, params string[] message)
    {
        Rect box = EditorGUILayout.BeginVertical("HelpBox");
        EditorGUILayout.LabelField(Title, EditorStyles.boldLabel);

        for (int i = 0; i < message.Length; i++)
        {
            EditorGUILayout.BeginHorizontal("HelpBox");
            EditorGUILayout.LabelField(new GUIContent(EditorGUIUtility.IconContent("DotFill")), GUILayout.Width(20));
            float height = EditorStyles.wordWrappedMiniLabel.CalcHeight(new GUIContent(message[i]), EditorGUIUtility.currentViewWidth - 90);
            EditorGUILayout.LabelField(new GUIContent(message[i]), EditorStyles.wordWrappedMiniLabel, GUILayout.Height(height));
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        return box;
    }


    private static Texture2D ScaleTexture(Texture2D source, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        rt.filterMode = FilterMode.Bilinear;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D result = new Texture2D(width, height);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }

    /*private static Color GetProgressBarColor()
    {

    }*/

#endif
}

public enum ProgressBarColors
{
    Defualt = 0,
    Red = 1,
    Orange = 2,
    Yellow = 3,
    Green = 4,
    LightBlue = 5,
    Blue = 6,
    Purple = 7,
    Violet = 8,
    Pink = 9
    
}