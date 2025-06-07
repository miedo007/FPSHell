using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using HellishBattle.Ability;
using HellishBattle.Enemies;
using HellishBattle.SaveSystem;

namespace HellishBattle.Weapon
{
    [RequireComponent(typeof(AudioSource))]
    public class RangeWeapon : BaseWeaponScript
    {
        // Base
        public AnimationWeaponType AnimationType;

        // Upgrade
        public List<RangeUpgradeClass> Upgrades;
        public List<WeaponSkinClass> Skins;

        // Fov
        public bool enableAiming = false;
        [Range(0, 180)] public float baseFOV = 60;
        [Range(0, 180)] public float aimFOV = 45;

        // Bullet
        public WeaponTypeShooting WeaponShootType;
        public RangeWeaponShootType FireMode;
        public BulleType bulletType;

        public DamageType DamageType;
        public float weaponDamage = 30;
        public float weaponSpread = 15;
        [Suffix("Secound")] public float timeBetweenShot = 0.15f;
        public bool EnableTBSCurve = false;
        public AnimationCurve TBSCurve;
        public LayerMask raycastMask;
        public float BulletForce = 10;
        //   Aim Settings
        public float weaponSpreadWhenAiming = 12;
        //   Shoot Settings
        public float hitscanRange = 50;
        public float bulletForce = 50;
        public GameObject rocketPrefab;
        [Suffix("KG")] public float ProjectileMass = 2f;
        public bool ProjectileUseGravity = true;
        public PhysicMaterial ProjectileMaterial;
        public bool BreakOnCollision = false;
        public AudioClip CollisionSound;
        public GameObject spawnpointPrefab;
        //   Fire Mode
        public int bulletCountPerShot = 1;
        //      Laser
        public int ammoCostPerShot = 1;
        public float timeBetweenLaserAmmo = .7f;
        //      Burst
        public int burstShot = 3;
        //      Charge
        public float ChargeTime = 1.2f;
        public bool ChargeCancel = false;
        public AnimationCurve ChargeCurve;
        [Range(0, 1)] public float ChargeMinLimit = .25f;
        //  Bullet Type
        public GameObject bulletHole;
        //      Explosion
        public float explosionRadius = 3.5f;
        public AnimationCurve explosionDamageCurve;
        public float explosionShakeDistance = 7;
        public GameObject explosionPrefab;
        public AudioClip explosionSound;

        // Magazine
        public AmmoType ammoType;
        public int magazineSize = 12;

        // Vibration
        public float VibrationLowFrequency = .25f;
        public float VibrationHighFrequency = .2f;
        public float VibrationTimer = 0.08f;
        public bool VibrationMobile = false;

        // Sound
        public AudioClip shotSound;
        public AudioClip reloadSound;
        public AudioClip emptyGunSound;

        // Events
        public UnityEvent AdditionalFireWeaponFunction;
        public UnityEvent AdditionalWeaponAimFunction;
        public UnityEvent AdditionalWeaponReloadFunction;

        // Privates
        AudioSource source;
        Animator anim;
        Transform mainCamera;
        [HideInInspector] public int ammoLeft;
        [HideInInspector] public int ammoClipLeft = 666;
        bool isShot;
        bool isReloading;
        [HideInInspector] public bool aim = false;
        float timer;
        float currentSpread;
        float _bulletSeries = 0;
        float chargeCurveMultiplier = 0;
        public float _chargeTime;
        public float _timeToNextAmmoLaser;
        private TinyInput tinyInput;

        private System.Action<InputAction.CallbackContext> FireStartedAction;
        private System.Action<InputAction.CallbackContext> FireCanceledAction;
        private System.Action<InputAction.CallbackContext> ReloadStartedAction;
        private System.Action<InputAction.CallbackContext> InspectStartedAction;


        void Awake()
        {
            tinyInput = InputManager.Instance.input;

            // GetComponents
            source = GetComponent<AudioSource>();
            anim = GetComponent<Animator>();

            // Load Skin and Upgrade
            LoadUpgrade();
            LoadSkin();

            // If not Infinity Ammo
            if (ammoType != AmmoType.InfinityAmmo) ammoLeft = GameManager.Instance.AmmoLeft(ammoType);
            // If doesn't have Magazine
            if (magazineSize > 1) ammoClipLeft = magazineSize;

            if (FireMode == RangeWeaponShootType.Burst) { timeBetweenShot += timeBetweenShot * burstShot; }

            // Passive
            GameSettingsManger _go = (GameSettingsManger)Resources.Load("Game_Settings");
            if (_go.EnablePlayerAbility)
            {
                PassiveAbilitySO _passive = GameManager.Instance.PassiveSkill;
                if (_passive.EnableRangeWeaponBoost)
                {
                    weaponDamage *= (int)_passive.RangeDamageIncrease;
                    weaponSpread *= (int)_passive.RangeRecoilReduce;
                    weaponSpreadWhenAiming *= (int)_passive.RangeRecoilReduce;
                    timeBetweenShot *= (int)_passive.RangeFireRateIncrease;
                }
            }

            spawnpointPrefab = Camera.main.transform.GetChild(0).gameObject;

            // Register Input Action
            SetupInputAction();
        }

