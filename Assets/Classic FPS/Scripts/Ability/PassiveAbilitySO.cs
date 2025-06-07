using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HellishBattle.Ability
{
    [CreateAssetMenu(fileName = "Passive Ability", menuName = "Hellish Battle/Passive Skill", order = 50)]
    public class PassiveAbilitySO : ScriptableObject
    {
        public Sprite Icon;
        public LocalizedString Name;
        public LocalizedString Description;

        // Health Regeneration
        public bool HealthRegeneration;
        public float HealthRegenerationTimeInterval = 2f;
        public float HealthRegenerationAmount = 5f;
        public float HealthRegenerationWaitingTime = 5f;

        // Armor Regeneration
        public bool ArmorRegeneration;
        public float ArmorRegenerationTimeInterval = 2f;
        public float ArmorRegenerationAmount = 5f;
        public float ArmorRegenerationWaitingTime = 5f;

        // Boost Health
        public bool BoostHealth;
        public float AdditionalHealth = 50;
        public float AdditionalStartHealth = 0;

        // Boost Armor
        public bool BoostArmor;
        public float AdditionalArmor = 50;
        public float AdditionalStartArmor = 0;

        // Boost Speed
        public bool BoostSpeed;
        [Range(1, 2)] public float SpeedIncrease = 1.2f;

        // Range Weapon Boost
        public bool EnableRangeWeaponBoost = false;
        [Range(1, 2)] public float RangeRecoilReduce = 1f;
        [Range(1, 2)] public float RangeDamageIncrease = 1f;
        [Range(1, 2)] public float RangeFireRateIncrease = 1f;

        // Melle Weapon Boost
        public bool EnableMelleWeaponBoost = false;
        [Range(1, 2)] public float MelleDamageIncrease = 1f;
        [Range(1, 2)] public float MelleFireRateIncrease = 1f;
        [Range(1, 2)] public float MelleRangeIncrease = 1f;

        // Health Vampirism
        public bool EnableHealthVampirizm;
        [Range(0, 1)] public float HealthVampirizmChange = .75f;
        [Range(0, 1)] public float HealthVampirizmPercent = .1f;

        // Health Vampirism
        public bool EnableArmorVampirizm;
        [Range(0, 1)] public float ArmorVampirizmChange = .5f;
        [Range(0, 1)] public float ArmorVampirizmPercent = .1f;

        // Stamina Size
        public bool EnableStaminaSize;
        [Range(0, 250)] public float StaminaSize = 100f;

        // Stamina Consume Buff
        public bool EnableStaminaConsume;
        [Range(1, 10)] public float RunningStaminaConsume = 3f;
        [Range(1, 50)] public float JumpStaminaConsume = 10f;

        // Stamina Regeneration Buff
        public bool EnableStaminaRegeneration;
        [Range(0, 5)] public float StaminaRegenerationInterval = 2f;
        [Range(1, 50)] public float StaminaRegenerationSpeed = 10f;

        // Multi Jump
        public bool EnableJumpModifier = false;
        public float JumpStrenght = 5.5f;
        [Range(1, 5)] public int MaxJumpCount = 1;

        // No Consume Ammo
        public bool EnableNoConsumeAmmo = false;
        public float NoConsumeAmmoChange = 8f;

        // Second Change
        public bool EnableSecondChange = false;
        public float SecondChangeHealth = 25;



        // Private
        public bool EditorHealthBool = false;
        public bool EditorArmorBool = false;
        public bool EditorStaminaBool = false;
        public bool EditorMovementBool = false;
        public bool EditorVampirizmBool = false;
        public bool EditorWeaponBool = false;
        public bool EditorOtherBool = false;
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(PassiveAbilitySO)), CanEditMultipleObjects]
    public class PassiveAbilitySOEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            PassiveAbilitySO passive = (PassiveAbilitySO)target;

            TinyGUI.DrawScript<PassiveAbilitySO>("Passive Skill Script", target);

            TinyGUI.BeginBoxGroup("Base Passive Skill Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Icon"), new GUIContent("Passive Skill Icon"));
            TinyGUI.LocalizedString(passive.Name, serializedObject.FindProperty("Name"));
            TinyGUI.LocalizedString(passive.Description, serializedObject.FindProperty("Description"));
            TinyGUI.EndBoxGroup();


            /* EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("Passive Skill Info", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Icon"), new GUIContent("Passive Skill Icon"));
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Name"));
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Description"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Passive Skill Settings", EditorStyles.boldLabel);
            */

            // Health
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUI.indentLevel++;
            passive.EditorHealthBool = EditorGUILayout.Foldout(passive.EditorHealthBool, new GUIContent($"Health Options"), true);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            if (passive.EditorHealthBool)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                passive.BoostHealth = EditorGUILayout.Toggle(new GUIContent("Health Boost"), passive.BoostHealth);
                if (passive.BoostHealth)
                {
                    EditorGUI.indentLevel++;
                    passive.AdditionalHealth = EditorGUILayout.FloatField(new GUIContent("Additional Health"), passive.AdditionalHealth);
                    passive.AdditionalStartHealth = EditorGUILayout.FloatField(new GUIContent("Additional Start Health"), passive.AdditionalStartHealth);
                    EditorGUI.indentLevel--;
                }

                passive.HealthRegeneration = EditorGUILayout.Toggle(new GUIContent("Health Regeneration"), passive.HealthRegeneration);
                if (passive.HealthRegeneration)
                {
                    EditorGUI.indentLevel++;
                    passive.HealthRegenerationAmount = EditorGUILayout.FloatField(new GUIContent("Amount"), passive.HealthRegenerationAmount);
                    passive.HealthRegenerationTimeInterval = EditorGUILayout.FloatField(new GUIContent("Time Interval"), passive.HealthRegenerationTimeInterval);
                    passive.HealthRegenerationWaitingTime = EditorGUILayout.FloatField(new GUIContent("Waiting Time"), passive.HealthRegenerationWaitingTime);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space(5);
                    EditorGUILayout.HelpBox(new GUIContent($"After {passive.HealthRegenerationWaitingTime} seconds of damage taken, every {passive.HealthRegenerationTimeInterval} seconds adds {passive.HealthRegenerationAmount} Health\nAverage regeneration is {passive.HealthRegenerationAmount / passive.HealthRegenerationTimeInterval} Health per secound"));
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            // Armor
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUI.indentLevel++;
            passive.EditorArmorBool = EditorGUILayout.Foldout(passive.EditorArmorBool, new GUIContent($"Armor Options"), true);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            if (passive.EditorArmorBool)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                passive.BoostArmor = EditorGUILayout.Toggle(new GUIContent("Boost Armor"), passive.BoostArmor);
                if (passive.BoostArmor)
                {
                    EditorGUI.indentLevel++;
                    passive.AdditionalArmor = EditorGUILayout.FloatField(new GUIContent("Additional Armor"), passive.AdditionalArmor);
                    passive.AdditionalStartArmor = EditorGUILayout.FloatField(new GUIContent("Additional Start Armor"), passive.AdditionalStartArmor);
                    EditorGUI.indentLevel--;
                }

                passive.ArmorRegeneration = EditorGUILayout.Toggle(new GUIContent("Armor Regeneration"), passive.ArmorRegeneration);
                if (passive.ArmorRegeneration)
                {
                    EditorGUI.indentLevel++;
                    passive.ArmorRegenerationAmount = EditorGUILayout.FloatField(new GUIContent("Amount"), passive.ArmorRegenerationAmount);
                    passive.ArmorRegenerationTimeInterval = EditorGUILayout.FloatField(new GUIContent("Time Interval"), passive.ArmorRegenerationTimeInterval);
                    passive.ArmorRegenerationWaitingTime = EditorGUILayout.FloatField(new GUIContent("Waiting Time"), passive.ArmorRegenerationWaitingTime);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space(5);
                    EditorGUILayout.HelpBox(new GUIContent($"After {passive.ArmorRegenerationWaitingTime} seconds of damage taken, every {passive.ArmorRegenerationTimeInterval} seconds adds {passive.ArmorRegenerationAmount} Armor\nAverage regeneration is {passive.ArmorRegenerationAmount / passive.ArmorRegenerationTimeInterval} Armor per secound"));
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            // Stamina Options
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUI.indentLevel++;
            passive.EditorStaminaBool = EditorGUILayout.Foldout(passive.EditorStaminaBool, new GUIContent($"Stamina Options"), true);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            if (passive.EditorStaminaBool)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                passive.EnableStaminaSize = EditorGUILayout.Toggle(new GUIContent("Stamina Size Buff"), passive.EnableStaminaSize);
                if (passive.EnableStaminaSize)
                {
                    EditorGUI.indentLevel++;
                    passive.StaminaSize = EditorGUILayout.Slider(new GUIContent("Stamina Size"), passive.StaminaSize, 0, 250);
                    EditorGUI.indentLevel--;
                }

                passive.EnableStaminaConsume = EditorGUILayout.Toggle(new GUIContent("Stamina Consume Buff"), passive.EnableStaminaConsume);
                if (passive.EnableStaminaConsume)
                {
                    EditorGUI.indentLevel++;
                    passive.RunningStaminaConsume = EditorGUILayout.Slider(new GUIContent("Running Stamina Consume"), passive.RunningStaminaConsume, 1, 10);
                    passive.JumpStaminaConsume = EditorGUILayout.Slider(new GUIContent("Jump Stamina Consume"), passive.JumpStaminaConsume, 1, 50);
                    EditorGUI.indentLevel--;
                }

                passive.EnableStaminaRegeneration = EditorGUILayout.Toggle(new GUIContent("Stamina Regeneration Buff"), passive.EnableStaminaRegeneration);
                if (passive.EnableStaminaRegeneration)
                {
                    EditorGUI.indentLevel++;
                    passive.StaminaRegenerationInterval = EditorGUILayout.Slider(new GUIContent("Stamina Regeneration Interval"), passive.StaminaRegenerationInterval, 0, 5);
                    passive.StaminaRegenerationSpeed = EditorGUILayout.Slider(new GUIContent("Stamina Regeneration Speed"), passive.StaminaRegenerationSpeed, 1, 50);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);


            // Jump Options
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUI.indentLevel++;
            passive.EditorMovementBool = EditorGUILayout.Foldout(passive.EditorMovementBool, new GUIContent($"Movement Options"), true);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            if (passive.EditorMovementBool)
            {
                EditorGUILayout.BeginVertical("HelpBox");

                passive.EnableJumpModifier = EditorGUILayout.Toggle(new GUIContent("Jump Modifiers"), passive.EnableJumpModifier);
                if (passive.EnableJumpModifier)
                {
                    EditorGUI.indentLevel++;
                    passive.JumpStrenght = EditorGUILayout.Slider(new GUIContent("Jump Strenght"), passive.JumpStrenght, 1, 15);
                    passive.MaxJumpCount = (int)EditorGUILayout.Slider(new GUIContent("Max Jump Count"), passive.MaxJumpCount, 1, 5);
                    EditorGUI.indentLevel--;
                }
                passive.BoostSpeed = EditorGUILayout.Toggle(new GUIContent("Speed Modifiers"), passive.BoostSpeed);
                if (passive.BoostSpeed)
                {
                    EditorGUI.indentLevel++;
                    passive.SpeedIncrease = EditorGUILayout.Slider(new GUIContent("Speed Multipliers"), passive.SpeedIncrease, .5f, 2f);
                    EditorGUI.indentLevel--;
                }
                // Boost Speed
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            // Vampirizm Options
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUI.indentLevel++;
            passive.EditorVampirizmBool = EditorGUILayout.Foldout(passive.EditorVampirizmBool, new GUIContent($"Vampirizm Options"), true);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            if (passive.EditorVampirizmBool)
            {
                EditorGUILayout.BeginVertical("HelpBox");

                passive.EnableHealthVampirizm = EditorGUILayout.Toggle(new GUIContent("Health Vampirizm"), passive.EnableHealthVampirizm);
                if (passive.EnableHealthVampirizm)
                {
                    EditorGUI.indentLevel++;
                    passive.HealthVampirizmChange = EditorGUILayout.Slider(new GUIContent("Vampirizm Change"), passive.HealthVampirizmChange, 0, 1);
                    passive.HealthVampirizmPercent = EditorGUILayout.Slider(new GUIContent("Vampirizm Percent"), passive.HealthVampirizmPercent, 0, 1);
                    EditorGUI.indentLevel--;
                }

                passive.EnableArmorVampirizm = EditorGUILayout.Toggle(new GUIContent("Armor Vampirizm"), passive.EnableArmorVampirizm);
                if (passive.EnableArmorVampirizm)
                {
                    EditorGUI.indentLevel++;
                    passive.ArmorVampirizmChange = EditorGUILayout.Slider(new GUIContent("Vampirizm Change"), passive.ArmorVampirizmChange, 0, 1);
                    passive.ArmorVampirizmPercent = EditorGUILayout.Slider(new GUIContent("Vampirizm Percent"), passive.ArmorVampirizmPercent, 0, 1);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            // Weapon Options
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUI.indentLevel++;
            passive.EditorWeaponBool = EditorGUILayout.Foldout(passive.EditorWeaponBool, new GUIContent($"Weapon Options"), true);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            if (passive.EditorWeaponBool)
            {
                EditorGUILayout.BeginVertical("HelpBox");

                passive.EnableRangeWeaponBoost = EditorGUILayout.Toggle(new GUIContent("Range Weapon Boost"), passive.EnableRangeWeaponBoost);
                if (passive.EnableRangeWeaponBoost)
                {
                    EditorGUI.indentLevel++;
                    passive.RangeDamageIncrease = EditorGUILayout.Slider(new GUIContent("Damage Modifier"), passive.RangeDamageIncrease, .5f, 3f);
                    passive.RangeRecoilReduce = EditorGUILayout.Slider(new GUIContent("Recoil Modifier"), passive.RangeRecoilReduce, .5f, 2f);
                    passive.RangeFireRateIncrease = EditorGUILayout.Slider(new GUIContent("Fire Rate Modifier"), passive.RangeFireRateIncrease, .5f, 2f);
                    EditorGUI.indentLevel--;
                }

                passive.EnableMelleWeaponBoost = EditorGUILayout.Toggle(new GUIContent("Melle Weapon Boost"), passive.EnableMelleWeaponBoost);
                if (passive.EnableMelleWeaponBoost)
                {
                    EditorGUI.indentLevel++;
                    passive.MelleDamageIncrease = EditorGUILayout.Slider(new GUIContent("Damage Modifier"), passive.MelleDamageIncrease, .5f, 3f);
                    passive.MelleFireRateIncrease = EditorGUILayout.Slider(new GUIContent("Fire Rate Modifier"), passive.MelleFireRateIncrease, .5f, 2f);
                    passive.MelleRangeIncrease = EditorGUILayout.Slider(new GUIContent("Range Modifier"), passive.MelleRangeIncrease, .5f, 2f);
                    EditorGUI.indentLevel--;
                }
                passive.EnableNoConsumeAmmo = EditorGUILayout.Toggle(new GUIContent("No Consume Ammo"), passive.EnableNoConsumeAmmo);
                if (passive.EnableNoConsumeAmmo)
                {
                    EditorGUI.indentLevel++;
                    passive.NoConsumeAmmoChange = EditorGUILayout.Slider(new GUIContent("No Consume Ammo Change"), passive.NoConsumeAmmoChange, 0, 100);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space(2);

            // Other Options
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUI.indentLevel++;
            passive.EditorOtherBool = EditorGUILayout.Foldout(passive.EditorOtherBool, new GUIContent($"Other Options"), true);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            if (passive.EditorOtherBool)
            {
                EditorGUILayout.BeginVertical("HelpBox");

                passive.EnableSecondChange = EditorGUILayout.Toggle(new GUIContent("Second Change"), passive.EnableSecondChange);
                if (passive.EnableSecondChange)
                {
                    EditorGUI.indentLevel++;
                    passive.SecondChangeHealth = EditorGUILayout.Slider(new GUIContent("Second Change Health"), passive.SecondChangeHealth, 1, 200);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

}