using HellishBattle.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HellishBattle.Level
{
    public class TeleportScript : MonoBehaviour
    {
        public Transform Spawnpoint;

        public void Teleport()
        {
            // Teleport
            CharacterController cc = Camera.main.transform.parent.GetComponent<CharacterController>();

            cc.transform.GetComponent<PlayerMovement>().fallStartPos = Spawnpoint.position;


            cc.enabled = false; // Disable CharacterController before changing position
            cc.transform.position = Spawnpoint.position;
            cc.enabled = true; // Enable CharacterController after position change
        }
    }
}
