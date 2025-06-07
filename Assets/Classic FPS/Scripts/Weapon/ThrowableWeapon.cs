using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using HellishBattle.Enemies;
using HellishBattle.SaveSystem;

namespace HellishBattle.Weapon
{

    [RequireComponent(typeof(AudioSource))]
    public class ThrowableWeapon : BaseWeaponScript
    {
        [Range(10, 170)] public float FOV = 60;
        public AmmoType AmmoType;
        public LayerMask RaycastMask;

        // Upgrade
        public List<ThrowableUpgradeClass> Upgrades;
        public List<WeaponSkinClass> Skins;

        // Base Settings
        public ThrowableItemType ItemType;
        [Suffix("Damage")] public float Damage = 80;
        public DamageType TypeDamage;
        //      Bullet Settings
        public GameObject ProjectileObject;
        [Suffix("KG")] public float ProjectileMass = 2f;
        public bool ProjectileUseGravity = true;
        public PhysicMaterial ProjectileMaterial;
        public bool BreakOnCollision = false;
        public AudioClip CollisionSound;
        public float HitForce = 40;
        public List<GameObject> CollisionFX;
        public List<GameObject> ActivationFX;
        //      Explosion
        [Suffix("Meters")] public float ExplosionRadius = 2.5f;
        public AnimationCurve ExplosionDamageCurve;
        [Suffix("Meters")] public float ExplosionShakeDistance = 5.5f;
        public GameObject ExplosionPrefab;
        public AudioClip ExplosionSound;
        //      Damage Zone
        [Suffix("Seconds")] public float DamageZoneDuration = 6;
        [Suffix("Seconds")] public float DamageZoneDMGInterval = .2f;
        [Suffix("Meters")] public float DamageZoneRadius = 2f;
        [Suffix("Meters")] public float MaxGroundDistance = .25f;
        //      Items

        // Base Throw
        [Suffix("Seconds")] public float BaseTimeLifeItem = 3f;
        [Suffix("Seconds")] public float BaseTimeBetweenThrow = .75f;
        public float BaseThrowForce = 25f;
        [Suffix("Seconds")] public float BaseSpawnAfter = .4f;
        public List<AudioClip> BaseThrowSound;
        public List<AnimationClip> BaseThrowAnimation;

        // Alternative Throw
        public bool AlternativeThrow = true;
        [Suffix("Seconds")] public float AlternativeTimeLifeItem = 3f;
        [Suffix("Seconds")] public float AlternativeTimeBetweenThrow = .75f;
        public float AlternativeThrowForce = 25f;
        [Suffix("Seconds")] public float AlternativeSpawnAfter = .4f;
        public List<AudioClip> AlternativeThrowSound;
        public List<AnimationClip> AlternativeThrowAnimation;

        // Objects
        public GameObject SpawnpointPrefab;

        // Events
        public UnityEvent OnActivate, OnCollider, OnThrow, OnAltThrow;

        // Privates
        AudioSource sound;
        Animator anim;
        Transform mainCamera;
        float timer;
        int itemCount = -1;
        TinyInput tinyInput;

        private System.Action<InputAction.CallbackContext> FireStartedAction;
        private System.Action<InputAction.CallbackContext> AimStartedAction;
        private System.Action<InputAction.CallbackContext> InspectStartedAction;

        void Awake()
        {
            // Setup Private Componenets
            tinyInput = InputManager.Instance.input;
            sound = GetComponent<AudioSource>();
            anim = GetComponent<Animator>();

            // Setup Available Ammo
            if (AmmoType != AmmoType.InfinityAmmo) itemCount = GameManager.Instance.AmmoLeft(AmmoType);

            // Setup Spawnpoint
            SpawnpointPrefab = Camera.main.transform.GetChild(0).gameObject;
            SetupInputAction();
        }

