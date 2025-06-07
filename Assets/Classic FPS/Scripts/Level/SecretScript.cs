using UnityEditor;
using UnityEngine;

namespace HellishBattle.Level
{
    public class SecretScript : MonoBehaviour
    {
        public LocalizedString Title;
        public LocalizedString Description;
        [HideInInspector] public int secret_id;

        bool find = false;

        private void Awake()
        {
            secret_id = Random.Range(0, 999999999);
        }

        private void Start()
        {
            LevelManager.Instance.SecretRoom.Add(this);
            LevelManager.Instance.FindSecret.Add(false);
        }

        public void FindSecret()
        {
            if (!find)
            {
                LevelManager.Instance.TryFindSecret(this);
                GameManager.Instance.PanelSetup(Title.GetLocalization(), Description.GetLocalization());
                find = true;
            }
        }
    }

    /// Custom Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(SecretScript))]
    public class SecretScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            SecretScript secret = (SecretScript)target;
            EditorGUI.indentLevel = 0;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Secret Script", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Title"), new GUIContent("Title"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Description"), new GUIContent("Description"));

            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"How to Use", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel = 1;

            GUIStyle myCustomStyle = new GUIStyle(GUI.skin.GetStyle("label"))
            {
                wordWrap = true
            };

            EditorGUILayout.LabelField("1. Open the script that you want to call the secret (such as Door Script)", myCustomStyle);
            EditorGUILayout.LabelField("2. Find the UnityEvents()field in the script", myCustomStyle);
            EditorGUILayout.LabelField("3. Create a new field in it and drag the SecretScript/n into it", myCustomStyle);
            EditorGUILayout.LabelField("4. Then in the drop-down menu eat SecretScript and in it the FindSecret() function.", myCustomStyle);
            EditorGUILayout.LabelField("5. Done now you have a working secret detection", myCustomStyle);

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}