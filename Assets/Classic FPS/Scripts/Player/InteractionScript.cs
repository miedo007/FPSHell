using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using TMPro;

namespace HellishBattle.Interaction
{
    public class InteractionScript : MonoBehaviour
    {
        public bool Interactable = true;
        public LocalizedString Name;
        public LocalizedString Description;
        public float InteractionTime = 1.0f;

        public InteractionItemType ItemType;
        public bool ShowPossibilityOfInteracion = true;
        public bool MultipleUses = false;

        public GameObject InteractionUIPrefab;
        public Vector3 UIPrefabRotation;
        public Vector3 UIPrefabPosition;

        public UnityEvent WhenInteraction;

        public GameObject UICanvas;
        int _state;

        bool _pressed;
        float _timer = 0;

        private void Awake()
        {
            UICanvas = Instantiate(InteractionUIPrefab, this.transform);

            UICanvas.transform.localPosition = UIPrefabPosition;
            UICanvas.transform.Rotate(UIPrefabRotation.x - transform.rotation.x, UIPrefabRotation.y - transform.rotation.y, UIPrefabRotation.z - transform.rotation.z, Space.World);

            UICanvas.transform.Find("Name_Txt").GetComponent<TMP_Text>().text = Name.GetLocalization();
            UICanvas.transform.Find("Description_Txt").GetComponent<TMP_Text>().text = Description.GetLocalization();

            UICanvas.AddComponent<FaceCamera>();
        }

#if MeetAndTalk
    public void StartDialogue(MeetAndTalk.DialogueContainerSO dialog)
    {
        this.Test("Start Dialog");
            MeetAndTalk.DialogueManager.Instance.StartDialogue(dialog);
    }
#endif

        public void Interaction()
        {
            if (Interactable)
            {
                if (transform.TryGetComponent<DoorScript>(out DoorScript door)) { _pressed = door.CheckKey(); }
                else { _pressed = true; }
            }
        }
        public void StopInteraction()
        {
            _pressed = false;
            _timer = 0;
        }

        public void OnDetection()
        {
            _state = 1;
        }

        public void OnSelected()
        {
            _state = 2;
        }

        public void Update()
        {
            if (_pressed) { _timer += Time.deltaTime; }
            if (_timer >= InteractionTime && _pressed)
            {
                WhenInteraction.Invoke();
                if (!MultipleUses) { Interactable = false; }
                _timer = 0;
                _pressed = false;
            }

            // Rotate
            Vector3 cameraDirection;
            cameraDirection = Camera.main.transform.forward;
            cameraDirection.y = 0;
            UICanvas.transform.rotation = Quaternion.LookRotation(cameraDirection);

            // UI
            if (_state == 1 && Interactable && ShowPossibilityOfInteracion) // Detection
            {
                UICanvas.gameObject.SetActive(true);
                UICanvas.transform.Find("Name_Txt").GetComponent<TMP_Text>().text = "";
                UICanvas.transform.Find("Description_Txt").GetComponent<TMP_Text>().text = "";
                UICanvas.transform.Find("Background_Normal").GetComponent<Transform>().gameObject.SetActive(false);
                UICanvas.transform.Find("Background_Key").GetComponent<Transform>().gameObject.SetActive(false);
                UICanvas.transform.Find("Fill").GetComponent<Image>().fillAmount = 0;
            }
            else if (_state == 2 && Interactable && ShowPossibilityOfInteracion) // Selected
            {
                UICanvas.gameObject.SetActive(true);
                UICanvas.transform.Find("Name_Txt").GetComponent<TMP_Text>().text = Name.GetLocalization();
                UICanvas.transform.Find("Description_Txt").GetComponent<TMP_Text>().text = Description.GetLocalization();
                if (ItemType == InteractionItemType.Normal) UICanvas.transform.Find("Background_Normal").GetComponent<Transform>().gameObject.SetActive(true);
                if (ItemType == InteractionItemType.KeyItem) UICanvas.transform.Find("Background_Key").GetComponent<Transform>().gameObject.SetActive(true);
                if (_pressed) { UICanvas.transform.Find("Fill").GetComponent<Image>().fillAmount = (float)_timer / InteractionTime; }
                else { UICanvas.transform.Find("Fill").GetComponent<Image>().fillAmount = 0; }
            }
            else
            {
                UICanvas.gameObject.SetActive(false);
                UICanvas.transform.Find("Name_Txt").GetComponent<TMP_Text>().text = "";
                UICanvas.transform.Find("Description_Txt").GetComponent<TMP_Text>().text = "";
                UICanvas.transform.Find("Background_Normal").GetComponent<Transform>().gameObject.SetActive(false);
                UICanvas.transform.Find("Background_Key").GetComponent<Transform>().gameObject.SetActive(false);
                UICanvas.transform.Find("Fill").GetComponent<Image>().fillAmount = 0;
            }
            _state = 0;
        }

