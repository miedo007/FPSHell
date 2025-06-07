using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using HellishBattle.Ability;
using HellishBattle.SaveSystem;

namespace HellishBattle.Weapon
{
    [RequireComponent(typeof(AudioSource))]
    public class MelleWeapon : BaseWeaponScript
    {
        //[Line("Base Settings")]
        public AlternativeMelleType AlternativeMode;
        [Range(0, 0180)] public float FOV = 60;
        public LayerMask enemyMask;
        public LayerMask occlusionMask;

        // Upgrade
        public List<MelleUpgradeClass> Upgrades;
        public List<WeaponSkinClass> Skins;

        //[Line("Base Attack")]
        public float weaponDamage = 30;
        public float impactForce = 20;
        public float range = 2.0f;
        public float angle = 90f;
        public float damageActivatorTime = 9f / 60;
        public float delay = .25f;
        public AnimationClip normalAttackAnimation;

        //[Line("Alternative Attack")]
        public float AlternativeWeaponDamage = 45;
        public float AlternativeImpactForce = 25;
        public float AlternativeRange = 2.0f;
        public float AlternativeAngle = 145f;
        public float AlternativeDamageActivatorTime = 9f / 60;
        public float AlternativeDelay = .35f;
        public AnimationClip AlternativeAttackAnimation;

        //[Line("Block")]
        [Suffix("%"), Range(0, 100)] public float BlockingDamageChange = 30;
        [Suffix("%"), Range(0, 100)] public float DamageReductionPercentage = 30;
        public AnimationClip BlockAnimation;

        //[Line("Sound")]
        public AudioClip attackSound;
        public AudioClip attackAlternativeSound;

        //[Line("Events")]
        public UnityEvent AdditionalBaseAttackFunction;
        public UnityEvent AdditionalAlternativeAttackFunction;
        public UnityEvent OnBlock;
        public UnityEvent OnBlockSuccess;
        public UnityEvent OnBlockFailure;

        // Privates
        [HideInInspector] public Animator anim;
        AudioSource source;
        Transform mainCamera;

        // Field on View
        List<GameObject> baseAttackTargetObjects = new List<GameObject>();
        List<GameObject> alternativeAttackTargetObjects = new List<GameObject>();
        Collider[] baseColliders = new Collider[50];
        Collider[] alternativeColliders = new Collider[50];
        int baseCount;
        int alternativeCount;
        Mesh angleMesh;
        Mesh AlternativeAngleMesh;

        float timer;
        private TinyInput tinyInput;

        private System.Action<InputAction.CallbackContext> FireStartedAction;
        private System.Action<InputAction.CallbackContext> AimStartedAction;
        private System.Action<InputAction.CallbackContext> AimCanceledAction;
        private System.Action<InputAction.CallbackContext> InspectStartedAction;

        void Awake()
        {
            tinyInput = InputManager.Instance.input;
            // GetComponents
            source = GetComponent<AudioSource>();
            anim = GetComponent<Animator>();

            // Passive
            GameSettingsManger _go = (GameSettingsManger)Resources.Load("Game_Settings");
            if (_go.EnablePlayerAbility)
            {
                PassiveAbilitySO _passive = GameManager.Instance.PassiveSkill;
                if (_passive.EnableMelleWeaponBoost)
                {
                    weaponDamage *= (int)_passive.MelleDamageIncrease;
                    delay *= (int)_passive.MelleFireRateIncrease;
                    AlternativeDelay *= (int)_passive.MelleFireRateIncrease;
                    range *= (int)_passive.MelleRangeIncrease;
                    AlternativeRange *= (int)_passive.MelleRangeIncrease;
                }
            }

            SetupInputAction();
        }

        void OnEnable()
        {
            mainCamera = Camera.main.transform;

            transform.parent.GetComponent<WeaponSwitch>().WeaponNameText.text = WeaponName();
            transform.parent.GetComponent<WeaponSwitch>().WeaponUIImage.sprite = weaponIcon;

            // Ready to Attack
            timer = 666;
            //tinyInput.Enable();

            // Add Input Action
            tinyInput.Weapon.Fire.started += FireStartedAction;
            tinyInput.Weapon.Aim.started += AimStartedAction;
            tinyInput.Weapon.Aim.canceled += AimCanceledAction;
            tinyInput.Weapon.Inspect.performed += InspectStartedAction;
        }

