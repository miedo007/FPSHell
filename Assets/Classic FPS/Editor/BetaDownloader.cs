using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Collections;

namespace HellishBattle.BetaDownloader
{
    public class BetaDownloader : EditorWindow
    {
        public Texture Banner;

        private static string staticString = "2.5.2a Stable";
        private static string latestVersion;

        private static string txtUrl = "https://pastebin.com/raw/N3XvAC2e";
        private static string unityPackageUrl = "https://github.com/BlyatTukan/TinySlimeStudio/raw/main/test.unitypackage";
        private string tempFilePath;
        private bool isDownloading = false;
        private float downloadProgress = 0;

        public static bool CheckUpdates;

        private const string CheckUpdatesKey = "HellishBattle_BetaDownloader_CheckUpdates";

        [MenuItem("Tiny Slime Studio/Try Update 3.0 Test Version")]
        public static void ShowWindow()
        {
            BetaDownloader window = GetWindow<BetaDownloader>(true, "Update 3.0 Test Version Installer");

            window.position = new Rect(710, 165, 500, 510 + 75);
            window.minSize = new Vector2(512, 256);
            window.maxSize = new Vector2(512, 256);

            latestVersion = "Loading ...";

            window.ShowPopup();
            window.CheckForUpdate();
        }

        private void OnEnable()
        {
            LoadPreferences();
        }

        private void OnDisable()
        {
            SavePreferences();
        }

        private void OnGUI()
        {
            GUI.DrawTexture(new Rect(208, 8, 96, 96), Banner, ScaleMode.ScaleToFit, true);

            GUI.Label(new Rect(0, 104, 512, 16), "Hellish Battle - 2.5D Retro FPS", EditorStyles.centeredGreyMiniLabel);
            GUI.Label(new Rect(0, 116, 512, 16), "Update 3.0 Test Version Installer", EditorStyles.centeredGreyMiniLabel);

            GUIContent installedVersionContent = new GUIContent("Installed Version");
            Vector2 installedVersionSize = GUI.skin.label.CalcSize(installedVersionContent);
            GUI.Label(new Rect(128 - (installedVersionSize.x / 2), 140, installedVersionSize.x, 16), "Installed Version");
            GUI.Label(new Rect(0, 152, 256, 16), staticString, EditorStyles.centeredGreyMiniLabel);

            GUIContent latestVersionContent = new GUIContent("Latest Test Version");
            Vector2 latestVersionSize = GUI.skin.label.CalcSize(latestVersionContent);
            GUI.Label(new Rect(384 - (latestVersionSize.x / 2), 140, latestVersionSize.x, 16), "Latest Test Version");
            GUI.Label(new Rect(256, 152, 256, 16), latestVersion, EditorStyles.centeredGreyMiniLabel);

            // BUTTON
            GUILayout.BeginArea(new Rect(8, 176, 496, 48));
            if (staticString == latestVersion)
            {
                GUI.enabled = false;
                TinyGUI.IconButton("Asset Store", "You have the Latest Test Version Installed", GUILayout.Height(48));
                GUI.enabled = true;
            }
            else if (latestVersion == "Loading ...")
            {
                GUI.enabled = false;
                TinyGUI.IconButton("Asset Store", "Search for Latest Test Version in progress", GUILayout.Height(48));
                GUI.enabled = true;
            }
            else if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                GUI.enabled = false;
                TinyGUI.IconButton("Asset Store", "No Internet Connection :(", GUILayout.Height(48));
                GUI.enabled = true;
            }
            else
            {
                if (TinyGUI.IconButton("Asset Store", "Install Latest Test Version", GUILayout.Height(48)))
                {
                    CheckAndDownloadRoutine();
                }
            }
            GUILayout.EndArea();

            // PROGRESS BAR
            if (isDownloading)
            {
                EditorGUI.ProgressBar(new Rect(8, 176, 496, 48), downloadProgress, $"Download {latestVersion} Version");
            }

            // TOOGLE
            GUILayout.BeginArea(new Rect(8, 232, 496, 16));
            EditorGUIUtility.labelWidth = 478;
            CheckUpdates = EditorGUILayout.Toggle("Allow Test Version Check When Launching The Editor", CheckUpdates);
            GUILayout.EndArea();
        }

