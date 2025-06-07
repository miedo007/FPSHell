using System;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)] [Conditional("UNITY_EDITOR")]
public class SuffixAttribute : PropertyAttribute
{
    public SuffixAttribute(string suffixLabel)
    {
        SuffixLabel = suffixLabel;
    }

    public string SuffixLabel { get; private set; }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(SuffixAttribute))]
public class SuffixAttributeDrawer : PropertyDrawer
{
    protected void OnGUISafe(Rect position, SerializedProperty property, GUIContent label)
    {
        //set up needed fields
        var suffixLabel = new GUIContent(Attribute.SuffixLabel);
        var suffixStyle = new GUIStyle(EditorStyles.miniLabel);
        var suffixWidth = suffixStyle.CalcSize(suffixLabel).x;

        //position.xMax -= suffixWidth;
        //draw standard property field
        EditorGUI.PropertyField(position, property, label, property.isExpanded);

        position.xMin = position.xMax - suffixWidth - 3;
        position.xMax += suffixWidth;
        //draw suffix label
        EditorGUI.LabelField(position, suffixLabel, suffixStyle);
    }

    protected virtual float GetPropertyHeightSafe(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label);
    }

    public override sealed float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return IsPropertyValid(property)
            ? GetPropertyHeightSafe(property, label)
            : base.GetPropertyHeight(property, label);
    }

    public override sealed void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (IsPropertyValid(property))
        {
            OnGUISafe(position, property, label);
            return;
        }

        var warningContent = new GUIContent(property.displayName + " has invalid property drawer");
    }

    public virtual bool IsPropertyValid(SerializedProperty property)
    {
        return true;
    }


    private SuffixAttribute Attribute => attribute as SuffixAttribute;
}

#endif