        void OnEnable()
        {
            isReloading = false;
            mainCamera = Camera.main.transform;

            // If not Infinity Ammo
            if (ammoType != AmmoType.InfinityAmmo) ammoLeft = GameManager.Instance.AmmoLeft(ammoType);

            if (!transform.parent.GetComponent<WeaponSwitch>().SafeZone)
            {
                transform.parent.GetComponent<WeaponSwitch>().WeaponNameText.text = WeaponName();
                transform.parent.GetComponent<WeaponSwitch>().WeaponUIImage.sprite = WeaponIcon();
            }
            else
            {
                transform.parent.GetComponent<WeaponSwitch>().WeaponNameText.text = "";
                transform.parent.GetComponent<WeaponSwitch>().WeaponUIImage.sprite = null;
            }

            // Ready to Shot
            timer = 666;
            //tinyInput.Enable();


            // Add Input Action
            tinyInput.Weapon.Fire.started += FireStartedAction;
            tinyInput.Weapon.Fire.canceled += FireCanceledAction;
            tinyInput.Weapon.Reload.started += ReloadStartedAction;
            tinyInput.Weapon.Inspect.performed += InspectStartedAction;
        }

        public void OnDisable()
        {
            // Remove Input Action
            tinyInput.Weapon.Fire.started -= FireStartedAction;
            tinyInput.Weapon.Fire.canceled -= FireCanceledAction;
            tinyInput.Weapon.Reload.started -= ReloadStartedAction;
            tinyInput.Weapon.Inspect.performed -= InspectStartedAction;
        }

        public void SetupInputAction()
        {
            FireStartedAction = ctx =>
            {
                // Single
                if (FireMode == RangeWeaponShootType.Single && isReloading == false && timer > timeBetweenShot && !Application.isMobilePlatform)
                {
                    if (magazineSize <= 0)
                    {
                        if (ammoLeft >= ammoCostPerShot) { isShot = true; }
                        else { source.PlayOneShot(emptyGunSound); timer = 0; }
                    }
                    else { isShot = true; }
                }

                // Burst
                if (FireMode == RangeWeaponShootType.Burst && isReloading == false && timer > timeBetweenShot && !Application.isMobilePlatform)
                {
                    if (magazineSize <= 0)
                    {
                        if (ammoLeft >= ammoCostPerShot) { isShot = true; }
                        else { source.PlayOneShot(emptyGunSound); timer = 0; }
                    }
                    else { StartCoroutine("BurstEnumerator"); }
                }

                // Laser
                if (FireMode == RangeWeaponShootType.Laser && isReloading == false && timer > timeBetweenShot && !Application.isMobilePlatform)
                {
                    if (magazineSize <= 0)
                    {
                        if (ammoLeft >= ammoCostPerShot)
                        {
                            if (aim) { anim.Play("Start_Aim_Shot", -1, 0f); }
                            else { anim.Play("Start_Shot", -1, 0f); }
                            isShot = true;
                            anim.SetBool("pressed", true);
                        }
                        else
                        {
                            source.PlayOneShot(emptyGunSound); timer = 0;
                        }
                    }
                    else if (ammoClipLeft > 0)
                    {
                        if (aim) { anim.Play("Start_Aim_Shot", -1, 0f); }
                        else { anim.Play("Start_Shot", -1, 0f); }
                        isShot = true;
                        anim.SetBool("pressed", true);
                    }
                }
            };

            // Fire Cancel Action
            FireCanceledAction = ctx =>
            {
                if (FireMode == RangeWeaponShootType.Charge && ChargeCancel && _bulletSeries == 0 && (_chargeTime / ChargeTime) > ChargeMinLimit)
                {
                    isShot = true;
                    chargeCurveMultiplier = ChargeCurve.Evaluate(_chargeTime / ChargeTime);
                }

                _bulletSeries = 0; _chargeTime = 0; anim.SetBool("pressed", false);
            };

            // Reload Action
            ReloadStartedAction = ctx => { if (isReloading == false && ammoClipLeft != magazineSize) { Reload(); } };

            // Inspect Action
            InspectStartedAction = ctx =>
            {
                // Get Shot Anim Lenght
                AnimationClip animationClip = null;
                foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips) { if (clip.name == "Shot") { animationClip = clip; } }
                // if Shot Anim End play Inspect Anim
                if (timer > animationClip.length && animationClip != null) anim.Play("Inspect", -1, 0f); ;
            };
        }


