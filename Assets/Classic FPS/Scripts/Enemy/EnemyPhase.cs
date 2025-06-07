using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace HellishBattle.Enemies
{
    [RequireComponent(typeof(Enemy))]
    public class EnemyPhase : MonoBehaviour
    {
        Enemy _enemy;
        public EnemyPhaseClass current = new EnemyPhaseClass(100, 1, 1, 1);
        public List<EnemyPhaseClass> enemyPhase;

        private void Awake()
        {
            enemyPhase = enemyPhase.OrderByDescending(ch => ch.requirementHealth).ToList();
            _enemy = GetComponent<Enemy>();
        }

        private void Update()
        {
            SetPhase();
        }

        public void SetPhase()
        {
            current = new EnemyPhaseClass(100, 1, 1, 1);
            for (int i = 0; i < enemyPhase.Count; i++)
            {
                if (_enemy.health <= _enemy.maxHealth / 100 * enemyPhase[i].requirementHealth) { current = enemyPhase[i]; }
            }
        }
    }

    [System.Serializable]
    public class EnemyPhaseClass
    {
        [Range(0, 100)] public float requirementHealth = 100;
        [Range(1, 2.5f)] public float damageMultiplier = 1;
        [Range(1, 2.5f)] public float speedMultiplier = 1;
        [Range(0.5f, 1)] public float reduceDelayMultiplier = 1;

        public EnemyPhaseClass()
        {
            requirementHealth = 100;
            damageMultiplier = 1;
            speedMultiplier = 1;
            reduceDelayMultiplier = 1;
        }
        public EnemyPhaseClass(float requirement, float damage, float speed, float reduce)
        {
            requirementHealth = requirement;
            damageMultiplier = damage;
            speedMultiplier = speed;
            reduceDelayMultiplier = reduce;
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(EnemyPhase))]
    public class EnemyPhaseEditor : Editor
    {
        //private bool isEditing = false;

        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            EnemyPhase phase = (EnemyPhase)target;

            //DrawDefaultInspector();
            if (phase.enemyPhase.Count > 0)
            {
                if (TinyGUI.IconButton("d_DefaultSorting", "Sort Phase", GUILayout.Height(30)))
                {
                    phase.enemyPhase = phase.enemyPhase.OrderByDescending(ch => ch.requirementHealth).ToList();
                }

                for (int i = 0; i < phase.enemyPhase.Count; i++)
                {
                    Rect r = EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
                    EditorGUILayout.LabelField($"{phase.name} Phase {i + 1}", EditorStyles.boldLabel);
                    if (GUILayout.Button("Delete", EditorStyles.toolbarButton, GUILayout.Width(75)))
                    {
                        phase.enemyPhase.Remove(phase.enemyPhase[i]);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.ProgressBar(new Rect(r.x + 2, r.y + 25, r.width - 5, 30), phase.enemyPhase[i].requirementHealth / 100, $"Health {phase.enemyPhase[i].requirementHealth}% or less");
                    EditorGUILayout.Space(32);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyPhase").GetArrayElementAtIndex(i).FindPropertyRelative("requirementHealth"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyPhase").GetArrayElementAtIndex(i).FindPropertyRelative("damageMultiplier"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyPhase").GetArrayElementAtIndex(i).FindPropertyRelative("speedMultiplier"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyPhase").GetArrayElementAtIndex(i).FindPropertyRelative("reduceDelayMultiplier"));

                    EditorGUILayout.EndVertical();
                }
            }

            if (TinyGUI.IconButton("Toolbar Plus", "Add New Phase", GUILayout.Height(30)))
            {
                phase.enemyPhase.Add(new EnemyPhaseClass());
                phase.enemyPhase[phase.enemyPhase.Count - 1].requirementHealth = 100;
                phase.enemyPhase[phase.enemyPhase.Count - 1].damageMultiplier = 1;
                phase.enemyPhase[phase.enemyPhase.Count - 1].speedMultiplier = 1;
                phase.enemyPhase[phase.enemyPhase.Count - 1].reduceDelayMultiplier = 1;
            }


            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}