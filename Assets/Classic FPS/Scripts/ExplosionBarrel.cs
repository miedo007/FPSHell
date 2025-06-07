using HellishBattle.Enemies;
using HellishBattle.Weapon;
using UnityEngine;

namespace HellishBattle
{
    public class ExplosionBarrel : MonoBehaviour
    {
        public Sprite Barrel;
        public Sprite BarrelDestroyed;

        public GameObject Explosion;
        public GameObject ExplosionPrefab;
        public float damage;
        public AnimationCurve damageCurve;
        public float radius;
        public float explosionShakeDistance;
        public AudioClip ExplosionSound;
        public LayerMask explosionLayerMask;

        bool exploded;

        private void Awake()
        {
            GetComponent<SpriteRenderer>().sprite = Barrel;
        }

        void GetDamage(DamageClass dmg)
        {
            if (!exploded)
            {
                GetComponent<SpriteRenderer>().sprite = BarrelDestroyed;

                Explosion.SetActive(true);
                Explosion.GetComponent<BulletScript>().SetupType(BulleType.Explosion);
                Explosion.GetComponent<BulletScript>().SetupBase(dmg.Damage, DamageType.Explosion, 20, 0);
                Explosion.GetComponent<BulletScript>().SetupExplosion(radius, explosionLayerMask, ExplosionPrefab, ExplosionSound, damageCurve, explosionShakeDistance);
                //Explosion.GetComponent<BulletScript>().life
                exploded = true;
            }
        }
    }
}