        void Update()
        {
            if (!GameManager.Instance.paused)
            {
                // Ammo Text
                if (ammoType != AmmoType.InfinityAmmo && magazineSize < 1) transform.parent.GetComponent<WeaponSwitch>().ammoText.text = ammoLeft.ToString();                             // No Magazine
                else if (ammoType != AmmoType.InfinityAmmo) transform.parent.GetComponent<WeaponSwitch>().ammoText.text = ammoClipLeft + "<size=8> / " + ammoLeft;                        // Normal
                else if (ammoType == AmmoType.InfinityAmmo && magazineSize > 0) transform.parent.GetComponent<WeaponSwitch>().ammoText.text = ammoClipLeft + "<size=8> / Infinity";       // Infinity Ammo with magazine
                else transform.parent.GetComponent<WeaponSwitch>().ammoText.text = "Infinity";                                                                                            // Infinity Ammo without magazine

                timer += Time.deltaTime;

                // Charge
                if (FireMode == RangeWeaponShootType.Charge && isReloading == false && timer > timeBetweenShot && !Application.isMobilePlatform)
                {
                    if ((tinyInput.Weapon.Fire.ReadValue<float>() == 1 && timer > timeBetweenShot && !ChargeCancel) ||
                        (tinyInput.Weapon.Fire.ReadValue<float>() == 1 && timer > timeBetweenShot && ChargeCancel && _bulletSeries == 0))
                    {
                        _chargeTime += Time.deltaTime;
                        if (_chargeTime > ChargeTime)
                        {
                            chargeCurveMultiplier = 1;
                            _chargeTime = 0;
                            if (magazineSize <= 0)
                            {
                                if (ammoLeft >= ammoCostPerShot) { isShot = true; }
                                else { source.PlayOneShot(emptyGunSound); timer = 0; }
                            }
                            else { isShot = true; }
                        }
                    }
                }

                // Automatic
                if (tinyInput.Weapon.Fire.ReadValue<float>() == 1) { if (FireMode == RangeWeaponShootType.Automatic && isReloading == false && timer > timeBetweenShot && !Application.isMobilePlatform) { if (magazineSize <= 0) { if (ammoLeft >= ammoCostPerShot) { isShot = true; } else { source.PlayOneShot(emptyGunSound); timer = 0; } } else { isShot = true; } } }

                // Laser
                if (tinyInput.Weapon.Fire.ReadValue<float>() == 1) { if (FireMode == RangeWeaponShootType.Laser && isReloading == false && !Application.isMobilePlatform) { if (magazineSize <= 0) { if (ammoLeft >= 1) { isShot = true; } else { source.PlayOneShot(emptyGunSound); timer = 0; anim.SetBool("pressed", false); } } else { if (ammoClipLeft <= 0) { anim.SetBool("pressed", false); } isShot = true; } } }

                // Aim on PC
                if (tinyInput.Weapon.Aim.ReadValue<float>() == 1 && enableAiming && isReloading == false) { anim.SetBool("aim", true); aim = true; AdditionalWeaponAimFunction.Invoke(); } else { anim.SetBool("aim", false); aim = false; }

                // Change FOV
                if (aim) { mainCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(mainCamera.GetComponent<Camera>().fieldOfView, aimFOV, .5f); mainCamera.Find("GUI_Camera").GetComponent<Camera>().fieldOfView = Mathf.Lerp(mainCamera.GetComponent<Camera>().fieldOfView, aimFOV, .5f); currentSpread = weaponSpreadWhenAiming; }
                else { mainCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(mainCamera.GetComponent<Camera>().fieldOfView, baseFOV, .5f); mainCamera.Find("GUI_Camera").GetComponent<Camera>().fieldOfView = Mathf.Lerp(mainCamera.GetComponent<Camera>().fieldOfView, baseFOV, .5f); currentSpread = weaponSpread; }

            }

        }

