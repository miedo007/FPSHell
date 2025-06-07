using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using HellishBattle.Ability;
using HellishBattle.Campaign;
using HellishBattle.Level;
using HellishBattle.Player;
using HellishBattle.SaveSystem;





#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

namespace HellishBattle.Enemies
{
    public class Enemy : MonoBehaviour
    {
        // Base
        public LocalizedString EnemyName;
        public EnemyType type;
        public int maxHealth;
        public float health;
        public GameObject EnemyUI;

        // Blood
        public GameObject[] Blood;
        public GameObject[] FloorBlood;

        // Score
        [Suffix("Score")] public int ScorePoints = 100;
        public EnemyScoreClass[] ScoreMultiplier;

        // Damage Multiplier
        public EnemyDamageMultiplierClass[] DamageMultiplier;

        // FX Settings
        [Range(0f, 100f)] public float BaseBloodChange = 100f;
        public int BaseBloodCount = 1;
        [Range(0f, 100f)] public float BaseFloorBloodChange = 75f;
        public int BaseFloorBloodCount = 1;
        // Extra Damage Settings
        public EnemyFXClass[] FXModifierList;
        public EnemyExtraFXClass[] ExtraFXList;

        public bool SpawnDeathBody = true;
        public Sprite DeathBodySprite;
        public Vector3 BodyColliderCenter = new Vector3(0, 0, 0);
        public Vector3 BodyColliderSize = new Vector3(1, 1, .1f);
        // Extra Death Settings
        public EnemyDeadBodyClass[] ExtraDeadBodyList;

        // Sounds
        public AudioClip[] RangeAttackSound;
        public AudioClip[] MelleAttackSound;
        public AudioClip[] BaseDamageSound;
        public AudioClip[] BaseDeathSound;
        public EnemySoundClass[] ExtraSoundList;

        // Drop Enemy
        public LootDrop lootDrop;
        public int RadnomLootChange;

        public UnityEvent DoBaseDamage;
        public UnityEvent DoBaseDeath;
        public UnityEvent DoWhenHeal;

        #region Private Values
        [HideInInspector] public AudioSource source;
        EnemyStates es;
        NavMeshAgent nma;
        SpriteRenderer sr;
        BoxCollider bc;
        GameSettingsManger _AM;
        float baseSpeed;
        [HideInInspector] public float PhaseDamageMultiplier = 1;
        [HideInInspector] public float PhaseSpeedMultiplier = 1;
        [HideInInspector] public float PhaseReduceDelayMultiplier = 1;
        [HideInInspector] public float healthMultiplier;
        [HideInInspector] public int secret_id;
        private Color DamageFlashColor;
        private Color HealFlashColor;

        private Slider HealthBar;
        private TMPro.TMP_Text NameText;
        #endregion

        // Set EnemyID
        private void Awake() { secret_id = Random.Range(0, 999999999); }

