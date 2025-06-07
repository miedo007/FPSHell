using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace HellishBattle.Enemies
{
    public class Vision : MonoBehaviour
    {
        [Line("LayerMask Settings")]
        public LayerMask playerMask;
        public LayerMask occlusionMask;

        //[Line(VirtualColor.Gray, "These settings are overwritten automatically"," when the script is loaded and they are automatically"," downloaded until a solution is found in the editor.")]
        [Line("LayerMask Settings")]
        public EnemyStates enemy;
        public NavMeshAgent navMeshAgent;

        Vector3 destination;

        // Field on View
        List<GameObject> TargetObjectList = new List<GameObject>();
        Collider[] CollidersList = new Collider[50];
        int objectCount;
        Mesh visionMesh;
        float patrolHeight = 1.8f;


        private void Awake()
        {

        }

        void Update()
        {
            if (transform.parent.GetComponent<EnemyStates>().navMeshAgent.enabled)
            {
                destination = transform.parent.GetComponent<UnityEngine.AI.NavMeshAgent>().destination;
                transform.LookAt(destination);
                transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
            }
        }

        Mesh CreateWedgeMesh(float angle, float range)
        {
            Mesh mesh = new Mesh();

            int segments = 16;
            int numTriangles = (segments * 4) + 2 + 2;
            int numVertices = numTriangles * 3;

            Vector3[] verticles = new Vector3[numVertices];
            int[] triangles = new int[numVertices];

            Vector3 bottomCenter = -transform.localPosition;
            Vector3 bottomLeft = -transform.localPosition + Quaternion.Euler(0, -angle, 0) * Vector3.forward * range;
            Vector3 bottomRight = -transform.localPosition + Quaternion.Euler(0, angle, 0) * Vector3.forward * range;

            Vector3 topCenter = bottomCenter + Vector3.up * patrolHeight;
            Vector3 topLeft = bottomLeft + Vector3.up * patrolHeight;
            Vector3 topRight = bottomRight + Vector3.up * patrolHeight;

            int vert = 0;

            // left side
            verticles[vert++] = bottomCenter;
            verticles[vert++] = bottomLeft;
            verticles[vert++] = topLeft;

            verticles[vert++] = topLeft;
            verticles[vert++] = topCenter;
            verticles[vert++] = bottomCenter;

            // right side
            verticles[vert++] = bottomCenter;
            verticles[vert++] = topCenter;
            verticles[vert++] = topRight;

            verticles[vert++] = topRight;
            verticles[vert++] = bottomRight;
            verticles[vert++] = bottomCenter;

            float currentAngle = -angle;
            float deltaAngle = (angle * 2) / segments;

            for (int i = 0; i < segments; i++)
            {
                bottomLeft = -transform.localPosition + Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * range;
                bottomRight = -transform.localPosition + Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * range;

                topLeft = bottomLeft + Vector3.up * patrolHeight;
                topRight = bottomRight + Vector3.up * patrolHeight;

                // far side
                verticles[vert++] = bottomLeft;
                verticles[vert++] = bottomRight;
                verticles[vert++] = topRight;

                verticles[vert++] = topRight;
                verticles[vert++] = topLeft;
                verticles[vert++] = bottomLeft;

                // top
                verticles[vert++] = topCenter;
                verticles[vert++] = topLeft;
                verticles[vert++] = topRight;

                // bottom
                verticles[vert++] = bottomCenter;
                verticles[vert++] = bottomRight;
                verticles[vert++] = bottomLeft;

                currentAngle += deltaAngle;
            }

            for (int i = 0; i < numVertices; i++)
            {
                triangles[i] = i;
            }

            mesh.vertices = verticles;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }

        private void OnDrawGizmosSelected()
        {
            if (visionMesh)
            {
                Gizmos.color = new Color(1, .75f, 0, .2f);
                Gizmos.DrawMesh(visionMesh, transform.position, transform.rotation);

                Gizmos.DrawWireSphere(transform.position, enemy.patrolRange);
            }

            Gizmos.color = new Color(1, 0, 0, .2f);
            for (int i = 0; i < objectCount; i++)
            {
                Gizmos.DrawSphere(CollidersList[i].transform.position, 0.2f);
            }

            Gizmos.color = new Color(0, 1, 0, .2f);
            foreach (var obj in TargetObjectList)
            {
                Gizmos.DrawSphere(obj.transform.position, 0.2f);
            }
        }

        private void OnValidate()
        {
            enemy = transform.parent.GetComponent<EnemyStates>();
            navMeshAgent = enemy.GetComponent<NavMeshAgent>();
            visionMesh = CreateWedgeMesh(enemy.viewAngle / 2, enemy.patrolRange);
        }

        public bool IsInSight(GameObject obj, float angle)
        {
            Vector3 origin = enemy.transform.position;
            Vector3 dest = obj.transform.position;
            Vector3 direction = dest - origin;
            if (direction.y < 0 || direction.y > patrolHeight)
            {
                return false;
            }

            direction.y = 0;
            float deltaAngle = Vector3.Angle(direction, transform.forward);
            if (deltaAngle > angle)
            {
                return false;
            }

            //origin.y += 
            dest.y = origin.y;
            if (Physics.Linecast(origin, dest, occlusionMask))
            {
                return false;
            }

            return true;
        }

        public void PatrolVoid()
        {
            // Object in Range
            objectCount = Physics.OverlapSphereNonAlloc(transform.position, enemy.patrolRange, CollidersList, playerMask, QueryTriggerInteraction.Collide);
            TargetObjectList.Clear();

            for (int i = 0; i < objectCount; i++)
            {
                GameObject obj = CollidersList[i].gameObject;
                if (IsInSight(obj, enemy.viewAngle / 2))
                {
                    TargetObjectList.Add(obj);
                }
            }
        }

        public bool EnemySpotted()
        {
            if (TargetObjectList.Count > 0)
            {
                enemy.chaseTarget = TargetObjectList[0].transform;
                return true;
            }
            else { return false; }
        }
    }
}