        public void OnDisable()
        {
            // Remove Input Action
            tinyInput.Weapon.Fire.started -= FireStartedAction;
            tinyInput.Weapon.Aim.started -= AimStartedAction;
            tinyInput.Weapon.Aim.canceled -= AimCanceledAction;
            tinyInput.Weapon.Inspect.performed -= InspectStartedAction;
        }

        public void SetupInputAction()
        {
            // Base Attack
            FireStartedAction = ctx =>
            {
                if (timer > delay && !Application.isMobilePlatform)
                {
                    StartCoroutine(BaseAttack());
                    timer = 0;
                }
            };

            // alternative Attack
            AimStartedAction = ctx =>
            {
                if (timer > AlternativeDelay && !Application.isMobilePlatform &&
                    AlternativeMode == AlternativeMelleType.AlternativeAttack)
                {
                    StartCoroutine(AlternativeAttack());
                    timer = 0;
                }
            };

            // Block
            AimStartedAction = ctx =>
            {
                if (timer > delay && !Application.isMobilePlatform &&
                    AlternativeMode == AlternativeMelleType.Block)
                {
                    anim.Play("Block", -1, 0f);
                    anim.SetBool("Block", true);
                    OnBlock.Invoke();
                }
            };
            AimCanceledAction = ctx =>
            {
                if (timer > delay && !Application.isMobilePlatform &&
                    AlternativeMode == AlternativeMelleType.Block)
                {
                    anim.SetBool("Block", false);
                }
            };

            // Inspect Weapon
            InspectStartedAction = ctx =>
            {
                // Get Shot Anim Lenght
                AnimationClip animationClip = null;
                foreach (AnimationClip clip in anim.runtimeAnimatorController
                             .animationClips)
                {
                    if (clip.name == "Attack")
                    {
                        animationClip = clip;
                    }
                }
                // if Shot Anim End play Inspect Anim
                if (timer <= 0 && animationClip != null)
                {
                    anim.Play("Inspect", -1, 0f);
                }
            };
        }

        public void Update()
        {


            // Object in Range
            baseCount = Physics.OverlapSphereNonAlloc(transform.position, range, baseColliders, enemyMask, QueryTriggerInteraction.Collide);
            alternativeCount = Physics.OverlapSphereNonAlloc(transform.position, AlternativeRange, alternativeColliders, enemyMask, QueryTriggerInteraction.Collide);

            baseAttackTargetObjects.Clear();
            alternativeAttackTargetObjects.Clear();

            for (int i = 0; i < baseCount; i++)
            {
                GameObject obj = baseColliders[i].gameObject;
                if (IsInSight(obj, angle))
                {
                    baseAttackTargetObjects.Add(obj);
                }
            }
            for (int i = 0; i < alternativeCount; i++)
            {
                GameObject obj = alternativeColliders[i].gameObject;
                if (IsInSight(obj, AlternativeAngle))
                {
                    alternativeAttackTargetObjects.Add(obj);
                }
            }
        }

        void FixedUpdate()
        {
            if (!GameManager.Instance.paused)
            {
                timer += Time.deltaTime;
                mainCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(mainCamera.GetComponent<Camera>().fieldOfView, FOV, .5f);
            }
        }

        IEnumerator BaseAttack()
        {
            anim.Play("Attack", 0, 0f);
            source.PlayOneShot(attackSound);
            anim.SetBool("Block", false);
            AdditionalBaseAttackFunction.Invoke();
            yield return new WaitForSeconds(damageActivatorTime);
            for (int i = 0; i < baseAttackTargetObjects.Count; i++)
            {
                baseAttackTargetObjects[i].SendMessage("GetDamage", new DamageClass(weaponDamage), SendMessageOptions.DontRequireReceiver);
                //baseAttackTargetObjects[i].SendMessage("ExplosionBlood", SendMessageOptions.DontRequireReceiver);
            }
        }

