using HellishBattle.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HellishBattle.Ability
{
    [CreateAssetMenu(fileName = "Instant Get", menuName = "Hellish Battle/Ability List/Instant Get", order = 48)]
    public class InstantGetAbilitySO : BaseAbilityScript
    {
        [Line("Health")]
        public int AddHealth = 50;
        [Line("Armor")]
        public int AddArmor = 50;

        public override void ActivateAbility()
        {
            if (AddHealth != 0) GameManager.Instance.transform.parent.GetComponent<PlayerHealth>().AddHealth(AddHealth, true);
            if (AddArmor != 0) GameManager.Instance.transform.parent.GetComponent<PlayerHealth>().AddArmor(AddArmor, true);
        }
    }
}
