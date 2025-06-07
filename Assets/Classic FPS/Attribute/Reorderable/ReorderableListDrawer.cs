using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

public class ReorderableListBase { }

[System.Serializable]
public class ReorderableList<T> : ReorderableListBase
{
    public List<T> List;
}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReorderableListBase), true)]
public class ReorderableListDrawer : PropertyDrawer
{
    private ReorderableList _list;

    private ReorderableList GetReorderableList(SerializedProperty property)
    {
        if (_list == null)
        {
            var listProperty = property.FindPropertyRelative("List");

            _list = new ReorderableList(property.serializedObject, listProperty, true, true, true, true);

            _list.drawHeaderCallback += delegate (Rect rect)
            {
                EditorGUI.LabelField(rect, property.displayName);
            };

            _list.drawElementCallback = delegate (Rect rect, int index, bool isActive, bool isFocused)
            {
                EditorGUI.PropertyField(rect, listProperty.GetArrayElementAtIndex(index), true);
            };
        }

        return _list;
    }


    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return GetReorderableList(property).GetHeight();
    }


    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var list = GetReorderableList(property);

        var listProperty = property.FindPropertyRelative("List");
        var height = 0f;
        for (var i = 0; i < listProperty.arraySize; i++)
        {
            height = Mathf.Max(height, EditorGUI.GetPropertyHeight(listProperty.GetArrayElementAtIndex(i)));
        }

        list.elementHeight = height;
        list.DoList(position);
    }
}

#endif
