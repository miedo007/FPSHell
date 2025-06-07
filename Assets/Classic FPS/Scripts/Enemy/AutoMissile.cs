using UnityEngine;

namespace HellishBattle.Enemies
{
    public class AutoMissile : MonoBehaviour
    {

        [HideInInspector]
        public float damage;
        [HideInInspector]
        public float speed;
        Transform player;
        int missileLife;
        float timer;

        void Start()
        {
            missileLife = 7;
            player = GameObject.FindGameObjectWithTag("Player").transform;

        }

        void Update()
        {
            transform.LookAt(player);
            timer += Time.deltaTime;
            if (timer > missileLife)
                Destroy(this.gameObject);
            transform.Translate(Vector3.forward * Time.deltaTime * speed);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.SendMessage("EnemyHit", damage, SendMessageOptions.DontRequireReceiver);
            }
            Destroy(this.gameObject);
        }
    }
}