        private void Start()
        {
            // Get Difficulty & Game Settings
            _AM = Resources.Load<GameSettingsManger>("Game_Settings");
            DifficultyManager _tmp = (DifficultyManager)Resources.Load("difficulty");
            DifficultyLevel difficulty = _tmp.CurrentDifficultyLevel();

            // Get Enemy Scripts
            es = GetComponent<EnemyStates>();
            nma = GetComponent<NavMeshAgent>();
            sr = GetComponent<SpriteRenderer>();
            bc = GetComponent<BoxCollider>();
            source = GetComponent<AudioSource>();

            // Setup Enemy UI
            if (EnemyUI != null)
            {
                HealthBar = EnemyUI.transform.Find("HealthBar").GetComponent<Slider>();
                NameText = EnemyUI.transform.Find("Health_Txt").GetComponent<TMPro.TMP_Text>();
            }

            // Get Health Multipliers
            if (type == EnemyType.BaseEnemy) { maxHealth = (int)(maxHealth * difficulty.healthMultiplier) + (int)(maxHealth * healthMultiplier); }
            else if (type == EnemyType.BossEnemy) { maxHealth = (int)(maxHealth * difficulty.bossHealthMultiplier) + (int)(maxHealth * healthMultiplier); }
            else if (type == EnemyType.MinionEnemy) { maxHealth = (int)(maxHealth * difficulty.bossHealthMultiplier) + (int)(maxHealth * healthMultiplier); }

            // Get Speed Multipliers
            if (type == EnemyType.BaseEnemy) { nma.speed *= difficulty.speedMultiplier; baseSpeed = nma.speed; }
            else if (type == EnemyType.BossEnemy) { nma.speed *= difficulty.bossSpeedMultiplier; baseSpeed = nma.speed; }
            else if (type == EnemyType.MinionEnemy) { nma.speed *= difficulty.minionSpeedMultiplier; baseSpeed = nma.speed; }

            // Set Health
            health = maxHealth;
            // Setup Enemy UI
            if (EnemyUI != null)
            {
                // Show SLider
                if (_AM.EnemyHealthBar) { HealthBar.gameObject.SetActive(true); }
                else { HealthBar.gameObject.SetActive(false); }
                // Setup Value
                HealthBar.maxValue = maxHealth;
                HealthBar.value = maxHealth;

                // SHow Name
                if (_AM.EnemyName) { NameText.gameObject.SetActive(true); }
                else { NameText.gameObject.SetActive(false); }
                // Setup Value
                NameText.text = $"{EnemyName.GetLocalization()} [{(int)health}/{(int)maxHealth}]";
            }

            // Register Enemy in Level Manager
            LevelManager.Instance.Enemy.Add(this);
            LevelManager.Instance.FindEnemy.Add(false);

            // Get Flash Color
            DamageFlashColor = _AM.DamageFlashEnemy;
            HealFlashColor = _AM.HealFlashEnemy;
        }

        private void Update()
        {
            // Update Enemy Flash
            Color col = transform.GetComponent<SpriteRenderer>().material.GetColor("_Color");
            transform.GetComponent<SpriteRenderer>().material.SetColor("_Color", Color.Lerp(col, new Color(1, 1, 1), .1f));

            // Update Speed with Phase
            if (health > 0) { nma.speed = baseSpeed * PhaseSpeedMultiplier; }
        }

