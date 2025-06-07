using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

public class RebindAction : MonoBehaviour
{
    public InputActionReference m_Action;
    public string base_BindingId;
    public string alternative_BindingId;
    public TMP_Text BaseBindText, AlternativeBindText;

}

#if UNITY_EDITOR
[CustomEditor(typeof(RebindAction))]
public class RebindActionEditor : Editor
{
    protected void OnEnable()
    {
        Action = serializedObject.FindProperty("m_Action");
        BaseBinding = serializedObject.FindProperty("base_BindingId");
        AlternativeBinding = serializedObject.FindProperty("alternative_BindingId");
        BaseBindingText = serializedObject.FindProperty("BaseBindText");
        AlternativeBindingText = serializedObject.FindProperty("AlternativeBindText");

        RefreshBaseBinding();
        RefreshAlternativeBinding();
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(Action);
        // Base
        var newSelectedBinding = EditorGUILayout.Popup(BaseBindingLabel, BaseBindingSelected, BaseBindingOption);
        if (newSelectedBinding != BaseBindingSelected)
        {
            var bindingId = BaseBindingOptionValue[newSelectedBinding];
            BaseBinding.stringValue = bindingId;
            BaseBindingSelected = newSelectedBinding;
        }
        // Alternative
        var AlternativeSelectedBinding = EditorGUILayout.Popup(AlternativeBindingLabel, AlternativeBindingSelected, AlternativeBindingOption);
        if (AlternativeSelectedBinding != AlternativeBindingSelected)
        {
            var bindingId = AlternativeBindingOptionValue[AlternativeSelectedBinding];
            AlternativeBinding.stringValue = bindingId;
            AlternativeBindingSelected = AlternativeSelectedBinding;
        }

        EditorGUILayout.PropertyField(BaseBindingText);
        EditorGUILayout.PropertyField(AlternativeBindingText);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            RefreshBaseBinding();
            RefreshAlternativeBinding();
        }
    }
    protected void RefreshBaseBinding()
    {
        var actionReference = (InputActionReference)Action.objectReferenceValue;
        var action = actionReference?.action;

        if (action == null)
        {
            BaseBindingOption = new GUIContent[0];
            BaseBindingOptionValue = new string[0];
            BaseBindingSelected = -1;
            return;
        }

        var bindings = action.bindings;
        var bindingCount = bindings.Count;

        BaseBindingOption = new GUIContent[bindingCount];
        BaseBindingOptionValue = new string[bindingCount];
        BaseBindingSelected = -1;

        var currentBindingId = BaseBinding.stringValue;
        for (var i = 0; i < bindingCount; ++i)
        {
            var binding = bindings[i];
            var bindingId = binding.id.ToString();
            var haveBindingGroups = !string.IsNullOrEmpty(binding.groups);

            var displayOptions =
                InputBinding.DisplayStringOptions.DontUseShortDisplayNames | InputBinding.DisplayStringOptions.IgnoreBindingOverrides;
            if (!haveBindingGroups)
                displayOptions |= InputBinding.DisplayStringOptions.DontOmitDevice;

            var displayString = action.GetBindingDisplayString(i, displayOptions);

            if (binding.isPartOfComposite)
                displayString = $"{ObjectNames.NicifyVariableName(binding.name)}: {displayString}";

            displayString = displayString.Replace('/', '\\');

            if (haveBindingGroups)
            {
                var asset = action.actionMap?.asset;
                if (asset != null)
                {
                    var controlSchemes = string.Join(", ",
                        binding.groups.Split(InputBinding.Separator)
                            .Select(x => asset.controlSchemes.FirstOrDefault(c => c.bindingGroup == x).name));

                    displayString = $"{displayString} ({controlSchemes})";
                }
            }

            BaseBindingOption[i] = new GUIContent(displayString);
            BaseBindingOptionValue[i] = bindingId;

            if (currentBindingId == bindingId)
                BaseBindingSelected = i;
        }
    }
    protected void RefreshAlternativeBinding()
    {
        var actionReference = (InputActionReference)Action.objectReferenceValue;
        var action = actionReference?.action;

        if (action == null)
        {
            AlternativeBindingOption = new GUIContent[0];
            AlternativeBindingOptionValue = new string[0];
            AlternativeBindingSelected = -1;
            return;
        }

        var bindings = action.bindings;
        var bindingCount = bindings.Count;

        AlternativeBindingOption = new GUIContent[bindingCount];
        AlternativeBindingOptionValue = new string[bindingCount];
        AlternativeBindingSelected = -1;

        var currentBindingId = AlternativeBinding.stringValue;
        for (var i = 0; i < bindingCount; ++i)
        {
            var binding = bindings[i];
            var bindingId = binding.id.ToString();
            var haveBindingGroups = !string.IsNullOrEmpty(binding.groups);

            var displayOptions =
                InputBinding.DisplayStringOptions.DontUseShortDisplayNames | InputBinding.DisplayStringOptions.IgnoreBindingOverrides;
            if (!haveBindingGroups)
                displayOptions |= InputBinding.DisplayStringOptions.DontOmitDevice;

            var displayString = action.GetBindingDisplayString(i, displayOptions);

            if (binding.isPartOfComposite)
                displayString = $"{ObjectNames.NicifyVariableName(binding.name)}: {displayString}";

            displayString = displayString.Replace('/', '\\');

            if (haveBindingGroups)
            {
                var asset = action.actionMap?.asset;
                if (asset != null)
                {
                    var controlSchemes = string.Join(", ",
                        binding.groups.Split(InputBinding.Separator)
                            .Select(x => asset.controlSchemes.FirstOrDefault(c => c.bindingGroup == x).name));

                    displayString = $"{displayString} ({controlSchemes})";
                }
            }

            AlternativeBindingOption[i] = new GUIContent(displayString);
            AlternativeBindingOptionValue[i] = bindingId;

            if (currentBindingId == bindingId)
                AlternativeBindingSelected = i;
        }
    }


    private SerializedProperty Action;
    private SerializedProperty BaseBinding;
    private SerializedProperty AlternativeBinding;
    private SerializedProperty BaseBindingText;
    private SerializedProperty AlternativeBindingText;

    private GUIContent BaseBindingLabel = new GUIContent("Base Binding");
    private GUIContent[] BaseBindingOption;
    private string[] BaseBindingOptionValue;
    private int BaseBindingSelected;
    //
    private GUIContent AlternativeBindingLabel = new GUIContent("Alternative Binding");
    private GUIContent[] AlternativeBindingOption;
    private string[] AlternativeBindingOptionValue;
    private int AlternativeBindingSelected;
}
#endif