using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using HellishBattle.Enemies;
using HellishBattle.Level;

namespace HellishBattle.Weapon
{
    [RequireComponent(typeof(AudioSource))]
    public class BulletScript : MonoBehaviour
    {
        ThrowableItemType bulletType;
        float Damage;
        DamageType damageType;
        float LifeTime = 3;
        float HitForce;
        bool BreakOnCollision = false;
        AudioClip CollisionSound;
        LayerMask DamageLayerMask;

        //      Explosion
        float ExplosionRadius;
        GameObject ExplosionPrefab;
        AudioClip ExplosionSound;
        AnimationCurve ExplosionDamageCurve;
        float ExplosionShakeDistamce;

        //      Damage Zone
        float DZDuration;
        float DZDamageInterval;
        float DZRadius;
        float DZMaxGroundDistance;

        // FX
        bool _collisionFXCheck, _activationFXCheck;
        List<GameObject> CollisionFX, AcivationFX;
        // Events
        public UnityEvent OnActivation, OnCollision;


        // Private
        float _expiredTime;
        AudioSource _sound;

        ParticleSystem _activateFX;
        ParticleSystem _colliderFX;

        private void Awake()
        {
            //
            _sound = transform.GetComponent<AudioSource>();
        }

        public void Update()
        {
            // TIme After Spawn
            _expiredTime += Time.deltaTime;

            // Activate After Life Time
            if (_expiredTime > LifeTime) { Activate(); }

        }

        void Activate()
        {
            // Type
            _activateFX = null;
            if (_activationFXCheck)
            {
                if (AcivationFX.Count != 0)
                {
                    GameObject toCheck = AcivationFX[Random.Range(0, AcivationFX.Count)];
                    if (toCheck != null)
                    {
                        GameObject go = Instantiate(toCheck, transform.position, Quaternion.identity);
                        _activateFX = go.GetComponent<ParticleSystem>();
                    }
                }
            }

            /// Explosion
            if (bulletType == ThrowableItemType.Explosion)
            {
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, ExplosionRadius, DamageLayerMask);
                GameObject explosionInstantiated = (GameObject)Instantiate(ExplosionPrefab, transform.position, Quaternion.identity);
                explosionInstantiated.GetComponent<Explosion>().explosionSound = ExplosionSound;

                foreach (Collider col in hitColliders)
                {
                    // Calculate Distance
                    float _distance = Vector3.Distance(col.transform.position, this.transform.position);
                    // Get Damage
                    col.SendMessage("GetDamage", new DamageClass(Damage * ExplosionDamageCurve.Evaluate(_distance / ExplosionRadius), DamageType.Explosion), SendMessageOptions.DontRequireReceiver);
                    // Add Force
                    Rigidbody hitRigidbody = col.GetComponent<Rigidbody>();
                    if (hitRigidbody != null) { hitRigidbody.AddForce(transform.forward * HitForce * ExplosionDamageCurve.Evaluate(_distance / ExplosionRadius), ForceMode.Impulse); }
                }
                // Shake Camera
                if (Vector3.Distance(this.transform.position, Camera.main.transform.position) < ExplosionShakeDistamce) { Camera.main.GetComponent<CameraShake>().ShakeCamera(); }
            }
            /// Normal
            if (bulletType == ThrowableItemType.Normal)
            {
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, .5f, DamageLayerMask);

