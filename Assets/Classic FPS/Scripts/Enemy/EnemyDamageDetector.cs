using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HellishBattle.Enemies
{
    public class EnemyDamageDetector : MonoBehaviour
    {
        public Enemy enemy;
        public DamageType damageType;
        [Suffix("x Damage")] public float DamageMultiplier = 1f;

        public void GetDamage(DamageClass damage)
        {
            if (damage != null)
            {
                damage.Damage *= DamageMultiplier;
                damage.Type = damageType;

                if (enemy != null) enemy.GetDamage(damage);
            }
            else
            {
                enemy.GetDamage(new DamageClass(0, DamageType.Normal));
            }
        }
    }

    /// Custom Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(EnemyDamageDetector))]
    [CanEditMultipleObjects]
    public class EnemyDamageDetectorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            EnemyDamageDetector enemy = (EnemyDamageDetector)target;
            EditorGUI.indentLevel = 0;

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((EnemyDamageDetector)target), typeof(EnemyDamageDetector), false);
            GUI.enabled = true;

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Damage Settings", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            if (enemy.enemy == null)
                EditorGUILayout.HelpBox("Enemy scripts not assigned", MessageType.Error);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("enemy"), new GUIContent("Enemy Script"));

            if (enemy.damageType != DamageType.Normal && enemy.damageType != DamageType.Critical)
                EditorGUILayout.HelpBox("Damage Type should be either Normal or Critical\n\nFall Damge and Explosion are the incorrect Damage Types at this time.", MessageType.Error);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("damageType"), new GUIContent("Damage Type"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DamageMultiplier"), new GUIContent("Damage Multiplier"));

            EditorGUILayout.EndVertical();

            EditorGUILayout.HelpBox($"If the basic damage is: 100\nIf this collider is hit, the opponent will receive {100 * enemy.DamageMultiplier} damage", MessageType.None);

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

    public enum DamageType
    {
        // Base
        Normal = 0, Critical = 1, Explosion = 2, FallDamage = 3,
        // Elementars
        Fire = 100, Poison = 101, Electricity = 102,
        // Others
        Laser = 200,

    }
}