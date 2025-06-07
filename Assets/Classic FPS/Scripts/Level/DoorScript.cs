using HellishBattle.Level;
using UnityEditor;
using UnityEngine;

namespace HellishBattle.Interaction
{
    public class DoorScript : MonoBehaviour
    {
        public DoorType type;
        public DoorTrigger trigger;
        public KeyType keyType;
        public float speed;
        public Vector3 endPosition;
        public Vector3 endRotation;

        public Material Base;
        public Material Red, Blue, Yellow, Green;

        // Privates
        Vector3 startPosition;
        Quaternion startRotation;
        GameObject doors;
        [HideInInspector] public bool isOpen = false;
        [HideInInspector] public bool isMoving = false;
        Animator anim;

        private Vector3 velocity = Vector3.zero;

        private void Awake()
        {
            doors = this.transform.Find("Door").gameObject;
            startPosition = doors.transform.localPosition;
            startRotation = doors.transform.localRotation;
            anim = doors.GetComponent<Animator>();
            doors.GetComponent<MeshRenderer>().material = GetMaterial();

            if (trigger != DoorTrigger.Interaction)
            {
                if (this.transform.TryGetComponent<InteractionScript>(out InteractionScript interaction))
                {
                    interaction.Interactable = false;
                }
            }
        }

        private void Update()
        {
            if (type == DoorType.Slide)
            {
                if (isOpen) { doors.transform.localPosition = Vector3.SmoothDamp(doors.transform.localPosition, (startPosition + endPosition), ref velocity, speed / Time.deltaTime); }
                else { doors.transform.localPosition = Vector3.SmoothDamp(doors.transform.localPosition, startPosition, ref velocity, speed / Time.deltaTime); }
            }
            else if (type == DoorType.Rotation)
            {
                if (isOpen) { doors.transform.localRotation = Quaternion.Slerp(doors.transform.localRotation, startRotation * Quaternion.Euler(endRotation.x, endRotation.y, endRotation.z), speed / Time.deltaTime); }
                else { doors.transform.localRotation = Quaternion.Slerp(doors.transform.localRotation, (startRotation), speed / Time.deltaTime); }
            }
        }

        public void Interaction()
        {
            if (trigger == DoorTrigger.Interaction && CheckKey())
            {
                isOpen = !isOpen; velocity = Vector3.zero;
            }
        }

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying) startRotation = transform.Find("Door").transform.localRotation;
#endif

            Gizmos.color = Color.cyan;
            if (type == DoorType.Slide) Gizmos.DrawWireMesh(transform.Find("Door").GetComponent<MeshCollider>().sharedMesh, transform.Find("Door").position + (transform.Find("Door").rotation * endPosition), transform.Find("Door").rotation, transform.Find("Door").localScale);
            //if (type == DoorType.Rotation) Gizmos.DrawWireMesh(transform.Find("Door").GetComponent<MeshCollider>().sharedMesh, transform.Find("Door").position, (startRotation * Quaternion.Euler(endRotation.x+transform.rotation.x, 180, endRotation.z + transform.rotation.z)), transform.Find("Door").localScale);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && trigger == DoorTrigger.Automatic && CheckKey())
            {
                isOpen = true; velocity = Vector3.zero;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && trigger == DoorTrigger.Automatic && CheckKey())
            {
                isOpen = false; velocity = Vector3.zero;
            }
        }

        public bool CheckKey()
        {
            switch (keyType)
            {
                case KeyType.RedKey:
                    if (LevelManager.Instance.redKeys == true) { return true; }
                    break;
                case KeyType.BlueKey:
                    if (LevelManager.Instance.blueKeys == true) { return true; }
                    break;
                case KeyType.YellowKey:
                    if (LevelManager.Instance.yellowKeys == true) { return true; }
                    break;
                case KeyType.GreenKey:
                    if (LevelManager.Instance.greenKeys == true) { return true; }
                    break;
                case KeyType.None:
                    return true;
                default:
                    return false;

            }

            return false;
        }

        Material GetMaterial()
        {
            switch (keyType)
            {
                case KeyType.RedKey:
                    return Red;
                case KeyType.BlueKey:
                    return Blue;
                case KeyType.YellowKey:
                    return Yellow;
                case KeyType.GreenKey:
                    return Green;
                default:
                    return Base;
            }
        }

        public void MoveDoor()
        {
            isOpen = !isOpen; velocity = Vector3.zero;
        }
        public void CloseDoor()
        {
            isOpen = false; velocity = Vector3.zero;
        }
        public void OpenDoor()
        {
            isOpen = true; velocity = Vector3.zero;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"> None = -1, Interaction = 0, Automatic = 1</param>
        public void ChangeTrigger(int type)
        {
            trigger = (DoorTrigger)type;
        }
    }


    /// Custom Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(DoorScript))]
    [CanEditMultipleObjects]
    public class DoorScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            DoorScript door = (DoorScript)target;
            EditorGUI.indentLevel = 0;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Door Script", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            door.type = (DoorType)EditorGUILayout.EnumPopup(new GUIContent("Door Type"), door.type);
            door.trigger = (DoorTrigger)EditorGUILayout.EnumPopup(new GUIContent("Door Trigger"), door.trigger);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("speed"), new GUIContent("Speed"));
            if (door.type == DoorType.Slide) EditorGUILayout.PropertyField(serializedObject.FindProperty("endPosition"), new GUIContent("End Position"));
            if (door.type == DoorType.Rotation) EditorGUILayout.PropertyField(serializedObject.FindProperty("endRotation"), new GUIContent("End Rotation"));

            //EditorGUILayout.BeginVertical("box");
            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Key Settings", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);


            door.keyType = (KeyType)EditorGUILayout.EnumPopup(new GUIContent("Key Type"), door.keyType);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Base"), new GUIContent("Base Material"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Red"), new GUIContent("Red Key Material"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Blue"), new GUIContent("Blue Key Material"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Yellow"), new GUIContent("Yellow Key Material"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Green"), new GUIContent("Green Key Material"));

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
                EditorGUILayout.LabelField($"Debug Value", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(4);

                EditorGUILayout.LabelField(new GUIContent($"Door is Open: {door.isOpen}"));
                EditorGUILayout.LabelField(new GUIContent($"Door is Moving: {door.isOpen}"));
                EditorGUILayout.LabelField(new GUIContent($"Door Locked by Key: {!door.CheckKey()}"));
            }

            EditorGUILayout.EndVertical();


            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

    public enum DoorType
    {
        Slide = 0, Rotation = 1
    }
    public enum DoorTrigger
    {
        None = -1, Interaction = 0, Automatic = 1
    }
}