        void OnEnable()
        {
            // Setup Main Camera
            mainCamera = Camera.main.transform;

            // Setup Available Ammo
            if (AmmoType != AmmoType.InfinityAmmo) itemCount = GameManager.Instance.AmmoLeft(AmmoType);

            // Setup Weapon UI (Weapon Switch)
            transform.parent.GetComponent<WeaponSwitch>().WeaponNameText.text = WeaponName();
            transform.parent.GetComponent<WeaponSwitch>().WeaponUIImage.sprite = weaponIcon;

            // Reset Timer (Ready to Throw)
            timer = 666;

            // Add Input Action
            tinyInput.Weapon.Fire.started += FireStartedAction;
            tinyInput.Weapon.Aim.started += AimStartedAction;
            tinyInput.Weapon.Inspect.performed += InspectStartedAction;
        }

        public void OnDisable()
        {
            // Remove Input Action
            tinyInput.Weapon.Fire.started -= FireStartedAction;
            tinyInput.Weapon.Aim.started -= AimStartedAction;
            tinyInput.Weapon.Inspect.performed -= InspectStartedAction;
        }

        public void SetupInputAction()
        {
            // Main Throw Input
            FireStartedAction = ctx =>
            {
                if (timer > BaseTimeBetweenThrow)
                {
                    if (itemCount != 0)
                    {
                        StartCoroutine(BaseItemThrow());
                    }
                }
            };

            // Alternative Throw Input
            AimStartedAction = ctx =>
            {
                if (AlternativeThrow && timer > AlternativeTimeBetweenThrow)
                {
                    if (itemCount != 0)
                    {
                        StartCoroutine(AlternativeItemThrow());
                    }
                }
            };

            // Inspect Weapon
            InspectStartedAction = ctx =>
            {
                // Get Shot Anim Lenght
                AnimationClip animationClip = null;
                foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips)
                {
                    if (clip.name == "Attack")
                    {
                        animationClip = clip;
                    }
                }
                // if Shot Anim End play Inspect Anim
                if (timer > animationClip.length && animationClip != null) anim.Play("Inspect", -1, 0f);
            };
        }

        void Update()
        {
            if (!GameManager.Instance.paused)
            {
                // Setup Weapon UI (Weapon Switch)
                transform.parent.GetComponent<WeaponSwitch>().ammoText.text = itemCount.ToString();

                // Update Timer
                timer += Time.deltaTime;

                // Update Field of View
                mainCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(mainCamera.GetComponent<Camera>().fieldOfView, FOV, .5f);
            }
        }

        // Invoce when Left Click Mouse (Defualt)
        IEnumerator BaseItemThrow()
        {
            // Play Random Animation
            anim.Play(BaseThrowAnimation[Random.Range(0, BaseThrowAnimation.Count)].name, -1, 0f);

            // Update Ammo
            itemCount--;
            if (AmmoType != AmmoType.InfinityAmmo) { GameManager.Instance.AddAmmo(AmmoType, -1); }

            // Reset Timer
            timer = 0;

            // Wait X Seconds to Spawn Object
            yield return new WaitForSeconds(BaseSpawnAfter);

            // Play Throw Audio
            sound.PlayOneShot(BaseThrowSound[Random.Range(0, BaseThrowSound.Count)]);

            // Generate Item
            GenerateThrowableItem(true);
        }

        // Invoce when Right Click Mouse (Defualt)
        IEnumerator AlternativeItemThrow()
        {
            // Play Random Animation
            anim.Play(AlternativeThrowAnimation[Random.Range(0, AlternativeThrowAnimation.Count)].name, -1, 0f);

            // Update Ammo
            itemCount--;
            if (AmmoType != AmmoType.InfinityAmmo) { GameManager.Instance.AddAmmo(AmmoType, -1); }

            // Reset Timer
            timer = 0;

            // Wait X Seconds to Spawn Object
            yield return new WaitForSeconds(AlternativeSpawnAfter);

            // Play Throw Audio
            sound.PlayOneShot(AlternativeThrowSound[Random.Range(0, AlternativeThrowSound.Count)]);

            // Generate Item
            GenerateThrowableItem(false);
        }

