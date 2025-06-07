using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEditor;

namespace HellishBattle.Level
{
    [RequireComponent(typeof(AudioSource))]
    public class LockpickScript : MonoBehaviour
    {

        public Slider TimeIndicator;
        public GameObject Indicator;

        public int SlotCount = 5;
        public Vector2 MinMaxValue = new Vector2(0.25f, 0.35f);

        [Suffix("From Side to Other Per Second")] public float timeToMove = 2.5f;
        public GameObject UI;
        public List<GameObject> Slots;

        public AudioClip SuccessSlotSound, FailSlotSound, UnlockSound;

        public UnityEvent OnUnlock;

        bool activated;
        bool toRight = true;
        int SolvedSlots = 0;
        float startPoint;
        float endPoint;
        AudioSource source;

        // Initialize the lockpicking system
        private void Awake()
        {
            UI.SetActive(false);
            TimeIndicator.value = Random.Range(0f, 1f);
            ResetSlots();
            source = GetComponent<AudioSource>();
        }

        // Reset the state of lockpicking slots
        void ResetSlots()
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                if (i < SlotCount)
                {
                    Slots[i].SetActive(true);
                    Slots[i].transform.GetChild(0).localPosition = new Vector3(0, 0, 0);
                }
                else
                {
                    Slots[i].SetActive(false);
                    Slots[i].transform.GetChild(0).localPosition = new Vector3(0, 0, 0);
                }
            }
        }

        // Update is called once per frame
        public void Update()
        {
            if (activated)
            {
                // Move the lockpicking indicator
                if (toRight)
                {
                    TimeIndicator.value += timeToMove * Time.deltaTime;
                    if (TimeIndicator.value >= 1) { toRight = false; }
                }
                else
                {
                    TimeIndicator.value -= timeToMove * Time.deltaTime;
                    if (TimeIndicator.value <= 0) { toRight = true; }
                }

                // Check for player input to attempt unlocking a slot
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    TryUnlockSlot();
                }
            }
        }

        // Attempt to unlock the current slot
        public void TryUnlockSlot()
        {
            if (startPoint < TimeIndicator.value && TimeIndicator.value < endPoint)
            {
                // Successfully unlock the slot
                SolvedSlots++;

                // Move the pin in the visual representation
                for (int i = 0; i < Slots.Count; i++)
                {
                    if (i < SolvedSlots)
                    {
                        Slots[i].transform.GetChild(0).localPosition = new Vector3(0, 36, 0);
                    }
                }

                // Change the range for the valid unlock zone
                Indicator.transform.localScale = new Vector3(Random.Range(MinMaxValue.x, MinMaxValue.y), 1, 1);
                float point = 58 - (58 * Indicator.transform.localScale.x);
                Indicator.transform.localPosition = new Vector3(Random.Range(point / 2 * (-1), point / 2), 0, 0);
                startPoint = ((Indicator.transform.localPosition.x + 29) / 58 - (Indicator.transform.localScale.x / 2));
                endPoint = ((Indicator.transform.localPosition.x + 29) / 58 + (Indicator.transform.localScale.x / 2));

                if (SolvedSlots == SlotCount)
                {
                    // Player successfully unlocked all slots
                    CloseLockpicking();
                    OnUnlock.Invoke();

                    source.PlayOneShot(UnlockSound);
                }
                else
                {
                    source.PlayOneShot(SuccessSlotSound);
                }
            }
            else
            {
                // Reset slots if the unlock attempt fails
                ResetSlots();
                SolvedSlots = 0;

                source.PlayOneShot(FailSlotSound);
            }
        }

        // Open the lockpicking system
        public void OpenLockpicking()
        {
            activated = true;
            UI.SetActive(true);
            UnlockCursor();
            ResetSlots();

            // Change the range for the valid unlock zone
            Indicator.transform.localScale = new Vector3(Random.Range(MinMaxValue.x, MinMaxValue.y), 1, 1);
            float point = 58 - (58 * Indicator.transform.localScale.x);
            Indicator.transform.localPosition = new Vector3(Random.Range(point / 2 * (-1), point / 2), 0, 0);
            startPoint = ((Indicator.transform.localPosition.x + 29) / 58 - (Indicator.transform.localScale.x / 2));
            endPoint = ((Indicator.transform.localPosition.x + 29) / 58 + (Indicator.transform.localScale.x / 2));
        }

        // Close the lockpicking system
        public void CloseLockpicking()
        {
            activated = false;
            UI.SetActive(false);
            LockCursor();
        }

        // Lock the cursor for regular gameplay
        public void LockCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            GameManager.Instance.paused = false;
        }

        // Unlock the cursor for UI interaction
        public void UnlockCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            GameManager.Instance.paused = true;
        }
    }


#if UNITY_EDITOR

    [CustomEditor(typeof(LockpickScript)), CanEditMultipleObjects]
    public class LockpickScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            LockpickScript safe = (LockpickScript)target;
            EditorGUI.indentLevel = 0;

            EditorGUILayout.BeginHorizontal("helpbox", GUILayout.Height(EditorGUIUtility.singleLineHeight + 6));
            EditorGUILayout.LabelField("Lockpick Minigame Script", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.ObjectField(MonoScript.FromMonoBehaviour((LockpickScript)target), typeof(LockpickScript), false);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Lockpick Minigame Settings", "Set Access Code, and number of attempts");
            TinyGUI.Guideline("Proposed Lockpick Settings", "Very Easy:    Slot: 3 | Size: 40% | Speed 1", "Easy:             Slot: 4 | Size: 35% | Speed 1.5", "Medium:        Slot: 6 | Size: 30% | Speed 2", "Hard:             Slot: 7 | Size: 25% | Speed 2.5", "Very Hard:    Slot: 9 | Size: 20% | Speed 3");

            safe.SlotCount = EditorGUILayout.IntSlider("Slot Count", safe.SlotCount, 3, 9);
            EditorGUILayout.MinMaxSlider("Unlock Size", ref safe.MinMaxValue.x, ref safe.MinMaxValue.y, 0, 1);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("timeToMove"), new GUIContent("Bounces Per Second"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("UI Elements", "Elements belonging to Keypad UI");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TimeIndicator"), new GUIContent("Keypad Text"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Indicator"), new GUIContent("Left Try Text"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("UI"), new GUIContent("Keypad UI"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Sounds", "Sounds made during interaction");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SuccessSlotSound"), new GUIContent("Success Slot Sound"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("FailSlotSound"), new GUIContent("Fail Slot Sound"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("UnlockSound"), new GUIContent("Unlock Sound"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Unity Events", "Add Action on OnUnlock");
            EditorGUILayout.EndVertical();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnUnlock"), new GUIContent("On Unlock"));

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}