        IEnumerator AlternativeAttack()
        {
            anim.Play("Alternative", -1, 0f);
            source.PlayOneShot(attackAlternativeSound);
            anim.SetBool("Block", false);
            AdditionalAlternativeAttackFunction.Invoke();
            yield return new WaitForSeconds(AlternativeDamageActivatorTime);
            for (int i = 0; i < alternativeAttackTargetObjects.Count; i++)
            {
                alternativeAttackTargetObjects[i].SendMessage("GetDamage", new DamageClass(AlternativeWeaponDamage), SendMessageOptions.DontRequireReceiver);
                //alternativeAttackTargetObjects[i].SendMessage("ExplosionBlood", SendMessageOptions.DontRequireReceiver);
            }
        }

        public override void PrimaryModeFunction()
        {
            //
        }

        public override void SecondaryModeFunction()
        {
            //
        }

        public override void ReloadFunction()
        {
            // Do Nothing
        }

        Mesh CreateWedgeMesh(float angle, float range)
        {
            Mesh mesh = new Mesh();

            int segments = 16;
            int numTriangles = (segments * 4) + 2 + 2;
            int numVertices = numTriangles * 3;

            Vector3[] verticles = new Vector3[numVertices];
            int[] triangles = new int[numVertices];

            Vector3 bottomCenter = Vector3.down;
            Vector3 bottomLeft = Vector3.down + Quaternion.Euler(0, -angle, 0) * Vector3.forward * range;
            Vector3 bottomRight = Vector3.down + Quaternion.Euler(0, angle, 0) * Vector3.forward * range;

            Vector3 topCenter = bottomCenter + Vector3.up * 2;
            Vector3 topLeft = bottomLeft + Vector3.up * 2;
            Vector3 topRight = bottomRight + Vector3.up * 2;

            int vert = 0;

            // left side
            verticles[vert++] = bottomCenter;
            verticles[vert++] = bottomLeft;
            verticles[vert++] = topLeft;

            verticles[vert++] = topLeft;
            verticles[vert++] = topCenter;
            verticles[vert++] = bottomCenter;

            // right side
            verticles[vert++] = bottomCenter;
            verticles[vert++] = topCenter;
            verticles[vert++] = topRight;

            verticles[vert++] = topRight;
            verticles[vert++] = bottomRight;
            verticles[vert++] = bottomCenter;

            float currentAngle = -angle;
            float deltaAngle = (angle * 2) / segments;

            for (int i = 0; i < segments; i++)
            {
                bottomLeft = Vector3.down + Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * range;
                bottomRight = Vector3.down + Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * range;

                topLeft = bottomLeft + Vector3.up * 2;
                topRight = bottomRight + Vector3.up * 2;

                // far side
                verticles[vert++] = bottomLeft;
                verticles[vert++] = bottomRight;
                verticles[vert++] = topRight;

                verticles[vert++] = topRight;
                verticles[vert++] = topLeft;
                verticles[vert++] = bottomLeft;

                // top
                verticles[vert++] = topCenter;
                verticles[vert++] = topLeft;
                verticles[vert++] = topRight;

                // bottom
                verticles[vert++] = bottomCenter;
                verticles[vert++] = bottomRight;
                verticles[vert++] = bottomLeft;

                currentAngle += deltaAngle;
            }

            for (int i = 0; i < numVertices; i++)
            {
                triangles[i] = i;
            }

            mesh.vertices = verticles;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }

        private void OnValidate()
        {
            angleMesh = CreateWedgeMesh(angle, range);
            AlternativeAngleMesh = CreateWedgeMesh(AlternativeAngle, AlternativeRange);
        }

