using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

namespace HellishBattle.Audio
{
    public class AudioManager : MonoBehaviour
    {

        private static AudioManager _instance;
        public static AudioManager Instance
        {
            get { return _instance; }
        }

        public AudioClip[] clips;
        public AudioSource Source;
        int Music_ID = -1;

        private void Awake()
        {
            // Set Static Object
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            StartCoroutine("PlayMusic");
        }
        IEnumerator PlayMusic()
        {
            Music_ID = Random.Range(0, clips.Length);

            Source.clip = clips[Music_ID];
            Source.Play();

            yield return new WaitForSeconds(clips[Music_ID].length);

            StartCoroutine("PlayMusic");
        }
    }
    /// Custom Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(AudioManager))]
    public class AudioManagerEditor : Editor
    {
        SerializedProperty MusicList;
        ReorderableList ReorderableList;

        private void OnEnable()
        {
            MusicList = serializedObject.FindProperty("clips");
            ReorderableList = new ReorderableList(serializedObject, MusicList, true, true, true, true);
            ReorderableList.drawElementCallback = DrawList;
            ReorderableList.drawHeaderCallback = DrawHeader;
        }

        void DrawHeader(Rect rect) { EditorGUI.LabelField(rect, "Background Music List"); }

        void DrawList(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = ReorderableList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, rect.height - 4), element, GUIContent.none);
        }

        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            AudioManager loot = (AudioManager)target;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Audio Manager", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Source"), new GUIContent("Background Music Source"));

            float lenght = 0;
            string time = "";
            for (int i = 0; i < loot.clips.Length; i++)
            {
                lenght += loot.clips[i].length;
            }
            int minutes = Mathf.FloorToInt(lenght / 60);
            int seconds = Mathf.FloorToInt(lenght % 60);
            time = minutes.ToString("00") + ":" + seconds.ToString("00");

            EditorGUILayout.EndVertical();

            ReorderableList.DoLayoutList();

            EditorGUILayout.LabelField($"All Background Musics Tracks are {time} Minutes in length.", EditorStyles.centeredGreyMiniLabel);

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

}