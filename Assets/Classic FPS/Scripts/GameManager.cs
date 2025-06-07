using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

using HellishBattle.Ability;
using HellishBattle.Enemies;
using HellishBattle.SaveSystem;
using HellishBattle.Level;
using HellishBattle.Player;


namespace HellishBattle
{
    public class GameManager : MonoBehaviour
    {
        // Set Static Object
        private static GameManager _instance;
        public static GameManager Instance
        {
            get { return _instance; }

        }

        public PlayerUIState UIState;


        public List<AmmoStartMaxClass> AmmoCount;

        // Total Points
        [Suffix("Points")] public int TotalPoints;
        public float ComboMultiplier = .5f;
        public int MaxMultiplierLevel = 8;
        public float LostComboTime = 12;
        public float AddScoreToNextLevel = 4;


        // Popup UI
        public GameObject PopupContainer;
        public GameObject PopupPrefab;

        //
        public GameObject RedKeyObject;
        public GameObject BlueKeyObject;
        public GameObject YellowKeyObject;
        public GameObject GreenKeyObject;

        public Campaign.CampaignManager campaign;


        // Privates
        private GameObject deathScreen;
        private GameObject pauseScreen;
        [HideInInspector] public bool paused;

        // Score Multiplier

        // Czas Do Usunięcia levela
        private float multiplierTimer;
        // Poziom Mnożnika
        private float MultiplierLevel;
        // Ile dodanych punktów do następnwego levela
        private float MultiplierScoreToNextLEvel;

        private TMPro.TMP_Text fps_text;
        private TinyInput tinyInput;

        //Ability
        [HideInInspector] public PassiveAbilitySO PassiveSkill;
        [HideInInspector] public BaseAbilityScript Ability;
        [HideInInspector] public Gamepad currentGamepad;