        private void OnDrawGizmos()
        {
            if (angleMesh)
            {
                Gizmos.color = new Color(1, .75f, 0, .5f);
                Gizmos.DrawMesh(angleMesh, transform.position, transform.rotation);

                Gizmos.DrawWireSphere(transform.position, range);
            }

            if (AlternativeAngleMesh && AlternativeMode == AlternativeMelleType.AlternativeAttack)
            {
                Gizmos.color = new Color(0, 1, 1, .2f);
                Gizmos.DrawMesh(AlternativeAngleMesh, transform.position, transform.rotation);

                Gizmos.DrawWireSphere(transform.position, AlternativeRange);
            }

            Gizmos.color = new Color(1, 0, 0, .2f);
            for (int i = 0; i < baseCount; i++)
            {
                Gizmos.DrawSphere(baseColliders[i].transform.position, 0.2f);
            }
            for (int i = 0; i < alternativeCount; i++)
            {
                Gizmos.DrawCube(alternativeColliders[i].transform.position, new Vector3(.15f, .15f, .15f));
            }

            Gizmos.color = new Color(0, 1, 0, .2f);
            foreach (var obj in baseAttackTargetObjects)
            {
                Gizmos.DrawSphere(obj.transform.position, 0.2f);
            }
            foreach (var obj in alternativeAttackTargetObjects)
            {
                Gizmos.DrawCube(obj.transform.position, new Vector3(.15f, .15f, .15f));
            }
        }

        public bool IsInSight(GameObject obj, float angle)
        {
            Vector3 origin = transform.position;
            Vector3 dest = obj.transform.position;
            Vector3 direction = dest - origin;
            if (direction.y < -3 || direction.y > 3)
            {
                return false;
            }

            direction.y = 0;
            float deltaAngle = Vector3.Angle(direction, transform.forward);
            if (deltaAngle > angle)
            {
                return false;
            }

            //origin.y += 
            dest.y = origin.y;
            if (Physics.Linecast(origin, dest, occlusionMask))
            {
                return false;
            }

            return true;
        }

        #region Skin & Upgrade
        public override void LoadUpgrade()
        {
            WeaponUpgradeSaveClass _tmpSaveInfo = TinySaveSystem.GetWeaponUpgrade($"Weapon_{WeaponID}");
            if (_tmpSaveInfo.selectedUpgrade != -1)
            {
                AlternativeMode = Upgrades[_tmpSaveInfo.selectedUpgrade].AlternativeMode;

                weaponDamage *= Upgrades[_tmpSaveInfo.selectedUpgrade].DamageIncrease;
                range *= Upgrades[_tmpSaveInfo.selectedUpgrade].RangeIncrease;
                angle = Upgrades[_tmpSaveInfo.selectedUpgrade].angle;
                damageActivatorTime = Upgrades[_tmpSaveInfo.selectedUpgrade].DamageDelay;
                delay = Upgrades[_tmpSaveInfo.selectedUpgrade].Delay;

                if (AlternativeMode == AlternativeMelleType.AlternativeAttack)
                {
                    AlternativeWeaponDamage *= Upgrades[_tmpSaveInfo.selectedUpgrade].AltDamageIncrease;
                    AlternativeRange *= Upgrades[_tmpSaveInfo.selectedUpgrade].AltRangeIncrease;
                    AlternativeAngle = Upgrades[_tmpSaveInfo.selectedUpgrade].AltAngle;
                    AlternativeDamageActivatorTime = Upgrades[_tmpSaveInfo.selectedUpgrade].AltDamageDelay;
                    AlternativeDelay = Upgrades[_tmpSaveInfo.selectedUpgrade].AltDelay;
                }
                if (AlternativeMode == AlternativeMelleType.Block)
                {
                    BlockingDamageChange = Upgrades[_tmpSaveInfo.selectedUpgrade].BlockingDamageChange;
                    DamageReductionPercentage = Upgrades[_tmpSaveInfo.selectedUpgrade].DamageReductionPercentage;
                }
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

    [CustomEditor(typeof(MelleWeapon))]
    public class MelleWeaponEditor : Editor
    {
        public bool Base = true;
        bool Upgrades = false;
        bool Skin = false;
        public bool Attack = true;
        public bool Block = true;
        public bool AltAttack = true;
        public bool Events = false;

        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            MelleWeapon weapon = (MelleWeapon)target;
            EditorGUI.indentLevel = 0;

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MelleWeapon)target), typeof(MelleWeapon), false);
            GUI.enabled = true;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ID", GUILayout.Width(50));
            weapon.WeaponID = EditorGUILayout.IntField(weapon.WeaponID);
            if (TinyGUI.IconButton("UnityLogo", "Generate", GUILayout.Height(EditorGUIUtility.singleLineHeight))) { weapon.WeaponID = Random.Range(111111111, 999999999); }
            EditorGUILayout.EndHorizontal();

