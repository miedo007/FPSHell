using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HellishBattle.Ability
{
    [CreateAssetMenu(fileName = "Supply Drop", menuName = "Hellish Battle/Ability List/Supply Drop", order = 48)]
    public class SupplyDropAbilitySO : BaseAbilityScript
    {
        [Line("Get Ammo")]
        public int AddBullet = 30;
        public int AddShell = 8;
        public int AddClip = 60;
        public int AddGrenade = 2;
        public int AddRocket = 2;
        public int AddEnergyCell = 24;
        public int AddHandGrenade = 3;

        public override void ActivateAbility()
        {
            GameManager.Instance.AddAmmo(AmmoType.Bullet, AddBullet);
            GameManager.Instance.AddAmmo(AmmoType.Shell, AddShell);
            GameManager.Instance.AddAmmo(AmmoType.Clip, AddClip);
            GameManager.Instance.AddAmmo(AmmoType.Grenade, AddGrenade);
            GameManager.Instance.AddAmmo(AmmoType.Rocket, AddRocket);
            GameManager.Instance.AddAmmo(AmmoType.EnergyCell, AddEnergyCell);
            GameManager.Instance.AddAmmo(AmmoType.HandGrenade, AddHandGrenade);
        }
    }
}
