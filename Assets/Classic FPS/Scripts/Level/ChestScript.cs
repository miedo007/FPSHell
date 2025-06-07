using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace HellishBattle.Level
{
    [RequireComponent(typeof(AudioSource))]
    public class ChestScript : MonoBehaviour
    {
        // Loot Settings
        public LootDrop drop;
        public int DropChange = 1;
        public Transform Spawnpoint;
        public float MaxspawnRange = 1;
        public float MinspawnRange = 2;

        // Lid Settings
        public Transform FlapTransform;
        public float LidSpeedMultiplier = 2;
        public Vector3 LidPositionOpen;
        public Quaternion LidRotationOpen;

        // Audio & Events
        public AudioClip OpenSound;
        public UnityEvent OnOpenChest;

        // Private
        bool chestOpen;
        AudioSource source;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
        }


        private void Update()
        {
            if (FlapTransform && chestOpen)
            {
                FlapTransform.localPosition = Vector3.Lerp(FlapTransform.localPosition, LidPositionOpen, LidSpeedMultiplier * Time.deltaTime);
                FlapTransform.localRotation = Quaternion.Lerp(FlapTransform.localRotation, LidRotationOpen, LidSpeedMultiplier * Time.deltaTime);
            }
        }

        public void OpenChest()
        {
            if (!chestOpen)
            {
                chestOpen = true;

                // Spawn Drop
                List<GameObject> guaranteed = drop.GetGuaranteeedLoot();
                List<GameObject> randomLoot = drop.GetRandomLoot(DropChange);
                for (int i = 0; i < guaranteed.Count; i++)
                {
                    GameObject tmp = Instantiate(guaranteed[i], new Vector3(Spawnpoint.position.x + Random.Range(-2f, 2f), Spawnpoint.position.y, Spawnpoint.position.z + Random.Range(-2f, 2f)), Quaternion.identity);
                    if (guaranteed[i].TryGetComponent<BonusScript>(out BonusScript bonus))
                    {
                        LevelManager.Instance.Items.RemoveAt(LevelManager.Instance.Items.Count - 1);
                        bonus.AutoRecordInLevelManager = false;
                    }
                }
                for (int i = 0; i < randomLoot.Count; i++)
                {
                    Instantiate(randomLoot[i], new Vector3(Spawnpoint.position.x + Random.Range(-2f, 2f), Spawnpoint.position.y, Spawnpoint.position.z + Random.Range(-2f, 2f)), Quaternion.identity);
                    if (randomLoot[i].TryGetComponent<BonusScript>(out BonusScript bonus))
                    {
                        LevelManager.Instance.Items.RemoveAt(LevelManager.Instance.Items.Count - 1);
                        bonus.AutoRecordInLevelManager = false;
                    }
                }

                //
                if (OpenSound) source.PlayOneShot(OpenSound);
                OnOpenChest.Invoke();
            }
        }

#if UNITY_EDITOR
        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green; // Set the color of the Gizmo circle
            Handles.color = Color.green;

            // Draw a wire sphere to represent the range
            //DrawCircleInYAxis(Spawnpoint.position, spawnRange);
            if (Spawnpoint) Handles.Label(Spawnpoint.position, $"Spawn Radius: {MaxspawnRange - MinspawnRange}m");

            drop.GizmoDrawSpawnRange(Spawnpoint, MinspawnRange, MaxspawnRange);
        }
#endif
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(ChestScript)), CanEditMultipleObjects]
    public class ChestScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            ChestScript safe = (ChestScript)target;
            EditorGUI.indentLevel = 0;

            TinyGUI.DrawScript<ChestScript>("Chest Script", target);

            TinyGUI.BeginBoxGroup("Loot Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("drop"), new GUIContent("Loot Pool"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DropChange"), new GUIContent("Drop Size"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Spawnpoint"), new GUIContent("Spawnpoint"));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Min-Max Spawn Radius", GUILayout.Width(EditorGUIUtility.labelWidth));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MinspawnRange"), GUIContent.none);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxspawnRange"), GUIContent.none);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OpenSound"), new GUIContent("Open Sound"));
            TinyGUI.EndBoxGroup();

            TinyGUI.BeginBoxGroup("Lid Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("FlapTransform"), new GUIContent("Lid Game Object"));
            if (safe.FlapTransform)
            {
                EditorGUILayout.BeginVertical("helpbox");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("LidSpeedMultiplier"), new GUIContent("Lid Open Speed Multiplier"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("LidPositionOpen"), new GUIContent("Lid Open Position"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("LidRotationOpen"), new GUIContent("Lid Open Rotation"));
                if (TinyGUI.IconButton("SceneLoadIn", "Load Lid Position"))
                {
                    safe.LidPositionOpen = safe.FlapTransform.localPosition;
                    safe.LidRotationOpen = safe.FlapTransform.localRotation;
                }
                EditorGUILayout.EndVertical();
            }
            TinyGUI.EndBoxGroup();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnOpenChest"), new GUIContent("On Open Chest"));


            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}
