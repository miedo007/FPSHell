using HellishBattle.Enemies;
using HellishBattle.Level;
using UnityEngine;

namespace HellishBattle.Ability
{
    [CreateAssetMenu(fileName = "Poison Cloud", menuName = "Hellish Battle/Ability List/Damage Cloud", order = 48)]
    public class PoisonCloudAbilitySO : BaseAbilityScript
    {
        [Line("Stay Time")]
        public float StayTime = 5f;
        [Line("Prefabs")]
        public GameObject FXPrefab;
        public GameObject DamageZonePrefab;
        [Line("Size")]
        public float Radius = 4f;
        public float Height = 2f;
        [Line("Damage")]
        public DamageType type = DamageType.Poison;
        public float Damage = 5f;
        public float Interval = .5f;

        public override void ActivateAbility()
        {
            // Particle
            GameObject FX = Instantiate(FXPrefab, Camera.main.transform.parent.position + new Vector3(0, -Camera.main.transform.parent.GetComponent<CharacterController>().height / 2, 0), new Quaternion());
            FX.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);

            ParticleSystem PS = FX.GetComponent<ParticleSystem>();

            var mainModule = PS.main;
            mainModule.duration = StayTime;

            var shape = PS.shape;
            shape.radius = Radius;
            shape.scale = new Vector3(1, 1, Height / Radius);

            // Damage Zone
            GameObject Zone = Instantiate(DamageZonePrefab, Camera.main.transform.parent.position + new Vector3(0, -Camera.main.transform.parent.GetComponent<CharacterController>().height / 2, 0), new Quaternion());

            DangerZone DZ = Zone.GetComponent<DangerZone>();
            DZ.BaseDamage = Damage;
            DZ.damageType = type;
            DZ.DamageInterval = Interval;

            Zone.GetComponent<BoxCollider>().size = new Vector3(Radius * 2, Height, Radius * 2);
            Zone.GetComponent<BoxCollider>().center = new Vector3(0, Height / 2, 0);
            Destroy(Zone, StayTime);
        }
    }
}