        private void CheckAndDownloadRoutine()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                EditorUtility.DisplayDialog("No Internet Connection", "Please check your internet connection and try again.", "OK");
                return;
            }

            var versionRequest = UnityWebRequest.Get(txtUrl);
            var versionOperator = versionRequest.SendWebRequest();

            versionOperator.completed += (aop) =>
            {
                if (versionRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to download txt file: " + versionRequest.error);
                }
                else if (versionRequest.downloadHandler.text != staticString)
                {
                    if (EditorUtility.DisplayDialog("New version found!", "Do you want to install the latest beta?\nMake a backup copy and remember that some functions may not work properly", "Yes, I know what I'm doing", "No"))
                    {
                        StartDownloadPackage();
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("You have the latest version", "No latest beta found", "OK");
                }

                versionRequest.Dispose();
            };
        }

        private void StartDownloadPackage()
        {
            tempFilePath = Path.Combine(Application.temporaryCachePath, "package.unitypackage");
            isDownloading = true;
            EditorCoroutine.Start(DownloadPackageRoutine());
        }

        private IEnumerator DownloadPackageRoutine()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                EditorUtility.DisplayDialog("No Internet Connection", "Please check your internet connection and try again.", "OK");
                yield break;
            }

            var packRequest = UnityWebRequest.Get(unityPackageUrl);
            packRequest.downloadHandler = new DownloadHandlerFile(tempFilePath);
            var packOperator = packRequest.SendWebRequest();

            while (!packRequest.isDone)
            {
                downloadProgress = packRequest.downloadProgress;
                Repaint();
                yield return null;
            }

            if (packRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to download package: " + packRequest.error);
            }
            else
            {
                AssetDatabase.ImportPackage(tempFilePath, true);
            }

            packRequest.Dispose();
            isDownloading = false;
            Repaint();
        }

        private void CheckForUpdate()
        {
            EditorCoroutine.Start(CheckForUpdateRoutine());
        }

        private IEnumerator CheckForUpdateRoutine()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                latestVersion = "No Internet Connection";
                Repaint();
                yield break;
            }

            using (var versionRequest = UnityWebRequest.Get(txtUrl))
            {
                var operation = versionRequest.SendWebRequest();
                while (!operation.isDone)
                {
                    yield return null;
                }

                if (versionRequest.result == UnityWebRequest.Result.Success)
                {
                    latestVersion = versionRequest.downloadHandler.text;
                    Repaint() ;

                    if (latestVersion != staticString)
                    {
                        if (EditorUtility.DisplayDialog("New version found!", "Do you want to install the latest beta?\nMake a backup copy and remember that some functions may not work properly", "Yes, I know what I'm doing", "No"))
                        {
                            StartDownloadPackage();
                        }
                    }
                }
                else
                {
                    Debug.LogError("Failed to download version info: " + versionRequest.error);
                }
            }
        }

        private void SavePreferences()
        {
            EditorPrefs.SetBool(CheckUpdatesKey, CheckUpdates);
        }

        public static void LoadPreferences()
        {
            CheckUpdates = EditorPrefs.GetBool(CheckUpdatesKey, true);
        }
    }

    public class EditorCoroutine
    {
        private readonly IEnumerator routine;

        private EditorCoroutine(IEnumerator routine)
        {
            this.routine = routine;
            EditorApplication.update += Update;
        }

        public static EditorCoroutine Start(IEnumerator routine)
        {
            return new EditorCoroutine(routine);
        }

        private void Update()
        {
            if (!routine.MoveNext())
            {
                EditorApplication.update -= Update;
            }
        }
    }

    [InitializeOnLoad]
    public static class BetaChecker
    {
        static BetaChecker()
        {
            // Load the CheckUpdates preference before checking for updates
            BetaDownloader.LoadPreferences();

            // Check if we've already checked the version in this session
            if (!SessionState.GetBool("CheckedForBetaUpdate", false) && BetaDownloader.CheckUpdates)
            {
                BetaDownloader.ShowWindow();
                SessionState.SetBool("CheckedForBetaUpdate", true);
            }
        }
    }
}