        public void GetDamage(DamageClass Class)
        {
            // Damage Multiplier
            for (int i = 0; i < DamageMultiplier.Length; i++)
            {
                // Do Modified FX
                if (Class.Type == DamageMultiplier[i].Type)
                {
                    Class.Damage *= DamageMultiplier[i].Multiplier;
                }
            }
            // Base Damage
            health -= Class.Damage;
            if (EnemyUI != null) HealthBar.value = health;
            if (EnemyUI != null) NameText.text = $"{EnemyName.GetLocalization()} [{(int)health}/{(int)maxHealth}]";

            // Do when Enemy is Alive
            if (health > 0 && es.enabled)
            {
                // Flash Enemy
                if (_AM.EnemyDamageFlash) transform.GetComponent<SpriteRenderer>().material.SetColor("_Color", DamageFlashColor);

                // Spawn FX
                bool _DefualtFX = true;
                for (int i = 0; i < FXModifierList.Length; i++)
                {
                    // Do Modified FX
                    if (Class.Type == FXModifierList[i].Type)
                    {
                        _DefualtFX = false;

                        // Spawn Normal FX
                        SpawnFX(FXModifierList[i].FXCount, FXModifierList[i].FXChange, Class.HitPosition, Blood);
                        // Spawn Floor FX
                        SpawnFloorFX(FXModifierList[i].FloorFXCount, FXModifierList[i].FloorFXChange, FloorBlood);
                    }
                }
                // Do Base FX
                if (_DefualtFX)
                {
                    // Spawn Normal FX
                    SpawnFX(BaseBloodCount, BaseBloodChange, Class.HitPosition, Blood);
                    // Spawn Floor FX
                    SpawnFloorFX(BaseFloorBloodCount, BaseFloorBloodChange, FloorBlood);
                }
                // Spawn Extra FX
                for (int i = 0; i < ExtraFXList.Length; i++)
                {
                    // Do Modified FX
                    if (Class.Type == ExtraFXList[i].Type)
                    {
                        _DefualtFX = false;

                        if (ExtraFXList[i].ExtraFXChange < 0 && ExtraFXList[i].ExtraFX.Length != 0)
                        {
                            SpawnFX(ExtraFXList[i].ExtraFXCount, ExtraFXList[i].ExtraFXChange, Class.HitPosition, ExtraFXList[i].ExtraFX);
                        }
                        // Extra Floor FX
                        if (ExtraFXList[i].ExtraFloorFXChange < 0 && ExtraFXList[i].ExtraFloorFX.Length != 0)
                        {
                            SpawnFloorFX(ExtraFXList[i].ExtraFloorFXCount, ExtraFXList[i].ExtraFloorFXChange, ExtraFXList[i].ExtraFloorFX);
                        }
                    }
                }

                // Spawn FX
                bool _DefualtSound = true;
                for (int i = 0; i < ExtraSoundList.Length; i++)
                {
                    // Do Modified FX
                    if (Class.Type == ExtraSoundList[i].Type)
                    {
                        _DefualtSound = ExtraSoundList[i].PlayOriginal;

                        // Play Extra Sound
                        if (ExtraSoundList[i].DamageSound.Length > 0) source.PlayOneShot(ExtraSoundList[i].DamageSound[Random.Range(0, ExtraSoundList[i].DamageSound.Length)]);
                    }
                }
                // Do Base FX
                if (_DefualtSound)
                {
                    if (BaseDamageSound.Length > 0) source.PlayOneShot(BaseDamageSound[Random.Range(0, BaseDamageSound.Length)]);
                }

                // Invoke Base Damage
                DoBaseDamage.Invoke();
            }

            // Do When Enemy was Died
            else if (health <= 0 && es.enabled)
            {
                // Remove all Children in Enemy Object
                foreach (Transform child in transform) { GameObject.Destroy(child.gameObject); }

                // Register Death in Level Manager
                LevelManager.Instance.TryFindEnemy(this);

                // Reset Flash Enemy Color
                transform.GetComponent<SpriteRenderer>().material.SetColor("_Color", new Color(1, 1, 1));

                // Disable Scripts
                es.enabled = false;
                nma.enabled = false;
                transform.GetComponent<DynamicBillboardChange>().enabled = false;
                transform.GetComponent<Animator>().enabled = false;

                // Disable Enemy Ability
                if (TryGetComponent(out EnemyAbility ability)) { ability.enabled = false; }

                // Spawn Drop
                SpawnDrop();

                // Score Points 
                bool _DefualtScore = true;
                for (int i = 0; i < ScoreMultiplier.Length; i++)
                {
                    // Do Modified
                    if (Class.Type == ScoreMultiplier[i].Type)
                    {
                        _DefualtScore = false;

                        // Add Score After Death
                        GameManager.Instance.AddPoints((int)(ScorePoints * ScoreMultiplier[i].Multiplier));
                    }
                }
                // Do Base
                if (_DefualtScore)
                {
                    // Add Score After Death
                    GameManager.Instance.AddPoints((int)ScorePoints);
                }

                // Dead Body
                bool _DefualtBody = true;
                for (int i = 0; i < ExtraDeadBodyList.Length; i++)
                {
                    // Do Modified
                    if (Class.Type == ExtraDeadBodyList[i].Type)
                    {
                        _DefualtBody = false;

                        if (ExtraDeadBodyList[i].SpawnDeathBody)
                        {
                            sr.sprite = ExtraDeadBodyList[i].DeathBodySprite;
                            bc.center = ExtraDeadBodyList[i].DeathBodyColliderCenter;
                            bc.size = ExtraDeadBodyList[i].DeathBodyColliderSize;
                        }
                        else { Destroy(this.transform.gameObject); }
                    }
                }
                // Do Base
                if (_DefualtBody)
                {
                    if (SpawnDeathBody)
                    {
                        if (SpawnDeathBody)
                        {
                            sr.sprite = DeathBodySprite;
                            bc.center = BodyColliderCenter;
                            bc.size = BodyColliderSize;
                        }
                        else { Destroy(this.transform.gameObject); }
                    }
                }

                // Sound
                bool _DefualtSound = true;
                for (int i = 0; i < ExtraSoundList.Length; i++)
                {
                    // Do Modified
                    if (Class.Type == ExtraSoundList[i].Type)
                    {
                        _DefualtSound = ExtraSoundList[i].PlayOriginal;

                        // Play Death Sound
                        if (ExtraSoundList[i].DeathSound.Length > 0) source.PlayOneShot(ExtraSoundList[i].DeathSound[Random.Range(0, ExtraSoundList[i].DeathSound.Length)]);
                    }
                }
                // Do Base
                if (_DefualtSound)
                {
                    // Play Death Sound
                    if (BaseDeathSound.Length > 0) source.PlayOneShot(BaseDeathSound[Random.Range(0, BaseDeathSound.Length)]);
                }

                // Death Invoke
                DoBaseDeath.Invoke();
            }

            /* PASSIVE SKILLS */
            // Vampirizm
            AbilityManager am = (AbilityManager)Resources.Load("Ability");
            PassiveAbilitySO selectedPassiveSkill = am.passiveAbilityList[TinySaveSystem.GetInt("selectedPassiveSkill")].skill;
            if (selectedPassiveSkill.EnableHealthVampirizm)
            {
                if (Random.Range(0f, 1f) > 1 - selectedPassiveSkill.HealthVampirizmChange)
                {
                    GameManager.Instance.transform.parent.GetComponent<PlayerHealth>().AddHealth(Class.Damage * selectedPassiveSkill.HealthVampirizmPercent, false);
                }
            }
            if (selectedPassiveSkill.EnableArmorVampirizm)
            {
                if (Random.Range(0f, 1f) > 1 - selectedPassiveSkill.ArmorVampirizmChange)
                {
                    GameManager.Instance.transform.parent.GetComponent<PlayerHealth>().AddArmor(Class.Damage * selectedPassiveSkill.ArmorVampirizmPercent, false);
                }
            }
        }