        private void Awake()
        {
            // Set Static Object
            if (_instance == null)
            {
                _instance = this;
            }
            InitGame();

            AbilityManager am = (AbilityManager)Resources.Load("Ability");
            PassiveSkill = am.passiveAbilityList[TinySaveSystem.GetInt("selectedPassiveSkill")].skill;
            Ability = am.AbilityList[TinySaveSystem.GetInt("selectedAbility")].skill;

            tinyInput = InputManager.Instance.input;

            foreach (Transform children in PopupContainer.transform)
            {
                GameObject.Destroy(children.gameObject);
            }

            tinyInput.Menu.Pause.performed += ctx =>
            {
                if (GameManager.Instance.UIState == PlayerUIState.Pause || GameManager.Instance.UIState == PlayerUIState.None)
                {
                    PauseMenuGame();

                }
            };

            /* Load Ammo */
            if (LevelManager.Instance.Gamemode.GameMode == GameModes.StoryMode && TinySaveSystem.HasKey("StoryMode_Save"))
            {
                PlayerSaveClass Save = TinySaveSystem.GetPlayerSave("StoryMode_Save");
                int[] ammoPoll = GameManager.Instance.StringToArray<int>(Save.Ammo);

                for (int i = 0; i < ammoPoll.Length; i++)
                {
                    AmmoCount[i].StartAmmo = ammoPoll[i];
                }
            }
        }
        public void OnEnable()
        {
            tinyInput.Enable();

            InputSystem.onDeviceChange += OnDeviceChange;
        }
        public void OnDisable()
        {
            tinyInput.Disable();
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        private void Start()
        {
            fps_text = transform.parent.Find("Canvas/UI").Find("FPS_Count").GetComponent<TMPro.TMP_Text>();
        }

        private void Update()
        {
            if (TinySaveSystem.GetInt("Settings_Show_FPS") == 0)
            {
                fps_text.text = $"FPS: {(int)(1.0f / Time.smoothDeltaTime)}";
            }
            else
            {
                fps_text.text = "";
            }

            // Score Combo Multiplier
            multiplierTimer += Time.deltaTime;
            if (multiplierTimer > LostComboTime && MultiplierLevel > 0)
            {
                MultiplierLevel--;
                multiplierTimer = 0;
                MultiplierScoreToNextLEvel = 0;
            }

            if (MultiplierLevel > 0)
            {
                GameManager.Instance.transform.parent.Find("Canvas/UI/Score Combo").gameObject.SetActive(true);
                GameManager.Instance.transform.parent.Find("Canvas/UI/Score Combo/Combo_Bar").GetComponent<Slider>().value = ((LostComboTime - multiplierTimer) / LostComboTime);
            }
            else
            {
                GameManager.Instance.transform.parent.Find("Canvas/UI/Score Combo").gameObject.SetActive(false);
            }

            TMPTextStringLocalization _tmp = GameManager.Instance.transform.parent.Find("Canvas/UI/Score Combo/Combo_Txt").GetComponent<TMPTextStringLocalization>();
            _tmp.text_tmp.text = string.Format(_tmp.ShowText.localization.GetString("x_combo"), (MultiplierLevel * ComboMultiplier + 1));

            TMPTextStringLocalization _tmp2 = GameManager.Instance.transform.parent.Find("Canvas/UI/Score Combo/Score_Txt").GetComponent<TMPTextStringLocalization>();
            _tmp2.text_tmp.text = string.Format(_tmp2.ShowText.GetLocalization(), TotalPoints);

            if (TinySaveSystem.GetInt("Settings_Show_Key_List") == 0)
            {
                RedKeyObject.SetActive(LevelManager.Instance.redKeys);
                BlueKeyObject.SetActive(LevelManager.Instance.blueKeys);
                YellowKeyObject.SetActive(LevelManager.Instance.yellowKeys);
                GreenKeyObject.SetActive(LevelManager.Instance.greenKeys);
            }
            else
            {
                RedKeyObject.SetActive(false);
                BlueKeyObject.SetActive(false);
                YellowKeyObject.SetActive(false);
                GreenKeyObject.SetActive(false);
            }
        }

        public void UnPause()
        {
            Time.timeScale = 1f;
            if (Application.isMobilePlatform == false)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        // Disable Cursor & Disable Death Screen
        void InitGame()
        {
            // Get Screens
            deathScreen = transform.Find("DeathScreen").gameObject;
            deathScreen.SetActive(false);
            pauseScreen = transform.Find("PauseScreen").gameObject;
            pauseScreen.SetActive(false);

            // Enable Moves
            Time.timeScale = 1f;
            if (Application.isMobilePlatform == false)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            paused = false;

            Application.targetFrameRate = 300;
        }


        // IF Player Death, this disable all Player Script, Enable Cursor and Enable Death Screen
        public void PlayerDeath()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player").gameObject;
            player.GetComponent<PlayerMovement>().enabled = false;
            player.GetComponent<PlayerHealth>().enabled = false;

            foreach (Transform child in player.transform)
            {
                if (child.name == "Weapons") { child.gameObject.SetActive(false); }
            }
            if (Application.isMobilePlatform == false)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            if (LevelManager.Instance.Gamemode.GameMode == GameModes.Classic || LevelManager.Instance.Gamemode.GameMode == GameModes.StoryMode)
            {
                deathScreen.SetActive(true);
            }
            else
            {
                LevelManager.Instance.GenerateEndStats();
            }
        }

        // Return Ammo Value
        public int AmmoLeft(AmmoType _ammo)
        {
            for (int i = 0; i < AmmoCount.Count; i++)
            {
                if (AmmoCount[i].Type == _ammo)
                {
                    if (_ammo == AmmoType.InfinityAmmo) { return 10121993; }
                    return AmmoCount[i].StartAmmo;
                }
            }
            return 0;
        }

        // Return Ammo Value
        public int MaxAmmo(AmmoType _ammo)
        {
            for (int i = 0; i < AmmoCount.Count; i++)
            {
                if (AmmoCount[i].Type == _ammo)
                {
                    if (_ammo == AmmoType.InfinityAmmo) { return 10121993; }
                    return AmmoCount[i].MaxAmmo;
                }
            }
            return 0;
        }

        // Setup new Bullet Amount
        public void SetNewAmmoAmount(AmmoType _ammo, int newAmount)
        {
            for (int i = 0; i < AmmoCount.Count; i++)
            {
                if (AmmoCount[i].Type == _ammo)
                {
                    AmmoCount[i].StartAmmo = newAmount;
                    if (AmmoCount[i].MaxAmmo < AmmoCount[i].StartAmmo) { AmmoCount[i].StartAmmo = AmmoCount[i].MaxAmmo; }
                }
            }
        }

        // Add new Ammo to old
        public void AddAmmo(AmmoType _ammo, int addAmount)
        {
            for (int i = 0; i < AmmoCount.Count; i++)
            {
                if (AmmoCount[i].Type == _ammo)
                {
                    AmmoCount[i].StartAmmo += addAmount;
                    if (AmmoCount[i].MaxAmmo < AmmoCount[i].StartAmmo) { AmmoCount[i].StartAmmo = AmmoCount[i].MaxAmmo; }
                }
            }
        }

        // Timed Void
        public void ReturnToMainMenu()
        {
            // Get Screens
            Time.timeScale = 1f;
            if (Application.isMobilePlatform == false)
            {
                Cursor.visible = true;
            }
            LoadingScreen.instance.loadingScreen(0);
            Debug.Log("Get to Main Menu");
        }
        // Timed Void
        public void RestartLevel()
        {
            LoadingScreen.instance.loadingScreen(SceneManager.GetActiveScene().name);
        }

        public void ExitTheGame()
        {
            TinySaveSystem.SaveToDisk();
            Application.Quit();
        }

        public void CheckPauseGame()
        {
            if (paused)
            {
                Time.timeScale = 0.0000001f;
                if (Application.isMobilePlatform == false)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
            }
            else
            {
                Time.timeScale = 1f;
                if (Application.isMobilePlatform == false)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }

        public void StartDialog()
        {
            if (Application.isMobilePlatform == false)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
        public void EndDialog()
        {
            if (Application.isMobilePlatform == false)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        public void PauseMenuGame()
        {
            paused = !paused;
            if (paused) { UIState = PlayerUIState.Pause; }
            else
            {
                UIState = PlayerUIState.None;
            }
            GenerateLevelInfoPause();
            pauseScreen.SetActive(paused);
            CheckPauseGame();
            Application.targetFrameRate = 300;
        }

        void GenerateLevelInfoPause()
        {
            pauseScreen.transform.Find("Level_name").GetComponent<TMPTextStringLocalization>().ShowText.key = campaign.FindKeyFromScene();

            //pauseScreen.transform.Find("Req_1").GetComponent<TMPTextStringLocalization>().ShowText.key = campaign.FindKeyFromScene();
            //pauseScreen.transform.Find("Req_2").GetComponent<TMPTextStringLocalization>().ShowText.key = campaign.FindKeyFromScene();
            //pauseScreen.transform.Find("Req_3").GetComponent<TMPTextStringLocalization>().ShowText.key = campaign.FindKeyFromScene();
            //pauseScreen.transform.Find("Req_4").GetComponent<TMPTextStringLocalization>().ShowText.key = campaign.FindKeyFromScene();
            //pauseScreen.transform.Find("Req_5").GetComponent<TMPTextStringLocalization>().ShowText.key = campaign.FindKeyFromScene();
            //Debug.Log(campaign.FindKeyFromScene());
        }

        public void AddPoints(int points)
        {
            TotalPoints += (int)(points * (MultiplierLevel * ComboMultiplier + 1));

            // Multiplier
            MultiplierScoreToNextLEvel++;
            multiplierTimer = 0;

            // Next Level Multiplier
            if (MultiplierScoreToNextLEvel >= AddScoreToNextLevel && MultiplierLevel < MaxMultiplierLevel)
            {
                MultiplierLevel++;
                MultiplierScoreToNextLEvel = 0;
            }
        }

        // In Game Popup
        public IEnumerator SpawnInGamePopup(string Text)
        {
            GameObject popup = Instantiate(PopupPrefab, PopupContainer.transform);
            popup.GetComponent<TMP_Text>().text = Text;
            yield return new WaitForSeconds(3f);
            Destroy(popup);
        }


        // Reworked
        public void PanelSetup(string Title, string Description)
        {
            StopCoroutine("SetupPanel");
            StartCoroutine(SetupPanel(Title, Description));
        }
        public void PanelSetup(string Title, string Description, string InGamePopup)
        {
            StopCoroutine("SetupPanel");
            StartCoroutine(SetupPanel(Title, Description, InGamePopup));
        }

        private IEnumerator SetupPanel(string Title, string Description)
        {
            Transform popup = Camera.main.transform.parent.Find("Main Panels").Find("PickUp Panel");

            popup.Find("Container/Description").GetComponent<TMPro.TMP_Text>().text = Description;
            popup.Find("Container/Title").GetComponent<TMPro.TMP_Text>().text = Title;

            popup.GetComponent<CanvasGroup>().alpha = 1;
            yield return new WaitForSeconds(2);
            popup.GetComponent<CanvasGroup>().alpha = 0;
        }
        private IEnumerator SetupPanel(string Title, string Description, string InGamePopup)
        {
            Transform popup = Camera.main.transform.parent.Find("Main Panels").Find("PickUp Panel");

            popup.Find("Container/Description").GetComponent<TMPro.TMP_Text>().text = Description;
            popup.Find("Container/Title").GetComponent<TMPro.TMP_Text>().text = Title;
            StartCoroutine(SpawnInGamePopup(InGamePopup));

            popup.GetComponent<CanvasGroup>().alpha = 1;
            yield return new WaitForSeconds(2);
            popup.GetComponent<CanvasGroup>().alpha = 0;
        }

        private void OnValidate()
        {
            // Add New Ammo UI if Needed
            if (AmmoCount.Count < Enum.GetValues(typeof(AmmoType)).Length)
            {
                while (AmmoCount.Count != Enum.GetValues(typeof(AmmoType)).Length)
                {
                    AmmoCount.Add(new AmmoStartMaxClass());
                }
            }
            // Remove Ammo UI if Needed
            if (AmmoCount.Count > Enum.GetValues(typeof(AmmoType)).Length)
            {
                while (AmmoCount.Count != Enum.GetValues(typeof(AmmoType)).Length)
                {
                    AmmoCount.RemoveAt(AmmoCount.Count - 1);
                }
            }
            // Setup Ammo Type
            for (int i = 0; i < Enum.GetValues(typeof(AmmoType)).Length; i++)
            {
                int enumID = 0;
                if (i != 0) { enumID = (int)AmmoCount[i - 1].Type + 1; }
                while (((AmmoType)enumID).ToString() == enumID.ToString()) { enumID++; }

                AmmoCount[i].Type = (AmmoType)enumID;
            }
        }

        /* Controller */
        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Added) { if (device is Gamepad) { currentGamepad = (Gamepad)device; } }
            else if (change == InputDeviceChange.Removed) { if (device is Gamepad) { currentGamepad = (Gamepad)device; } }
        }

        public IEnumerator Vibrate(float lowFrequency, float highFrequency, float time, bool mobile)
        {
#if UNITY_ANDROID || UNITY_IOS
        if(mobile) Handheld.Vibrate();
#endif

            // Controller
            if (currentGamepad != null) currentGamepad.SetMotorSpeeds(lowFrequency, highFrequency);
            yield return new WaitForSeconds(time);
            if (currentGamepad != null) currentGamepad.SetMotorSpeeds(lowFrequency, highFrequency);

        }

        /*  */
        public string ArrayToString<T>(T[] array)
        {
            string separator = ", ";
            return string.Join(separator, array);
        }
        public T[] StringToArray<T>(string value)
        {
            string separator = ", ";

            string[] stringArray = value.Split(new string[] { separator }, StringSplitOptions.None);
            T[] resultArray = new T[stringArray.Length];

            for (int i = 0; i < stringArray.Length; i++) { resultArray[i] = (T)Convert.ChangeType(stringArray[i], typeof(T)); }

            return resultArray;
        }
    }

    /// Custom Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(GameManager))]
    public class GameManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            GameManager manager = (GameManager)target;

            TinyGUI.DrawScript<GameManager>("Game Manager", target);

            TinyGUI.BeginBoxGroup("Ammo Settings", "Set the Maximum and initial amount of ammo");
            for (int i = 0; i + 1 < manager.AmmoCount.Count; i++)
            {
                EditorGUILayout.BeginVertical("Helpbox");
                EditorGUILayout.LabelField((manager.AmmoCount[i].Type).ToString() + " Ammo", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("AmmoCount").GetArrayElementAtIndex(i).FindPropertyRelative("MaxAmmo"), new GUIContent($"Max Ammo Count"));
                int MaxCount = manager.AmmoCount[i].MaxAmmo;
                manager.AmmoCount[i].StartAmmo = EditorGUILayout.IntSlider(new GUIContent($"Ammo Count on Start"), manager.AmmoCount[i].StartAmmo, 0, MaxCount);
                EditorGUILayout.EndVertical();
            }
            TinyGUI.EndBoxGroup();

            TinyGUI.BeginBoxGroup("Combo Score Settings", "Combo point accrual settings");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("TotalPoints"), new GUIContent("Actual Score Points"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ComboMultiplier"), new GUIContent("Combo Multiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxMultiplierLevel"), new GUIContent("Max Multiplier Level"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("LostComboTime"), new GUIContent("Lost Combo Time"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("AddScoreToNextLevel"), new GUIContent("Add Score To Next Level"));

            TinyGUI.EndBoxGroup();

            TinyGUI.BeginBoxGroup("UI Settings", "Assign all required UI elements here");

            TinyGUI.Title("Pop-Up UI");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("PopupContainer"), new GUIContent("Pop-Up Container"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("PopupPrefab"), new GUIContent("Pop-Up Prefab"));

            TinyGUI.Title("Key List UI");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("RedKeyObject"), new GUIContent("Red Key UI"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("BlueKeyObject"), new GUIContent("Blue Key UI"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("YellowKeyObject"), new GUIContent("Yellow Key UI"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("GreenKeyObject"), new GUIContent("Green Key UI"));

            TinyGUI.EndBoxGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

    public enum AmmoType
    {
        // Bullets
        Bullet = 0, Shell = 1, Clip = 2, Grenade = 3, Rocket = 4, EnergyCell = 5, Arrow = 6,
        // Grenades
        HandGrenade = 100, Molotov = 101, Kunai = 102,
        // Infinity
        InfinityAmmo = 666
    }

    [System.Serializable]
    public class AmmoIconClass
    {
        public AmmoType Type;
        public Sprite Icon;
    }
    [System.Serializable]
    public class AmmoStartMaxClass
    {
        public AmmoType Type;
        [Range(0, 999)] public int StartAmmo;
        [Range(0, 999)] public int MaxAmmo;
    }

    public enum PlayerUIState
    {
        None = 0, Inventory = 1, Pause = 2, Console = 3
    }

    [System.Serializable]
    public class DamageClass
    {
        [Min(0), Suffix("Base DMG")] public float Damage;
        public DamageType Type;

        public Vector3 HitPosition;

        public DamageClass(float DMG, DamageType DMGType, Vector3 Hit)
        {
            Damage = DMG;
            Type = DMGType;
            HitPosition = Hit;
        }

        public DamageClass(float DMG, DamageType DMGType)
        {
            Damage = DMG;
            Type = DMGType;
            HitPosition = new Vector3(0, 0, 0);
        }

        public DamageClass(float DMG)
        {
            Damage = DMG;
            Type = DamageType.Normal;
            HitPosition = new Vector3(0, 0, 0);
        }
    }
}