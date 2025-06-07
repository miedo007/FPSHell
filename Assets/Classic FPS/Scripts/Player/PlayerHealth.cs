using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using HellishBattle.Ability;
using HellishBattle.SaveSystem;
using HellishBattle.Level;
using HellishBattle.Weapon;

namespace HellishBattle.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        // Health
        public int godHealth;
        public int maxHealth;
        public int startHealth;

        // Armor
        public int godArmor;
        public int maxArmor;
        public int startArmor;

        // Audio
        public List<AudioClip> hit;
        public List<AudioClip> deathClip;

        //[Line("Prefabs & Objects")]
        public FlashScreen flash;

        // Health & Armor Text
        public TMP_Text HealthText;
        public TMP_Text ArmorText;

        // Health & Armor Bar
        public Slider HealthBar;
        public Slider OverHealthBar;
        public Slider ArmorBar;
        public Slider OverArmorBar;

        // Fall Damage
        public AnimationCurve FallDamage;

        // Player FX
        public int FloorBloodCount = 3;
        public float FloorBloodChange = 90f;
        public List<GameObject> FloorBlood;

        // Damage Indicator
        public GameObject DamageIndicatorPrefab;
        public Transform IndicatorParent;
        public float IndicatorShowTime;
        [HideInInspector] public List<Transform> IndicatorPosition;
        [HideInInspector] public List<float> IndicatorTimeContainer;

        // Face Settings
        public bool FaceAnimation = false;
        public List<UIFaceClass> UIFace;
        public Image FaceImage;

        public UnityEvent OnTakeDamage, OnHeal, OnDeath;

        // Private Values
        [HideInInspector] public float armor;
        [HideInInspector] public float health;
        [HideInInspector] public float WeaponBlockingChange;
        [HideInInspector] public float WeaponDamageReductionPercentage;
        AudioSource source;
        bool armorRegen = false;
        bool healthRegen = false;
        bool _EnableSecoundChange = true;
        GameSettingsManger _go;

        Vector3 _spawnPoint;

        /// <summary>
        /// 
        /// </summary>
        /// 

        private void Awake()
        {
            _spawnPoint = transform.position;
        }
        
        void Start()
        {
            // Setup Audio Source
            source = GetComponent<AudioSource>();

            // Get Passeve Skill
            _go = (GameSettingsManger)Resources.Load("Game_Settings");
            if (_go.EnablePlayerAbility)
            {
                PassiveAbilitySO _passive = GameManager.Instance.PassiveSkill;
                if (_passive.BoostHealth)
                {
                    maxHealth += (int)_passive.AdditionalHealth;
                    godHealth += (int)_passive.AdditionalHealth;
                    startHealth += (int)_passive.AdditionalStartHealth;
                }
                if (_passive.BoostArmor)
                {
                    maxArmor += (int)_passive.AdditionalArmor;
                    godArmor += (int)_passive.AdditionalArmor;
                    startArmor += (int)_passive.AdditionalStartArmor;
                }
                if (_passive.HealthRegeneration) { healthRegen = true; }
                if (_passive.ArmorRegeneration) { armorRegen = true; }
            }

            if (LevelManager.Instance.Gamemode.GameMode == GameModes.StoryMode && TinySaveSystem.HasKey("StoryMode_Save"))
            {
                startHealth = (int)TinySaveSystem.GetPlayerSave("StoryMode_Save").Health;
                startArmor = (int)TinySaveSystem.GetPlayerSave("StoryMode_Save").Armor;

                if (startHealth <= 0)
                {
                    Debug.Log("Change");
                    startHealth = 100;
                    startArmor = 0;
                }
            }

            // Setup Start Health & Armor
            armor = startArmor;
            health = startHealth;

            // Health & Armor Text
            HealthText.text = health.ToString();
            ArmorText.text = armor.ToString();

            // Health & Armor Bar
            HealthBar.minValue = 0;
            HealthBar.value = startHealth;
            HealthBar.maxValue = maxHealth;

            OverHealthBar.minValue = maxHealth;
            OverHealthBar.value = startHealth;
            OverHealthBar.maxValue = godHealth;

            ArmorBar.minValue = 0;
            ArmorBar.value = startArmor;
            ArmorBar.maxValue = maxArmor;

            OverArmorBar.minValue = maxArmor;
            OverArmorBar.value = startArmor;
            OverArmorBar.maxValue = godArmor;

            // Disable Face Animation
            if (!FaceAnimation)
            {
                FaceImage.GetComponent<Animator>().enabled = false;
            }
        }

        private void Update()
        {
            // Min - Max Health & Armor
            armor = Mathf.Clamp(armor, 0, godArmor);
            health = Mathf.Clamp(health, -Mathf.Infinity, godHealth);

            // Death 
            if (health <= 0)
            {
                if (transform.GetComponent<PlayerAbility>().SelectedPassive.EnableSecondChange && _EnableSecoundChange)
                {
                    _EnableSecoundChange = false;
                    Heal(GameManager.Instance.PassiveSkill.SecondChangeHealth - health);
                }
                else
                {
                    source.PlayOneShot(deathClip[Random.Range(0, deathClip.Count)]);
                    OnDeath.Invoke();

                    // Normal Gamemodes
                    if (LevelManager.Instance.Gamemode.GameMode != GameModes.Labolatory)
                    {
                        GameManager.Instance.PlayerDeath();
                    }
                    // Labolatory Maps
                    else
                    {
                        // Teleport Player
                        CharacterController cc = Camera.main.transform.parent.GetComponent<CharacterController>();
                        cc.enabled = false;
                        cc.transform.position = _spawnPoint;
                        cc.enabled = true;

                        // Setup Health & Armor
                        health = startHealth;
                        armor = startArmor;

                        // Update UI
                        HealthText.text = Mathf.Ceil(health).ToString();
                        ArmorText.text = Mathf.Ceil(armor).ToString();

                        HealthBar.value = health;
                        OverHealthBar.value = health;
                        ArmorBar.value = armor;
                        OverArmorBar.value = armor;

                        StartCoroutine(GameManager.Instance.SpawnInGamePopup("Respawning..."));
                    }

                }
            }

            // Damage Indicator
            foreach (Transform child in IndicatorParent.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            if (_go.EnablePlayerDamageIndicator)
            {
                for (int i = 0; i < IndicatorPosition.Count; i++)
                {
                    GameObject _prefab = Instantiate(DamageIndicatorPrefab, IndicatorParent);
                    _prefab.SetActive(true);

                    // Posiotion
                    float AngleRad = Mathf.Atan2(IndicatorPosition[i].position.x - transform.position.x, IndicatorPosition[i].position.z - transform.position.z);
                    float AngleDeg = (180 / Mathf.PI) * AngleRad;
                    if (transform.rotation.eulerAngles.y > 0) { _prefab.transform.rotation = Quaternion.Euler(0, 0, (AngleDeg - transform.rotation.eulerAngles.y) * -1); }
                    else if (transform.rotation.eulerAngles.y < 0) { _prefab.transform.rotation = Quaternion.Euler(0, 0, (AngleDeg + transform.rotation.eulerAngles.y) * -1); }

                    IndicatorTimeContainer[i] -= Time.deltaTime;

                    // Destroy Time
                    if (IndicatorTimeContainer[i] < 0)
                    {
                        IndicatorTimeContainer.RemoveAt(i);
                        IndicatorPosition.RemoveAt(i);
                    }
                }
            }
        }

        public void AddHealth(float value, bool GodBonus)
        {
            if (!GodBonus)
            {
                if (health > maxHealth) { }
                else if (health + value <= maxHealth)
                {
                    health += value;
                }
                else
                {
                    health = maxHealth;
                }
            }
            else
            {
                if (health + value <= godHealth)
                {
                    health += value;
                }
                else
                {
                    health = godHealth;
                }
            }

            HealthText.text = Mathf.Ceil(health).ToString();

            HealthBar.value = health;
            OverHealthBar.value = health;

            OnHeal.Invoke();
            FaceSprite();
        }

        public void AddArmor(float value, bool GodBonus)
        {
            if (!GodBonus)
            {
                if (armor > maxArmor) { }
                else if (armor + value <= maxArmor)
                {
                    armor += value;
                }
                else
                {
                    armor = maxArmor;
                }
            }
            else
            {
                if (armor + value <= godArmor)
                {
                    armor += value;
                }
                else
                {
                    armor = godArmor;
                }
            }

            ArmorText.text = Mathf.Ceil(armor).ToString();

            ArmorBar.value = armor;
            OverArmorBar.value = armor;

            OnHeal.Invoke();
            FaceSprite();
        }



        public void Heal(float health) { AddHealth(health, false); flash.HealthBonus(); }

        public void GetDamage(DamageClass DMG)
        {
            // Reduce Damage if Enable
            if (Random.Range(0, 100) < WeaponBlockingChange)
            {
                DMG.Damage -= DMG.Damage * (WeaponDamageReductionPercentage / 100);

                // Try Get Melle Weapon
                if (transform.Find("Weapons").GetComponent<WeaponSwitch>().actualWeapon.TryGetComponent<MelleWeapon>(out MelleWeapon melle))
                {
                    melle.OnBlockSuccess.Invoke();
                }
            }
            else
            {
                // Try Get Melle Weapon
                if (transform.Find("Weapons").GetComponent<WeaponSwitch>().actualWeapon.TryGetComponent<MelleWeapon>(out MelleWeapon melle))
                {
                    melle.OnBlockFailure.Invoke();
                }
            }

            // Take Damage
            if (armor > 0 && armor >= DMG.Damage)
            {
                armor -= DMG.Damage;
            }
            else if (armor > 0 && armor < DMG.Damage)
            {
                DMG.Damage -= armor;
                armor = 0;
                health -= DMG.Damage;
            }
            else
            {
                health -= DMG.Damage;
            }

            HealthText.text = Mathf.Ceil(health).ToString();
            ArmorText.text = Mathf.Ceil(armor).ToString();

            HealthBar.value = health;
            OverHealthBar.value = health;
            ArmorBar.value = armor;
            OverArmorBar.value = armor;

            source.PlayOneShot(hit[Random.Range(0, hit.Count)]);
            flash.TookDamage();
            FaceSprite();

            // Only if Regeneration
            if (armorRegen || healthRegen) StopAllCoroutines();
            if (healthRegen) StartCoroutine(HealthRegenCooldown());
            if (armorRegen) StartCoroutine(ArmorRegenCooldown());
            OnTakeDamage.Invoke();

            // Blood FX
            if (FloorBlood.Count != 0)
            {
                for (int i = 0; i < FloorBloodCount; i++)
                {
                    if (Random.Range(0f, 100f) <= FloorBloodChange)
                    {
                        GameObject _temp = Instantiate(FloorBlood[Random.Range(0, FloorBlood.Count)], this.transform.position, new Quaternion(0, 0, 0, 0));

                        CharacterController characterController = transform.GetComponent<CharacterController>();
                        Vector3 controllerCenterPos = characterController.transform.position + characterController.center;
                        Vector3 groundPos = controllerCenterPos - Vector3.up * (characterController.height * 0.5f);

                        _temp.transform.position += new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                        _temp.transform.position = new Vector3(_temp.transform.position.x, Random.Range(0 + groundPos.y, .02f + groundPos.y), _temp.transform.position.z);
                        int rotate = Random.Range(0, 8) * 90;
                        _temp.transform.rotation = Quaternion.Euler(0f, rotate, 0f) * _temp.transform.rotation;
                    }
                }
            }
        }

        void FaceSprite()
        {
            for (int i = UIFace.Count - 1; i > 0; i--)
            {
                if (health >= UIFace[i].RequiredHealth && !FaceAnimation) { FaceImage.sprite = UIFace[i].sprite; }
                if (health >= UIFace[i].RequiredHealth && FaceAnimation) { FaceImage.transform.GetComponent<Animator>().Play(UIFace[i].anim.name); }
            }
        }

        // Health Regen
        IEnumerator HealthRegenCooldown()
        {
            AbilityManager am = (AbilityManager)Resources.Load("Ability");
            PassiveAbilitySO selectedPassiveSkill = am.passiveAbilityList[TinySaveSystem.GetInt("selectedPassiveSkill")].skill;
            yield return new WaitForSeconds(selectedPassiveSkill.HealthRegenerationWaitingTime);
            StartCoroutine(HealthRegen());
        }
        IEnumerator HealthRegen()
        {
            AbilityManager am = (AbilityManager)Resources.Load("Ability");
            PassiveAbilitySO selectedPassiveSkill = am.passiveAbilityList[TinySaveSystem.GetInt("selectedPassiveSkill")].skill;
            while (true)
            {
                AddHealth(selectedPassiveSkill.HealthRegenerationAmount, false);
                if (health == maxHealth) { StopCoroutine(HealthRegen()); }
                yield return new WaitForSeconds(selectedPassiveSkill.HealthRegenerationTimeInterval);
            }
        }

        // Armor Regen
        IEnumerator ArmorRegenCooldown()
        {
            AbilityManager am = (AbilityManager)Resources.Load("Ability");
            PassiveAbilitySO selectedPassiveSkill = am.passiveAbilityList[TinySaveSystem.GetInt("selectedPassiveSkill")].skill;
            yield return new WaitForSeconds(selectedPassiveSkill.ArmorRegenerationWaitingTime);
            StartCoroutine(ArmorRegen());
        }
        IEnumerator ArmorRegen()
        {
            AbilityManager am = (AbilityManager)Resources.Load("Ability");
            PassiveAbilitySO selectedPassiveSkill = am.passiveAbilityList[TinySaveSystem.GetInt("selectedPassiveSkill")].skill;
            while (true)
            {
                AddArmor(selectedPassiveSkill.ArmorRegenerationAmount, false);
                if (armor == maxArmor) { StopCoroutine(ArmorRegen()); }
                yield return new WaitForSeconds(selectedPassiveSkill.ArmorRegenerationTimeInterval);
            }
        }

        public void DamageIndicatorFunction(Transform source)
        {
            IndicatorPosition.Add(source);
            IndicatorTimeContainer.Add(IndicatorShowTime);
        }
    }

    /// Custom Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(PlayerHealth))]
    public class PlayerHealthEditor : Editor
    {
        private bool FloorBlood = false;
        private bool TakeDamage = false;
        private bool DeathSound = false;
        private bool FaceList = false;

        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            PlayerHealth player = (PlayerHealth)target;
            EditorGUI.indentLevel = 0;

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((PlayerHealth)target), typeof(PlayerHealth), false);
            GUI.enabled = true;

            EditorGUILayout.BeginVertical("box");

            TinyGUI.EditorTitle("Health Settings");
            float overload_health_value = player.health - player.maxHealth;
            if (overload_health_value < 0) { overload_health_value = 0; }
            float overload_health_max = player.godHealth - player.maxHealth;
            if (overload_health_max < 0) { overload_health_max = 0; }
            float overload_health_base = player.health;
            if (overload_health_base > player.maxHealth) { overload_health_base = player.maxHealth; }
            EditorGUILayout.Space(2);

            TinyGUI.ProgressBar(overload_health_value, overload_health_max, "Overload Health", Color.red);
            TinyGUI.ProgressBar(overload_health_base, player.maxHealth, "Normal Health", 54, Color.red);
            if (!Application.isPlaying) player.health = player.startHealth;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("godHealth"), new GUIContent("Overload Health"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHealth"), new GUIContent("Max Health"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startHealth"), new GUIContent("Start Health"));


            TinyGUI.EditorTitle("Armor Settings");
            float overload_armor_value = player.armor - player.maxArmor;
            if (overload_armor_value < 0) { overload_armor_value = 0; }
            float overload_armor_max = player.godArmor - player.maxArmor;
            if (overload_armor_max < 0) { overload_armor_max = 0; }
            float overload_armor_base = player.armor;
            if (overload_armor_base > player.maxArmor) { overload_armor_base = player.maxArmor; }
            EditorGUILayout.Space(2);

            TinyGUI.ProgressBar(overload_armor_value, overload_armor_max, "Overload Armor", new Color(.04f, .3f, .57f));
            TinyGUI.ProgressBar(overload_armor_base, player.maxArmor, "Normal Armor", 54, new Color(.04f, .3f, .57f));

            if (!Application.isPlaying) player.armor = player.startArmor;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("godArmor"), new GUIContent("Overload Armor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxArmor"), new GUIContent("Max Armor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startArmor"), new GUIContent("Start Armor"));

            TinyGUI.EditorTitle("Fall Damage");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("FallDamage"), new GUIContent("Damage Fall Curve"));

            TinyGUI.EditorTitle("Player FX");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("FloorBloodCount"), new GUIContent("Floor Blood Count"));
            serializedObject.FindProperty("FloorBloodChange").floatValue = EditorGUILayout.Slider("Floor Blood Change", serializedObject.FindProperty("FloorBloodChange").floatValue, 0, 100);
            TinyGUI.ShowArray(serializedObject, "FloorBlood", "Floor Blood", ref FloorBlood);

            TinyGUI.EditorTitle("UI Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OverHealthBar"), new GUIContent("Overload Health Bar"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("HealthBar"), new GUIContent("Health Bar"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("HealthText"), new GUIContent("Health Text"));
            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OverArmorBar"), new GUIContent("Overload Armor Bar"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ArmorBar"), new GUIContent("Armor Bar"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ArmorText"), new GUIContent("Armor Text"));

            TinyGUI.EditorTitle("Damage Indicator Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DamageIndicatorPrefab"), new GUIContent("Damage Indicator Prefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IndicatorParent"), new GUIContent("Indicator Parent"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IndicatorShowTime"), new GUIContent("Indicator Show Time"));

            TinyGUI.EditorTitle("Audio Settings");
            TinyGUI.ShowArray(serializedObject, "hit", "Take Damage Sound", ref TakeDamage);
            TinyGUI.ShowArray(serializedObject, "deathClip", "Death Sound", ref DeathSound);

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Face Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("FaceAnimation"), new GUIContent("Animation"));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("FaceImage"), new GUIContent("Face Image"));
            TinyGUI.ShowArray(serializedObject, "UIFace", "Face List", ref FaceList);
            for (int i = 0; i < player.UIFace.Count; i++)
            {
                player.UIFace[i].isAnim = player.FaceAnimation;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnTakeDamage"), new GUIContent("On Take Damage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnHeal"), new GUIContent("On Heal"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDeath"), new GUIContent("On Death"));

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

    [System.Serializable]
    public class UIFaceClass
    {
        [Min(0)] public int RequiredHealth;
        public Sprite sprite;
        public AnimationClip anim;

        public bool isAnim = false;
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(UIFaceClass))]
    public class UIFaceClassDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw the RequiredHealth field with the Min attribute
            property.FindPropertyRelative("RequiredHealth").intValue = EditorGUI.IntField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), new GUIContent("Required Health"), property.FindPropertyRelative("RequiredHealth").intValue);
            if (!property.FindPropertyRelative("isAnim").boolValue) property.FindPropertyRelative("sprite").objectReferenceValue = EditorGUI.ObjectField(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight), new GUIContent("Face Sprite"), property.FindPropertyRelative("sprite").objectReferenceValue, typeof(Sprite), false);
            if (property.FindPropertyRelative("isAnim").boolValue) property.FindPropertyRelative("anim").objectReferenceValue = EditorGUI.ObjectField(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight), new GUIContent("Face Animation"), property.FindPropertyRelative("anim").objectReferenceValue, typeof(AnimationClip), false);

            // Draw the sprite field with a custom label
            //SerializedProperty spriteProperty = property.FindPropertyRelative("sprite");
            //Rect spritePosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
            //EditorGUI.LabelField(spritePosition, "Custom Sprite Field");
            // spritePosition.y += EditorGUIUtility.singleLineHeight;
            //spriteProperty.objectReferenceValue = EditorGUI.ObjectField(spritePosition, spriteProperty.objectReferenceValue, typeof(Sprite), false);

            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 4;
        }
    }

#endif
}
