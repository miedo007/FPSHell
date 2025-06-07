using System;
using UnityEngine;
using UnityEditor;


[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public class LineAttribute : PropertyAttribute
{
    public const VirtualColor DefaultColor = VirtualColor.Theme;

    public float Height { get; set; }
    public VirtualColor Color { get; private set; }
    public string[] Label { get; set; }

    public LineAttribute(VirtualColor color = DefaultColor, params string[] label)
    {
        Color = color;
        Label = label;
    }
    public LineAttribute(params string[] lines)
    {
        Color = DefaultColor;
        Label = lines;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(LineAttribute))]
public class LineAttributeDecoratorDrawer : DecoratorDrawer
{
    public override float GetHeight()
    {
        LineAttribute lineAttr = (LineAttribute)attribute;
#if UNITY_2023_1_OR_NEWER
        return EditorGUIUtility.singleLineHeight + lineAttr.Height + EditorGUIUtility.singleLineHeight;
#endif
        return EditorGUIUtility.singleLineHeight + lineAttr.Height;
    }

    public override void OnGUI(Rect position)
    {
        Rect rect = EditorGUI.IndentedRect(position);
        rect.y += EditorGUIUtility.singleLineHeight / 2.0f;
        LineAttribute lineAttr = (LineAttribute)attribute;
        rect.height = 5;

        for (int i = 0; i < lineAttr.Label.Length; i++) { rect.height += 19; }
        lineAttr.Height = rect.height;

        //rect.height = lineAttr.Height;
        EditorGUI.DrawRect(rect, lineAttr.Color.GetBackgroundColor());

        GUIStyle style = new GUIStyle();
        style.normal.textColor = lineAttr.Color.GetFontColor();
        style.fontStyle = FontStyle.Bold;
        style.fontSize = 12;
        style.richText = true;
        for (int i = 0; i < lineAttr.Label.Length; i++) { EditorGUI.LabelField(new Rect(rect.x + 5, rect.y + 5 + (17 * i), 12, rect.height), lineAttr.Label[i], style); }


    }
}
#endif
public enum VirtualColor
{
    Theme,
    Black,
    Gray,
    Red,
    DarkRed,
    Pink,
    Orange,
    Yellow,
    Lime,
    Green,
    SeaGreen,
    Blue,
    DarkBlue,
    Purple,
}

public static class VirtualColorExtensions
{
    public static Color GetBackgroundColor(this VirtualColor color)
    {
#if UNITY_EDITOR
        switch (color)
        {

            case VirtualColor.Black: return new Color32(0, 0, 0, 255);
            case VirtualColor.Gray: return new Color32(128, 128, 128, 255);
            case VirtualColor.Red: return new Color32(255, 0, 0, 255);
            case VirtualColor.DarkRed: return new Color32(139, 0, 0, 255);
            case VirtualColor.Pink: return new Color32(255, 20, 147, 255);
            case VirtualColor.Orange: return new Color32(255, 140, 0, 255);
            case VirtualColor.Yellow: return new Color32(255, 255, 0, 255);
            case VirtualColor.Lime: return new Color32(173, 255, 47, 255);
            case VirtualColor.Green: return new Color32(0, 255, 0, 255);
            case VirtualColor.SeaGreen: return new Color32(0, 255, 127, 255);
            case VirtualColor.Blue: return new Color32(0, 206, 209, 255);
            case VirtualColor.DarkBlue: return new Color32(0, 0, 139, 255);
            case VirtualColor.Purple: return new Color32(148, 0, 211, 255);

            default: if (EditorGUIUtility.isProSkin) { return new Color32(40, 40, 40, 255); } else { return new Color32(200, 200, 200, 255); }
        }
#else
            return new Color32(0,0,0,0);
#endif
    }

    public static Color GetFontColor(this VirtualColor color)
    {
#if UNITY_EDITOR
        switch (color)
        {
            case VirtualColor.Black: return new Color32(255, 255, 255, 255);
            case VirtualColor.Gray: return new Color32(255, 255, 255, 255);
            case VirtualColor.Red: return new Color32(255, 255, 255, 255);
            case VirtualColor.DarkRed: return new Color32(255, 255, 255, 255);
            case VirtualColor.Pink: return new Color32(255, 255, 255, 255);
            case VirtualColor.Orange: return new Color32(0, 0, 0, 255);
            case VirtualColor.Yellow: return new Color32(0, 0, 0, 255);
            case VirtualColor.Lime: return new Color32(0, 0, 0, 255);
            case VirtualColor.Green: return new Color32(0, 0, 0, 255);
            case VirtualColor.SeaGreen: return new Color32(0, 0, 0, 255);
            case VirtualColor.Blue: return new Color32(255, 255, 255, 255);
            case VirtualColor.DarkBlue: return new Color32(255, 255, 255, 255);
            case VirtualColor.Purple: return new Color32(255, 255, 255, 255);

            default: if (EditorGUIUtility.isProSkin) { return new Color32(196, 196, 196, 255); } else { return new Color32(9, 9, 9, 255); }
        }
#else
            return new Color32(0,0,0,0);
#endif

    }
}