using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HellishBattle.Ability
{
    [CreateAssetMenu(fileName = "Self Explosion", menuName = "Hellish Battle/Ability List/Self Explosion", order = 48)]
    public class SelfExplosionAbilitySO : BaseAbilityScript
    {
        [Line("Explosion")]
        public LayerMask layerMask;
        public float Damage = 250;
        public float Radius = 25f;
        public AnimationCurve DamageCurve;

        public override void ActivateAbility()
        {
            Collider[] hitColliders = Physics.OverlapSphere(GameManager.Instance.transform.parent.transform.position, Radius, layerMask);

            foreach (Collider col in hitColliders)
            {
                float _distance = Vector3.Distance(col.transform.position, GameManager.Instance.transform.parent.transform.position);

                col.SendMessage("TakeDamage", Damage * DamageCurve.Evaluate(_distance / Radius), SendMessageOptions.DontRequireReceiver);
                col.SendMessage("ExplosionBlood", SendMessageOptions.DontRequireReceiver);
            }

            Camera.main.GetComponent<CameraShake>().ShakeCamera();
        }
    }
}
