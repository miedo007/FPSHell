#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.Networking;
using System.Net;
using System.IO;

namespace HellishBattle.Feedback
{
    public class HellishBattleFeedback : EditorWindow
    {
        public string Version = "2.5.2a";

        private FeedbackType type;
        private string Title = "Request Title";
        private string Content = "Request Content";
        private string Description = "Describe Your Problem Here";
        private bool showSpecs = true;

        [MenuItem("Tiny Slime Studio/Hellish Battle Feedback")]
        static void Init()
        {
            HellishBattleFeedback window = (HellishBattleFeedback)EditorWindow.GetWindow(typeof(HellishBattleFeedback));

            // Load the icon texture from the Assets folder
            Texture2D icon = EditorGUIUtility.IconContent("d_EventSystem Icon").image as Texture2D;
            if (icon != null)
            {
                // Set the window's icon
                GUIContent titleContent = new GUIContent("Feedback System", icon);
                window.titleContent = titleContent;
            }

            window.Show();
        }


        private void SendMessageCoroutine()
        {
            string jsonPayload = $@"{{
    ""username"": ""Hellish Battle Feedback System"",
    ""content"": ""{GetMessageContent()}"",
    ""embeds"": [{{
        ""title"": ""**Description**"",
        ""description"": ""{Description}"",
        ""color"": 832452
    }},{{
        ""title"": ""**System Specification**"",
        ""description"": ""{GetSpecification()}"",
        ""color"": 832452
    }},{{
        ""title"": ""**Unity Version**"",
        ""description"": ""{GetUnityVersion()}"",
        ""color"": 832452
    }}]
}}";

            UnityWebRequest request = new UnityWebRequest("https://discord.com/api/webhooks/1122494785836552212/n-eRs-2ZF37EFKesdGpp3fMuoBSSCxcqSNfuOm5_AydPmx3q1UVISShfu8nXK9BJVwmm", "POST");

            request.SetRequestHeader("Content-Type", "application/json");

            byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(payloadBytes);
            request.downloadHandler = new DownloadHandlerBuffer();

            try
            {
                request.SendWebRequest();
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error sending request: " + e.Message);
            }

        }

        private void OnGUI()
        {
            // Toolbar
            /*
            EditorGUILayout.BeginHorizontal("toolbar");
            EditorGUILayout.LabelField("Hellish Battle Feedback System", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Type:", EditorStyles.boldLabel, GUILayout.Width(40));
            type = (FeedbackType)EditorGUILayout.EnumPopup(type, EditorStyles.toolbarDropDown, GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();*/

            //TinyGUI.InfoBox("text\ntezt\nresr");

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * .6f));
            TinyGUI.BeginBoxGroup("Feedback");
            EditorGUILayout.BeginHorizontal();
            // Labels
            EditorGUILayout.BeginVertical(GUILayout.Width(100));
            EditorGUILayout.LabelField("Category", GUILayout.Width(100));
            EditorGUILayout.LabelField("Title", GUILayout.Width(100));
            EditorGUILayout.LabelField("Description", GUILayout.Width(100));
            EditorGUILayout.LabelField("", GUILayout.Width(100), GUILayout.Height(60 - EditorGUIUtility.singleLineHeight));
            EditorGUILayout.LabelField("Content", GUILayout.Width(100));
            EditorGUILayout.LabelField("", GUILayout.Width(100), GUILayout.Height((position.height - 80 - (EditorGUIUtility.singleLineHeight * 2) - 95 - 50) - EditorGUIUtility.singleLineHeight));
            EditorGUILayout.LabelField("Show", GUILayout.Width(100));
            EditorGUILayout.LabelField("Specifications", GUILayout.Width(100));
            EditorGUILayout.EndVertical();
            // Field
            EditorGUILayout.BeginVertical();
            type = (FeedbackType)EditorGUILayout.EnumPopup(type);
            Title = EditorGUILayout.TextField(Title);
            Content = EditorGUILayout.TextArea(Content, GUILayout.Height(60));
            Description = EditorGUILayout.TextArea(Description, GUILayout.Height((position.height - 80 - (EditorGUIUtility.singleLineHeight * 2) - 90 - 50)));
            EditorGUILayout.Space(16);
            showSpecs = EditorGUILayout.Toggle(showSpecs);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            TinyGUI.EndBoxGroup();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * .4f - 8));
            TinyGUI.BeginBoxGroup("Preview Message");
            GUIStyle TitleStyle = new GUIStyle(GUI.skin.label);
            TitleStyle.fontSize = 30;
            TitleStyle.fontStyle = FontStyle.Bold;

            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField($"{Title}", TitleStyle, GUILayout.Height(32));
            EditorGUILayout.LabelField(Content);
            EditorGUILayout.LabelField($"Category: {type}");
            EditorGUILayout.LabelField("");
            EditorGUILayout.HelpBox($"Description\n\n{Description}", MessageType.None);
            EditorGUILayout.HelpBox($"System Specification\n\n{GetSpecification().Replace("*", "")}", MessageType.None);
            EditorGUILayout.HelpBox($"Unity Version\n\n{GetUnityVersion().Replace("*", "").ToString()}", MessageType.None);
            TinyGUI.EndBoxGroup();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if (TinyGUI.IconButton("Animation.Play", "Submit", GUILayout.Height(64)))
            {
                SendMessageCoroutine();
            }
        }

        public string GetMessageContent()
        {
            return $"# {Title}\\n{Content}\\n**Category:** *{type}*";
        }

        public string GetSpecification()
        {
            string tmp = "";
            if (showSpecs)
            {
                tmp += $"**System:** *{SystemInfo.operatingSystem}*\\n";
                tmp += $"**Processor:** *{SystemInfo.processorType}*\\n";
                tmp += $"**Ram:** *{SystemInfo.systemMemorySize}MB*\\n";
                tmp += $"**Graphic Card:** *{SystemInfo.graphicsDeviceName}*\\n";
                tmp += $"**Graphic Engine:** *{SystemInfo.graphicsDeviceVersion}*";
            }
            else
            {
                tmp += "The user did not want to Share his components :(";
            }
            return tmp;
        }

        public string GetUnityVersion()
        {
            return $"**Unity:** *{Application.unityVersion}*\n**Hellish Battle Version:** *{Version}*";
        }



        enum FeedbackType
        {
            CriticalBug, MinorBug, CosmeticBug, Feedback, Suggestion, Idea
        }
    }
}
#endif