        void FixedUpdate()
        {
            if (isShot == true && ammoClipLeft > 0 && isReloading == false)
            {
                AdditionalFireWeaponFunction.Invoke();
                isShot = false;
                source.PlayOneShot(shotSound);
                // Tetst
                //Camera.main.GetComponent<CameraShake>().ShotShakeCamera();
                StartCoroutine("shot");
                StartCoroutine(GameManager.Instance.Vibrate(VibrationLowFrequency, VibrationHighFrequency, VibrationTimer, VibrationMobile));

                if (FireMode != RangeWeaponShootType.Laser)
                {
                    if (magazineSize >= 1) { ammoClipLeft -= ammoCostPerShot; }
                    else if (ammoType != AmmoType.InfinityAmmo) { GameManager.Instance.AddAmmo(ammoType, -1); ammoLeft -= ammoCostPerShot; }
                }
                else
                {
                    _timeToNextAmmoLaser += Time.deltaTime;
                    if (_timeToNextAmmoLaser > timeBetweenLaserAmmo)
                    {
                        _timeToNextAmmoLaser = 0;
                        if (magazineSize >= 1) ammoClipLeft -= 1;
                        else if (ammoType != AmmoType.InfinityAmmo) { GameManager.Instance.AddAmmo(ammoType, -1); ammoLeft -= 1; }
                    }
                }

                DynamicCrosshair.spread += currentSpread;

                // Bullet Series Count
                _bulletSeries++;

                if (WeaponShootType == WeaponTypeShooting.Hitscan)
                {
                    float damage = weaponDamage;
                    if (FireMode == RangeWeaponShootType.Laser) { damage = weaponDamage * Time.deltaTime; }
                    if (FireMode == RangeWeaponShootType.Charge && ChargeCancel) { damage = weaponDamage * chargeCurveMultiplier; }

                    float bulletCount = bulletCountPerShot;
                    if (FireMode == RangeWeaponShootType.Laser) { bulletCount = 1; }

                    for (int i = 0; i < bulletCount; i++)
                    {

                        Vector3 deviation3D = Random.insideUnitCircle * DynamicCrosshair.spread / 10;
                        Quaternion rot = Quaternion.LookRotation(Vector3.forward * hitscanRange + deviation3D);
                        Vector3 mainCameraForwardWithSpread = mainCamera.transform.rotation * rot * Vector3.forward * hitscanRange;

                        Ray ray = new Ray(mainCamera.transform.position, mainCameraForwardWithSpread);

                        RaycastHit hit;

                        if (Physics.Raycast(ray, out hit, hitscanRange, raycastMask))
                        {
                            //hit.collider.gameObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
                            hit.collider.gameObject.SendMessage("GetDamage", new DamageClass(damage, DamageType, hit.point), SendMessageOptions.DontRequireReceiver);
                            if (hit.transform.CompareTag("Enemy"))
                            {
                                if (bulletType == BulleType.Normal)
                                {
                                    Enemy _enemy = hit.transform.GetComponent<Enemy>();
                                    //_enemy.ShotBlood(hit.point);

                                    if (hit.transform.GetComponent<EnemyStates>().currentState == hit.transform.GetComponent<EnemyStates>().patrolState
                                        || hit.transform.GetComponent<EnemyStates>().currentState == hit.transform.GetComponent<EnemyStates>().alertState)
                                        // Send the target firing position to the target if it is in a patrol or alert state
                                        hit.collider.gameObject.SendMessage("HiddenShot", transform.parent.transform.position, SendMessageOptions.DontRequireReceiver);
                                }
                            }
                            else
                            {
                                if (FireMode != RangeWeaponShootType.Laser) { Instantiate(bulletHole, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal)).transform.parent = hit.collider.gameObject.transform; }

                                if (bulletType == BulleType.Explosion)
                                {
                                    GameObject explosionInstantiated = (GameObject)Instantiate(explosionPrefab, hit.point, Quaternion.identity);
                                    explosionInstantiated.GetComponent<Explosion>().explosionSound = explosionSound;

                                    Collider[] damageColliders = Physics.OverlapSphere(hit.point, explosionRadius);
                                    foreach (var hitCollider in damageColliders)
                                    {
                                        float _distance = Vector3.Distance(hitCollider.transform.position, hit.point);
                                        hitCollider.SendMessage("TakeDamage", damage * explosionDamageCurve.Evaluate(_distance / explosionRadius), SendMessageOptions.DontRequireReceiver);
                                        hitCollider.SendMessage("ExplosionBlood", SendMessageOptions.DontRequireReceiver);

                                        // Add Force
                                        Rigidbody hitRigidbody = hitCollider.GetComponent<Rigidbody>();
                                        if (hitRigidbody != null) { hitRigidbody.AddForce(transform.forward * BulletForce * explosionDamageCurve.Evaluate(_distance / explosionRadius), ForceMode.Impulse); }
                                    }

                                    if (Vector3.Distance(this.transform.position, Camera.main.transform.position) < explosionShakeDistance) { Camera.main.GetComponent<CameraShake>().ShakeCamera(); }
                                }
                                else
                                {
                                    // Add Force
                                    Rigidbody hitRigidbody = hit.collider.GetComponent<Rigidbody>();
                                    if (hitRigidbody != null) { hitRigidbody.AddForce(transform.forward * BulletForce, ForceMode.Impulse); }
                                }
                            }
                        }
                    }
                }

                if (WeaponShootType == WeaponTypeShooting.Projectile)
                {
                    float damage = weaponDamage;
                    if (FireMode == RangeWeaponShootType.Laser) { damage = weaponDamage * Time.deltaTime; }
                    if (FireMode == RangeWeaponShootType.Charge && ChargeCancel) { damage = weaponDamage * chargeCurveMultiplier; }


                    float bulletCount = bulletCountPerShot;
                    if (FireMode == RangeWeaponShootType.Laser) { bulletCount = 1; }

                    for (int i = 0; i < bulletCount; i++)
                    {
                        GameObject rocketInstantiated = (GameObject)Instantiate(rocketPrefab, spawnpointPrefab.transform.position, Quaternion.identity);

                        rocketInstantiated.GetComponent<BulletScript>().SetupType(bulletType);
                        rocketInstantiated.GetComponent<BulletScript>().SetupBase(damage, DamageType, BulletForce, 10);
                        rocketInstantiated.GetComponent<BulletScript>().SetupObject(ProjectileUseGravity, ProjectileMass, ProjectileMaterial, BreakOnCollision, CollisionSound);

                        if (bulletType == BulleType.Normal) { rocketInstantiated.GetComponent<BulletScript>().SetupNormal(raycastMask); }
                        if (bulletType == BulleType.Explosion) { rocketInstantiated.GetComponent<BulletScript>().SetupExplosion(explosionRadius, raycastMask, explosionPrefab, explosionSound, explosionDamageCurve, explosionShakeDistance); }

                        var randomNumberX = Random.Range(-DynamicCrosshair.spread / 900, DynamicCrosshair.spread / 900);
                        var randomNumberY = Random.Range(-DynamicCrosshair.spread / 900, DynamicCrosshair.spread / 900);
                        var randomNumberZ = Random.Range(-DynamicCrosshair.spread / 900, DynamicCrosshair.spread / 900);

                        Vector3 ve = new Vector3(randomNumberX, randomNumberY, randomNumberZ);

                        Rigidbody rocketRb = rocketInstantiated.GetComponent<Rigidbody>();

                        if (FireMode == RangeWeaponShootType.Charge && ChargeCancel) { rocketRb.AddForce(((Camera.main.transform.forward + ve) * bulletForce) * chargeCurveMultiplier, ForceMode.Impulse); Debug.Log(chargeCurveMultiplier); }
                        else rocketRb.AddForce((Camera.main.transform.forward + ve) * bulletForce, ForceMode.Impulse);
                    }
                }
                //timer = 0;
                if (EnableTBSCurve) { timer = -(timeBetweenShot * (1 - TBSCurve.Evaluate(_bulletSeries))); }
                else { timer = 0; }
            }
            // Hitscan & Projectile
            else if (isShot == true && ammoClipLeft <= 0 && isReloading == false)
            {
                isShot = false;
                Reload();
            }
        }

        IEnumerator BurstEnumerator()
        {
            for (int i = 0; i < burstShot; i++)
            {
                isShot = true;
                yield return new WaitForSeconds(timeBetweenShot / (burstShot + 1));
            }
            yield return new WaitForSeconds(0);
        }

        // Function responsible for reloading weapons
        void Reload()
        {
            // Calculate How Many Bullets Should We Reload
            int bulletsToReload = magazineSize - ammoClipLeft;
            if (magazineSize > 0)
            {
                if (ammoType != AmmoType.InfinityAmmo)
                {
                    if (ammoLeft >= bulletsToReload)
                    {
                        StartCoroutine("ReloadWeapon");
                        ammoLeft -= bulletsToReload;
                        if (magazineSize > 1) ammoClipLeft = magazineSize;

                        GameManager.Instance.AddAmmo(ammoType, -bulletsToReload);
                    }
                    else if (ammoLeft < bulletsToReload && ammoLeft > 0)
                    {
                        StartCoroutine("ReloadWeapon");
                        if (magazineSize > 1) ammoClipLeft += ammoLeft;
                        ammoLeft = 0;
                        GameManager.Instance.SetNewAmmoAmount(ammoType, 0);
                    }
                    else if (ammoLeft <= 0)
                    {
                        if (timer > timeBetweenShot)
                        {
                            source.PlayOneShot(emptyGunSound);
                            timer = 0;
                        }
                    }
                }
                else
                {
                    StartCoroutine("ReloadWeapon");
                    if (magazineSize > 1) ammoClipLeft = magazineSize;
                }
            }
        }

        IEnumerator ReloadWeapon()
        {
            isReloading = true;
            source.PlayOneShot(reloadSound);
            anim.Play("Reload", -1, 0f);

            AdditionalWeaponReloadFunction.Invoke();

            yield return new WaitForSeconds(.01f);
            float _reloadTime = anim.GetCurrentAnimatorStateInfo(0).length;

            yield return new WaitForSeconds(_reloadTime);
            isReloading = false;

            if (tinyInput.Weapon.Fire.ReadValue<float>() == 1 && FireMode == RangeWeaponShootType.Laser) { anim.Play("Start_Shot", -1, 0f); }
        }

        IEnumerator shot()
        {
            // When Aimong Set Aiming Shot Animation
            if (tinyInput.Weapon.Aim.ReadValue<float>() == 1 && enableAiming)
            {
                if (FireMode != RangeWeaponShootType.Laser)
                {
                    if (AnimationType == AnimationWeaponType.DualWeapon)
                    {
                        if (ammoClipLeft % 2 == 0) { anim.Play("Aim_Shot_Right", -1, 0f); }
                        else { anim.Play("Aim_Shot_Left", -1, 0f); }
                    }
                    else { anim.Play("Aim_Shot", -1, 0f); }
                }
            }
            // Else Show Normal Shot Animation
            else
            {
                if (FireMode != RangeWeaponShootType.Laser)
                {
                    if (AnimationType == AnimationWeaponType.DualWeapon)
                    {
                        if (ammoClipLeft % 2 == 0) { anim.Play("Shot_Right", -1, 0f); }
                        else { anim.Play("Shot_Left", -1, 0f); }
                    }
                    else { anim.Play("Shot", -1, 0f); }
                }
            }

            yield return new WaitForSeconds(0.1f);
        }

        public void AddAmmo(int value) { ammoLeft += value; }

        /* Interactions */
        // Shoot
        public override void UpdateLeftAmmo()
        {
            if (ammoType != AmmoType.InfinityAmmo && magazineSize > 1) ammoLeft = GameManager.Instance.AmmoLeft(ammoType);
            else if (ammoType != AmmoType.InfinityAmmo && magazineSize <= 0) { ammoLeft = GameManager.Instance.AmmoLeft(ammoType); }
        }

        public override void PrimaryModeFunction()
        {
            if (isReloading == false && timer > timeBetweenShot) { if (magazineSize <= 0) { if (ammoLeft >= ammoCostPerShot) { isShot = true; } else { source.PlayOneShot(emptyGunSound); timer = 0; } } else { isShot = true; } }
        }
        // Aim
        public override void SecondaryModeFunction()
        {
            if (enableAiming && isReloading == false) { anim.SetBool("aim", true); aim = true; AdditionalWeaponAimFunction.Invoke(); } else { anim.SetBool("aim", false); aim = false; }
        }
        // Reload
        public override void ReloadFunction()
        {
            if (isReloading == false && ammoClipLeft != magazineSize) { Reload(); }
        }

        public void CameraShake(float Strength)
        {
            Camera.main.GetComponent<CameraShake>().ShotShakeCamera(Strength);
        }

        #region Skin & Upgrade
        public override void LoadUpgrade()
        {
            WeaponUpgradeSaveClass _tmpSaveInfo = TinySaveSystem.GetWeaponUpgrade($"Weapon_{WeaponID}");
            if (_tmpSaveInfo.selectedUpgrade != -1)
            {
                DamageType = Upgrades[_tmpSaveInfo.selectedUpgrade].DamageType;
                weaponDamage *= Upgrades[_tmpSaveInfo.selectedUpgrade].DamageIncrease;
                weaponSpread *= Upgrades[_tmpSaveInfo.selectedUpgrade].SpreadIncrease;
                SpeedMultiplier *= Upgrades[_tmpSaveInfo.selectedUpgrade].SpeedIncrease;
                timeBetweenShot *= Upgrades[_tmpSaveInfo.selectedUpgrade].FirerateIncrease;
                BulletForce *= Upgrades[_tmpSaveInfo.selectedUpgrade].ImpactForceIncrease;

                weaponSpreadWhenAiming *= Upgrades[_tmpSaveInfo.selectedUpgrade].AimingSpreadIncrease;
                AimSpeedMultiplier *= Upgrades[_tmpSaveInfo.selectedUpgrade].AimingSpeedIncrease;

                bulletForce *= Upgrades[_tmpSaveInfo.selectedUpgrade].ForceIncrease;
                ammoCostPerShot = Upgrades[_tmpSaveInfo.selectedUpgrade].ammoCostPerShot;
                timeBetweenLaserAmmo = Upgrades[_tmpSaveInfo.selectedUpgrade].timeBetweenLaserAmmo;
                burstShot = Upgrades[_tmpSaveInfo.selectedUpgrade].burstShot;
                ChargeTime *= Upgrades[_tmpSaveInfo.selectedUpgrade].ChargeTimeIncrease;

                bulletCountPerShot = Upgrades[_tmpSaveInfo.selectedUpgrade].BulletPerShot;
                ammoType = Upgrades[_tmpSaveInfo.selectedUpgrade].ammoType;
                magazineSize = Upgrades[_tmpSaveInfo.selectedUpgrade].magazineSize;

                Debug.Log($"Loaded {_tmpSaveInfo.selectedUpgrade} Upgrade");
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

    [CustomEditor(typeof(RangeWeapon))]
    public class RangeWeaponEditor : Editor
    {
        bool EditorBool = false;
        bool Base = true;
        bool Upgrades = false;
        bool Skin = false;
        bool FOV = true;
        bool Bullet = true;
        bool Magazine = true;
        bool Vibration = true;
        bool Sound = true;
        bool Events = false;

        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            RangeWeapon weapon = (RangeWeapon)target;
            EditorGUI.indentLevel = 0;

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((RangeWeapon)target), typeof(RangeWeapon), false);
            GUI.enabled = true;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ID", GUILayout.Width(50));
            weapon.WeaponID = EditorGUILayout.IntField(weapon.WeaponID);
            if (TinyGUI.IconButton("UnityLogo", "Generate", GUILayout.Height(EditorGUIUtility.singleLineHeight))) { weapon.WeaponID = Random.Range(111111111, 999999999); }
            EditorGUILayout.EndHorizontal();


            // Play Mode Only
            if (Application.isPlaying)
            {
                EditorBool = TinyGUI.FoldoutGroup(" Preview Info", "UnityLogo", EditorBool);
                if (EditorBool)
                {
                    EditorGUILayout.BeginVertical("HelpBox");
                    TinyGUI.EditorTitle("Ammo Info (Editor Only)");
                    TinyGUI.ProgressBar(weapon.ammoClipLeft, weapon.magazineSize, "Magazine");
                    if (weapon.ammoType != AmmoType.InfinityAmmo) TinyGUI.ProgressBar(weapon.ammoLeft, GameManager.Instance.MaxAmmo(weapon.ammoType), "Ammo Left");
                    else { TinyGUI.InfoBox("Infinity Ammo Pool", "console.warnicon"); }
                    EditorGUILayout.EndHorizontal();
                }
            }

            Base = TinyGUI.FoldoutGroup(" Base Settings", "Text Icon", Base);
            if (Base)
            {
                EditorGUILayout.BeginVertical("HelpBox");

                TinyGUI.LocalizedString(weapon.weaponName, serializedObject.FindProperty("weaponName"));
                TinyGUI.LocalizedString(weapon.weaponDesc, serializedObject.FindProperty("weaponDesc"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("Slot"), new GUIContent("Weapon Slot"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Category"), new GUIContent("Weapon Category"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("AnimationType"), new GUIContent("Animaiton Type"));

                TinyGUI.Title("Base Graphics");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponIcon"), new GUIContent("Weapon Icon"));

                EditorGUILayout.EndVertical();
            }

            Upgrades = TinyGUI.FoldoutGroup($" Upgrades [{weapon.Upgrades.Count}]", "LightmapParameters Icon", Upgrades);

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

                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.LabelField("Field of View", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("enableAiming"), new GUIContent("Enable Aim"));
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.LabelField("Bullet Settings", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("DamageType"), new GUIContent("Damage Type"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("DamageIncrease"), new GUIContent("Damage Increase"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("SpreadIncrease"), new GUIContent("Spread Increase"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("SpeedIncrease"), new GUIContent("Mov Speed Increase"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("FirerateIncrease"), new GUIContent("Firerate Increase"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("ImpactForceIncrease"), new GUIContent("Impact Force Increase"));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("AimingSpreadIncrease"), new GUIContent("Aiming Spread Increase"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("AimingSpeedIncrease"), new GUIContent("Aiming Mov Speed Increase"));


                    if (weapon.WeaponShootType == WeaponTypeShooting.Projectile) EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("ForceIncrease"), new GUIContent("Force Increase"));
                    if (weapon.FireMode == RangeWeaponShootType.Laser) EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("ammoCostPerShot"), new GUIContent("Ammo Cost per Shot"));
                    if (weapon.FireMode == RangeWeaponShootType.Laser) EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("timeBetweenLaserAmmo"), new GUIContent("Time Between Laser Ammo"));
                    if (weapon.FireMode == RangeWeaponShootType.Burst) EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("burstShot"), new GUIContent("Burst Shot"));
                    if (weapon.FireMode == RangeWeaponShootType.Charge) EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("ChargeTimeIncrease"), new GUIContent("Charge Time Increase"));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("BulletPerShot"), new GUIContent("Bullet Per Shot"));
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.LabelField("Magazine Settings", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("ammoType"), new GUIContent("Ammo Type"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Upgrades").GetArrayElementAtIndex(index).FindPropertyRelative("magazineSize"), new GUIContent("Magazine Size"));
                    EditorGUILayout.EndVertical();

                    TinyGUI.InfoBox("Only the basic data will be overwritten, and all increases will remain unchanged");
                    if (TinyGUI.IconButton("IN foldout act on", " Load Base Stats"))
                    {
                        weapon.Upgrades[i].DamageType = weapon.DamageType;
                        weapon.Upgrades[i].enableAiming = weapon.enableAiming;
                        weapon.Upgrades[i].BulletPerShot = weapon.bulletCountPerShot;
                        weapon.Upgrades[i].ammoType = weapon.ammoType;
                        weapon.Upgrades[i].magazineSize = weapon.magazineSize;
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
                    weapon.Upgrades.Add(new RangeUpgradeClass());
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.EndVertical(); // End of List Vertical
            }

            Skin = TinyGUI.FoldoutGroup($" Skins [{weapon.Skins.Count}]", "Cloth Icon", Skin);
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


            FOV = TinyGUI.FoldoutGroup(" Field of View Settings", "ViewToolOrbit On", FOV);
            if (FOV)
            {
                EditorGUILayout.BeginVertical("HelpBox");

                EditorGUILayout.PropertyField(serializedObject.FindProperty("enableAiming"), new GUIContent("Enable Aim"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("baseFOV"), new GUIContent("Base Field of View"));
                if (weapon.enableAiming) EditorGUILayout.PropertyField(serializedObject.FindProperty("aimFOV"), new GUIContent("Aiming Field of View"));

                EditorGUILayout.EndVertical();
            }


            Bullet = TinyGUI.FoldoutGroup(" Bullet Settings", "SettingsIcon", Bullet);
            if (Bullet)
            {
                EditorGUILayout.BeginVertical("HelpBox");

                TinyGUI.InfoBox($"Damage: {weapon.weaponDamage} x {weapon.bulletCountPerShot}\nFire Rate: {(1 / weapon.timeBetweenShot).ToString("0.00")} Bullet / s\nMagazine: {weapon.magazineSize} {weapon.ammoType}\nAmmo Type: {weapon.bulletType} {weapon.WeaponShootType}\nWeapon Damage per Second: {(weapon.weaponDamage * weapon.bulletCountPerShot * (1 / weapon.timeBetweenShot)).ToString("0.0")}", "AudioMixerController Icon");

                EditorGUILayout.PropertyField(serializedObject.FindProperty("WeaponShootType"), new GUIContent("Shoot Type"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("FireMode"), new GUIContent("Fire Mode"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletType"), new GUIContent("Bullet Type"));
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DamageType"), new GUIContent("Damage Type"));
                if (weapon.bulletType == BulleType.Explosion && weapon.DamageType != DamageType.Explosion) { TinyGUI.InfoBox("If Bullet Type is set to Explosion, Damage Type will be switched to Explosion"); }
                EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponDamage"), new GUIContent("Weapon Damage"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponSpread"), new GUIContent("Weapon Spread"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("SpeedMultiplier"), new GUIContent("Speed Multiplier"));
                //if (weapon.enableAiming) EditorGUILayout.HelpBox($"If aim then the speed of movement is {weapon.SpeedMultiplier * weapon.AimSpeedMultiplier} ({weapon.SpeedMultiplier} * {weapon.AimSpeedMultiplier}).", MessageType.None);

                // Firerate
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("timeBetweenShot"), new GUIContent("Firerate"));
                EditorGUILayout.LabelField(new GUIContent("Curve"), GUILayout.Width(50));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("EnableTBSCurve"), GUIContent.none, GUILayout.Width(25));
                EditorGUILayout.EndHorizontal();
                if (weapon.EnableTBSCurve) { EditorGUILayout.PropertyField(serializedObject.FindProperty("TBSCurve"), new GUIContent("Firerate Curve")); }
                //
                EditorGUILayout.PropertyField(serializedObject.FindProperty("raycastMask"), new GUIContent("Hit Raycast Mask"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BulletForce"), new GUIContent("Impact Bullet Force"));

                // Aim Settings
                if (weapon.enableAiming)
                {
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.BeginVertical("HelpBox");
                    EditorGUILayout.LabelField("Aim Settings", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponSpreadWhenAiming"), new GUIContent("Aiming Spread"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AimSpeedMultiplier"), new GUIContent("Aim Speed Multiplier"));

                    EditorGUILayout.EndVertical();
                }

                // Shoot Type
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.LabelField("Shoot Settings", EditorStyles.boldLabel);
                EditorGUILayout.EndVertical();

                if (weapon.WeaponShootType == WeaponTypeShooting.Hitscan) EditorGUILayout.PropertyField(serializedObject.FindProperty("hitscanRange"), new GUIContent("Hitscan Range"));
                if (weapon.WeaponShootType == WeaponTypeShooting.Projectile) EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletForce"), new GUIContent("Bullet Force"));
                if (weapon.WeaponShootType == WeaponTypeShooting.Projectile) EditorGUILayout.PropertyField(serializedObject.FindProperty("rocketPrefab"), new GUIContent("Missle Prefab"));
                if (weapon.WeaponShootType == WeaponTypeShooting.Projectile) EditorGUILayout.PropertyField(serializedObject.FindProperty("ProjectileMass"), new GUIContent("Mass"));
                if (weapon.WeaponShootType == WeaponTypeShooting.Projectile) EditorGUILayout.PropertyField(serializedObject.FindProperty("ProjectileUseGravity"), new GUIContent("Use Gravity"));
                if (weapon.WeaponShootType == WeaponTypeShooting.Projectile) EditorGUILayout.PropertyField(serializedObject.FindProperty("ProjectileMaterial"), new GUIContent("Physic Material"));
                if (weapon.WeaponShootType == WeaponTypeShooting.Projectile) EditorGUILayout.PropertyField(serializedObject.FindProperty("BreakOnCollision"), new GUIContent("Activate On Collision"));
                if (weapon.WeaponShootType == WeaponTypeShooting.Projectile) EditorGUILayout.PropertyField(serializedObject.FindProperty("CollisionSound"), new GUIContent("Collision Sound"));

                EditorGUILayout.EndVertical();

                // Fire Mode
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.LabelField("Fire Mode Settings", EditorStyles.boldLabel);
                EditorGUILayout.EndVertical();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletCountPerShot"), new GUIContent("Bullet per Shot"));
                if (weapon.FireMode == RangeWeaponShootType.Laser) EditorGUILayout.LabelField("Laser Exclusive Settings", EditorStyles.boldLabel);
                if (weapon.FireMode == RangeWeaponShootType.Laser) EditorGUILayout.PropertyField(serializedObject.FindProperty("ammoCostPerShot"), new GUIContent("Ammo per Shot"));
                if (weapon.FireMode == RangeWeaponShootType.Laser) EditorGUILayout.PropertyField(serializedObject.FindProperty("timeBetweenLaserAmmo"), new GUIContent("Time Between Laser Ammo"));
                if (weapon.FireMode == RangeWeaponShootType.Burst) EditorGUILayout.LabelField("Burst Exclusive Settings", EditorStyles.boldLabel);
                if (weapon.FireMode == RangeWeaponShootType.Burst) EditorGUILayout.PropertyField(serializedObject.FindProperty("burstShot"), new GUIContent("Burst Shot Count"));
                if (weapon.FireMode == RangeWeaponShootType.Charge) EditorGUILayout.LabelField("Charge Exclusive Settings", EditorStyles.boldLabel);
                if (weapon.FireMode == RangeWeaponShootType.Charge) EditorGUILayout.PropertyField(serializedObject.FindProperty("ChargeTime"), new GUIContent("Charge Time"));
                if (weapon.FireMode == RangeWeaponShootType.Charge) EditorGUILayout.PropertyField(serializedObject.FindProperty("ChargeCancel"), new GUIContent("Cancel Charge Enable"));
                if (weapon.FireMode == RangeWeaponShootType.Charge && weapon.ChargeCancel) EditorGUILayout.PropertyField(serializedObject.FindProperty("ChargeCurve"), new GUIContent("Charge Curve"));
                if (weapon.FireMode == RangeWeaponShootType.Charge && weapon.ChargeCancel) EditorGUILayout.PropertyField(serializedObject.FindProperty("ChargeMinLimit"), new GUIContent("Charge Limit"));

                EditorGUILayout.EndVertical();

                // Bullet Type
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.LabelField("Bullet Settings", EditorStyles.boldLabel);
                EditorGUILayout.EndVertical();
                if (weapon.bulletType == BulleType.Normal)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletHole"), new GUIContent("Bullet Hole"));
                }
                if (weapon.bulletType == BulleType.Explosion)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionRadius"), new GUIContent("Explosion Radius"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionDamageCurve"), new GUIContent("Explosion Damage Curve"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionShakeDistance"), new GUIContent("Explosion Shake Distance"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionPrefab"), new GUIContent("Explosion Prefab"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionSound"), new GUIContent("Explosion Sound"));
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndVertical();
            }



            Magazine = TinyGUI.FoldoutGroup(" Ammo & Magazine Settings", "ParentConstraint Icon", Magazine);
            if (Magazine)
            {
                EditorGUILayout.BeginVertical("HelpBox");

                EditorGUILayout.PropertyField(serializedObject.FindProperty("ammoType"), new GUIContent("Ammo Type"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("magazineSize"), new GUIContent("Magazine Size"));

                //EditorGUILayout.HelpBox("Settings Magazine\nIf Magazine Size is 0, the weapon takes ammunition directly from the ammo poll, otherwise the weapon will have a magazine of the set size", MessageType.Info);
                TinyGUI.InfoBox("Settings Magazine\nIf Magazine Size is 0, the weapon takes ammunition directly from the ammo poll, otherwise the weapon will have a magazine of the set size", "d_Settings Icon");

                EditorGUILayout.EndVertical();
            }

            Vibration = TinyGUI.FoldoutGroup(" Vibration Settings", "AudioSpatializerMicrosoft Icon", Vibration);
            if (Vibration)
            {
                EditorGUILayout.BeginVertical("HelpBox");

                //TinyGUI.InfoBox("Default vibration is enabled only on Joystick, unless you enable it more equally on mobile devices", "console.infoicon");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("VibrationLowFrequency"), new GUIContent("Vibration Low Frequency"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("VibrationHighFrequency"), new GUIContent("Vibration High Frequency"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("VibrationTimer"), new GUIContent("Vibration Timer"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("VibrationMobile"), new GUIContent("Vibration On Mobile"));

                TinyGUI.Guideline("How to Use Vibration?", "By default, vibrations are enabled only for Joystick.", "You can enable vibrations on mobile devices by setting \"Vibration On Mobile\" to True.");

                EditorGUILayout.EndVertical();
            }

            Sound = TinyGUI.FoldoutGroup(" Sound Settings", "AudioListener Icon", Sound);
            if (Sound)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("shotSound"), new GUIContent("Shot Sound"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("reloadSound"), new GUIContent("Reload Sound"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("emptyGunSound"), new GUIContent("Empty Shot Sound"));
                EditorGUILayout.EndVertical();
            }
            Events = TinyGUI.FoldoutGroup(" Unity Events Settings", "EventSystem Icon", Events);
            if (Events)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AdditionalFireWeaponFunction"), new GUIContent("On Fire"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AdditionalWeaponAimFunction"), new GUIContent("On Aim"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AdditionalWeaponReloadFunction"), new GUIContent("On Reload"));
                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}