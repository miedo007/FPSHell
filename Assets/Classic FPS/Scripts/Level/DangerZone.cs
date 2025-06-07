using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using HellishBattle.Enemies;
using HellishBattle.Campaign;

namespace HellishBattle.Level
{
    public class DangerZone : MonoBehaviour
    {
        public AreaType Type;


        // Heal
        [Suffix("per Interval")] public float BaseHealth = 2f;
        [Suffix("Seconds")] public float HealthInterval = 5f;

        // Damage
        [Suffix("per Interval")] public float BaseDamage = 5f;
        public LayerMask DamageMask;
        public DamageType damageType;
        [Suffix("Seconds")] public float DamageInterval = 2f;

        public UnityEvent OnEnterHeal, OnExitHeal;
        public UnityEvent OnEnterDamage, OnExitDamage;

        private Dictionary<Collider, Coroutine> colliderCoroutines = new Dictionary<Collider, Coroutine>();

        // OLD
        //public float damage;
        //[Suffix("secounds")]public float interval;

        private void Start()
        {
            // Get Difficulty Level
            DifficultyManager _tmp = (DifficultyManager)Resources.Load("difficulty");
            DifficultyLevel difficulty = _tmp.CurrentDifficultyLevel();

            BaseDamage *= difficulty.areaDamageMultiplier;
        }

        // When Player Enter Danger Zone
        private void OnTriggerEnter(Collider collider)
        {
            if (colliderCoroutines.ContainsKey(collider))
                return; // If the collider already exists in the dictionary, ignore it.

            Coroutine coroutine = StartCoroutine(TryGetAction(collider));
            colliderCoroutines.Add(collider, coroutine);

            // Cast Unity Events
            if (Type == AreaType.HealArea) OnEnterHeal.Invoke();
            if (Type == AreaType.DamageArea) OnEnterDamage.Invoke();
        }




        // When Player Exit Danger Zone
        private void OnTriggerExit(Collider collider)
        {
            if (colliderCoroutines.TryGetValue(collider, out Coroutine coroutine))
            {
                StopCoroutine(coroutine);
                colliderCoroutines.Remove(collider);
            }

            // Cast Unity Events
            if (Type == AreaType.HealArea) OnExitHeal.Invoke();
            if (Type == AreaType.DamageArea) OnExitDamage.Invoke();
        }

        IEnumerator TryGetAction(Collider collider)
        {
            while (true)
            {
                if (!collider || !collider.bounds.Intersects(GetComponent<Collider>().bounds))
                {
                    // Jeœli collider przesta³ byæ w zasiêgu triggera, przerwij pêtlê.
                    yield break;
                }

                if (Type == AreaType.DamageArea && IsLayerInMask(collider.gameObject.layer, DamageMask))
                {
                    collider.SendMessage("GetDamage", new DamageClass(BaseDamage, damageType), SendMessageOptions.DontRequireReceiver);
                    yield return new WaitForSeconds(DamageInterval);
                }
                else if (Type == AreaType.HealArea)
                {
                    collider.SendMessage("Heal", BaseHealth, SendMessageOptions.DontRequireReceiver);
                    yield return new WaitForSeconds(HealthInterval);
                }
                else
                {
                    yield return new WaitForSeconds(5);
                }
            }
        }

        private bool IsLayerInMask(int layer, LayerMask mask)
        {
            return (mask.value & (1 << layer)) != 0;
        }
    }

    public enum AreaType
    {
        DamageArea, HealArea
    }


    /// Custom Editor/
#if UNITY_EDITOR

    [CustomEditor(typeof(DangerZone))]
    public class DangerZoneEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            DangerZone zone = (DangerZone)target;

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((DangerZone)target), typeof(DangerZone), false);
            GUI.enabled = true;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Damage / Heal Zone", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Type"), GUIContent.none, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            if (zone.Type == AreaType.HealArea)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BaseHealth"), new GUIContent("Base Health"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("HealthInterval"), new GUIContent("Health Interval"));

                TinyGUI.InfoBox($"You treat {zone.BaseHealth} points every {zone.HealthInterval} seconds. resulting in treatment at {zone.BaseHealth / zone.HealthInterval}/ s");
            }
            else if (zone.Type == AreaType.DamageArea)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BaseDamage"), new GUIContent("Base Damage"));
                TinyGUI.InfoBox("If Damage Zone is to work on Enemy\nUse Enemy and not EnemyBody!", "console.warnicon");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DamageMask"), new GUIContent("Damage Mask"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("damageType"), new GUIContent("Damage Type"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DamageInterval"), new GUIContent("Damage Interval"));

                TinyGUI.InfoBox($"Deals {zone.BaseDamage} damage of {zone.damageType} Type, every {zone.DamageInterval} seconds resulting in {zone.BaseDamage / zone.DamageInterval} {zone.damageType} DPS");
            }

            EditorGUILayout.EndVertical();

            if (zone.Type == AreaType.HealArea)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnEnterHeal"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnExitHeal"));
            }
            if (zone.Type == AreaType.DamageArea)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnEnterDamage"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnExitDamage"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}