        public void SetupInteraction(bool Action) { Interactable = Action; }
        public void DisableInteractable()
        {
            Interactable = false;
        }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position + (transform.rotation * UIPrefabPosition), .05f);
            Handles.Label(transform.position + (transform.rotation * (UIPrefabPosition + new Vector3(0f, 0.1f, 0f))), "UI Show Here");
        }
#endif
    }
    /// Custom Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(InteractionScript))]
    [CanEditMultipleObjects]
    public class InteractionScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            EditorGUI.indentLevel = 0;

            InteractionScript interaction = (InteractionScript)target;
            TinyGUI.DrawScript<InteractionScript>("Interaction Script", target);

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginVertical("toolbar", GUILayout.Height(EditorGUIUtility.singleLineHeight));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Base Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Interactable"), new GUIContent("Interactable"));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(-2);
            EditorGUILayout.BeginVertical("toolbar", GUILayout.Height(EditorGUIUtility.singleLineHeight));
            EditorGUILayout.LabelField("Change Basic Interaction Settings", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("MultipleUses"), new GUIContent("Multiple Use"));
            interaction.InteractionTime = EditorGUILayout.Slider(new GUIContent("Interaction Time"), interaction.InteractionTime, 0, 10);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("UI Settings");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("ShowPossibilityOfInteracion"), new GUIContent("Show UI"));
            if (interaction.ShowPossibilityOfInteracion)
            {
                interaction.ItemType = (InteractionItemType)EditorGUILayout.EnumPopup(new GUIContent("Item Type"), interaction.ItemType);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("InteractionUIPrefab"), new GUIContent("UI Prefab"));
                /*Add Button Here*/
                if (TinyGUI.IconButton("Selectable Icon", "  Select UI", GUILayout.Width(100), GUILayout.Height(20)))
                {
                    string path = "Assets/Classic FPS/Prefabs/UI/Interaction_UI_01.prefab";
                    GameObject interactionUIPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    if (interactionUIPrefab != null)
                    {
                        Selection.activeObject = interactionUIPrefab;
                    }
                    else
                    {
                        Debug.LogError("Interaction_UI prefab not found at path: " + path);
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("UIPrefabRotation"), new GUIContent("UI Rotation"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("UIPrefabPosition"), new GUIContent("UI Poistion"));

                EditorGUILayout.Space(5);
                TinyGUI.EditorTitle("Localized String");
                TinyGUI.LocalizedString(interaction.Name, serializedObject.FindProperty("Name"));
                TinyGUI.LocalizedString(interaction.Description, serializedObject.FindProperty("Description"));
            }

            EditorGUILayout.EndVertical();




            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Interaction Unity Events", "Here assign the actions to be performed after the interaction");
            EditorGUILayout.EndVertical();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("WhenInteraction"), new GUIContent("On Interaction"));


            serializedObject.ApplyModifiedProperties();
        }
    }

#endif


    [System.Serializable]
    public enum InteractionItemType
    {
        Normal = 0, KeyItem = 1
    }
}
