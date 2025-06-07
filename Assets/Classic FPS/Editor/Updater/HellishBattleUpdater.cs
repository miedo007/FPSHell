using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System;
using System.Collections;

[InitializeOnLoad]
public class HellishBattleUpdater : EditorWindow
{
    static string assetVersion = "2.5.2a";

    public Texture AssetLogo;
    public Texture DrawTheLineLogo;
    public Texture MeetAndTalkLogo;
    public Texture LootTableLogo;

    // Patch Notes
    static PatchNotesClass patchNotes;

    // To Get Version
    static string HellishBattle_Version;
    static string DrawTheLine_Version;
    static string LootTable_Version;
    static string MeetAndTalk_Version = "1.7.1a";

    // Private
    static Vector2 scrollPos;
    static int SelectedPatchNotes = -1;

    [MenuItem("Tiny Slime Studio/Hellish Battle Updater")]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow<HellishBattleUpdater>(true, "Hellish Battle Update Check");

        window.position = new Rect(710, 165, 500, 510+75);
        window.minSize = new Vector2(500, 825);
        window.maxSize = new Vector2(500, 825);
        //window.showOnStart = true;

        Reload();

        window.ShowPopup();
    }
    public void OnGUI()
    {
        GUIStyle myStyle = new GUIStyle();
        myStyle.alignment = TextAnchor.UpperCenter;
        myStyle.normal.textColor = GUI.color;
        myStyle.fontSize = 18;

        #region Base Info

        GUILayout.BeginArea(new Rect(0, 0, 500, 30));
        EditorGUILayout.BeginVertical("Window", GUILayout.Height(30));
        EditorGUILayout.EndVertical();
        GUILayout.EndArea();


        GUILayout.BeginArea(new Rect(0, 0, 500, 140));
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField($"Hellish Battle - Version {assetVersion}", myStyle);
        GUILayout.Space(115);

        GUILayout.BeginArea(new Rect(5, 32, 100, 100));
        EditorGUILayout.BeginVertical("Window");
        GUI.DrawTexture(new Rect(2.5f, 2.5f, 95, 95), AssetLogo, ScaleMode.ScaleToFit, true);
        EditorGUILayout.EndVertical();
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(105, 32, 390, 100));
        EditorGUILayout.BeginVertical();
        int size = GUI.skin.label.fontSize;
        Color color = GUI.color;

        GUI.color = new Color(color.r, color.g, color.b, .65f);
        GUILayout.Label("Tiny Slime Studio");
        GUI.color = color;
        GUI.skin.label.fontSize = 20;
        GUILayout.Label("Hellish Battle - 2.5D Retro FPS");
        GUI.color = new Color(color.r, color.g, color.b, .4f);
        GUI.skin.label.fontSize = size;
        GUILayout.Label("Hellish Battle is a renewed version of the Game with the same title");
        GUILayout.Label("created by a 13-year-old boy who was just starting his adventure with programming");
        GUILayout.Label("and after 5 years he decided to renew his idea.");
        GUI.color = color;

        EditorGUILayout.EndVertical();
        GUILayout.EndArea();

        EditorGUILayout.EndVertical();
        GUILayout.EndArea();

        #endregion

        if (SelectedPatchNotes == -1) // If Data is loading
        {
            GUILayout.BeginArea(new Rect(0, 150, 500, 30));
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(30));
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(0, 150, 500, 490));
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Try To Connection", myStyle);
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Attempting to download data from the server", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("If it takes more than 30 seconds, check your Internet connection");
            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("Possible Reasons for Internet Problems:");
            EditorGUILayout.LabelField("    - The computer is not connected to the Internet");
            EditorGUILayout.LabelField("    - A large amount of files are being downloaded on the computer");
            EditorGUILayout.LabelField("    - Internet overload");
            EditorGUILayout.LabelField("    - Poor Performance of this editor (First ever editor created)");
            EditorGUILayout.Space(20);
            if(GUILayout.Button("Speed Test"))
            {
                Application.OpenURL("https://www.speedtest.net/");
            }

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
        }
        else { 

            if (HellishBattle_Version != assetVersion)
            {
                #region Update Available

                GUILayout.BeginArea(new Rect(0, 150, 500, 30));
                EditorGUILayout.BeginVertical("Window", GUILayout.Height(30));
                EditorGUILayout.EndVertical();
                GUILayout.EndArea();

                GUILayout.BeginArea(new Rect(0, 150, 500, 75));
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.LabelField("Update Available", myStyle);
                GUILayout.Space(10);

                EditorGUILayout.LabelField("New Update is detected!", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Version {assetVersion} -> {HellishBattle_Version}");
                GUILayout.BeginArea(new Rect(395, 35, 100, 35));
                if (GUILayout.Button("Download", GUILayout.Height(35)))
                {
                    Application.OpenURL("https://u3d.as/2Nyc");
                }
                GUILayout.EndArea();
                GUILayout.Space(40);
                EditorGUILayout.EndVertical();
                GUILayout.EndArea();

                #endregion
            }
            else
            {

                GUILayout.BeginArea(new Rect(0, 150, 500, 30));
                EditorGUILayout.BeginVertical("Window", GUILayout.Height(30));
                EditorGUILayout.EndVertical();
                GUILayout.EndArea();

                GUILayout.BeginArea(new Rect(0, 150, 500, 75));
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.LabelField("Update Available", myStyle);
                GUILayout.Space(10);

                EditorGUILayout.LabelField("You have the latest update installed", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Version {assetVersion} is up to date");

                GUILayout.Space(40);
                EditorGUILayout.EndVertical();
                GUILayout.EndArea();
            }

            #region Patch Notes

            GUILayout.BeginArea(new Rect(0, 150 + 85, 500, 30));
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(30));
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(0, 150 + 85, 500, 220));
            EditorGUILayout.BeginVertical("box", GUILayout.Height(220));

            if (0 != SelectedPatchNotes)
            {
                GUILayout.BeginArea(new Rect(3, 3, 19, 19));
                if (GUILayout.Button("<", GUILayout.Height(19)))
                {
                    SelectedPatchNotes--;
                }
                GUILayout.EndArea();
            }

            EditorGUILayout.LabelField($"{patchNotes.patch_note[SelectedPatchNotes].name} [{patchNotes.patch_note[SelectedPatchNotes].version}]", myStyle);
            GUILayout.Space(5);

            if (patchNotes.patch_note.Length - 1 != SelectedPatchNotes)
            {
                GUILayout.BeginArea(new Rect(478, 3, 19, 1198));
                if (GUILayout.Button(">", GUILayout.Height(19)))
                {
                    SelectedPatchNotes++;
                }
                GUILayout.EndArea();
            }

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();

            EditorGUILayout.Space(180 + 85);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(190), GUILayout.Width(500));

            GUILayout.Label(patchNotes.patch_note[SelectedPatchNotes].notes);


            EditorGUILayout.EndScrollView();

            #endregion

            #region Other Assets

            GUIStyle myStyle2 = new GUIStyle();
            myStyle2.fontSize = 16;
            myStyle2.fontStyle = FontStyle.Bold;
            myStyle2.normal.textColor = GUI.color;

            GUILayout.BeginArea(new Rect(0, 380 + 85, 500, 30));
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(30));
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(0, 380 + 85, 500, 175+150));
            EditorGUILayout.BeginVertical("box", GUILayout.Height(175+75));

            EditorGUILayout.LabelField("Other Assets", myStyle);

            GUILayout.BeginArea(new Rect(5, 30, 490, 70), "", "helpbox");
            EditorGUILayout.BeginVertical();
            GUI.DrawTexture(new Rect(5, 5, 60, 60), DrawTheLineLogo, ScaleMode.ScaleToFit, true);
            GUILayout.BeginArea(new Rect(70, 5, 425, 60));
            EditorGUILayout.LabelField(" Draw The Line - Game Template", myStyle2);
            EditorGUILayout.LabelField($"  Version:  {DrawTheLine_Version}   -   Price: 4.99$", EditorStyles.boldLabel);
            GUILayout.BeginArea(new Rect(315, 0, 100, 60));
            if (TinyGUI.IconButton("Asset Store", " Asset\n Store", GUILayout.Height(60)))
            {
                Application.OpenURL("https://u3d.as/2M29");
            }
            GUILayout.EndArea();
            GUILayout.EndArea();

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(5, 30, 70, 70), "", "helpbox");
            GUILayout.EndArea();


            GUILayout.BeginArea(new Rect(5, 105, 490, 70), "", "helpbox");
            EditorGUILayout.BeginVertical();
            GUI.DrawTexture(new Rect(5, 5, 60, 60), LootTableLogo, ScaleMode.ScaleToFit, true);
            GUILayout.BeginArea(new Rect(70, 5, 425, 60));
            EditorGUILayout.LabelField(" Loot Table - Universal Loot System", myStyle2);
            EditorGUILayout.LabelField($"  Version: 2.0.0a   -   Price: Free (built-in)", EditorStyles.boldLabel);

            GUILayout.BeginArea(new Rect(315, 0, 100, 60));
            if (TinyGUI.IconButton("Asset Store", " Asset\n Store", GUILayout.Height(60)))
            {
                Application.OpenURL("https://u3d.as/2VXG");
            }
            GUILayout.EndArea();
            GUILayout.EndArea();

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(5, 105, 70, 70), "", "helpbox");
            GUILayout.EndArea();


            GUILayout.BeginArea(new Rect(5, 180, 490, 70), "", "helpbox");
            EditorGUILayout.BeginVertical();
            GUI.DrawTexture(new Rect(5, 5, 60, 60), MeetAndTalkLogo, ScaleMode.ScaleToFit, true);
            GUILayout.BeginArea(new Rect(70, 5, 425, 60));
            EditorGUILayout.LabelField(" Meet and Talk - Dialogue System", myStyle2);
            EditorGUILayout.LabelField($"  Version: {MeetAndTalk_Version}   -   Price: 4.99$", EditorStyles.boldLabel);

            GUILayout.BeginArea(new Rect(315, 0, 100, 60));
            if (TinyGUI.IconButton("Asset Store", " Asset\n Store", GUILayout.Height(60)))
            {
                Application.OpenURL("https://u3d.as/30sy");
            }
            GUILayout.EndArea();
            GUILayout.EndArea();

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(5, 180, 70, 70), "", "helpbox");
            GUILayout.EndArea();


            myStyle2.fontSize = 13;
            GUILayout.BeginArea(new Rect(5, 255, 490, 70), "", "helpbox");
            EditorGUILayout.BeginVertical();
            GUI.DrawTexture(new Rect(5, 5, 60, 60), MeetAndTalkLogo, ScaleMode.ScaleToFit, true);
            GUILayout.BeginArea(new Rect(70, 5, 425, 60));
            EditorGUILayout.LabelField(" Meet and Talk - Dialogue System - Free Version", myStyle2);
            EditorGUILayout.LabelField($"  Version: {MeetAndTalk_Version}   -   Price: Free", EditorStyles.boldLabel);

            GUILayout.BeginArea(new Rect(315, 0, 100, 60));
            if (TinyGUI.IconButton("Asset Store", " Asset\n Store", GUILayout.Height(60)))
            {
                Application.OpenURL("https://u3d.as/30vK");
            }
            GUILayout.EndArea();
            GUILayout.EndArea();

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(5, 255, 70, 70), "", "helpbox");
            GUILayout.EndArea();


            // 

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();

            #endregion

        }

        #region Buttons

        GUILayout.BeginArea(new Rect(5, 800, 85, 20));
        if (TinyGUI.IconButton("Refresh"," Refresh", GUILayout.Height(20)))
        {
            Reload();
        }
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(95, 800, 150, 20));
        if (TinyGUI.IconButton("LODGroup Icon", " Roadmap", GUILayout.Height(20)))
        {
            Application.OpenURL("https://trello.com/b/LLsG1tUJ/hellish-battle-25d-retro-fps-game-template");
        }
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(250, 800, 245, 20));
        if (TinyGUI.IconButton("TextAsset Icon", " Documentation", GUILayout.Height(20)))
        {
            Application.OpenURL("https://blyattukan.github.io/hellish-battle-documentation/");
        }
        GUILayout.EndArea();

        #endregion
        //}
    }

    static void Reload()
    {
        HellishBattle_Version = null;
        DrawTheLine_Version = null;
        LootTable_Version = null;

        var PatchNotesRequest = UnityWebRequest.Get("https://pastebin.com/raw/fydNEELu");
        var PatchNotesOp = PatchNotesRequest.SendWebRequest(); PatchNotesOp.completed += (aop) => {
            patchNotes = JsonUtility.FromJson<PatchNotesClass>(PatchNotesRequest.downloadHandler.text);
            PatchNotesRequest.Dispose();
            SelectedPatchNotes = patchNotes.patch_note.Length - 1;
        };

        var HellishBattlereq = UnityWebRequest.Get("https://pastebin.com/raw/sAb5VFBR");
        var HellishBattleop = HellishBattlereq.SendWebRequest(); HellishBattleop.completed += (aop) => { HellishBattle_Version = HellishBattlereq.downloadHandler.text; HellishBattlereq.Dispose(); };

        var DrawTheLinereq = UnityWebRequest.Get("https://pastebin.com/raw/n84fZudv");
        var DrawTheLineop = DrawTheLinereq.SendWebRequest(); DrawTheLineop.completed += (aop) => { DrawTheLine_Version = DrawTheLinereq.downloadHandler.text; DrawTheLinereq.Dispose(); };

        //var DrawTheLineIconreq = UnityWebRequestTexture.GetTexture("https://assetstorev1-prd-cdn.unity3d.com/key-image/b92c98f0-d952-42a6-b98c-be85529deead.png");
        //var DrawTheLineIconop = DrawTheLineIconreq.SendWebRequest(); DrawTheLineIconop.completed += (aop) => { DrawTheLineLogo = DownloadHandlerTexture.GetContent(DrawTheLineIconreq) as Texture; DrawTheLineIconreq.Dispose(); };

        var lootTablereq = UnityWebRequest.Get("https://pastebin.com/raw/eJExHB30");
        var lootTableop = lootTablereq.SendWebRequest(); lootTableop.completed += (aop) => { LootTable_Version = lootTablereq.downloadHandler.text; lootTablereq.Dispose(); };

        //var MeetAndTalkreq = UnityWebRequest.Get("https://pastebin.com/raw/eJExHB30");
        //var MeetAndTalkop = MeetAndTalkreq.SendWebRequest(); MeetAndTalkop.completed += (aop) => { MeetAndTalk_Version = MeetAndTalkreq.downloadHandler.text; MeetAndTalkreq.Dispose(); };


    }
}

[Serializable]
public class PatchNotesClass
{
    public patch_notes[] patch_note;

    [System.Serializable]
    public class patch_notes
    {
        public string name;
        public string version;
        public string notes;
    }
}

public class OtherAssetClass
{
    public asset[] other_asset;

    [System.Serializable]
    public class asset
    {
        public string name;
        public string version;
        public string unity_version;
        public string icon_url;
        public string link_url;
    }
}