        void SpawnFX(int Count, float Change, Vector3 Pos, GameObject[] FX)
        {
            for (int i = 0; i < Count; i++)
            {
                if (Random.Range(0f, 100f) <= Change)
                {
                    GameObject _temp = Instantiate(FX[Random.Range(0, FX.Length)], this.transform.position, this.transform.rotation, this.transform);
                    if (Pos == new Vector3(0, 0, 0)) { _temp.transform.position += new Vector3(Random.Range(bc.size.x / -1.75f, bc.size.x / 1.75f), Random.Range(bc.size.y / -1.75f, bc.size.y / 1.75f), Random.Range(-.1f, .1f)); }
                    else { _temp.transform.position = Pos; }
                }
            }
        }

        void SpawnFloorFX(int Count, float Change, GameObject[] FX)
        {
            for (int i = 0; i < Count; i++)
            {
                if (FX.Length > 0)
                {
                    int random = Random.Range(0, FX.Length);
                    if (Random.Range(0f, 100f) <= Change && FX[random] != null)
                    {
                        GameObject _temp = Instantiate(FX[random], this.transform.position, new Quaternion(0, 0, 0, 0));
                        _temp.transform.position += new Vector3(Random.Range(-1f, 1f), Random.Range(-.02f, .02f), Random.Range(-1f, 1f));
                        int rotate = Random.Range(0, 8) * 90;
                        _temp.transform.rotation = Quaternion.Euler(0f, rotate, 0f) * _temp.transform.rotation;
                    }
                }
            }
        }

        // Heal
        public void Heal(float value)
        {
            // Heal Enemy
            health += value;
            if (health > maxHealth) { health = maxHealth; }

            // Setup Enemy UI
            if (EnemyUI != null) HealthBar.value = health;
            if (EnemyUI != null) NameText.text = $"{EnemyName.GetLocalization()} [{(int)health}/{(int)maxHealth}]";

            // FLash Enemy on Heal
            if (_AM.EnemyDamageFlash) transform.GetComponent<SpriteRenderer>().material.SetColor("_Color", HealFlashColor);

            // Invoke OnHeal Events
            DoWhenHeal.Invoke();
        }

