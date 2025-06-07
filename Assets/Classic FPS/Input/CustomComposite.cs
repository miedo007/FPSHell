using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.InputSystem.Editor;
#endif

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
[DisplayStringFormat("{multiplier}*{stick}")]
public class CustomComposite : InputBindingComposite<Vector2>
{
#if UNITY_EDITOR
    static CustomComposite()
    {
        Initialize();
    }

#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        InputSystem.RegisterBindingComposite<CustomComposite>();
    }

    [InputControl(layout = "Axis")]
    public int multiplier;

    [InputControl(layout = "Vector2")]
    public int stick;

    public float scaleFactor = 1;

    public override Vector2 ReadValue(ref InputBindingCompositeContext context)
    {
        var stickValue = context.ReadValue<Vector2, Vector2MagnitudeComparer>(stick);
        var multiplierValue = context.ReadValue<float>(multiplier);

        return stickValue * (multiplierValue * scaleFactor);
    }
}

#if UNITY_EDITOR
public class CustomCompositeEditor : InputParameterEditor<CustomComposite>
{
    public override void OnGUI()
    {
        var currentValue = target.scaleFactor;
        target.scaleFactor = EditorGUILayout.Slider(m_ScaleFactorLabel, currentValue, 0, 2);
    }

    private GUIContent m_ScaleFactorLabel = new GUIContent("Scale Factor");
}
#endif