                foreach (Collider col in hitColliders)
                {
                    // Debug.Log(col.name);
                    // Get Damage
                    col.SendMessage("GetDamage", new DamageClass(Damage, damageType), SendMessageOptions.DontRequireReceiver);
                }
            }
            /// Damage Zone
            if (bulletType == ThrowableItemType.DamageZone)
            {
                GameObject DamageZoneObj = new GameObject("DamageZoneObject");
                DamageZoneObj.transform.position = transform.position;
                DamageZoneObj.transform.rotation = Quaternion.identity;
                // Add Collider
                DamageZoneObj.AddComponent<BoxCollider>();
                DamageZoneObj.GetComponent<BoxCollider>().size = new Vector3(DZRadius * 2, .5f, DZRadius * 2);
                DamageZoneObj.GetComponent<BoxCollider>().isTrigger = true;
                DamageZoneObj.GetComponent<BoxCollider>().center = new Vector3(0, .25f, 0);
                // Add Damage Zone
                DamageZoneObj.AddComponent<DangerZone>();
                DamageZoneObj.GetComponent<DangerZone>().Type = AreaType.DamageArea;
                DamageZoneObj.GetComponent<DangerZone>().damageType = damageType;
                DamageZoneObj.GetComponent<DangerZone>().DamageMask = DamageLayerMask;
                DamageZoneObj.GetComponent<DangerZone>().BaseDamage = Damage;
                DamageZoneObj.GetComponent<DangerZone>().DamageInterval = DZDamageInterval;

                Destroy(_activateFX, DZDuration);
                Destroy(DamageZoneObj, DZDuration);
            }

            // Destroy After
            Destroy(this.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (BreakOnCollision)
            {
                Activate();
            }
            else
            {
                if (_collisionFXCheck)
                {
                    GameObject toCheck = CollisionFX[Random.Range(0, CollisionFX.Count)];
                    if (toCheck != null)
                    {
                        GameObject go = Instantiate(toCheck, transform.position, Quaternion.identity);
                        _colliderFX = go.GetComponent<ParticleSystem>();
                    }
                }

                if (CollisionSound != null) _sound.PlayOneShot(CollisionSound);
                else Debug.LogWarning("Empty AudioClip");
            }
        }

        #region Setup
        // Setup Bullet Type From Range Weapon Script
        public void SetupType(BulleType type) { if (type == BulleType.Explosion) { bulletType = ThrowableItemType.Explosion; } else { bulletType = ThrowableItemType.Normal; } }
        // Setup Bullet Type From Throwable Weapon Script
        public void SetupType(ThrowableItemType type) { bulletType = type; }

        public void SetupBase(float DMG, DamageType DMGType, float BaseHitForce, float Lifetime)
        {
            Damage = DMG;
            damageType = DMGType;
            LifeTime = Lifetime;
            HitForce = BaseHitForce;
        }

        public void SetupExplosion(float Radius, LayerMask LayersMask, GameObject Prefab, AudioClip Sound, AnimationCurve Curve, float ShakeDistance)
        {
            DamageLayerMask = LayersMask;

            ExplosionRadius = Radius;
            ExplosionPrefab = Prefab;
            ExplosionSound = Sound;
            ExplosionDamageCurve = Curve;
            ExplosionShakeDistamce = ShakeDistance;
        }

        public void SetupNormal(LayerMask LayersMask)
        {
            DamageLayerMask = LayersMask;
        }

        public void SetupDamageZone(LayerMask LayersMask, float damage, float Duration, float Interval, float Radius, float MaxDistance)
        {
            Damage = damage;
            DZDuration = Duration;
            DZDamageInterval = Interval;
            DZRadius = Radius;
            DZMaxGroundDistance = MaxDistance;
            DamageLayerMask = LayersMask;
        }

        public void SetupObject(bool Gravity, float ObjectMass, PhysicMaterial Material, bool ActivateColision, AudioClip Collision)
        {
            transform.GetComponent<Rigidbody>().mass = ObjectMass;
            transform.GetComponent<Rigidbody>().useGravity = Gravity;
            transform.GetComponent<Collider>().material = Material;

            BreakOnCollision = ActivateColision;
            CollisionSound = Collision;
        }

        public void SetupActivateFX(List<GameObject> list) { AcivationFX = list; _activationFXCheck = true; }
        public void SetupCollisionFX(List<GameObject> list) { CollisionFX = list; _collisionFXCheck = true; }

        #endregion

        
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(BulletScript))]
    public class BulletScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            TinyGUI.InfoBox("Bullet Script cannot be edited in the inspector, all Bullet Script variables are assigned when the object is spawned in scripts such as Throwable Weapon, Range Weapon, etc.");
            TinyGUI.InfoBox("Rigidbody script is also overwritten by Bullet Script");
        }
    }
#endif
}