        // Spawn Throwed Item
        public void GenerateThrowableItem(bool Base)
        {
            // Spawn Object
            GameObject projectileInstantiated = (GameObject)Instantiate(ProjectileObject, SpawnpointPrefab.transform.position, Quaternion.identity);

            // Velocity 
            float Force = 0;
            float lifeTime = 0;

            // Setup Value
            if (Base)
            {
                Force = BaseThrowForce;
                lifeTime = BaseTimeLifeItem;
                OnThrow.Invoke();
            }
            else
            {
                Force = AlternativeThrowForce;
                lifeTime = AlternativeTimeLifeItem;
                OnAltThrow.Invoke();
            }


            projectileInstantiated.GetComponent<BulletScript>().SetupType(ItemType);
            projectileInstantiated.GetComponent<BulletScript>().SetupBase(Damage, TypeDamage, HitForce, lifeTime);
            projectileInstantiated.GetComponent<BulletScript>().SetupObject(ProjectileUseGravity, ProjectileMass, ProjectileMaterial, BreakOnCollision, CollisionSound);

            if (ActivationFX.Count != 0) { projectileInstantiated.GetComponent<BulletScript>().SetupActivateFX(ActivationFX); }

            if (ItemType == ThrowableItemType.Explosion) projectileInstantiated.GetComponent<BulletScript>().SetupExplosion(ExplosionRadius, RaycastMask, ExplosionPrefab, ExplosionSound, ExplosionDamageCurve, ExplosionShakeDistance);
            if (ItemType == ThrowableItemType.Normal) projectileInstantiated.GetComponent<BulletScript>().SetupNormal(RaycastMask);
            if (ItemType == ThrowableItemType.DamageZone) projectileInstantiated.GetComponent<BulletScript>().SetupDamageZone(RaycastMask, (Damage / (DamageZoneDuration / DamageZoneDMGInterval)), DamageZoneDuration, DamageZoneDMGInterval, DamageZoneRadius, MaxGroundDistance);



            // Spread
            var randomNumberX = Random.Range(-DynamicCrosshair.spread / 900, DynamicCrosshair.spread / 900);
            var randomNumberY = Random.Range(-DynamicCrosshair.spread / 900, DynamicCrosshair.spread / 900);
            var randomNumberZ = Random.Range(-DynamicCrosshair.spread / 900, DynamicCrosshair.spread / 900);
            Vector3 RandomSpread = new Vector3(randomNumberX, randomNumberY, randomNumberZ);

            // Moving Item
            Rigidbody rocketRb = projectileInstantiated.GetComponent<Rigidbody>();
            rocketRb.AddForce((Camera.main.transform.forward + RandomSpread) * Force, ForceMode.Impulse);
        }

        #region Skin & Upgrade
        public override void LoadUpgrade()
        {
            WeaponUpgradeSaveClass _tmpSaveInfo = TinySaveSystem.GetWeaponUpgrade($"Weapon_{WeaponID}");
            if (_tmpSaveInfo.selectedUpgrade != -1)
            {
                ItemType = Upgrades[_tmpSaveInfo.selectedUpgrade].ItemType;

                Damage = Upgrades[_tmpSaveInfo.selectedUpgrade].Damage;
                ProjectileMass = Upgrades[_tmpSaveInfo.selectedUpgrade].ProjectileMass;
                ProjectileMaterial = Upgrades[_tmpSaveInfo.selectedUpgrade].ProjectileMaterial;
                BreakOnCollision = Upgrades[_tmpSaveInfo.selectedUpgrade].BreakOnCollision;

                ExplosionRadius = Upgrades[_tmpSaveInfo.selectedUpgrade].ExplosionRadius;
                ExplosionDamageCurve = Upgrades[_tmpSaveInfo.selectedUpgrade].ExplosionDamageCurve;
                ExplosionShakeDistance = Upgrades[_tmpSaveInfo.selectedUpgrade].ExplosionShakeDistance;
                ExplosionPrefab = Upgrades[_tmpSaveInfo.selectedUpgrade].ExplosionPrefab;
                ExplosionSound = Upgrades[_tmpSaveInfo.selectedUpgrade].ExplosionSound;

                DamageZoneDuration = Upgrades[_tmpSaveInfo.selectedUpgrade].DamageZoneDuration;
                DamageZoneDMGInterval = Upgrades[_tmpSaveInfo.selectedUpgrade].DamageZoneDMGInterval;
                DamageZoneRadius = Upgrades[_tmpSaveInfo.selectedUpgrade].DamageZoneRadius;
                MaxGroundDistance = Upgrades[_tmpSaveInfo.selectedUpgrade].MaxGroundDistance;

                BaseTimeLifeItem = Upgrades[_tmpSaveInfo.selectedUpgrade].BaseTimeLifeItem;
                BaseTimeBetweenThrow = Upgrades[_tmpSaveInfo.selectedUpgrade].BaseTimeBetweenThrow;
                BaseThrowForce = Upgrades[_tmpSaveInfo.selectedUpgrade].BaseThrowForce;
                BaseSpawnAfter = Upgrades[_tmpSaveInfo.selectedUpgrade].BaseSpawnAfter;

                AlternativeThrow = Upgrades[_tmpSaveInfo.selectedUpgrade].AlternativeThrow;
                AlternativeTimeLifeItem = Upgrades[_tmpSaveInfo.selectedUpgrade].AlternativeTimeLifeItem;
                AlternativeTimeBetweenThrow = Upgrades[_tmpSaveInfo.selectedUpgrade].AlternativeTimeBetweenThrow;
                AlternativeThrowForce = Upgrades[_tmpSaveInfo.selectedUpgrade].AlternativeThrowForce;
                AlternativeSpawnAfter = Upgrades[_tmpSaveInfo.selectedUpgrade].AlternativeSpawnAfter;

            }
        }

