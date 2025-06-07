using HellishBattle.Enemies;
using UnityEngine;

namespace HellishBattle.AI
{

    public class ChaseState : IEnemyAI
    {

        EnemyStates enemy;

        public ChaseState(EnemyStates enemy)
        {
            this.enemy = enemy;
        }

        public void UpdateActions()
        {
            Watch();
            Chase();
        }
        // Function responsible for 'seeing' the opponent
        // When the opponent notices the player, we set him as our target of the race
        // When the opponent loses sight of the player, we go into the alert state
        void Watch()
        {
            if (!enemy.visionScript.EnemySpotted())
            {
                ToAlertState();
            }
        }
        // Function responsible for chasing the opponent
        // If the opponent is close enough, it goes into attacking state
        void Chase()
        {
            enemy.navMeshAgent.destination = enemy.chaseTarget.position;
            enemy.navMeshAgent.isStopped = false;


            if (enemy.navMeshAgent.remainingDistance <= enemy.ActualMelleAttack.attackRange && ((int)enemy.AttackType == 0 || (int)enemy.AttackType == 2))
            {
                enemy.navMeshAgent.isStopped = true;
                ToAttackState();
            }
            else if (enemy.navMeshAgent.remainingDistance <= enemy.ActualRangeAttack.shootRange && ((int)enemy.AttackType == 1 || (int)enemy.AttackType == 2))
            {
                enemy.navMeshAgent.isStopped = true;
                ToAttackState();
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
            // Debug.Log( "I'm starting to attack the player!");
            enemy.currentState = enemy.attackState;
        }

        public void ToAlertState()
        {
            // Debug.Log("I've lost sight of the player!");
            enemy.currentState = enemy.alertState;
        }

        public void ToChaseState()
        {
            // Debug.Log("He's chasing him already!");
        }

    }
}