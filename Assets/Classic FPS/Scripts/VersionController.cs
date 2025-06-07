using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using TMPro;

namespace HellishBattle.Online
{
    public class VersionController : MonoBehaviour
    {
        [Space(5)] public ReorderableList<patch_notes> VersionList;
        public bool URL;
        //public LocalizedString PatchNotesURL;
        public string PatchNoteURL = "https://pastebin.com/raw/fydNEELu";

        [Line("UI")]
        public Transform Container;
        public TMP_Text VersionText;
        public GameObject NewUpdateObj;
        public TMP_Text UpdateName;
        public TMP_Text VersienNumber;
        public TMP_Text PatchDescription;

        void Start()
        {
            if (URL)
            {
                var PatchNotesRequest = UnityWebRequest.Get(PatchNoteURL);
                var PatchNotesOp = PatchNotesRequest.SendWebRequest(); PatchNotesOp.completed += (aop) =>
                {
                    PatchNotesClass Versions = JsonUtility.FromJson<PatchNotesClass>(PatchNotesRequest.downloadHandler.text);
                    PatchNotesRequest.Dispose();

                    VersionList.List.AddRange(Versions.patch_note);
                    SetupPatchNotes();
                };
            }
            else
            {
                SetupPatchNotes();
            }
        }

        public void SetupPatchNotes()
        {
            for (int i = VersionList.List.Count - 1; i >= 0; i--)
            {
                TMP_Text name = Instantiate(UpdateName, Container);
                name.text = VersionList.List[i].name;

                TMP_Text version = Instantiate(VersienNumber, Container);
                version.text = VersionList.List[i].version;

                TMP_Text description = Instantiate(PatchDescription, Container);
                description.text = VersionList.List[i].notes;
            }

            Destroy(UpdateName.gameObject);
            Destroy(VersienNumber.gameObject);
            Destroy(PatchDescription.gameObject);
            VersionText.text = "V. " + Application.version;
            if (VersionList.List[VersionList.List.Count - 1].version != Application.version)
            {
                NewUpdateObj.SetActive(true);
            }
            else { NewUpdateObj.SetActive(false); }
        }
    }

    [System.Serializable]
    public class PatchNotesClass
    {
        public patch_notes[] patch_note;
    }

    [System.Serializable]
    public class patch_notes
    {
        public string name;
        public string version;
        public string notes;
    }


#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(patch_notes))]
    public class patch_notesPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var labelRect = new Rect(position.x, position.y, position.width, 18);
            var amountRect = new Rect(position.x, position.y + 20, position.width, 18);
            var unitRect = new Rect(position.x, position.y + 40, position.width, 18);
            var nameRect = new Rect(position.x, position.y + 60, position.width, 90);

            if (property.FindPropertyRelative("version").stringValue == "" || property.FindPropertyRelative("name").stringValue == "")
            {
                EditorGUI.LabelField(labelRect, "No Data", EditorStyles.boldLabel);
            }
            else
            {
                EditorGUI.LabelField(labelRect, $"Update {property.FindPropertyRelative("version").stringValue} - {property.FindPropertyRelative("name").stringValue}", EditorStyles.boldLabel);
            }
            EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("name"), new GUIContent("Update Name"));
            EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("version"), new GUIContent("Version Number"));

            EditorGUI.TextArea(nameRect, property.FindPropertyRelative("notes").stringValue);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 20 + 20 + 20 + 90;
        }

    }
#endif

}