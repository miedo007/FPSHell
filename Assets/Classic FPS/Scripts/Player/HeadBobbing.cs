using UnityEngine;
using UnityEditor;
using System.Collections;

namespace HellishBattle.Player
{
    public class HeadBobbing : MonoBehaviour
    {
        TinyInput input;

        public float bobbingSpeed = 0.015f;
        public float bobbingHeight = 0.015f;

        public float crouchBobbingSpeed = 0.01f;
        public float crouchBobbingHeight = 0.01f;

        public float sprintBobbingSpeed = 0.025f;
        public float sprintBobbingHeight = 0.02f;

        public float midpoint = 0.75f;
        public bool isHeadBobbing = true;

        private float timer = 0.0f;

        private void Awake()
        {
            input = InputManager.Instance.input;
        }

        void Update()
        {
            float speed = 0;
            float height = 0;

            //if(input.Player.Sprint.ReadValue<float>() == 0) { speed = bobbingSpeed; height = bobbingHeight; }
            //else { speed = sprintBobbingSpeed; height = sprintBobbingHeight; }
            MovementState state = Camera.main.transform.parent.GetComponent<PlayerMovement>().state;

            if (state == MovementState.Sprinting) { speed = sprintBobbingSpeed; height = sprintBobbingHeight; }
            else if (state == MovementState.Crouch) { speed = crouchBobbingSpeed; height = crouchBobbingHeight; }
            else { speed = bobbingSpeed; height = bobbingHeight; }

            float waveslice = 0.0f;
            float horizontal = input.Player.Move.ReadValue<Vector2>().x;
            float vertical = input.Player.Move.ReadValue<Vector2>().y;

            Vector3 cSharpConversion = transform.localPosition;

            if (Mathf.Abs(horizontal) == 0 && Mathf.Abs(vertical) == 0)
            {
                timer = 0.0f;
            }
            else if (!transform.parent.GetComponent<PlayerMovement>().cc.isGrounded)
            {
                timer = 0.0f;
            }
            else if (Cursor.lockState == CursorLockMode.Locked)
            {
                waveslice = Mathf.Sin(timer);
                timer = timer + speed;
                if (timer > Mathf.PI * 2)
                {
                    timer = timer - (Mathf.PI * 2);
                }
            }
            if (waveslice != 0)
            {
                float translateChange = waveslice * height;
                float totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
                totalAxes = Mathf.Clamp(totalAxes, 0.0f, 1.0f);
                translateChange = totalAxes * translateChange;
                if (isHeadBobbing == true)
                    cSharpConversion.y = midpoint + translateChange;
                else if (isHeadBobbing == false)
                    cSharpConversion.x = translateChange;
            }
            else
            {
                if (isHeadBobbing == true)
                    cSharpConversion.y = midpoint;
                else if (isHeadBobbing == false)
                    cSharpConversion.x = 0;
            }

            transform.localPosition = cSharpConversion;
        }



    }

    /// Custom Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(HeadBobbing))]
    public class HeadBobbingEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            HeadBobbing bibbing = (HeadBobbing)target;
            EditorGUI.indentLevel = 0;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Base Settings", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Is Head", GUILayout.Width(55));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isHeadBobbing"), GUIContent.none, GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("midpoint"), new GUIContent("Midpoint"));

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Walk", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("bobbingSpeed"), new GUIContent("Bobbing Speed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bobbingHeight"), new GUIContent("Bobbing Height"));

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Sprint", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("sprintBobbingSpeed"), new GUIContent("Bobbing Speed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sprintBobbingHeight"), new GUIContent("Bobbing Height"));

            EditorGUILayout.EndVertical();


            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}
