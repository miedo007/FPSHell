using UnityEngine;
using System;
using Object = UnityEngine.Object;

public static class TinyDebug
{
    private static void DoLog(Action<string, Object> LogFunction, string prefix, Object component, params object[] message)
    {
#if UNITY_EDITOR
        LogFunction(string.Format($"{prefix} {String.Join(" | ", message)}", component.name, component.GetType().Name), component);
#endif
    }

    public static void Info(params object[] message)
    {
        Object component = FindCallingObject();
        if (component != null) DoLog(Debug.Log, "♦️ <b>[{0}]</b> {1}.cs:", component, message);
        else Debug.Log(String.Join(" | ", message));
    }

    public static void Warning(params object[] message)
    {
        Object component = FindCallingObject();
        if (component != null) DoLog(Debug.LogWarning, "<color=#FDCA40>⚠️ <b>[{0}]</b> {1}.cs:</color>", component, message);
        else Debug.LogWarning(String.Join(" | ", message));
    }

    public static void Error(params object[] message)
    {
        Object component = FindCallingObject();
        if (component != null) DoLog(Debug.LogError, "<color=#DF2935>‼️ <b>[{0}]</b> {1}.cs:</color> ", component, message);
        else Debug.LogError(String.Join(" | ", message));
    }

    public static void Test(params object[] message)
    {
        Object component = FindCallingObject();
        if (component != null) DoLog(Debug.Log, "<color=#947BD3>❇️ <b>[{0}]</b> {1}.cs:</color>" + component.GetType().Name, component, message);
        else Debug.Log(String.Join(" | ", message));
    }

    public static void Success(params object[] message)
    {
        Object component = FindCallingObject();
        if (component != null) DoLog(Debug.Log, "<color=lime>♦️ <b>[{0}]</b> {1}.cs:</color> ", component, message);
        else Debug.Log(String.Join(" | ", message));
    }

    private static Object FindCallingObject()
    {
        var stackTrace = new System.Diagnostics.StackTrace();
        var frames = stackTrace.GetFrames();

        if (frames == null)
        {
            return null;
        }

        foreach (var frame in frames)
        {
            var method = frame.GetMethod();
            var declaringType = method.DeclaringType;

            if (declaringType != null && typeof(MonoBehaviour).IsAssignableFrom(declaringType))
            {
                var component = (MonoBehaviour)Object.FindObjectOfType(declaringType);
                if (component != null)
                {
                    return component;
                }
            }
        }

        return null;
    }
}

