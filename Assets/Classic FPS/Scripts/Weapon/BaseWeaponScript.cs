using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using HellishBattle.Enemies;

namespace HellishBattle.Weapon
{
    public class BaseWeaponScript : MonoBehaviour
    {
        // ID to Save Stats
        public int WeaponID;

        public SlotType Slot;
        public WeaponCategory Category;
        public LocalizedString weaponName;
        public LocalizedString weaponDesc;
        public Sprite weaponIcon;

        [Range(0, 1.5f), Suffix("x Speed")] public float SpeedMultiplier = 1;
        [Range(0, 1), Suffix("x Speed")] public float AimSpeedMultiplier = .8f;


        public virtual void PrimaryModeFunction() { }

        public virtual void SecondaryModeFunction() { }

        public virtual void ReloadFunction() { }

        public virtual void UpdateLeftAmmo() { }

        // Skin / Upgrade
        public virtual void LoadUpgrade() { }

        public virtual void LoadSkin() { }

        public virtual Sprite WeaponIcon()
        {
            return null;
        }

        public virtual string WeaponName() { return "Error"; }

        public virtual List<WeaponSkinClass> GetSkinClass() { return new List<WeaponSkinClass>(); }
        public virtual List<WeaponUpgradeClass> GetUpgradeClass() { return new List<WeaponUpgradeClass>(); }
    }

    public enum SlotType
    {
        PrimaryWeapon = 0, SecoundaryWeapon = 1, MelleWeapon = 2, ThrowableWeapon = 3
    }
    public enum WeaponCategory
    {
        Pistol = 0, Revolver = 1, SMG = 2, LKM = 3, RKM = 4, Rifle = 5, LevelActionRifle = 6, Sniper = 7, Shotgun = 8, RocketLancher = 9, GrenadeLancher = 10, Laser = 13, Bow = 11, Special = 12,
        Knife = 100, Axe = 101, Bat = 102, Sword = 103, Dagger = 104, Staff = 105, Whip = 106,
        Grenade = 200, ThrowableKnife = 201, ThrowableDagger = 202
    }

    public enum WeaponTypeShooting
    {
        Hitscan = 0,
        Projectile = 1
    }
    // OLD
    public enum PrimaryFireMode
    {
        Single = 0,
        //Burst = 1,
        Automatic = 2
    }
    public enum SecondaryFireMode
    {
        None = 0,
        Aim = 1
    }
    // OLD

    public enum RangeWeaponShootType
    {
        Single = 0,
        Burst = 1,
        Automatic = 2,
        Charge = 3,
        Laser = 4
    }

    public enum BulleType
    {
        Normal = 0,
        Explosion = 1
    }

    public enum AlternativeMelleType
    {
        None = 0,
        AlternativeAttack = 1,
        Block = 2
    }

    public enum AnimationWeaponType
    {
        SingleWeapon = 0,
        DualWeapon = 1
    }

    [System.Serializable]
    public class WeaponSkinClass
    {
        public int Cost;
        public LocalizedString WeaponName;
        public RuntimeAnimatorController SkinAnimation;
        public Sprite SkinUIIcon;
    }

    [System.Serializable]
    public class WeaponUpgradeClass : WeaponSkinClass
    {
        public LocalizedString UpgradeDescription;
    }

    [System.Serializable]
    public class RangeUpgradeClass : WeaponUpgradeClass
    {
        // FOV
        public bool enableAiming = false;

        // Bullet
        public DamageType DamageType;
        [Range(0.2f, 2f)] public float DamageIncrease = 1;
        [Range(0.2f, 2f)] public float SpreadIncrease = 1;
        [Range(0.2f, 2f)] public float SpeedIncrease = 1;
        [Range(0.2f, 2f)] public float FirerateIncrease = 1f;
        [Range(0.2f, 2f)] public float ImpactForceIncrease = 1;

        [Range(0.2f, 2f)] public float AimingSpreadIncrease = 1;
        [Range(0.2f, 2f)] public float AimingSpeedIncrease = 1;

        [Range(0.2f, 2f)] public float ForceIncrease = 1;       // Projectile
        public int ammoCostPerShot = 1;                         // Laser
        public float timeBetweenLaserAmmo = .7f;                // Laser
        public int burstShot = 3;                               // Burst
        [Range(0.2f, 2f)] public float ChargeTimeIncrease = 1;  // Charge

        public int BulletPerShot = 1;

        // Ammo
        public AmmoType ammoType;
        public int magazineSize;
    }
    [System.Serializable]
    public class MelleUpgradeClass : WeaponUpgradeClass
    {
        public AlternativeMelleType AlternativeMode;

        // Base Attack
        [Range(0.2f, 2f)] public float DamageIncrease = 1;
        [Range(0.2f, 2f)] public float RangeIncrease = 1;
        [Range(1, 180)] public float angle = 45;
        public float DamageDelay = 0.2f;
        public float Delay = 0.2f;

        // Alt Base Attack
        [Range(0.2f, 2f)] public float AltDamageIncrease = 1;
        [Range(0.2f, 2f)] public float AltRangeIncrease = 1;
        [Range(1, 180)] public float AltAngle = 45;
        public float AltDamageDelay = 0.2f;
        public float AltDelay = 0.2f;

        //[Line("Block")]
        [Suffix("%"), Range(0, 100)] public float BlockingDamageChange = 30;
        [Suffix("%"), Range(0, 100)] public float DamageReductionPercentage = 30;
    }
    [System.Serializable]
    public class ThrowableUpgradeClass : WeaponUpgradeClass
    {

        public ThrowableItemType ItemType;

        // Base
        public float Damage = 80;
        [Range(0.01f, 10f)] public float ProjectileMass = 2;
        public PhysicMaterial ProjectileMaterial;
        public bool BreakOnCollision = false;

        // Explosion
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

        // Base Throw
        [Suffix("Seconds")] public float BaseTimeLifeItem = 3f;
        [Suffix("Seconds")] public float BaseTimeBetweenThrow = .75f;
        public float BaseThrowForce = 25f;
        [Suffix("Seconds")] public float BaseSpawnAfter = .4f;

        // Alternative Throw
        public bool AlternativeThrow = true;
        [Suffix("Seconds")] public float AlternativeTimeLifeItem = 3f;
        [Suffix("Seconds")] public float AlternativeTimeBetweenThrow = .75f;
        public float AlternativeThrowForce = 25f;
        [Suffix("Seconds")] public float AlternativeSpawnAfter = .4f;
    }
}
