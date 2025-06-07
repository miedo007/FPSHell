using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HellishBattle.Level
{
    public class AutoTurretScript : MonoBehaviour
    {
        public float ViewRange = 7;
        public float AttackRange = 3;

        public GameObject BaseTurret;
        public float RotationSpeed = 4;

        bool viewPlayer = false;

        private void Update()
        {
            if (viewPlayer)
            {
                // See Player
            }
            else
            {
                float rotationAmount = RotationSpeed * Time.deltaTime;
                BaseTurret.transform.Rotate(0f, rotationAmount, 0f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            int numPoints = 36;
            float angleIncrement = 360f / numPoints;

            Gizmos.color = Color.blue;

            // Draw Max Radius
            for (int i = 0; i < numPoints; i++)
            {
                float angle = i * angleIncrement;
                float x = transform.position.x + Mathf.Sin(Mathf.Deg2Rad * angle) * ViewRange;
                float y = transform.position.y;
                float z = transform.position.z + Mathf.Cos(Mathf.Deg2Rad * angle) * ViewRange;

                Vector3 startPoint = new Vector3(x, y, z);

                // Calculate the next angle
                float nextAngle = (i + 1) * angleIncrement;
                float nextX = transform.position.x + Mathf.Sin(Mathf.Deg2Rad * nextAngle) * ViewRange;
                float nextZ = transform.position.z + Mathf.Cos(Mathf.Deg2Rad * nextAngle) * ViewRange;

                Vector3 endPoint = new Vector3(nextX, y, nextZ);

                // Draw the gizmo line
                Gizmos.DrawLine(startPoint, endPoint);
            }


            Gizmos.color = Color.red;

            // Draw Max Radius
            for (int i = 0; i < numPoints; i++)
            {
                float angle = i * angleIncrement;
                float x = transform.position.x + Mathf.Sin(Mathf.Deg2Rad * angle) * AttackRange;
                float y = transform.position.y;
                float z = transform.position.z + Mathf.Cos(Mathf.Deg2Rad * angle) * AttackRange;

                Vector3 startPoint = new Vector3(x, y, z);

                // Calculate the next angle
                float nextAngle = (i + 1) * angleIncrement;
                float nextX = transform.position.x + Mathf.Sin(Mathf.Deg2Rad * nextAngle) * AttackRange;
                float nextZ = transform.position.z + Mathf.Cos(Mathf.Deg2Rad * nextAngle) * AttackRange;

                Vector3 endPoint = new Vector3(nextX, y, nextZ);

                // Draw the gizmo line
                Gizmos.DrawLine(startPoint, endPoint);
            }

        }
    }
}