        // Spawn Drop when Death
        void SpawnDrop()
        {
            DifficultyManager _tmp = (DifficultyManager)Resources.Load("difficulty");
            DifficultyLevel difficulty = _tmp.CurrentDifficultyLevel();

            if (Random.Range(0f, 1f) >= 1 - difficulty.lootDropChange)
            {
                List<GameObject> guaranteed = lootDrop.GetGuaranteeedLoot();
                List<GameObject> randomLoot = lootDrop.GetRandomLoot(RadnomLootChange);

                for (int i = 0; i < guaranteed.Count; i++)
                {
                    GameObject tmp = Instantiate(guaranteed[i], new Vector3(transform.position.x + Random.Range(-1, 1), transform.position.y, transform.position.z + Random.Range(-1, 1)), Quaternion.identity);
                    if (guaranteed[i].TryGetComponent<BonusScript>(out BonusScript bonus))
                    {
                        Debug.Log(LevelManager.Instance.Items[LevelManager.Instance.Items.Count - 1]);
                        LevelManager.Instance.Items.RemoveAt(LevelManager.Instance.Items.Count - 1);
                        bonus.AutoRecordInLevelManager = false;
                    }

                }

                for (int i = 0; i < randomLoot.Count; i++)
                {
                    GameObject tmp = Instantiate(randomLoot[i], new Vector3(transform.position.x + Random.Range(-1, 1), transform.position.y, transform.position.z + Random.Range(-1, 1)), Quaternion.identity);
                    if (randomLoot[i].TryGetComponent<BonusScript>(out BonusScript bonus))
                    {
                        //Debug.Log(LevelManager.Instance.Items[LevelManager.Instance.Items.Count - 1]);
                        // FIX THIS SHIT !!!
                        //LevelManager.Instance.Items.RemoveAt(LevelManager.Instance.Items.Count - 1);
                        bonus.AutoRecordInLevelManager = false;
                    }
                }
            }
        }
    }

