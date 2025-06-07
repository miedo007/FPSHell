using HellishBattle.Level;
using HellishBattle.Weapon;
using UnityEditor;
using UnityEngine;


namespace HellishBattle
{
    public class BonusScript : MonoBehaviour
    {
        public LocalizedString PickUpLocalization;
        public LocalizedString BonusLocalization;

        //[Line("Bonus Type")]
        public BonusType bonusType;
        public bool godBonus;

        // [Line("Auto Record")]
        public bool AutoRecordInLevelManager = true;


        //[Line("Bonus Settings")]
        public int Health;
        public int Armor;
        public AmmoType ammoType;
        public int AmmoAmount;
        public KeyType keyType;
        public Transform weaponPrefab;

        //[Line("Score Bonus")]
        [Suffix("Score")] public int ScorePoints = 50;

        [HideInInspector] public int secret_id;

        private void Awake()
        {
            secret_id = Random.Range(0, 999999999);
            if (bonusType == BonusType.Weapon) BonusLocalization = weaponPrefab.GetComponent<BaseWeaponScript>().weaponName;

            if (TryGetComponent<LightSourceSetup>(out LightSourceSetup light) && weaponPrefab != null)
            {
                // Scale Icon
                float scale = 1;
                Texture _tmp = weaponPrefab.GetComponent<BaseWeaponScript>().weaponIcon.texture;

                if (_tmp.width > _tmp.height)
                {
                    scale = _tmp.width / 32f;
                }
                else if (_tmp.width < _tmp.height)
                {
                    scale = _tmp.height / 32f;
                }
                else
                {
                    scale = _tmp.width / 32f;
                }


                light.scale = scale;
                light.EmissionTexture = weaponPrefab.GetComponent<BaseWeaponScript>().weaponIcon.texture;
            }
        }

        public void Start()
        {
            if (AutoRecordInLevelManager)
            {
                LevelManager.Instance.Items.Add(this);
                LevelManager.Instance.FindItems.Add(false);
            }

            if (weaponPrefab != null && bonusType == BonusType.Weapon) transform.GetComponent<SpriteRenderer>().sprite = weaponPrefab.GetComponent<BaseWeaponScript>().weaponIcon;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && bonusType != BonusType.Weapon)
            {
                LevelManager.Instance.TryFindItem(this);
            }
            if (bonusType == BonusType.Weapon)
            {
                // Do Nothing Now
            }
        }
    }

    /// Custom Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(BonusScript))]
    public class BonusScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            BonusScript bonus = (BonusScript)target;
            TinyGUI.DrawScript<BonusScript>("Pick-UP Script", target);

            TinyGUI.BeginBoxGroup("Base");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bonusType"), new GUIContent("Pick-Up Category"));
            TinyGUI.EndBoxGroup();

            TinyGUI.BeginBoxGroup("Pop-Up UI Text");
            TinyGUI.LocalizedString(bonus.PickUpLocalization, serializedObject.FindProperty("PickUpLocalization"));
            TinyGUI.LocalizedString(bonus.BonusLocalization, serializedObject.FindProperty("BonusLocalization"));
            TinyGUI.EndBoxGroup();

            TinyGUI.BeginBoxGroup("Pick-UP Reward");
            TinyGUI.InfoBox("I recommend disabling \"Record in Level Manager\" only if the bonus in question is in a hidden level, or is generated as a random Loot");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("AutoRecordInLevelManager"), new GUIContent("Record in Level Manager"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ScorePoints"), new GUIContent("Score Points"));

            if (bonus.bonusType == BonusType.Health)
            {
                TinyGUI.Title("Health Pick-Up");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Health"), new GUIContent("Health Bonus"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("godBonus"), new GUIContent("Value Over The Limit"));
            }
            else if (bonus.bonusType == BonusType.Armor)
            {
                TinyGUI.Title("Armor Pick-Up");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Armor"), new GUIContent("Armor Bonus"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("godBonus"), new GUIContent("Value Over The Limit"));
            }
            else if (bonus.bonusType == BonusType.Ammo)
            {
                TinyGUI.Title("Ammo Pick-Up");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ammoType"), new GUIContent("Ammp Type"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AmmoAmount"), new GUIContent("Ammo Bonus"));
            }
            else if (bonus.bonusType == BonusType.Key)
            {
                TinyGUI.Title("Key Pick-Up");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("keyType"), new GUIContent("Key Type"));
            }
            else if (bonus.bonusType == BonusType.Weapon)
            {
                TinyGUI.Title("Weapon Pick-Up");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponPrefab"), new GUIContent("Weapon Prefab"));
                // Update UI 
                if (bonus.weaponPrefab != null) bonus.GetComponent<SpriteRenderer>().sprite = bonus.weaponPrefab.GetComponent<BaseWeaponScript>().weaponIcon;
            }
            else if (bonus.bonusType == BonusType.HealthAndArmor)
            {
                TinyGUI.Title("Health & Armor Pick-Up");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Health"), new GUIContent("Health Bonus"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Armor"), new GUIContent("Armor Bonus"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("godBonus"), new GUIContent("Value Over The Limit"));
            }

            TinyGUI.EndBoxGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

    public enum BonusType
    {
        Health = 0, Armor = 1, HealthAndArmor = 6, Ammo = 2, Key = 3, Weapon = 4, Score = 5
    }
    public enum KeyType
    {
        None = 0, RedKey = 1, BlueKey = 2, YellowKey = 3, GreenKey = 4
    }
}