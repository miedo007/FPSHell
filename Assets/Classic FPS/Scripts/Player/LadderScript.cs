using HellishBattle.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HellishBattle
{
    public class LadderScript : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Camera.main.transform.parent.GetComponent<PlayerMovement>().state = MovementState.Climbing;
            Camera.main.transform.parent.GetComponent<PlayerMovement>().cc.Move(new Vector3(0, .1f, 0));
        }

        private void OnTriggerStay(Collider other)
        {
            Camera.main.transform.parent.GetComponent<PlayerMovement>().state = MovementState.Climbing;

        }

        private void OnTriggerExit(Collider other)
        {
            Camera.main.transform.parent.GetComponent<PlayerMovement>().state = MovementState.Idle;
        }
    }
}
