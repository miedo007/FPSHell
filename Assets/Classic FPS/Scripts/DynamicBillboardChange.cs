using UnityEngine;
using UnityEditor;
using HellishBattle.Enemies;

namespace HellishBattle
{
    public class DynamicBillboardChange : MonoBehaviour
    {

        public Sprite[] sprites;
        public string[] animStates = new string[4] { "Forward", "Backward", "Right", "Left" };
        public string deathAnim = "Death";
        public Sprite deathSprite;
        public Enemy enemy;
        public bool isAnimated;

        Animator anim;
        SpriteRenderer sr;

        private void Awake()
        {
            anim = GetComponent<Animator>();
            sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            GetAngle();
        }

        void GetAngle()
        {
            if (enemy != null && enemy.health <= 0)
            {
                //ChangeSprite(-1);
            }
            else
            {
                Vector3 playerDir = Camera.main.transform.forward;
                playerDir.y = 0;
                Vector3 enemyDir = transform.Find("Vision").forward;
                enemyDir.y = 0;

                float dotProduct = Vector3.Dot(playerDir, enemyDir);

                if (dotProduct < -0.5f && dotProduct >= -1.0f)
                    ChangeSprite(0);
                else if (dotProduct > 0.5f && dotProduct <= 1.0f)
                    ChangeSprite(1);
                else
                {
                    Vector3 playerRight = Camera.main.transform.right;
                    playerRight.y = 0;
                    dotProduct = Vector3.Dot(playerRight, enemyDir);
                    if (dotProduct >= 0)
                        ChangeSprite(2);
                    else
                        ChangeSprite(3);
                }
            }
        }

        void ChangeSprite(int index)
        {
            if (index < 0)
            {
                if (isAnimated)
                    anim.Play(deathAnim);
                else
                    sr.sprite = deathSprite;
            }
            if (index >= 0)
            {
                if (isAnimated)
                    anim.Play(animStates[index]);
                else
                    sr.sprite = sprites[index];
            }
        }
    }


    /// Custom Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(DynamicBillboardChange))]
    public class DynamicBillboardEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            DynamicBillboardChange tiles = (DynamicBillboardChange)target;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Enemy Billboarding", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);


            EditorGUILayout.PropertyField(serializedObject.FindProperty("enemy"), new GUIContent("Enemy Script"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isAnimated"), new GUIContent("Animation?"));

            if (tiles.isAnimated)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
                EditorGUILayout.LabelField($"Animation", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(4);
                if (tiles.animStates.Length != 4) { tiles.animStates = new string[4]; }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("animStates").GetArrayElementAtIndex(0), new GUIContent("Front"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("animStates").GetArrayElementAtIndex(2), new GUIContent("Right"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("animStates").GetArrayElementAtIndex(1), new GUIContent("Back"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("animStates").GetArrayElementAtIndex(3), new GUIContent("Left"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("deathAnim"), new GUIContent("Death Animation"));
            }
            else
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
                EditorGUILayout.LabelField($"Sprite", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(4);
                if (tiles.sprites.Length != 4) { tiles.sprites = new Sprite[4]; }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("sprites").GetArrayElementAtIndex(0), new GUIContent("Front"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("sprites").GetArrayElementAtIndex(2), new GUIContent("Right"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("sprites").GetArrayElementAtIndex(1), new GUIContent("Back"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("sprites").GetArrayElementAtIndex(3), new GUIContent("Left"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("deathSprite"), new GUIContent("Death Sprite"));
            }

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

}