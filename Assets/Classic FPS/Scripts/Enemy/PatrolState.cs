using HellishBattle.Enemies;
using UnityEngine;
using UnityEngine.AI;

namespace HellishBattle.AI
{
    public class PatrolState : IEnemyAI
    {

        EnemyStates enemy;
        int nextWayPoint = 0;
        public Vector3 nextWayPointPosition;

        public NavMeshAgent spawnPosition;
        public NavMeshPath navMeshPath;

        public PatrolState(EnemyStates enemy)
        {
            this.enemy = enemy;
            navMeshPath = new NavMeshPath();
        }

        public void UpdateActions()
        {
            Watch();
            Patrol();
        }
        // Function responsible for 'seeing' the opponent
        // When the opponent notices the hero, he goes into a chase state
        void Watch()
        {
            if (enemy.visionScript.EnemySpotted())
            {
                //Debug.Log("I See a Enemy");
                ToChaseState();
            }
        }
        // Function responsible for patrolling along designated points
        void Patrol()
        {
            enemy.navMeshAgent.destination = nextWayPointPosition;  //enemy.waypoints[nextWayPoint].position;
            enemy.navMeshAgent.isStopped = false;

            if (enemy.navMeshAgent.remainingDistance <= enemy.navMeshAgent.stoppingDistance && !enemy.navMeshAgent.pathPending)
            {
                if (enemy.OrderWaypoints == TargetSelectType.Numerically)
                {
                    nextWayPointPosition = enemy.waypoints[(nextWayPoint + 1) % enemy.waypoints.Length].position;
                }
                else if (enemy.OrderWaypoints == TargetSelectType.Random)
                {
                    nextWayPointPosition = enemy.waypoints[(Random.Range(0, enemy.waypoints.Length)) % enemy.waypoints.Length].position;
                }
                else if (enemy.OrderWaypoints == TargetSelectType.ProcedurallyWaypoints)
                {
                    nextWayPointPosition = RandomNavmeshLocation(enemy.ProceduralWaypointRadius);
                }
            }
            if (enemy.OrderWaypoints == TargetSelectType.PlayerTarget)
            {

                nextWayPointPosition = GameManager.Instance.transform.parent.position;
            }

            enemy.visionScript.PatrolVoid();
        }


        public void OnTriggerEnter(Collider enemy)
        {
            if (enemy.gameObject.CompareTag("Player"))
            {
                ToAlertState();
            }
        }

        public void ToPatrolState()
        {
            Debug.Log("I'm already patrolling!");
        }

        public void ToAttackState()
        {
            enemy.currentState = enemy.attackState;
        }

        public void ToAlertState()
        {
            enemy.currentState = enemy.alertState;
        }

        public void ToChaseState()
        {
            enemy.currentState = enemy.chaseState;
        }

        public Vector3 RandomNavmeshLocation(float radius)
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += enemy.transform.position;

            NavMeshHit hit;
            Vector3 finalPosition = Vector3.zero;


            if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
            {
                finalPosition = hit.position;
            }
            return finalPosition;
        }

    }
}