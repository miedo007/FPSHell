using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

namespace HellishBattle.Interaction
{
    public class InGameButtonScript : MonoBehaviour
    {
        [Suffix("Second")] public float PressTime = .5f;
        public GameObject ButtonObj;    // Not Requirement
        public AudioClip ButtonFX;

        public UnityEvent ButtonDown;
        public UnityEvent ButtonUp;

        [Suffix("in Y Local Direction")] public float moveDown = 0;
        float _time = 0;
        AudioSource source;

        void MoveButtonObject(float Y)
        {
            if (ButtonObj != null)
            {
                Vector3 tmp = ButtonObj.transform.localPosition;
                //this.Log(ButtonObj.transform.localPosition.y, Y, (tmp.y + Y));
                ButtonObj.transform.localPosition = new Vector3(tmp.x, tmp.y + Y, tmp.z);
            }
        }

        private void Update()
        {
            _time += Time.deltaTime;
        }

        public void Click()
        {
            if (_time > PressTime) StartCoroutine(OnClick());

            source = GetComponent<AudioSource>();
            if (source != null && ButtonFX != null) { source.PlayOneShot(ButtonFX); }
        }

        IEnumerator OnClick()
        {
            _time = -0.1f;

            ButtonDown.Invoke();
            if (moveDown != 0) MoveButtonObject(moveDown);

            yield return new WaitForSeconds(PressTime);

            ButtonUp.Invoke();
            if (moveDown != 0) MoveButtonObject(-moveDown);
        }
    }
    //}

#if UNITY_EDITOR

    [CustomEditor(typeof(InGameButtonScript)), CanEditMultipleObjects]
    public class InGameButtonScriptEditor : Editor
    {
        private bool FamilySwitch = true;

        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            InGameButtonScript safe = (InGameButtonScript)target;
            EditorGUI.indentLevel = 0;

            EditorGUILayout.BeginHorizontal("helpbox", GUILayout.Height(EditorGUIUtility.singleLineHeight + 6));
            EditorGUILayout.LabelField("In-Game Button Script", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.ObjectField(MonoScript.FromMonoBehaviour((InGameButtonScript)target), typeof(InGameButtonScript), false);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Base Settings", "Set Press Time and Sounds");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("PressTime"), new GUIContent("Press Time"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ButtonFX"), new GUIContent("Button Click Sound"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Button Object Settings", "Set Button Object and Move it");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ButtonObj"), new GUIContent("Button Object"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("moveDown"), new GUIContent("Object Move"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Unity Events", "Add Action on ButtonDown or ButtonUp");
            EditorGUILayout.EndVertical();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("ButtonDown"), new GUIContent("On Button Down (Instant)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ButtonUp"), new GUIContent("On Button Up (Delay)"));

        }
    }

#endif
}