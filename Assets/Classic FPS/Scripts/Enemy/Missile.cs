using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HellishBattle.Enemies
{
    public class Missile : MonoBehaviour
    {
        [HideInInspector]
        public float damage;
        [HideInInspector]
        public float speed;
        [HideInInspector]
        public Transform source;
        [HideInInspector]
        public bool Guided = false;

        [HideInInspector]
        public Collider[] EnemyColli;


        // Privates
        Transform player;
        int missileLife;
        float timer;
        [HideInInspector]
        public float rotationSpeed = .5f;

        void Start()
        {
            missileLife = 15;
            player = GameObject.FindGameObjectWithTag("Player").transform;
            transform.LookAt(player);
        }

        void Update()
        {
            timer += Time.deltaTime;
            if (timer > missileLife) Destroy(this.gameObject);

            if (Guided)
            {
                if (player != null)
                {
                    Vector3 targetDirection = (player.position - transform.position).normalized;
                    transform.forward = Vector3.Slerp(transform.forward, targetDirection, Time.deltaTime * rotationSpeed);
                    transform.position += transform.forward * Time.deltaTime * speed;
                }
            }
            else
            {
                transform.Translate(Vector3.forward * Time.deltaTime * speed);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            bool Collision = true;
            for (int i = 0; i < EnemyColli.Length; i++)
            {
                if (EnemyColli[i] == other) { Collision = false; }
            }

            if (Collision)
            {
                other.SendMessage("GetDamage", new DamageClass(damage), SendMessageOptions.DontRequireReceiver);
                other.SendMessage("DamageIndicatorFunction", source, SendMessageOptions.DontRequireReceiver);
                Destroy(this.gameObject);
            }
        }
    }
}