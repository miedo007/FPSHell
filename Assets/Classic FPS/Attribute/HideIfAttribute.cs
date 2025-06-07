using System.Reflection;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class HideIfAttribute : PropertyAttribute
{
    public int ID = 1;

    public string ConditionalSourceField = "";
    public int enumInt = 0;
    public bool HideInInspector = true;

    public HideIfAttribute(string conditionalSourceField)
    {
        ID = 1;
        ConditionalSourceField = conditionalSourceField;
        HideInInspector = true;
    }

    public HideIfAttribute(string conditionalSourceField, bool hideInInspector)
    {
        ID = 1;
        ConditionalSourceField = conditionalSourceField;
        HideInInspector = hideInInspector;
    }

    public HideIfAttribute(string conditionalSourceField, int intFromEnum)
    {
        ID = 2;
        ConditionalSourceField = conditionalSourceField;
        enumInt = intFromEnum;
    }

    public HideIfAttribute(string conditionalSourceField, int intFromEnum, bool hideInInspector)
    {
        ID = 2;
        ConditionalSourceField = conditionalSourceField;
        enumInt = intFromEnum;
        HideInInspector = hideInInspector;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(HideIfAttribute))]
public class HideIfPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        HideIfAttribute condHAtt = (HideIfAttribute)attribute;
        bool enabled = GetConditionalHideAttributeResult(condHAtt, property);

        bool wasEnabled = GUI.enabled;
        GUI.enabled = enabled;
        if (!condHAtt.HideInInspector || enabled)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }

        GUI.enabled = wasEnabled;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        HideIfAttribute condHAtt = (HideIfAttribute)attribute;
        bool enabled = GetConditionalHideAttributeResult(condHAtt, property);

        if (!condHAtt.HideInInspector || enabled)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
        else
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }

    private bool GetConditionalHideAttributeResult(HideIfAttribute condHAtt, SerializedProperty property)
    {
        bool enabled = true;
        string propertyPath = property.propertyPath; //returns the property path of the property we want to apply the attribute to
        string conditionPath = propertyPath.Replace(property.name, condHAtt.ConditionalSourceField); //changes the path to the conditionalsource property path
        SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

        if (sourcePropertyValue != null)
        {
            if (condHAtt.ID == 1) { enabled = sourcePropertyValue.boolValue; }
            if (condHAtt.ID == 2) { if (sourcePropertyValue.intValue != condHAtt.enumInt) { enabled = true; } else { enabled = false; } }

        }
        else
        {
            Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: " + condHAtt.ConditionalSourceField);
        }

        return enabled;
    }
}

#endif