        public override void LoadSkin()
        {
            WeaponUpgradeSaveClass _tmpSaveInfo = TinySaveSystem.GetWeaponUpgrade($"Weapon_{WeaponID}");
            // Setup Skin

            if (_tmpSaveInfo.selectedSkin != -1) { anim.runtimeAnimatorController = Skins[_tmpSaveInfo.selectedSkin].SkinAnimation; }
            else if (_tmpSaveInfo.selectedUpgrade != -1)
            {
                // If Upgrade Icon isn't Empty
                if (Upgrades[_tmpSaveInfo.selectedUpgrade].SkinUIIcon != null) { anim.runtimeAnimatorController = Upgrades[_tmpSaveInfo.selectedUpgrade].SkinAnimation; }
            }
        }

        public override List<WeaponSkinClass> GetSkinClass() { return Skins; }
        public override List<WeaponUpgradeClass> GetUpgradeClass()
        {
            List<WeaponUpgradeClass> allUpgrades = new List<WeaponUpgradeClass>();
            allUpgrades.AddRange(Upgrades);
            return allUpgrades;
        }

        public override Sprite WeaponIcon()
        {
            WeaponUpgradeSaveClass _tmpSaveInfo = TinySaveSystem.GetWeaponUpgrade($"Weapon_{WeaponID}");

            // If Skin is Selected
            if (_tmpSaveInfo.selectedSkin != -1)
            {
                return Skins[_tmpSaveInfo.selectedSkin].SkinUIIcon;
            }
            // If Skin isn't selected and Upgrades is Selected
            else if (_tmpSaveInfo.selectedUpgrade != -1)
            {
                // If Upgrade Icon isn't Empty
                if (Upgrades[_tmpSaveInfo.selectedUpgrade].SkinUIIcon != null) { return Upgrades[_tmpSaveInfo.selectedUpgrade].SkinUIIcon; }
                // if Empty display Defualt
                return weaponIcon;
            }
            // If Skin and Upgrade isn't Selected
            else
            {
                return weaponIcon;
            }
        }

        public override string WeaponName()
        {
            WeaponUpgradeSaveClass _tmpSaveInfo = TinySaveSystem.GetWeaponUpgrade($"Weapon_{WeaponID}");

            // Upgrades is Selected
            if (_tmpSaveInfo.selectedUpgrade != -1)
            {
                return Upgrades[_tmpSaveInfo.selectedUpgrade].WeaponName.GetLocalization();
            }
            // If Upgrade isn't Selected
            else
            {
                return weaponName.GetLocalization();
            }
        }

