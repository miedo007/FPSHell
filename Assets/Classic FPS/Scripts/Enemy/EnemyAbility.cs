using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HellishBattle.Enemies
{
    public class EnemyAbility : MonoBehaviour
    {
        public bool EnableRegeneration;
        [Suffix("Secounds")] public float RegenerationInterval = 3;
        public float RegenerationHealth = 5;

        public bool EnableRage;
        public float RageHealthMaxLimit = 50;
        [Suffix("%")] public float RageDamageBuff = 3;
        [Suffix("%")] public float RageMovementSpeedBuff = 3;
        [Suffix("%")] public float RageReduceDaleyBuff = 3;

        public bool EnableSummonMinions;
        [Suffix("Metres")] public float MaximumSpawnPlayerDistance = 20;
        [Suffix("Secounds")] public float SummonCooldown = 15;
        public int SummonEnemyMinCount = 2;
        public int SummonEnemyMaxCount = 4;
        public GameObject[] SummonEnemyPrefab;

        public BuffChangeClass HealthBuff;
        public BuffChangeClass DamageBuff;
        public BuffChangeClass MovementSpeedBuff;
        public BuffChangeClass ReduceDaleyBuff;

        Enemy enemy;
        EnemyStates enemyState;

        float healthMultiplier;
        float damageMultiplier;
        float movementSpeedMultiplier;
        float reduceDelayMultiplier;
        float regenerationTimer;

        float spawnTimer;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            enemyState = GetComponent<EnemyStates>();

            // Generate Buffs
            if (Random.Range(0, 100) < HealthBuff.BuffChange) { HealthBuff._buffValue = Random.Range(HealthBuff.BuffPercentMin / 100, HealthBuff.BuffPercentMax / 100); }
            if (Random.Range(0, 100) < DamageBuff.BuffChange) { DamageBuff._buffValue = Random.Range(DamageBuff.BuffPercentMin / 100, DamageBuff.BuffPercentMax) / 100; }
            if (Random.Range(0, 100) < MovementSpeedBuff.BuffChange) { MovementSpeedBuff._buffValue = Random.Range(MovementSpeedBuff.BuffPercentMin / 100, MovementSpeedBuff.BuffPercentMax / 100); }
            if (Random.Range(0, 100) < ReduceDaleyBuff.BuffChange) { ReduceDaleyBuff._buffValue = Random.Range(ReduceDaleyBuff.BuffPercentMin / 100, ReduceDaleyBuff.BuffPercentMax / 100); }

            healthMultiplier = HealthBuff._buffValue;
            damageMultiplier = DamageBuff._buffValue;
            reduceDelayMultiplier = ReduceDaleyBuff._buffValue;

            enemy.healthMultiplier = healthMultiplier;
            enemyState.abilityDamageMultiplier = damageMultiplier;
            enemyState.reduceDelayMultiplier = reduceDelayMultiplier;
        }

        public void Update()
        {
            regenerationTimer += Time.deltaTime;

            /* Rage */

            damageMultiplier = DamageBuff._buffValue;
            reduceDelayMultiplier = ReduceDaleyBuff._buffValue;

            // Rage
            if (enemy.health < RageHealthMaxLimit && EnableRage)
            {
                damageMultiplier += RageDamageBuff / 100;
                reduceDelayMultiplier += RageReduceDaleyBuff / 100;
            }

            enemyState.abilityDamageMultiplier = damageMultiplier;
            enemyState.reduceDelayMultiplier = reduceDelayMultiplier;

            /* Regeneration */

            if (regenerationTimer > RegenerationInterval && EnableRegeneration && enemy.health < enemy.maxHealth)
            {
                //enemy.Heal(RegenerationHealth);
                regenerationTimer = 0;
            }

            /* Spawn Minion */
            if (EnableSummonMinions)
            {
                spawnTimer -= Time.deltaTime;

                float distance = Vector3.Distance(Camera.main.transform.position, transform.position);
                if (distance < MaximumSpawnPlayerDistance)
                {
                    if (spawnTimer < 0)
                    {
                        int count = Random.Range(SummonEnemyMinCount, SummonEnemyMaxCount);
                        for (int i = 0; i < count; i++)
                        {
                            Instantiate(SummonEnemyPrefab[Random.Range(0, SummonEnemyPrefab.Length)], new Vector3(Random.Range(transform.position.x - 2, transform.position.x + 2), transform.position.y, Random.Range(transform.position.z - 2, transform.position.z + 2)), transform.rotation);
                        }
                        spawnTimer = SummonCooldown;
                    }
                }
            }
        }

        [System.Serializable]
        public class BuffChangeClass
        {
            [Suffix("%")] public float BuffChange = 30;
            [Suffix("%")] public float BuffPercentMin = 10;
            [Suffix("%")] public float BuffPercentMax = 25;

            // Private Only
            [HideInInspector] public float _buffValue;
        }

#if UNITY_EDITOR

        [CustomPropertyDrawer(typeof(BuffChangeClass))]
        public class BuffChangeClassDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                var weightRectLabel = new Rect(position.x, position.y, position.width - 115, 18);
                var weightRect = new Rect(position.x, position.y + 20, position.width - 115, 18);

                EditorGUI.LabelField(weightRectLabel, "Chance of an appearance");
                EditorGUI.PropertyField(weightRect, property.FindPropertyRelative("BuffChange"), GUIContent.none);

                var MinMaxRectLabel = new Rect(position.x + position.width - 110, position.y, 110, 18);

                var MinRect = new Rect(position.x + position.width - 110, position.y + 20, 50, 18);
                var MinMaxRect = new Rect(position.x + position.width - 59, position.y + 20, 9, 18);
                var MaxRect = new Rect(position.x + position.width - 50, position.y + 20, 50, 18);

                EditorGUI.LabelField(MinMaxRectLabel, "    Min      -      Max");
                EditorGUI.PropertyField(MinRect, property.FindPropertyRelative("BuffPercentMin"), GUIContent.none);
                EditorGUI.LabelField(MinMaxRect, "-");
                EditorGUI.PropertyField(MaxRect, property.FindPropertyRelative("BuffPercentMax"), GUIContent.none);

                EditorGUI.EndProperty();
            }
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return 40;
            }
        }

        [CustomEditor(typeof(EnemyAbility))]
        [CanEditMultipleObjects]
        public class EnemyAbilityEditor : Editor
        {
            bool SummonEnemyFoldout;
            public override void OnInspectorGUI()
            {
                EditorUtility.SetDirty(target);
                EnemyAbility enemy = (EnemyAbility)target;
                EditorGUI.indentLevel = 0;
                GUI.enabled = false;
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((EnemyAbility)target), typeof(EnemyAbility), false);
                GUI.enabled = true;

                EditorGUILayout.BeginVertical("box");

                TinyGUI.EditorTitle("Enemy Regeneration");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("EnableRegeneration"), new GUIContent("Enable Regeneration"));
                if (enemy.EnableRegeneration)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RegenerationInterval"), new GUIContent("Regeneration Interval"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RegenerationHealth"), new GUIContent("Regeneration Health"));
                }

                TinyGUI.EditorTitle("Enemy Rage");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("EnableRage"), new GUIContent("Enable Rage"));
                if (enemy.EnableRage)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RageHealthMaxLimit"), new GUIContent("Health Max Limit"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RageDamageBuff"), new GUIContent("Damage Buff"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RageMovementSpeedBuff"), new GUIContent("Movement Speed Buff"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RageReduceDaleyBuff"), new GUIContent("Reduce Daley Buff"));
                }

                TinyGUI.EditorTitle("Summmon Minions");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("EnableSummonMinions"), new GUIContent("Enable Summon Minions"));
                if (enemy.EnableSummonMinions)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("MaximumSpawnPlayerDistance"), new GUIContent("Max Spawn Player Distance"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("SummonCooldown"), new GUIContent("Summon Cooldown"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("SummonEnemyMinCount"), new GUIContent("Summon Enemy Min Count"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("SummonEnemyMaxCount"), new GUIContent("Summon Enemy Max Count"));
                    TinyGUI.ShowArray(serializedObject, "SummonEnemyPrefab", "Summonable Enemy", ref SummonEnemyFoldout);
                }

                TinyGUI.EditorTitle("Static Buffs");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("HealthBuff"), new GUIContent("Summon Cooldown"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DamageBuff"), new GUIContent("Summon Cooldown"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MovementSpeedBuff"), new GUIContent("Summon Cooldown"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ReduceDaleyBuff"), new GUIContent("Summon Cooldown"));

                EditorGUILayout.EndVertical();
                serializedObject.ApplyModifiedProperties();
            }
        }

        /*[Line("Buffs")]
        public BuffChangeClass HealthBuff;
        public BuffChangeClass DamageBuff;
        public BuffChangeClass MovementSpeedBuff;
        public BuffChangeClass ReduceDaleyBuff;*/
#endif
    }
}
