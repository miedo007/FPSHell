using HellishBattle.Ability;
using HellishBattle.Effects;
using HellishBattle.Enemies;
using HellishBattle.Level;
using HellishBattle.Mobile;
using HellishBattle.Weapon;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HellishBattle.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        // Speed
        public float playerWalkingSpeed = 6f;
        public float playerRunningSpeed = 11f;
        public float playerCrouchSpeed = 4f;
        // Player Height
        public float NormalPlayerHeight = 1.8f;
        public float CrouchPlayerHeight = 1.2f;
        // Camera Height
        public float NormalCameraHeight = .75f;
        public float CrouchCameraHeight = .6f;
        // Stamina System
        public bool EnableStamina = true;
        public float RunningStaminaConsume = 5f;
        public float JumpStaminaConsume = 15f;
        public float StaminaRegenerationInterval = 2f;
        public float StaminaRegenerationSpeed = 10f;
        // Jump Settings
        public float jumpStrength = 20f;
        public int maxNumberOfJump = 1;
        // Ladder Settings
        public float LadderSpeed = 3.2f;
        // Rotation
        public float verticalRotationLimit = 80f;

        // Save Object with Components
        public AudioClip pickupSound;
        public FlashScreen flash;
        public MobileJoystick MovementJoystick;
        public MobileJoystick CameraJoystick;
        public MovementState state;

        [HideInInspector] public Vector3 additionalMovement;
        [HideInInspector] public float stamina = 100;
        [HideInInspector] public float weaponSpeedMultiplier;

        // Sccript values
        float forwardMovement;
        float sidewaysMovement;
        public float verticalVelocity;
        float verticalRotation = 0;
        float horizontalRotation = 0;
        int numbersOfJumps;

        private bool isFalling = false;
        [HideInInspector] public Vector3 fallStartPos;
        private float _ccHeight;
        private float _cameraMidpoint;
        float lastUseStamina = 999;

        // Scripts Components
        GameObject MainCamera;
        GameObject WeaponCamera;
        GameObject WeaponsContainer;

        GameObject PopupScreen;
        TinyInput tinyInput;
        [HideInInspector] public CharacterController cc;
        AudioSource source;
        GameSettingsManger _gm;


        private Vector3 currentVelocity;
        void Awake()
        {
            // Setup Camera
            MainCamera = transform.Find("Main Camera").gameObject;
            WeaponCamera = transform.Find("Weapon Camera").gameObject;
            WeaponsContainer = transform.Find("Weapons").gameObject;

            tinyInput = InputManager.Instance.input;
            var rebinds = PlayerPrefs.GetString("rebinds");
            tinyInput.asset.LoadBindingOverridesFromJson(rebinds);

            cc = GetComponent<CharacterController>();
            source = GetComponent<AudioSource>();
            PopupScreen = transform.Find("Main Panels").Find("PickUp Panel").gameObject;

            _gm = (GameSettingsManger)Resources.Load("Game_Settings");
            if (_gm.EnablePlayerAbility)
            {
                PassiveAbilitySO _passive = GameManager.Instance.PassiveSkill;
                if (_passive.BoostSpeed)
                {
                    playerRunningSpeed *= _passive.SpeedIncrease;
                    playerWalkingSpeed *= _passive.SpeedIncrease;
                }
                if (_passive.EnableJumpModifier)
                {
                    jumpStrength = _passive.JumpStrenght;
                    maxNumberOfJump = _passive.MaxJumpCount;
                }
                if (_passive.EnableStaminaSize)
                {
                    stamina = _passive.StaminaSize;
                }
                if (_passive.EnableStaminaConsume)
                {
                    JumpStaminaConsume = _passive.JumpStaminaConsume;
                    RunningStaminaConsume = _passive.RunningStaminaConsume;
                }
                if (_passive.EnableStaminaRegeneration)
                {
                    StaminaRegenerationInterval = _passive.StaminaRegenerationInterval;
                    StaminaRegenerationSpeed = _passive.StaminaRegenerationSpeed;
                }
            }

            // Setup Stamina
            if (EnableStamina) transform.Find("Canvas").Find("UI").Find("Stamina_Bar").GetComponent<Slider>().maxValue = stamina;
        }
        public void OnEnable() { tinyInput.Enable(); }
        public void OnDisable() { tinyInput.Disable(); }

        public Vector2 MovementValue()
        {
            float SpeedMultiplier = 1;
            if (Camera.main.transform.parent.TryGetComponent<StatusEffectManager>(out StatusEffectManager EffectPool))
            {
                // Check Movement Speed Boost (Status Effect)
                if (EffectPool.IsActive("Move_B") != null) { SpeedMultiplier *= float.Parse(EffectPool.IsActive("Move_B")[0]); }
                // Check Movement Speed Nerf (Status Effect)
                if (EffectPool.IsActive("Move_N") != null) { SpeedMultiplier *= float.Parse(EffectPool.IsActive("Move_N")[0]); }
            }
            SpeedMultiplier *= weaponSpeedMultiplier;

            // Return Mobie Joystick Direction
            if (MovementJoystick.Direction.x != 0 && MovementJoystick.Direction.y != 0) return MovementJoystick.Direction * SpeedMultiplier;
            // Return Direction
            return tinyInput.Player.Move.ReadValue<Vector2>() * SpeedMultiplier;
        }

        public Vector2 LookValue()
        {
            if (CameraJoystick.Direction.x != 0 && CameraJoystick.Direction.y != 0) return CameraJoystick.Direction * 8;
            return tinyInput.Player.Look.ReadValue<Vector2>();
        }

        void Update()
        {
            if (!GameManager.Instance.paused)
            {
                // Change Height
                cc.height = Mathf.Lerp(cc.height, _ccHeight, .2f);
                MainCamera.GetComponent<HeadBobbing>().midpoint = Mathf.Lerp(MainCamera.GetComponent<HeadBobbing>().midpoint, _cameraMidpoint, .2f);

                // Player Rotation
                horizontalRotation = LookValue().x;
                verticalRotation -= LookValue().y;
                transform.Rotate(0, horizontalRotation, 0);
                verticalRotation = Mathf.Clamp(verticalRotation, -verticalRotationLimit, verticalRotationLimit);
                Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

                // Player Movement
                if (cc.isGrounded)
                {
                    // Reset Gravity
                    verticalVelocity = 0;


                    if (state != MovementState.Climbing)
                    {
                        // Crouch
                        if (tinyInput.Player.Crouch.ReadValue<float>() == 1 && _gm.EnablePlayerCrouch)
                        {
                            forwardMovement = MovementValue().y * playerCrouchSpeed;
                            sidewaysMovement = MovementValue().x * playerCrouchSpeed;

                            state = MovementState.Crouch;

                            _ccHeight = CrouchPlayerHeight;
                            MainCamera.transform.localPosition = new Vector3(MainCamera.transform.localPosition.x, CrouchCameraHeight, MainCamera.transform.localPosition.z);
                            _cameraMidpoint = CrouchCameraHeight;
                            WeaponCamera.transform.localPosition = new Vector3(WeaponCamera.transform.localPosition.x, CrouchCameraHeight, WeaponCamera.transform.localPosition.z);
                            WeaponsContainer.transform.localPosition = new Vector3(WeaponsContainer.transform.localPosition.x, CrouchCameraHeight, WeaponsContainer.transform.localPosition.z);
                        }

                        // Sprint
                        else if (tinyInput.Player.Sprint.ReadValue<float>() == 1 && _gm.EnablePlayerSprint && MovementValue() != new Vector2(0, 0) && ((EnableStamina && RunningStaminaConsume <= stamina) || !EnableStamina))
                        {
                            forwardMovement = MovementValue().y * playerRunningSpeed;
                            sidewaysMovement = MovementValue().x * playerRunningSpeed;

                            state = MovementState.Sprinting;
                            if (EnableStamina) { stamina -= RunningStaminaConsume * Time.deltaTime; lastUseStamina = Time.time + StaminaRegenerationInterval; }

                            _ccHeight = NormalPlayerHeight;
                            MainCamera.transform.localPosition = new Vector3(MainCamera.transform.localPosition.x, NormalCameraHeight, MainCamera.transform.localPosition.z);
                            _cameraMidpoint = NormalCameraHeight;
                            WeaponCamera.transform.localPosition = new Vector3(WeaponCamera.transform.localPosition.x, NormalCameraHeight, WeaponCamera.transform.localPosition.z);
                            WeaponsContainer.transform.localPosition = new Vector3(WeaponsContainer.transform.localPosition.x, NormalCameraHeight, WeaponsContainer.transform.localPosition.z);
                        }

                        // Walk
                        else if (MovementValue() != new Vector2(0, 0))
                        {
                            forwardMovement = MovementValue().y * playerWalkingSpeed;
                            sidewaysMovement = MovementValue().x * playerWalkingSpeed;

                            state = MovementState.Walk;

                            _ccHeight = NormalPlayerHeight;
                            MainCamera.transform.localPosition = new Vector3(MainCamera.transform.localPosition.x, NormalCameraHeight, MainCamera.transform.localPosition.z);
                            _cameraMidpoint = NormalCameraHeight;
                            WeaponCamera.transform.localPosition = new Vector3(WeaponCamera.transform.localPosition.x, NormalCameraHeight, WeaponCamera.transform.localPosition.z);
                            WeaponsContainer.transform.localPosition = new Vector3(WeaponsContainer.transform.localPosition.x, NormalCameraHeight, WeaponsContainer.transform.localPosition.z);
                        }

                        // Idle
                        else
                        {
                            forwardMovement = 0;
                            sidewaysMovement = 0;

                            state = MovementState.Idle;

                            _ccHeight = NormalPlayerHeight;
                            MainCamera.transform.localPosition = new Vector3(MainCamera.transform.localPosition.x, NormalCameraHeight, MainCamera.transform.localPosition.z);
                            _cameraMidpoint = NormalCameraHeight;
                            WeaponCamera.transform.localPosition = new Vector3(WeaponCamera.transform.localPosition.x, NormalCameraHeight, WeaponCamera.transform.localPosition.z);
                            WeaponsContainer.transform.localPosition = new Vector3(WeaponsContainer.transform.localPosition.x, NormalCameraHeight, WeaponsContainer.transform.localPosition.z);
                        }
                    }
                }
                if (state == MovementState.Climbing) { verticalVelocity = 0; }

                // Fall Damage
                if (isFalling && cc.isGrounded)
                {
                    float fallDistance = fallStartPos.y - transform.position.y;
                    if (fallDistance > 5)
                    {
                        float calculatedDamage = transform.GetComponent<PlayerHealth>().FallDamage.Evaluate(fallDistance);
                        transform.GetComponent<PlayerHealth>().GetDamage(new DamageClass(calculatedDamage, DamageType.FallDamage));
                    }
                    //Debug.Log(fallDistance);
                    isFalling = false;
                }

                if (!isFalling && !cc.isGrounded)
                {
                    isFalling = true;
                    fallStartPos = transform.position;
                }

                if (cc.isGrounded) { fallStartPos = transform.position; }


                verticalVelocity += Physics.gravity.y * 2 * Time.deltaTime;

                // Jumping after pressing the jump button
                if (tinyInput.Player.Jump.triggered)
                {
                    UIJump();
                }
                if (tinyInput.Player.Interaction.ReadValue<float>() == 1)
                {
                    //UiInteraction();
                }

                if (state == MovementState.Climbing && cc.isGrounded) { state = MovementState.Idle; }

                Vector3 playerMovement = new Vector3(sidewaysMovement, verticalVelocity, forwardMovement);
                //playerMovement += additionalMovement;
                //additionalMovement = new Vector3();
                //playerMovement = Vector3.smoo(playerMovement, playerMovement + additionalMovement, ref currentVelocity, .1f);

                if (state != MovementState.Climbing) cc.Move((transform.rotation * (playerMovement) * Time.deltaTime) + (additionalMovement));

                //if (state != MovementState.Climbing) cc.Move((transform.rotation * playerMovement * Time.deltaTime) + (additionalMovement/2));
                else { cc.Move(transform.rotation * new Vector3(MovementValue().x * LadderSpeed / 2, MovementValue().y * LadderSpeed, 0) * Time.deltaTime); }

                additionalMovement = new Vector3();

                // Stamina
                if (EnableStamina)
                {
                    transform.Find("Canvas").Find("UI").Find("Stamina_Bar").gameObject.SetActive(true);
                    transform.Find("Canvas").Find("UI").Find("Stamina_Bar").GetComponent<Slider>().value = stamina;

                    //this.Log(lastUseStamina, Time.time);
                    if (lastUseStamina < Time.time)
                    {
                        stamina += (StaminaRegenerationSpeed * Time.deltaTime);
                    }
                }
                else
                {
                    transform.Find("Canvas").Find("UI").Find("Stamina_Bar").gameObject.SetActive(false);
                }
            }

        }

        public void UIJump()
        {
            if (state != MovementState.Climbing)
            {
                if (EnableStamina)
                {
                    // First Jump
                    if (cc.isGrounded && JumpStaminaConsume <= stamina) { verticalVelocity = jumpStrength; numbersOfJumps = 1; stamina -= jumpStrength; lastUseStamina = Time.time + StaminaRegenerationInterval; }
                    // Optional Extra Jump
                    if (!cc.isGrounded && numbersOfJumps < maxNumberOfJump && JumpStaminaConsume <= stamina) { verticalVelocity = jumpStrength; numbersOfJumps++; stamina -= jumpStrength; lastUseStamina = Time.time + StaminaRegenerationInterval; }
                }
                else
                {
                    // First Jump
                    if (cc.isGrounded) { verticalVelocity = jumpStrength; numbersOfJumps = 1; }
                    // Optional Extra Jump
                    if (!cc.isGrounded && numbersOfJumps < maxNumberOfJump) { verticalVelocity = jumpStrength; numbersOfJumps++; }
                }
            }
            else
            {
                state = MovementState.Idle;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Pick-Up SMTH
            if (other.CompareTag("PickUp"))
            {
                if (other.transform.TryGetComponent<BonusScript>(out BonusScript pickup))
                {
                    // Health Pick-Up
                    if (pickup.bonusType == BonusType.Health)
                    {
                        // Add Health
                        GetComponent<PlayerHealth>().AddHealth(pickup.Health, pickup.godBonus);
                        // Flash Screen
                        flash.HealthBonus();
                        // Show Panel
                        GameManager.Instance.PanelSetup(pickup.PickUpLocalization.GetLocalization() + " " + pickup.BonusLocalization.GetLocalization(),
                            "<color=#1EBE0A>+" + pickup.Health + " " + pickup.PickUpLocalization.localization.GetString("Health") + "</color>",
                            pickup.PickUpLocalization.GetLocalization() + " " + pickup.BonusLocalization.GetLocalization());
                    }

                    // Armor Pick-Up
                    if (pickup.bonusType == BonusType.Armor)
                    {
                        // Add Armor
                        GetComponent<PlayerHealth>().AddArmor(pickup.Armor, pickup.godBonus);
                        // Flash Screen
                        flash.ArmorBonus();
                        // Show Panel
                        GameManager.Instance.PanelSetup(pickup.PickUpLocalization.GetLocalization() + " " + pickup.BonusLocalization.GetLocalization(),
                            "<color=#008AFF>+" + pickup.Armor + " " + pickup.PickUpLocalization.localization.GetString("Armor") + "</color>",
                            pickup.PickUpLocalization.GetLocalization() + " " + pickup.BonusLocalization.GetLocalization());
                    }

                    // Health & Armor Pick-Up
                    if (pickup.bonusType == BonusType.HealthAndArmor)
                    {
                        // Add Health & Armor
                        GetComponent<PlayerHealth>().AddHealth(pickup.Health, pickup.godBonus);
                        GetComponent<PlayerHealth>().AddArmor(pickup.Armor, pickup.godBonus);
                        // Flash Screen
                        flash.HealthBonus();
                        flash.ArmorBonus();
                        // Show Panel
                        GameManager.Instance.PanelSetup(pickup.PickUpLocalization.GetLocalization() + " " + pickup.BonusLocalization.GetLocalization(),
                            "<color=#008AFF>+" + pickup.Armor + " " + pickup.PickUpLocalization.localization.GetString("Armor") + "</color>\n" + "<color=#1EBE0A>+" + pickup.Health + " " + pickup.PickUpLocalization.localization.GetString("Health") + "</color>",
                            pickup.PickUpLocalization.GetLocalization() + " " + pickup.BonusLocalization.GetLocalization());
                    }

                    // Ammo Pick-Up
                    if (pickup.bonusType == BonusType.Ammo)
                    {
                        // Add Ammo
                        GameManager.Instance.AddAmmo(pickup.ammoType, pickup.AmmoAmount);
                        // Update Weapon Switch Script
                        WeaponSwitch container = GetComponentInChildren(typeof(WeaponSwitch)) as WeaponSwitch;
                        if (container != null) { container.actualWeapon.UpdateLeftAmmo(); }
                        // Flash Screen
                        flash.AmmoBonus();
                        // Show Panel
                        GameManager.Instance.PanelSetup(pickup.PickUpLocalization.GetLocalization() + " " + pickup.BonusLocalization.GetLocalization(),
                            "<color=#E59B10>+" + pickup.AmmoAmount + " " + pickup.PickUpLocalization.localization.GetString(pickup.ammoType.ToString()) + "</color>",
                            pickup.PickUpLocalization.GetLocalization() + " " + pickup.BonusLocalization.GetLocalization());
                    }

                    // Ammo Pick-Up
                    if (pickup.bonusType == BonusType.Ammo)
                    {
                        // Add Ammo

                        // Flash Screen
                        flash.ArmorBonus();
                        // Show Panel
                        GameManager.Instance.PanelSetup(pickup.PickUpLocalization.GetLocalization() + " " + pickup.BonusLocalization.GetLocalization(),
                            "<color=#E59B10>+" + pickup.AmmoAmount + " " + pickup.PickUpLocalization.localization.GetString(pickup.ammoType.ToString()) + "</color>",
                            pickup.PickUpLocalization.GetLocalization() + " " + pickup.BonusLocalization.GetLocalization());
                    }

                    // Key Pick-Up
                    if (pickup.bonusType == BonusType.Key)
                    {
                        // Add Key
                        switch (other.GetComponent<BonusScript>().keyType)
                        {
                            case (KeyType.RedKey):
                                LevelManager.Instance.redKeys = true;
                                break;
                            case (KeyType.BlueKey):
                                LevelManager.Instance.blueKeys = true;
                                break;
                            case (KeyType.YellowKey):
                                LevelManager.Instance.yellowKeys = true;
                                break;
                            case (KeyType.GreenKey):
                                LevelManager.Instance.greenKeys = true;
                                break;
                            default:
                                break;
                        }
                        // Flash Screen
                        flash.AmmoBonus();
                        // Show Panel
                        GameManager.Instance.PanelSetup(pickup.PickUpLocalization.GetLocalization() + " " + pickup.BonusLocalization.GetLocalization(),
                            "",
                            pickup.PickUpLocalization.GetLocalization() + " " + pickup.BonusLocalization.GetLocalization());
                    }

                    // Weapon Pick-Up
                    if (pickup.bonusType == BonusType.Weapon)
                    {
                        // Add Weapon
                        WeaponSwitch weaponSwitch = FindObjectOfType<WeaponSwitch>();
                        if (weaponSwitch != null)
                        {
                            weaponSwitch.RegisterNewWeapon(other.GetComponent<BonusScript>().weaponPrefab);
                        }
                        // Flash Screen
                        flash.AmmoBonus();
                        // Show Panel
                        GameManager.Instance.PanelSetup(pickup.PickUpLocalization.GetLocalization() + " " + pickup.BonusLocalization.GetLocalization(),
                            "",
                            pickup.PickUpLocalization.GetLocalization() + " " + pickup.BonusLocalization.GetLocalization());
                    }



                    // Score Pick-Up
                    if (pickup.bonusType == BonusType.Score)
                    {
                        // Flash Screen
                        flash.ArmorBonus();
                        // Show Panel
                        GameManager.Instance.PanelSetup(pickup.PickUpLocalization.GetLocalization() + " <color=#10E2E5>" + pickup.BonusLocalization.GetLocalization() + "</color>",
                            "");
                    }

                    // Add Points
                    GameManager.Instance.AddPoints(pickup.ScorePoints);
                    // Play Pickup Sound (All Types)
                    source.PlayOneShot(pickupSound);
                    // Destroy Object (All Types)
                    Destroy(other.gameObject);
                }
                else
                {
                    TinyDebug.Warning("This Object doesn't have BonusScript!");
                }
            }
        }

        public IEnumerator Dash(float time, float speed)
        {
            float startTime = Time.time;

            while (Time.time < startTime + time)
            {
                cc.Move(transform.forward * speed * Time.deltaTime);
                yield return null;
            }
        }
    }

    [System.Serializable]
    public enum MovementState { Idle = 0, Walk = 1, Sprinting = 2, Crouch = 3, Jump = 4, Climbing = 5 }


    /// Custom Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(PlayerMovement))]
    public class PlayerMovementEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            PlayerMovement player = (PlayerMovement)target;
            EditorGUI.indentLevel = 0;

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((PlayerMovement)target), typeof(PlayerMovement), false);
            GUI.enabled = true;

            EditorGUILayout.BeginVertical("box");

            TinyGUI.EditorTitle("Player Speed");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("playerWalkingSpeed"), new GUIContent("Walking Speed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("playerRunningSpeed"), new GUIContent("Running Speed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("playerCrouchSpeed"), new GUIContent("Crouch Speed"));

            TinyGUI.EditorTitle("Player Height");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("NormalPlayerHeight"), new GUIContent("Player Normal Height"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CrouchPlayerHeight"), new GUIContent("Player Crouch height"));

            TinyGUI.EditorTitle("Camera Height");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("NormalCameraHeight"), new GUIContent("Camera Normal Height"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CrouchCameraHeight"), new GUIContent("Camera Crouch height"));

            TinyGUI.EditorTitle("Stamina System");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("EnableStamina"), new GUIContent("Enable Stamina System"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("RunningStaminaConsume"), new GUIContent("Running Stamina Consume"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("JumpStaminaConsume"), new GUIContent("Jump Stamina Consume"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("StaminaRegenerationInterval"), new GUIContent("Stamina Regen. Interval"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("StaminaRegenerationSpeed"), new GUIContent("Stamina Regen. Speed"));

            TinyGUI.EditorTitle("Jump Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpStrength"), new GUIContent("Jump Strength"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxNumberOfJump"), new GUIContent("Max Jump Number"));

            TinyGUI.EditorTitle("Rotation Limit");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("verticalRotationLimit"), new GUIContent("Rotation Limit (Y)"));

            EditorGUILayout.EndVertical();


            serializedObject.ApplyModifiedProperties();
        }
    }


#endif
}
