using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.Events;

namespace HellishBattle.Enemies
{
    public class BossScript : MonoBehaviour
    {
        public LocalizedString BossLocalization;
        public Enemy Boss;

        public UnityEvent OnEnter;
        public UnityEvent OnExit;

        [HideInInspector] public Slider bossBar;

        private void Start()
        {
            GameManager.Instance.transform.parent.Find("Canvas/UI/Boss_UI").gameObject.SetActive(false);
            bossBar = GameManager.Instance.transform.parent.Find("Canvas/UI/Boss_UI/Boss_Healthbar").GetComponent<Slider>();
            bossBar.maxValue = Boss.maxHealth;
        }

        private void Update()
        {
            bossBar.value = Boss.health;
            if (Boss.health <= 0) { GameManager.Instance.transform.parent.Find("Canvas/UI/Boss_UI").gameObject.SetActive(false); }

        }

        // SHow Hide UI
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.transform.parent.Find("Canvas/UI/Boss_UI").gameObject.SetActive(false);
                bossBar.transform.Find("Boss_Name_Txt").GetComponent<TMPro.TMP_Text>().text = BossLocalization.GetLocalization();
                OnExit.Invoke();
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (Boss.health > 0) GameManager.Instance.transform.parent.Find("Canvas/UI/Boss_UI").gameObject.SetActive(true);
                else { GameManager.Instance.transform.parent.Find("Canvas/UI/Boss_UI").gameObject.SetActive(false); }
                bossBar.transform.Find("Boss_Name_Txt").GetComponent<TMPro.TMP_Text>().text = BossLocalization.GetLocalization();
                OnEnter.Invoke();
            }
        }
        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (Boss.health > 0) GameManager.Instance.transform.parent.Find("Canvas/UI/Boss_UI").gameObject.SetActive(true);
                else { GameManager.Instance.transform.parent.Find("Canvas/UI/Boss_UI").gameObject.SetActive(false); }
            }
        }

        public void DestroyObject()
        {
            GameManager.Instance.transform.parent.Find("Canvas/UI/Boss_UI").gameObject.SetActive(false);
            Destroy(transform.gameObject);
        }
    }

    /// Custom Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(BossScript))]
    public class BossScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            BossScript boss = (BossScript)target;
            EditorGUI.indentLevel = 0;

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Boss", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Boss"), new GUIContent("Enemy Script"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("BossLocalization"), new GUIContent("Boss Localization"));

            EditorGUILayout.EndVertical();


            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnEnter"), new GUIContent("On Enter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnExit"), new GUIContent("On Exit"));


            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}