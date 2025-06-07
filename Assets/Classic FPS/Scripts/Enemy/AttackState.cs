using HellishBattle.Enemies;
using UnityEngine;

namespace HellishBattle.AI
{
    public class AttackState : IEnemyAI
    {

        EnemyStates enemy;
        float timer;

        public AttackState(EnemyStates enemy)
        {
            this.enemy = enemy;
        }


        public void UpdateActions()
        {
            timer += Time.deltaTime;
            float distance = Vector3.Distance(enemy.chaseTarget.transform.position, enemy.transform.position);

            Watch();

            // Chase Detect
            if (enemy.AttackType == EnemyAttackType.Melle && distance > enemy.ActualMelleAttack.attackRange)
            {
                ToChaseState();

            }
            if (enemy.AttackType == EnemyAttackType.Range && distance > enemy.ActualRangeAttack.shootRange)
            {
                ToChaseState();
            }

            float biggerRange = enemy.ActualRangeAttack.shootRange;
            if (biggerRange < enemy.ActualMelleAttack.attackRange) { biggerRange = enemy.ActualMelleAttack.attackRange; }

            if (enemy.AttackType == EnemyAttackType.MelleAndRange && distance > biggerRange)
            {
                ToChaseState();
            }

            // Do Attack

            // When Melle and Range
            if (distance <= enemy.ActualRangeAttack.shootRange && distance > enemy.ActualMelleAttack.attackRange && timer >= enemy.ActualRangeAttack.Delay & enemy.AttackType == EnemyAttackType.MelleAndRange)
            {
                Attack(true);
                timer = 0;

                enemy.GetNextRangeAttack();
            }
            // When Only Range
            if (distance <= enemy.ActualRangeAttack.shootRange && timer >= enemy.ActualRangeAttack.Delay & enemy.AttackType == EnemyAttackType.Range)
            {
                Attack(true);
                timer = 0;

                enemy.GetNextRangeAttack();
            }
            // When Melle
            if (distance <= enemy.ActualMelleAttack.attackRange && timer >= enemy.ActualMelleAttack.Delay & enemy.AttackType != EnemyAttackType.Range)
            {
                Attack(false);
                timer = 0;

                enemy.GetNextMelleAttack();
            }
        }

        void Attack(bool shot)
        {
            // Melle Attack
            if (shot == false)
            {
                enemy.chaseTarget.SendMessage("GetDamage", new DamageClass(enemy.ActualMelleAttack.meleeDamage), SendMessageOptions.DontRequireReceiver);
                enemy.chaseTarget.SendMessage("DamageIndicatorFunction", enemy.transform, SendMessageOptions.DontRequireReceiver);
                enemy.GetComponent<Enemy>().source.PlayOneShot(enemy.GetComponent<Enemy>().MelleAttackSound[Random.Range(0, enemy.GetComponent<Enemy>().MelleAttackSound.Length)]);
            }
            // Range Attack
            else if (shot == true)
            {
                Vector3 SpawnPoint = enemy.vision.position;
                GameObject missile = GameObject.Instantiate(enemy.ActualRangeAttack.missile, SpawnPoint, enemy.vision.rotation);

                // Add Colliders
                missile.GetComponent<Missile>().EnemyColli = enemy.transform.GetComponentsInChildren<Collider>();
                missile.GetComponent<Missile>().Guided = enemy.ActualRangeAttack.AutoAim;
                missile.GetComponent<Missile>().speed = enemy.ActualRangeAttack.missileSpeed;
                missile.GetComponent<Missile>().damage = enemy.ActualRangeAttack.missileDamage;
                missile.GetComponent<Missile>().source = enemy.transform;
                enemy.GetComponent<Enemy>().source.PlayOneShot(enemy.GetComponent<Enemy>().RangeAttackSound[Random.Range(0, enemy.GetComponent<Enemy>().RangeAttackSound.Length)]);
            }
        }

        void Watch()
        {
            if (!enemy.visionScript.EnemySpotted())
            {
                ToAlertState();
            }
        }

        public void OnTriggerEnter(Collider enemy)
        {

        }

        public void ToPatrolState()
        {
            Debug.Log("I shouldn't be able to do this!");
        }

        public void ToAttackState()
        {
            Debug.Log("I shouldn't be able to do this!");
        }

        public void ToAlertState()
        {
            enemy.currentState = enemy.alertState;
        }

        public void ToChaseState()
        {
            enemy.currentState = enemy.chaseState;
        }

    }
}