        #endregion
    }

    /// Custom Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(ThrowableWeapon))]
    public class ThrowableWeaponEditor : Editor
    {
        public bool UI = true;
        public bool Base = true;
        bool Upgrades = false;
        bool Skin = false;
        public bool BaseThrow = true;
        public bool AltThrow = true;
        public bool Events = false;
        //
        public bool _BaseSound;
        public bool _BaseThrow;
        public bool _AltSound;
        public bool _AltThrow;

        bool CollisionFX, ActivateFX;

        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            ThrowableWeapon weapon = (ThrowableWeapon)target;
            EditorGUI.indentLevel = 0;

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((ThrowableWeapon)target), typeof(ThrowableWeapon), false);
            GUI.enabled = true;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ID", GUILayout.Width(50));
            weapon.WeaponID = EditorGUILayout.IntField(weapon.WeaponID);
            if (TinyGUI.IconButton("UnityLogo", "Generate", GUILayout.Height(EditorGUIUtility.singleLineHeight))) { weapon.WeaponID = Random.Range(111111111, 999999999); }
            EditorGUILayout.EndHorizontal();

            UI = TinyGUI.FoldoutGroup("UI Settings", UI);
            if (UI)
            {
                EditorGUILayout.BeginVertical("HelpBox");

                EditorGUILayout.PropertyField(serializedObject.FindProperty("Slot"), new GUIContent("Weapon Slot"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Category"), new GUIContent("Weapon Category"));
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponName"), new GUIContent("Weapon Name"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponIcon"), new GUIContent("Weapon Icon"));
                EditorGUILayout.Space(5);

                if (weapon.ItemType == ThrowableItemType.Explosion) { TinyGUI.InfoBox("The object, upon hitting a surface or the passage of time, explodes, inflicting damage in its area"); }
                if (weapon.ItemType == ThrowableItemType.DamageZone) { TinyGUI.InfoBox("The object, after hitting the surface or the passage of time, creates a Damage Zone over a certain area and certain values"); }
                if (weapon.ItemType == ThrowableItemType.Normal) { TinyGUI.InfoBox("An object, upon hitting a surface, inflicts damage on the object it hits"); }
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ItemType"), new GUIContent("Item Type"));

                EditorGUILayout.EndVertical();
            }

            Base = TinyGUI.FoldoutGroup("Base Settings", Base);
            if (Base)
            {
                EditorGUILayout.BeginVertical("HelpBox");

                TinyGUI.InfoBox($"Item Type: {weapon.ItemType}\nDamage: {weapon.Damage} {weapon.TypeDamage} Damage\nAmmo Type: {weapon.AmmoType}\nBreak on Collision: {weapon.BreakOnCollision}", "AudioMixerController Icon");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("TypeDamage"), new GUIContent("Damage Type"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Damage"), new GUIContent("Damage"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AmmoType"), new GUIContent("Ammo Type"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastMask"), new GUIContent("Raycast Mask"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("FOV"), new GUIContent("Field of View"));

                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.LabelField("Projectile Settings", EditorStyles.boldLabel);
                EditorGUILayout.EndVertical();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ProjectileObject"), new GUIContent("Projectile Object"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ProjectileMass"), new GUIContent("Mass"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ProjectileUseGravity"), new GUIContent("Use Gravity"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ProjectileMaterial"), new GUIContent("Physic Material"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BreakOnCollision"), new GUIContent("Activate On Collision"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CollisionSound"), new GUIContent("Collision Sound"));
                TinyGUI.ShowArray(serializedObject, "CollisionFX", "Collision FX", ref CollisionFX);
                TinyGUI.ShowArray(serializedObject, "ActivationFX", "Activation FX", ref ActivateFX);
                EditorGUILayout.EndVertical();

                if (weapon.ItemType == ThrowableItemType.Explosion)
                {
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.LabelField("Explosion Settings", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ExplosionRadius"), new GUIContent("Radius"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ExplosionDamageCurve"), new GUIContent("Damage Curve"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ExplosionShakeDistance"), new GUIContent("Shake Distance"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ExplosionPrefab"), new GUIContent("Prefab"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ExplosionSound"), new GUIContent("Sound"));
                    EditorGUILayout.EndVertical();
                }
                if (weapon.ItemType == ThrowableItemType.DamageZone)
                {
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.LabelField("Damage Zone Item Settings", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();
                    TinyGUI.InfoBox($"Damage Zone for {weapon.DamageZoneDuration} seconds, every {(weapon.DamageZoneDMGInterval).ToString("f2")} seconds will inflict {(weapon.Damage / (weapon.DamageZoneDuration / weapon.DamageZoneDMGInterval)).ToString("f2")} damage. (Total {weapon.Damage}  Damage).");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DamageZoneDuration"), new GUIContent("DZ Duration"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DamageZoneDMGInterval"), new GUIContent("DZ Damage Interval"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DamageZoneRadius"), new GUIContent("DZ Radius"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxGroundDistance"), new GUIContent("DZ Spawn Max Distance"));
                    EditorGUILayout.EndVertical();
                }
                if (weapon.ItemType == ThrowableItemType.Normal)
                {
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.LabelField("Item Settings", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();
                    TinyGUI.InfoBox("This type of object has no additional variables");
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();
            }

            Upgrades = TinyGUI.FoldoutGroup($"Upgrades [{weapon.Upgrades.Count}]", "LightmapParameters Icon", Upgrades);

            if (Upgrades)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                for (int i = 0; i < weapon.Upgrades.Count; i++)
                {
                    int index = i;
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.BeginVertical("HelpBox");

                    EditorGUILayout.Space(-8);
                    TinyGUI.Title("Name & Price");
                    TinyGUI.LocalizedString(weapon.Upgrades[i].WeaponName, serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("WeaponName"));
                    TinyGUI.LocalizedString(weapon.Upgrades[i].UpgradeDescription, serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("UpgradeDescription"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("Cost"), new GUIContent("Unlock Cost"));

                    TinyGUI.Title("Graphics Setup");
                    if (weapon.Upgrades[i].SkinAnimation == null) TinyGUI.InfoBox("If \"Upgrade Animations\" is empty then script loads default weapon animation");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("SkinAnimation"), new GUIContent("Upgrade Animations"));
                    if (weapon.Upgrades[i].SkinAnimation == null) TinyGUI.InfoBox("If \"Upgrade Icon\" is empty then script loads default weapon icon");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("SkinUIIcon"), new GUIContent("Upgrade Icon"));

                    TinyGUI.Title("Modified Stats", "Here you can change the weapon stats that will be applied when you select an upgrade");

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("ItemType"), new GUIContent("Item Type"));

                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.LabelField("Base", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("Damage"), new GUIContent("Damage Increase"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("ProjectileMass"), new GUIContent("Projectile Mass"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("ProjectileMaterial"), new GUIContent("Projectile Material"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("BreakOnCollision"), new GUIContent("Break On Collision"));
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.LabelField("Base Throw", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("BaseTimeLifeItem"), new GUIContent("Time Life Item"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("BaseTimeBetweenThrow"), new GUIContent("Time Between Throw"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("BaseThrowForce"), new GUIContent("Throw Force"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("BaseSpawnAfter"), new GUIContent("Spawn After"));
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.LabelField("Alternative Throw", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("AlternativeThrow"), new GUIContent("Alternative Throw Enable"));
                    if (weapon.Upgrades[index].AlternativeThrow)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("AlternativeTimeLifeItem"), new GUIContent("Time Life Item"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("AlternativeTimeBetweenThrow"), new GUIContent("Time Between Throw"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("AlternativeThrowForce"), new GUIContent("Throw Force"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("AlternativeSpawnAfter"), new GUIContent("Spawn After"));
                    }
                    EditorGUILayout.EndVertical();

                    if (weapon.Upgrades[index].ItemType == ThrowableItemType.Explosion)
                    {
                        EditorGUILayout.BeginVertical("HelpBox");
                        EditorGUILayout.BeginVertical("HelpBox");
                        EditorGUILayout.LabelField("Explosion", EditorStyles.boldLabel);
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("ExplosionRadius"), new GUIContent("Explosion Radius"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("ExplosionDamageCurve"), new GUIContent("Explosion Damage Curve"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("ExplosionShakeDistance"), new GUIContent("Explosion Shake Distance"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("ExplosionPrefab"), new GUIContent("Explosion Prefab"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("ExplosionSound"), new GUIContent("Explosion Sound"));
                        EditorGUILayout.EndVertical();
                    }
                    if (weapon.Upgrades[index].ItemType == ThrowableItemType.DamageZone)
                    {

                        EditorGUILayout.BeginVertical("HelpBox");
                        EditorGUILayout.BeginVertical("HelpBox");
                        EditorGUILayout.LabelField("Damage Zone", EditorStyles.boldLabel);
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("DamageZoneDuration"), new GUIContent("DZ Duration"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("DamageZoneDMGInterval"), new GUIContent("DZ DMG Interval"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("DamageZoneRadius"), new GUIContent("DZ Radius"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("MaxGroundDistance"), new GUIContent("Max Ground Distance"));
                        EditorGUILayout.EndVertical();
                    }

                    TinyGUI.InfoBox("Only the basic data will be overwritten, and all increases will remain unchanged");
                    if (TinyGUI.IconButton("SceneLoadIn", " Load Base Stats"))
                    {
                        weapon.Upgrades[i].ItemType = weapon.ItemType;

                        weapon.Upgrades[i].Damage = weapon.Damage;
                        weapon.Upgrades[i].ProjectileMass = weapon.ProjectileMass;
                        weapon.Upgrades[i].ProjectileMaterial = weapon.ProjectileMaterial;
                        weapon.Upgrades[i].BreakOnCollision = weapon.BreakOnCollision;

                        if (weapon.ItemType == ThrowableItemType.Explosion)
                        {
                            weapon.Upgrades[i].ExplosionRadius = weapon.ExplosionRadius;
                            weapon.Upgrades[i].ExplosionDamageCurve = weapon.ExplosionDamageCurve;
                            weapon.Upgrades[i].ExplosionShakeDistance = weapon.ExplosionShakeDistance;
                            weapon.Upgrades[i].ExplosionPrefab = weapon.ExplosionPrefab;
                            weapon.Upgrades[i].ExplosionSound = weapon.ExplosionSound;
                        }

                        if (weapon.ItemType == ThrowableItemType.DamageZone)
                        {
                            weapon.Upgrades[i].DamageZoneDuration = weapon.DamageZoneDuration;
                            weapon.Upgrades[i].DamageZoneDMGInterval = weapon.DamageZoneDMGInterval;
                            weapon.Upgrades[i].DamageZoneRadius = weapon.DamageZoneRadius;
                            weapon.Upgrades[i].MaxGroundDistance = weapon.MaxGroundDistance;
                        }

                        weapon.Upgrades[i].BaseTimeLifeItem = weapon.BaseTimeLifeItem;
                        weapon.Upgrades[i].BaseTimeBetweenThrow = weapon.BaseTimeBetweenThrow;
                        weapon.Upgrades[i].BaseThrowForce = weapon.BaseThrowForce;
                        weapon.Upgrades[i].BaseSpawnAfter = weapon.BaseSpawnAfter;

                        weapon.Upgrades[i].AlternativeThrow = weapon.AlternativeThrow;
                        if (weapon.AlternativeThrow)
                        {
                            weapon.Upgrades[i].AlternativeTimeLifeItem = weapon.AlternativeTimeLifeItem;
                            weapon.Upgrades[i].AlternativeTimeBetweenThrow = weapon.AlternativeTimeBetweenThrow;
                            weapon.Upgrades[i].AlternativeThrowForce = weapon.AlternativeThrowForce;
                            weapon.Upgrades[i].AlternativeSpawnAfter = weapon.AlternativeSpawnAfter;
                        }
                    }

                    EditorGUILayout.EndVertical();

                    if (TinyGUI.IconButton("d_P4_DeletedLocal", "", GUILayout.Width(25)))
                    {
                        weapon.Upgrades.RemoveAt(index);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (TinyGUI.IconButton("Toolbar Plus", $"Add New Weapon Upgrade"))
                {
                    weapon.Upgrades.Add(new ThrowableUpgradeClass());
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.EndVertical(); // End of List Vertical
            }

            Skin = TinyGUI.FoldoutGroup($"Skins [{weapon.Skins.Count}]", "Cloth Icon", Skin);
            if (Skin)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                for (int i = 0; i < weapon.Skins.Count; i++)
                {
                    int index = i;
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.BeginVertical("HelpBox");

                    EditorGUILayout.Space(-8);
                    TinyGUI.Title("Name & Price");
                    TinyGUI.LocalizedString(weapon.Skins[i].WeaponName, serializedObject.FindProperty("Skins").GetArrayElementAtIndex(index).FindPropertyRelative("WeaponName"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Skins").GetArrayElementAtIndex(index).FindPropertyRelative("Cost"), new GUIContent("Unlock Cost"));

                    TinyGUI.Title("Graphics Setup");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Skins").GetArrayElementAtIndex(index).FindPropertyRelative("SkinAnimation"), new GUIContent("Skin Animations"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Skins").GetArrayElementAtIndex(index).FindPropertyRelative("SkinUIIcon"), new GUIContent("Skin Icon"));

                    EditorGUILayout.EndVertical();

                    if (TinyGUI.IconButton("d_P4_DeletedLocal", "", GUILayout.Width(25)))
                    {
                        weapon.Skins.RemoveAt(index);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (TinyGUI.IconButton("Toolbar Plus", $"Add New Weapon Skin"))
                {
                    weapon.Skins.Add(new RangeUpgradeClass());
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.EndVertical(); // End of List Vertical
            }


            BaseThrow = TinyGUI.FoldoutGroup("Base Throw Settings", BaseThrow);
            if (BaseThrow)
            {
                EditorGUILayout.BeginVertical("HelpBox");

                EditorGUILayout.PropertyField(serializedObject.FindProperty("BaseTimeLifeItem"), new GUIContent("Item Life Time"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BaseTimeBetweenThrow"), new GUIContent("Throw Cooldown"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BaseThrowForce"), new GUIContent("Throw Force"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BaseSpawnAfter"), new GUIContent("Spawn Delay"));
                TinyGUI.ShowArray(serializedObject, "BaseThrowSound", "Throw Sound", ref _BaseSound);
                TinyGUI.ShowArray(serializedObject, "BaseThrowAnimation", "Throw Animation", ref _BaseThrow);

                EditorGUILayout.EndVertical();
            }

            AltThrow = TinyGUI.FoldoutGroup("Alternative Throw Settings", AltThrow);
            if (AltThrow)
            {
                EditorGUILayout.BeginVertical("HelpBox");

                EditorGUILayout.PropertyField(serializedObject.FindProperty("AlternativeThrow"), new GUIContent("Enable Alternative Throw"));
                if (weapon.AlternativeThrow)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AlternativeTimeLifeItem"), new GUIContent("Item Life Time"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AlternativeTimeBetweenThrow"), new GUIContent("Throw Cooldown"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AlternativeThrowForce"), new GUIContent("Throw Force"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AlternativeSpawnAfter"), new GUIContent("Spawn Delay"));
                    TinyGUI.ShowArray(serializedObject, "AlternativeThrowSound", "Throw Sound", ref _AltSound);
                    TinyGUI.ShowArray(serializedObject, "AlternativeThrowAnimation", "Throw Animation", ref _AltThrow);
                }
                else
                {
                    TinyGUI.InfoBox("This item has the ability to throw it with basic values, to add an alternative throwing mode you must enable AlternativeThrow");
                }

                EditorGUILayout.EndVertical();
            }

            Events = TinyGUI.FoldoutGroup("Unity Events Settings", Events);
            if (Events)
            {
                EditorGUILayout.BeginVertical("HelpBox");

                //EditorGUILayout.PropertyField(serializedObject.FindProperty("OnActivate"), new GUIContent("On Activation"));
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("OnCollider"), new GUIContent("On Collider"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnThrow"), new GUIContent("On Base Throw"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnAltThrow"), new GUIContent("On Alternative Throw"));

                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

    [System.Serializable]
    public enum ThrowableItemType
    {
        Normal = 0,
        Explosion = 1,
        DamageZone = 2
    }
}