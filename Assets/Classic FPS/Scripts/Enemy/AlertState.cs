using HellishBattle.Enemies;
using UnityEngine;

namespace HellishBattle.AI
{

    public class AlertState : IEnemyAI
    {

        EnemyStates enemy;
        float timer = 0;

        public AlertState(EnemyStates enemy)
        {
            this.enemy = enemy;
        }

        public void UpdateActions()
        {
            Search();
            Watch();
            // Only look around when you have reached a known place
            if (enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance)
                LookAround();
        }
        // Function for 'seeing' opponents
        // When the opponent notices the hero, it sets the placement as the new target
        // go to the 'chasing' state
        void Watch()
        {
            if (enemy.visionScript.EnemySpotted())
            {
                enemy.navMeshAgent.destination = enemy.lastKnownPosition;
                ToChaseState();
            }
        }
        // Function responsible for 'watching' the opponent
        // When the opponent reaches the opponent's last known location
        // Spends X time there, then goes back to patrolling
        void LookAround()
        {
            timer += Time.deltaTime;
            if (timer >= enemy.stayAlertTime)
            {
                timer = 0;
                ToPatrolState();
            }
        }
        // This function sets the hero's last known location as target
        void Search()
        {
            enemy.navMeshAgent.destination = enemy.lastKnownPosition;
            enemy.navMeshAgent.isStopped = false;
        }

        public void OnTriggerEnter(Collider enemy)
        {

        }

        public void ToPatrolState()
        {
            enemy.currentState = enemy.patrolState;
        }

        public void ToAttackState()
        {
            Debug.Log("I shouldn't be able to do this!");
        }

        public void ToAlertState()
        {
            Debug.Log("I'm already in alert mode!");
        }

        public void ToChaseState()
        {
            enemy.currentState = enemy.chaseState;
        }
    }
}