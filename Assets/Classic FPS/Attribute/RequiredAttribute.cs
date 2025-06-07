using System;
using UnityEditor;
using UnityEngine;

namespace HellishBattle
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class RequiredAttribute : PropertyAttribute
    {
        public string Message;

        public RequiredAttribute(string message = "This field is required.")
        {
            this.Message = message;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RequiredAttribute requiredAttribute = (RequiredAttribute)attribute;

            bool isEmpty = false;

            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    isEmpty = property.objectReferenceValue == null;
                    break;
                case SerializedPropertyType.String:
                    isEmpty = string.IsNullOrEmpty(property.stringValue);
                    break;
                case SerializedPropertyType.Integer:
                    isEmpty = property.intValue == 0;
                    break;
                case SerializedPropertyType.Float:
                    isEmpty = property.floatValue == 0f;
                    break;
                case SerializedPropertyType.Boolean:
                    isEmpty = !property.boolValue;
                    break;
                case SerializedPropertyType.Vector2:
                    isEmpty = property.vector2Value == Vector2.zero;
                    break;
                case SerializedPropertyType.Vector3:
                    isEmpty = property.vector3Value == Vector3.zero;
                    break;
                case SerializedPropertyType.Color:
                    isEmpty = property.colorValue == Color.clear;
                    break;
                // Add other property types as needed
                default:
                    isEmpty = false;
                    break;
            }

            if (isEmpty)
            {
                Rect helpBoxPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight * 2);
                EditorGUI.HelpBox(helpBoxPosition, requiredAttribute.Message, MessageType.Error);
                position.y += EditorGUIUtility.singleLineHeight * 2;
            }

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUI.GetPropertyHeight(property, label, true);

            bool isEmpty = false;

            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    isEmpty = property.objectReferenceValue == null;
                    break;
                case SerializedPropertyType.String:
                    isEmpty = string.IsNullOrEmpty(property.stringValue);
                    break;
                case SerializedPropertyType.Integer:
                    isEmpty = property.intValue == 0;
                    break;
                case SerializedPropertyType.Float:
                    isEmpty = property.floatValue == 0f;
                    break;
                case SerializedPropertyType.Boolean:
                    isEmpty = !property.boolValue;
                    break;
                case SerializedPropertyType.Vector2:
                    isEmpty = property.vector2Value == Vector2.zero;
                    break;
                case SerializedPropertyType.Vector3:
                    isEmpty = property.vector3Value == Vector3.zero;
                    break;
                case SerializedPropertyType.Color:
                    isEmpty = property.colorValue == Color.clear;
                    break;
                // Add other property types as needed
                default:
                    isEmpty = false;
                    break;
            }

            if (isEmpty)
            {
                height += EditorGUIUtility.singleLineHeight * 2;
            }

            return height;
        }
    }
#endif
}