            Base = TinyGUI.FoldoutGroup("Base Settings", Base);
            if (Base)
            {
                EditorGUILayout.BeginVertical("HelpBox");

                //EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponName"), new GUIContent("Weapon Name"));
                TinyGUI.LocalizedString(weapon.weaponName, serializedObject.FindProperty("weaponName"));
                TinyGUI.LocalizedString(weapon.weaponDesc, serializedObject.FindProperty("weaponDesc"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("Slot"), new GUIContent("Weapon Slot"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Category"), new GUIContent("Weapon Category"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("AlternativeMode"), new GUIContent("Alternative Mode"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("FOV"), new GUIContent("Field of View"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyMask"), new GUIContent("Enemy Mask"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("occlusionMask"), new GUIContent("Occlusion Mask"));

                TinyGUI.Title("Base Graphics");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponIcon"), new GUIContent("Weapon Icon"));

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

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("AlternativeMode"), new GUIContent("Alternative Mode"));

                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.LabelField("Base Attack", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("DamageIncrease"), new GUIContent("Damage Increase"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("RangeIncrease"), new GUIContent("Range Increase"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("angle"), new GUIContent("Attack Angle"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("DamageDelay"), new GUIContent("Animation Damage Delay"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("Delay"), new GUIContent("Delay"));
                    EditorGUILayout.EndVertical();

                    if (weapon.Upgrades[index].AlternativeMode == AlternativeMelleType.AlternativeAttack)
                    {
                        EditorGUILayout.BeginVertical("HelpBox");
                        EditorGUILayout.BeginVertical("HelpBox");
                        EditorGUILayout.LabelField("Alternative Attack", EditorStyles.boldLabel);
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("AltDamageIncrease"), new GUIContent("Damage Increase"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("AltRangeIncrease"), new GUIContent("Range Increas"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("AltAngle"), new GUIContent("Attack Angle"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("AltDamageDelay"), new GUIContent("Animation Damage Delay"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("AltDelay"), new GUIContent("Delay"));
                        EditorGUILayout.EndVertical();
                    }

                    if (weapon.Upgrades[index].AlternativeMode == AlternativeMelleType.Block)
                    {
                        EditorGUILayout.BeginVertical("HelpBox");
                        EditorGUILayout.BeginVertical("HelpBox");
                        EditorGUILayout.LabelField("Block", EditorStyles.boldLabel);
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("BlockingDamageChange"), new GUIContent("Blocking Change"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("DamageReductionPercentage"), new GUIContent("Damage Reduction"));
                        EditorGUILayout.EndVertical();
                    }

                    TinyGUI.InfoBox("Only the basic data will be overwritten, and all increases will remain unchanged");
                    if (TinyGUI.IconButton("SceneLoadIn", " Load Base Stats"))
                    {
                        weapon.Upgrades[i].AlternativeMode = weapon.AlternativeMode;

                        weapon.Upgrades[i].angle = weapon.angle;
                        weapon.Upgrades[i].DamageDelay = weapon.damageActivatorTime;
                        weapon.Upgrades[i].Delay = weapon.delay;

                        weapon.Upgrades[i].AltAngle = weapon.AlternativeAngle;
                        weapon.Upgrades[i].AltDamageDelay = weapon.AlternativeDamageActivatorTime;
                        weapon.Upgrades[i].AltDelay = weapon.AlternativeDelay;

                        weapon.Upgrades[i].BlockingDamageChange = weapon.BlockingDamageChange;
                        weapon.Upgrades[i].DamageReductionPercentage = weapon.DamageReductionPercentage;
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
                    weapon.Upgrades.Add(new MelleUpgradeClass());
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
                    weapon.Skins.Add(new MelleUpgradeClass());
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.EndVertical(); // End of List Vertical
            }


            Attack = TinyGUI.FoldoutGroup("Attack Settings", Attack);
            if (Attack)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                TinyGUI.InfoBox($"Base Attack\nDamage: {weapon.weaponDamage}\nFirerate: {(1 / weapon.delay).ToString("f2")}/s\nRange: {weapon.range}m\nDPS: {(weapon.weaponDamage * (1 / weapon.delay)).ToString("f2")}", "AudioMixerController Icon");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponDamage"), new GUIContent("Damage"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("impactForce"), new GUIContent("Impact Force"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("range"), new GUIContent("Range"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("angle"), new GUIContent("Angle"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("damageActivatorTime"), new GUIContent("Damage Anim Delay"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("delay"), new GUIContent("Delay"));
                EditorGUI.indentLevel = 1;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("normalAttackAnimation"), new GUIContent("Attack Aniamtion"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("attackSound"), new GUIContent("Attack Sound List"));
                EditorGUI.indentLevel = 0;
                EditorGUILayout.EndVertical();
            }

            AltAttack = TinyGUI.FoldoutGroup("Alternative Settings", AltAttack);
            if (AltAttack)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                if (weapon.AlternativeMode == AlternativeMelleType.AlternativeAttack)
                {
                    TinyGUI.InfoBox($"Alternative Attack\nDamage: {weapon.AlternativeWeaponDamage}\nFirerate: {(1 / weapon.AlternativeDelay).ToString("f2")}/s\nRange: {weapon.AlternativeRange}m\nDPS: {(weapon.AlternativeWeaponDamage * (1 / weapon.AlternativeDelay)).ToString("f2")}", "AudioMixerController Icon");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AlternativeWeaponDamage"), new GUIContent("Damage"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AlternativeImpactForce"), new GUIContent("Impact Force"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AlternativeRange"), new GUIContent("Range"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AlternativeAngle"), new GUIContent("Angle"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AlternativeDamageActivatorTime"), new GUIContent("Damage Anim Delay"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AlternativeDelay"), new GUIContent("Delay"));
                    EditorGUI.indentLevel = 1;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AlternativeAttackAnimation"), new GUIContent("Attack Aniamtion"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("attackAlternativeSound"), new GUIContent("Attack Sound List"));
                    EditorGUI.indentLevel = 0;
                }
                else
                {
                    TinyGUI.InfoBox("Select Block in Alternative Mode to view available options in this mode");
                }
                EditorGUILayout.EndVertical();
            }

            Block = TinyGUI.FoldoutGroup("Block Settings", Block);
            if (Block)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                if (weapon.AlternativeMode == AlternativeMelleType.Block)
                {
                    TinyGUI.InfoBox($"Block\nBlock Change: {weapon.BlockingDamageChange}%\nBlock Effectivity: {weapon.DamageReductionPercentage}%", "AudioMixerController Icon");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("BlockingDamageChange"), new GUIContent("Blocking Change"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DamageReductionPercentage"), new GUIContent("Damage Reduction"));
                    EditorGUI.indentLevel = 1;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("BlockAnimation"), new GUIContent("Block Animation List"));
                    EditorGUI.indentLevel = 0;
                }
                else
                {
                    TinyGUI.InfoBox("Select Block in Alternative Mode to view available options in this mode");
                }
                EditorGUILayout.EndVertical();
            }

            Events = TinyGUI.FoldoutGroup("Unity Events", Events);
            if (Events)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AdditionalBaseAttackFunction"), new GUIContent("On Base Attack"));
                if (weapon.AlternativeMode == AlternativeMelleType.AlternativeAttack) EditorGUILayout.PropertyField(serializedObject.FindProperty("AdditionalAlternativeAttackFunction"), new GUIContent("On Alternative Attack"));
                if (weapon.AlternativeMode == AlternativeMelleType.Block) EditorGUILayout.PropertyField(serializedObject.FindProperty("OnBlock"), new GUIContent("On Block"));
                if (weapon.AlternativeMode == AlternativeMelleType.Block) EditorGUILayout.PropertyField(serializedObject.FindProperty("OnBlockSuccess"), new GUIContent("On Block Success"));
                if (weapon.AlternativeMode == AlternativeMelleType.Block) EditorGUILayout.PropertyField(serializedObject.FindProperty("OnBlockFailure"), new GUIContent("On Block Failure"));
                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}
