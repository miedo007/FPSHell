using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HellishBattle.Interaction
{
    public class PlayerInteraction : MonoBehaviour
    {
        public float InteractionRange = 2;
        public float DetectionRange = 3;

        InteractionScript interactionObject;
        private TinyInput tinyInput;

        private void Awake()
        {
            tinyInput = InputManager.Instance.input;
            tinyInput.Player.Interaction.started += ctx => { if (interactionObject) { interactionObject.Interaction(); } };
            tinyInput.Player.Interaction.canceled += ctx => { if (interactionObject) { interactionObject.StopInteraction(); } };
        }
        public void OnEnable() { tinyInput.Enable(); }
        public void OnDisable() { tinyInput.Disable(); }

        public void Update()
        {
            interactionObject = null;
            Collider[] colliderArray = Physics.OverlapSphere(Camera.main.transform.position, DetectionRange);
            foreach (Collider collider in colliderArray)
            {
                if (collider.TryGetComponent<InteractionScript>(out InteractionScript interaction))
                {
                    interaction.OnDetection();
                }
            }
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward) * InteractionRange, out hit, InteractionRange))
            {
                if (hit.transform.TryGetComponent<InteractionScript>(out InteractionScript interaction))
                {
                    interaction.OnSelected();
                    interactionObject = interaction;
                }
            }
        }

        // Gizmos
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            //Gizmos.color = new Color(1, 0.9f, 0.015f, 0.1f);
            //Gizmos.DrawSphere(Camera.main.transform.position, DetectionRange);
            //Gizmos.DrawWireSphere(Camera.main.transform.position, DetectionRange);

            // Interaction Line
            Gizmos.color = Color.green;
            Gizmos.DrawRay(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward) * InteractionRange);
            // Interaction Text
            UnityEditor.Handles.color = Color.green;
            UnityEditor.Handles.Label(Camera.main.transform.position + Camera.main.transform.TransformDirection(Vector3.forward) * InteractionRange, $"Interaction Radius: {InteractionRange}m");

            // Interaction Line
            Gizmos.color = Color.blue;
            for (int i = 0; i < 36; i++)
            {
                float angle = i * 10;
                float x = Camera.main.transform.position.x + Mathf.Sin(Mathf.Deg2Rad * angle) * DetectionRange;
                float y = Camera.main.transform.position.y;
                float z = Camera.main.transform.position.z + Mathf.Cos(Mathf.Deg2Rad * angle) * DetectionRange;

                Vector3 startPoint = new Vector3(x, y, z);

                // Calculate the next angle
                float nextAngle = (i + 1) * 10;
                float nextX = Camera.main.transform.position.x + Mathf.Sin(Mathf.Deg2Rad * nextAngle) * DetectionRange;
                float nextZ = Camera.main.transform.position.z + Mathf.Cos(Mathf.Deg2Rad * nextAngle) * DetectionRange;

                Vector3 endPoint = new Vector3(nextX, y, nextZ);

                // Draw the gizmo line
                Gizmos.DrawLine(startPoint, endPoint);
            }
            Gizmos.DrawRay(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.right) * DetectionRange);

            // Interaction Text
            UnityEditor.Handles.color = Color.blue;
            UnityEditor.Handles.Label(Camera.main.transform.position + Camera.main.transform.TransformDirection(Vector3.right) * DetectionRange, $"Detection Radius: {DetectionRange}m");
        }
#endif
    }
}
