using HellishBattle.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HellishBattle.Ability
{
    [CreateAssetMenu(fileName = "Dash Ability", menuName = "Hellish Battle/Ability List/Dash Ability", order = 48)]
    public class DashAbilitySO : BaseAbilityScript
    {
        [Line("Dash Settings")]
        public float DashTime = .2f;
        public float DashSpeed = 20;

        public override void ActivateAbility()
        {
            Camera.main.transform.parent.GetComponent<PlayerMovement>().StartCoroutine(Camera.main.transform.parent.GetComponent<PlayerMovement>().Dash(DashTime, DashSpeed));
        }
    }
}