    /// Custom Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(Enemy))]
    [CanEditMultipleObjects]
    public class EnemyEditor : Editor
    {
        private bool DamageMultiplier = false;
        private bool Blood = false;
        private bool FloorBlood = false;
        private bool ScoreMultiplier = false;
        private bool FXModifierList = false;
        private bool ExtraFXList = false;
        private bool ExtraDeadBodyList = false;
        private bool RangeAttackSound = false;
        private bool MelleAttackSound = false;
        private bool BaseDamageSound = false;
        private bool BaseDeathSound = false;
        private bool ExtraSoundList = false;

        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            Enemy enemy = (Enemy)target;
            EditorGUI.indentLevel = 0;
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((Enemy)target), typeof(Enemy), false);
            GUI.enabled = true;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("EnemyName"), new GUIContent("Enemy Name"));
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Enemy Health", EditorStyles.boldLabel);
            enemy.type = (EnemyType)EditorGUILayout.EnumPopup(enemy.type, EditorStyles.toolbarDropDown, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
            TinyGUI.ProgressBar(enemy.health, enemy.maxHealth, "Health");
            if (Application.isEditor && !Application.isPlaying) enemy.health = enemy.maxHealth;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHealth"), new GUIContent("Max Health"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("EnemyUI"), new GUIContent("Enemy UI"));
            TinyGUI.ShowArray(serializedObject, "DamageMultiplier", "Damage Multiplier", ref DamageMultiplier);

            TinyGUI.EditorTitle("Blood Settings");
            TinyGUI.ShowArray(serializedObject, "Blood", "Blood List", ref Blood);
            TinyGUI.ShowArray(serializedObject, "FloorBlood", "Floor Blood List", ref FloorBlood);

            TinyGUI.EditorTitle("Score after Death");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ScorePoints"), new GUIContent("Score after Death"));
            TinyGUI.ShowArray(serializedObject, "ScoreMultiplier", "Score Multiplier", ref ScoreMultiplier);

            TinyGUI.EditorTitle("FX Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("BaseBloodChange"), new GUIContent("Base FX Change"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("BaseBloodCount"), new GUIContent("Base FX Count"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("BaseFloorBloodChange"), new GUIContent("Floor FX Change"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("BaseFloorBloodCount"), new GUIContent("Floor FX Count"));
            TinyGUI.ShowArray(serializedObject, "FXModifierList", "FX Modifier", ref FXModifierList);
            TinyGUI.ShowArray(serializedObject, "ExtraFXList", "Extra FX", ref ExtraFXList);

            TinyGUI.EditorTitle("Dead Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SpawnDeathBody"), new GUIContent("Spawn Death Body"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DeathBodySprite"), new GUIContent("Death Body Sprite"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("BodyColliderCenter"), new GUIContent("Body Collider Center"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("BodyColliderSize"), new GUIContent("Body Collider Size"));
            TinyGUI.ShowArray(serializedObject, "ExtraDeadBodyList", "Extra Dead Body Settings", ref ExtraDeadBodyList);

            TinyGUI.EditorTitle("Drop Loot Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lootDrop"), new GUIContent("Loot Drop"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("RadnomLootChange"), new GUIContent("Change to Drop"));

            TinyGUI.EditorTitle("Sound Settings");
            TinyGUI.ShowArray(serializedObject, "RangeAttackSound", "Range Attack Sound", ref RangeAttackSound);
            TinyGUI.ShowArray(serializedObject, "MelleAttackSound", "Melle Attack Sound", ref MelleAttackSound);
            TinyGUI.ShowArray(serializedObject, "BaseDamageSound", "Base Damage Sound", ref BaseDamageSound);
            TinyGUI.ShowArray(serializedObject, "BaseDeathSound", "Base Death Sound", ref BaseDeathSound);
            TinyGUI.ShowArray(serializedObject, "ExtraSoundList", "Extra Sound List", ref ExtraSoundList);

            EditorGUILayout.EndVertical();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("DoWhenHeal"), new GUIContent("On Heal"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DoBaseDamage"), new GUIContent("On Damage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DoBaseDeath"), new GUIContent("On Death"));


            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

    public enum EnemyType
    {
        BaseEnemy = 0, BossEnemy = 1, MinionEnemy = 2
    }

    [System.Serializable]
    public class EnemyDeadBodyClass
    {
        public DamageType Type = DamageType.Normal;

        public bool SpawnDeathBody = true;
        public Sprite DeathBodySprite;
        public Vector3 DeathBodyColliderCenter = new Vector3(0, .5f, 0);
        public Vector3 DeathBodyColliderSize = new Vector3(1, 1, .1f);
    }

    [System.Serializable]
    public class EnemyFXClass
    {
        public DamageType Type = DamageType.Normal;

        //[Header("Blood FX Settings")]
        [Range(0f, 100f)] public float FXChange = 100f;
        [Min(0)] public int FXCount = 1;
        [Range(0f, 100f)] public float FloorFXChange = 75f;
        [Min(0)] public int FloorFXCount = 1;

        public EnemyFXClass()
        {
            // Change
            FXChange = 100;
            FloorFXChange = 75;
            // Count
            FXCount = 1;
            FloorFXCount = 1;
        }
    }

    [System.Serializable]
    public class EnemyExtraFXClass
    {
        public DamageType Type = DamageType.Normal;

        //[Header("Extra FX Settings")]
        [Range(0f, 100f)] public float ExtraFXChange = 90f;
        [Min(0)] public int ExtraFXCount = 0;
        public GameObject[] ExtraFX;
        //[Space(5)]
        [Range(0f, 100f)] public float ExtraFloorFXChange = 90f;
        [Min(0)] public int ExtraFloorFXCount = 0;
        public GameObject[] ExtraFloorFX;

        public EnemyExtraFXClass()
        {
            // Change
            ExtraFXChange = 90;
            ExtraFloorFXChange = 90;
            // Count
            ExtraFXCount = 1;
            ExtraFloorFXCount = 1;
            // Arrays
            ExtraFX = new GameObject[0];
            ExtraFloorFX = new GameObject[0];
        }
    }
    [System.Serializable]
    public class EnemySoundClass
    {
        public DamageType Type = DamageType.Normal;

        public bool PlayOriginal = true;
        public AudioClip[] DamageSound;
        public AudioClip[] DeathSound;
    }

    [System.Serializable]
    public class EnemyScoreClass
    {
        public DamageType Type = DamageType.Normal;
        [Range(0.5f, 5f)] public float Multiplier = 1;
    }

    [System.Serializable]
    public class EnemyDamageMultiplierClass
    {
        public DamageType Type = DamageType.Normal;
        [Range(0.25f, 5f)] public float Multiplier = 1;
    }
}

