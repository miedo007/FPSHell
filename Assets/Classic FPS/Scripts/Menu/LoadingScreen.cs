using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

namespace HellishBattle
{
    public class LoadingScreen : MonoBehaviour
    {
        public static LoadingScreen instance;

        public Image ProgressBar;
        public TMP_Text ProgressText;
        public bool LoopBackground = true;
        public List<GameObject> BackgroundList;
        public float backgroundLoopTime;

        public TMP_Text HintText;
        //public StringLocalization Localization;
        public float HintsShowTime = 5;
        public List<LocalizedString> Hints;

        // Loading Scene By Name
        public void loadingScreen(string sceneName)
        {
            this.transform.Find("Panel").gameObject.SetActive(true);
            if (LoopBackground) StartCoroutine(transitionImage());
            StartCoroutine(AutoChangeHint());
            StartCoroutine(Loading(sceneName));
        }

        // Loading Scene By ID
        public void loadingScreen(int sceneNo)
        {
            this.transform.Find("Panel").gameObject.SetActive(true);
            if (LoopBackground) StartCoroutine(transitionImage());
            StartCoroutine(AutoChangeHint());
            StartCoroutine(Loading(sceneNo));
        }

        private void Awake()
        {
            instance = this;
            this.transform.Find("Panel").gameObject.SetActive(false);
            // Hint
            HintText.text = Hints[Random.Range(0, Hints.Count)].GetLocalization();
        }


        // Auto Change Background Image
        IEnumerator transitionImage()
        {
            for (int i = 0; i < BackgroundList.Count; i++)
            {
                yield return new WaitForSeconds(backgroundLoopTime);

                for (int j = 0; j < BackgroundList.Count; j++) BackgroundList[j].SetActive(false);

                BackgroundList[i].SetActive(true);
            }
        }

        // Auto Change Background Image
        IEnumerator AutoChangeHint()
        {

            HintText.text = Hints[Random.Range(0, Hints.Count)].GetLocalization();
            yield return new WaitForSeconds(HintsShowTime);
        }

        // Coroutine Loading Scene By ID
        IEnumerator Loading(int sceneNo)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneNo);
            while (!operation.isDone)
            {
                float progressValue = Mathf.Clamp01(operation.progress / 0.9f);
                ProgressBar.fillAmount = progressValue;
                ProgressText.text = (progressValue * 100) + " %";
                yield return null;
            }
        }

        // Coroutine Loading Scene By Name
        IEnumerator Loading(string sceneName)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            while (!operation.isDone)
            {
                float progressValue = Mathf.Clamp01(operation.progress / 0.9f);
                ProgressBar.fillAmount = progressValue;
                ProgressText.text = (progressValue * 100) + " %";
                yield return null;
            }
        }

    }

#if UNITY_EDITOR

    [CustomEditor(typeof(LoadingScreen))]
    public class LoadingScreenEditor : Editor
    {
        bool HintList = false;
        bool BackgroundList = false;
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            LoadingScreen screen = (LoadingScreen)target;

            EditorGUILayout.BeginHorizontal("helpbox", GUILayout.Height(EditorGUIUtility.singleLineHeight + 6));
            EditorGUILayout.LabelField("Loading Screen Script", EditorStyles.boldLabel);
            GUI.enabled = false;
            EditorGUILayout.ObjectField(MonoScript.FromMonoBehaviour((LoadingScreen)target), typeof(LoadingScreen), false);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Progress Bar Settings", "Assign Progress Bar and Progress Text here");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ProgressBar"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ProgressText"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Hint Settings", "Settings and Hint List");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("HintsShowTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("HintText"));
            //TinyGUI.ShowArray(serializedObject, "Hints", "Hint List", ref HintList);

            #region Hints
            EditorGUILayout.BeginVertical("HelpBox");
            SerializedProperty property = serializedObject.FindProperty("Hints");
            int count = property.arraySize;
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUI.indentLevel++;
            GUIContent foldoutContent = new GUIContent($"Hint List [{count}]");
            HintList = EditorGUILayout.Foldout(HintList, foldoutContent, true);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            if (HintList)
            {
                EditorGUILayout.BeginVertical("HelpBox");
                for (int i = 0; i < count; i++)
                {
                    int index = i;
                    EditorGUILayout.BeginHorizontal();
                    TinyGUI.LocalizedString(screen.Hints[i], property.GetArrayElementAtIndex(i));
                    //EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i), GUIContent.none);
                    if (TinyGUI.IconButton("d_P4_DeletedLocal", "", GUILayout.Width(25), GUILayout.Height(44)))
                    {
                        property.DeleteArrayElementAtIndex(index);
                        serializedObject.ApplyModifiedProperties();
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                if (TinyGUI.IconButton("Toolbar Plus", $"Add New Hint"))
                {
                    property.arraySize++;
                    serializedObject.ApplyModifiedProperties();
                }

            }

            EditorGUILayout.EndVertical(); // End of Main Vertical
            #endregion

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            TinyGUI.EditorTitle("Background Settings", "Settings and Backgrounds List");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("backgroundLoopTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("LoopBackground"));
            TinyGUI.ShowArray(serializedObject, "BackgroundList", "Background List", ref BackgroundList);
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}
