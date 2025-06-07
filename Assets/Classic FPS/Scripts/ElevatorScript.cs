using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using HellishBattle.Player;

namespace HellishBattle
{
    public class ElevatorScript : MonoBehaviour
    {
        public bool Automatic = true;
        public GameObject platform;
        public List<GameObject> Waypoints;
        [Suffix("m/s")] public float speed;
        [Suffix("sec")] public float stayTime = 1;

        // Privates
        int waypointIndex = 0;
        public Vector3 velocity = Vector3.zero;
        float timeLeft;
        bool manualDrive = false;
        public TriggerEventDetector trigger;
        private Vector3 lastPosition;

        private void Start()
        {
            waypointIndex = 0;

            if (platform.GetComponent<TriggerEventDetector>() != null && platform != null)
                trigger = platform.GetComponent<TriggerEventDetector>();
        }

        public void Update()
        {
            if (Automatic)
            {
                // Movement
                if (timeLeft < 0) platform.transform.position = Vector3.SmoothDamp(platform.transform.position, Waypoints[waypointIndex].transform.position, ref velocity, speed * Time.deltaTime - 0.01f, speed);
                // Get Next Target
                timeLeft -= Time.deltaTime;
                if (.001f > Vector3.Distance(platform.transform.position, Waypoints[waypointIndex].transform.position)) { waypointIndex++; if (waypointIndex >= Waypoints.Count) { waypointIndex = 0; } timeLeft = stayTime; }
            }
            if (manualDrive)
            {
                platform.transform.position = Vector3.SmoothDamp(platform.transform.position, Waypoints[waypointIndex].transform.position, ref velocity, speed * Time.deltaTime, speed);
                if (.001f > Vector3.Distance(platform.transform.position, Waypoints[waypointIndex].transform.position)) { if (waypointIndex >= Waypoints.Count) { waypointIndex = 0; } manualDrive = false; }
            }

            if (trigger.status == TriggerStatus.Stay && trigger != null)
            {
                PlayerMovement playerMovement = Camera.main.transform.parent.GetComponent<PlayerMovement>();

                // Przekszta³æ velocity na podstawie rotacji kamery
                //Vector3 velocityWithRotation = Quaternion.Euler(0, -Camera.main.transform.parent.eulerAngles.y, 0) * velocity;
                // Przypisz przekszta³con¹ prêdkoœæ do additionalMovement w komponencie PlayerMovement
                //playerMovement.additionalMovement = velocityWithRotation;
                //playerMovement.additionalMovement = transform.TransformDirection(velocity);

                //this.Log(velocity, transform.TransformDirection(velocity));

                //Vector3 globalVelocity = Quaternion.Euler(0, Camera.main.transform.parent.eulerAngles.y, 0) * velocity;

                //playerMovement.additionalMovement = Camera.main.transform.parent.InverseTransformDirection(platform.transform.position - lastPosition);


                //this.Test("Elevator Move: ", (platform.transform.position - lastPosition));

                //Vector3 localVelocity = Camera.main.transform.parent.InverseTransformDirection(velocity);
                Vector3 localVelocity = Camera.main.transform.parent.InverseTransformDirection(velocity);



                //Vector3 localVelocity = transform.InverseTransformDirection(velocity);
                //float verticalVelocity = -Physics.gravity.y * 2 ;
                ////localVelocity.y -= playerMovement.verticalVelocity;

                // Last 
                //playerMovement.additionalMovement = (localVelocity);
                //playerMovement.additionalMovement = (localVelocity * Time.deltaTime);

                //Vector3 transformedVelocity = Quaternion.Inverse(Camera.main.transform.parent.transform.rotation) * velocity;
                //Camera.main.transform.parent.GetComponent<CharacterController>().Move(transformedVelocity * Time.deltaTime);

                // Pobierz obrót gracza wokó³ osi Y
                float playerYRotation = Camera.main.transform.parent.transform.eulerAngles.y;

                // Utwórz rotacjê tylko w osi Y
                Quaternion rotationY = Quaternion.Euler(0, playerYRotation, 0);

                // Przekszta³cenie velocity windy zgodnie z rotacj¹ gracza wokó³ osi Y
                Vector3 transformedVelocity = velocity;

                // Przesuñ gracza
                Camera.main.transform.parent.GetComponent<CharacterController>().Move(transformedVelocity * Time.deltaTime);
                //Camera.main.transform.parent.GetComponent<CharacterController>().Move(transformedVelocity * Time.deltaTime);
            }
            lastPosition = platform.transform.position;
        }

        public void OnEnter(Collider other)
        {
            other.transform.parent = platform.transform;
        }
        public void OnExit(Collider other)
        {
            other.transform.parent = null;
        }


        public void ManualStart()
        {
            waypointIndex++;
            if (waypointIndex >= Waypoints.Count) { waypointIndex = 0; }
            manualDrive = true;
        }

        private void OnDrawGizmosSelected()
        {
            for (int i = 0; i < Waypoints.Count; i++)
            {
                int next = i + 1;
                if (next == Waypoints.Count) next = 0;
                Debug.DrawLine(Waypoints[i].transform.position, Waypoints[next].transform.position, Color.green);
            }
        }
    }

    /// Custom Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(ElevatorScript))]
    public class ElevatorScriptEditor : Editor
    {
        bool Waypoints;
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            ElevatorScript elevator = (ElevatorScript)target;
            EditorGUI.indentLevel = 0;

            TinyGUI.DrawScript<ElevatorScript>("Elevator Script", target);

            /*EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Elevator Settings", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Automatic", GUILayout.Width(75));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Automatic"), GUIContent.none, GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();*/

            TinyGUI.BeginBoxGroup("Base Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Automatic"), new GUIContent("Automatic Move"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("platform"), new GUIContent("Platform Object"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("speed"), new GUIContent("Speed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stayTime"), new GUIContent("Stay Time"));
            TinyGUI.EndBoxGroup();

            TinyGUI.ShowArray(serializedObject, "Waypoints", "Waypoints", ref Waypoints);
            //EditorGUILayout.EndVertical();



            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}