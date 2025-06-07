using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

using UnityEditorInternal;

public static class ReorderableListUtility
{
	public static ReorderableList GetListWithFoldout(SerializedObject serializedObject, SerializedProperty property, bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton)
	{
		var list = new ReorderableList(serializedObject, property, draggable, displayHeader, displayAddButton, displayRemoveButton);

		list.drawHeaderCallback = (Rect rect) => {
			var newRect = new Rect(rect.x + 10, rect.y, rect.width - 10, rect.height);
			property.isExpanded = EditorGUI.Foldout(newRect, property.isExpanded, property.displayName);
		};
		list.drawElementCallback =
			(Rect rect, int index, bool isActive, bool isFocused) => {
				if (!property.isExpanded)
				{
					GUI.enabled = index == list.count;
					return;
				}

				var element = list.serializedProperty.GetArrayElementAtIndex(index);
				rect.y += 2;
				EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
			};
		list.elementHeightCallback = (int indexer) => {
			if (!property.isExpanded)
				return 0;
			else
				return list.elementHeight;
		};

		return list;
	}
}

#endif
