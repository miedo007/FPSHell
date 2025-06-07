using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace HellishBattle.Level
{
    public class DestructibleObject : MonoBehaviour
    {
        public float Health;
        public UnityEvent OnDestruction;

        [HideInInspector] public float _health;

        private void Start()
        {
            _health = Health;
        }

        public void GetDamage(DamageClass dmg)
        {
            _health -= dmg.Damage;
            if (_health <= 0)
            {
                OnDestruction.Invoke();
            }
        }

        public void GetLoot(LootDrop drop)
        {
            drop.SpawnDrop(this.transform, 1, 2);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(DestructibleObject))]
    public class DestructibleObjectEditor : Editor
    {
        SerializedProperty _health;
        SerializedProperty _event;

        void OnEnable()
        {
            _health = serializedObject.FindProperty("Health");
            _event = serializedObject.FindProperty("OnDestruction");
        }

        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            DestructibleObject loot = (DestructibleObject)target;

            serializedObject.Update();
            Rect r = EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginVertical("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField("Destructible Object", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();

            EditorGUI.ProgressBar(new Rect(r.x + 2, r.y + 25, r.width - 5, 30), loot._health / loot.Health, $"Health [{loot._health}/{loot.Health}]");
            if (Application.isEditor && !Application.isPlaying) loot._health = loot.Health;
            EditorGUILayout.Space(32);
            EditorGUILayout.PropertyField(_health, new GUIContent("Object Start Health"));

            EditorGUILayout.EndVertical();
            EditorGUILayout.PropertyField(_event);

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
