using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEngine.AI;
using UnityEngine.Rendering.PostProcessing;
using HellishBattle.Player.UI;
using HellishBattle.Enemies;
using HellishBattle.SaveSystem;
using HellishBattle.Weapon;
using HellishBattle.Player;
using HellishBattle.Campaign;

namespace HellishBattle.Level
{
    public class LevelManager : MonoBehaviour
    {

        private static LevelManager _instance;
        public static LevelManager Instance
        {
            get { return _instance; }
        }

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
            FindSecret.Clear();
            for (int i = 0; i < SecretRoom.Count; i++)
            {
                FindSecret.Add(false);
            }
            FindItems.Clear();
            for (int i = 0; i < Items.Count; i++)
            {
                FindItems.Add(false);
            }
        }

        public CampaignManager campaign;
        public GameModeManager Gamemode;

        /* Inventory Settings */
        public List<Transform> ClassicWeapon;
        public List<Transform> PrimaryWeapon = new List<Transform>(32);
        public List<Transform> SecoundaryWeapon = new List<Transform>(32);
        public List<Transform> MelleWeapon = new List<Transform>(32);
        public List<Transform> ThrowableWeapon = new List<Transform>(32);
        /* Inventory Settings */

        /* Enemy Settings */
        [Suffix("Metres")] public float EnemySpawnRadius = 25;
        public List<EnemyChangeStruct> SpawnChangePercent;
        public List<GameObject> CommonEnemyList;
        public List<GameObject> UncommonEnemyList;
        public List<GameObject> RareEnemyList;
        public List<GameObject> BossEnemyList;
        /* Enemy Settings */

        /* Deadly Race Mode */
        public float DeadlyRaceMode_EnemySpawnRate = 8f;
        public Vector2 DeadlyRaceMode_EnemySpawnSize = new Vector2(3, 4);
        public float DeadlyRaceMode_MultiplierSpawn = 1.08f;
        public int DeadlyRaceMode_SpawnBetweenBoss = 10;
        public float DeadlyRaceMode_BossSpawnMultiplier = .7f;
        public float DeadlyRaceMode_MaxEnemySpawn = 60;
        /* Deadly Race Mode */

        /* Wave Mode */
        [Suffix("Seconds")]
        public float WaveMode_MaxWaveTime = 30f;
        [Suffix("Enemy per Wave")]
        public int WaveMode_WaveSpawnSize = 8;
        [Suffix("* Wave Size + Base Spawn")]
        public float WaveMode_SpawnMultiplier = 0.1f;
        public int WaveMode_WaveBetweenBoss = 5;
        [Suffix("* Wave Size")]
        public float WaveMode_BossWaveMultiplier = .5f;
        /* Wave Mode */

        /* Arcade Mode */
        public int ArcadeMode_StartItemCount = 10;
        public List<GameObject> ArcadeMode_StartItemList;
        /* Arcade Mode */



        public int StartObjectSpawnCount = 30;
        public List<GameObject> StartObjectSpawnList;
        [Suffix("Metres")] public float BonusWaveItemSpawnRange = 25;
        public LootDrop BonusWaveItemDrop;
        public int BonudWaveItemChange = 2;
        /* Spawn Object Settings */

        [Line("Key Doors")]
        public bool redKeys;
        public bool blueKeys;
        public bool yellowKeys;
        public bool greenKeys;


        //[Line("To Dev")]
        [HideInInspector] public List<SecretScript> SecretRoom;
        public List<BonusScript> Items;
        [HideInInspector] public List<Enemy> Enemy;
        [HideInInspector] public int WaveLevel;
        [HideInInspector] public List<bool> FindItems;
        [HideInInspector] public List<bool> FindSecret;
        [HideInInspector] public List<bool> FindEnemy;

        float timer;
        float gamemodeTimer = 999;
        int _DealtyRaceCounter;

        int foundSecret;
        int foundItems;
        int foundEnemy;

        bool EndLevel;

        private void Start()
        {
            PlayerUIManager.Instance.EndScreen_BackToMenuBtn.GetComponent<Button>().onClick.AddListener(delegate () { BackToMenu(); });

            PlayerUIManager.Instance.EndScreen_Panel.SetActive(false);

            Transform _special_ui = GameObject.Find("Player/Canvas/UI/Gamemodes_UI").GetComponent<Transform>();
            foreach (Transform child in _special_ui) { child.gameObject.SetActive(false); }

            if (Gamemode.GameMode == GameModes.Classic) { /* Do Nothing */ }

            if (Gamemode.GameMode == GameModes.DeadlyRace)
            {
                _special_ui.Find("Deadly_Race_UI").GetComponent<Transform>().gameObject.SetActive(true);

                // Spawn Start Objects
                for (int i = 0; i < StartObjectSpawnCount; i++)
                {
                    Instantiate(StartObjectSpawnList[Random.Range(0, (StartObjectSpawnList.Count))], GetRandomPoint(), Quaternion.identity);
                }
            }

            if (Gamemode.GameMode == GameModes.WaveMode)
            {
                _special_ui.Find("Wave_UI").GetComponent<Transform>().gameObject.SetActive(true);

                // Spawn Start Objects
                for (int i = 0; i < StartObjectSpawnCount; i++)
                {
                    Instantiate(StartObjectSpawnList[Random.Range(0, (StartObjectSpawnList.Count))], GetRandomPoint(), Quaternion.identity);
                }
            }

            if (Gamemode.GameMode == GameModes.ArcadeMode)
            {
                _special_ui.Find("Arcade_UI").GetComponent<Transform>().gameObject.SetActive(true);

                // Spawn Start Objects
                for (int i = 0; i < ArcadeMode_StartItemCount; i++)
                {
                    Instantiate(ArcadeMode_StartItemList[Random.Range(0, (ArcadeMode_StartItemList.Count))], GetRandomPoint(), Quaternion.identity);
                }
            }

            #region Get Settings

            // Anti-Aliassing
            PostProcessLayer _antiAliasing = (PostProcessLayer)GameObject.FindObjectOfType(typeof(PostProcessLayer));
            _antiAliasing.antialiasingMode = (PostProcessLayer.Antialiasing)TinySaveSystem.GetInt("Settings_PostProcessing");
            // Post Processing
            PostProcessVolume _pp = (PostProcessVolume)GameObject.FindObjectOfType(typeof(PostProcessVolume));
            _pp.weight = (TinySaveSystem.GetBool("Settings_PostProcessing") == true ? 0 : 1);

            PostProcessVolume postProcessVolume = Camera.main.transform.GetComponent<PostProcessVolume>();
            MotionBlur motionBlurEffect = null;
            postProcessVolume.profile.TryGetSettings(out motionBlurEffect);
            motionBlurEffect.active = (TinySaveSystem.GetInt("Settings_MotionBlur") == 1 ? false : true);

            #endregion
        }

        public void TryFindSecret(SecretScript secretObject)
        {
            for (int i = 0; i < SecretRoom.Count; i++)
            {
                if (SecretRoom[i].secret_id == secretObject.secret_id) { FindSecret[i] = true; foundSecret++; }
            }
        }

        public void TryFindItem(BonusScript itemObject)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].secret_id == itemObject.secret_id) { FindItems[i] = true; foundItems++; }
            }
        }

        public void TryFindEnemy(Enemy enemyObject)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Enemy[i].secret_id == enemyObject.secret_id) { FindEnemy[i] = true; foundEnemy++; }
            }
        }

        private void Update()
        {
            if (!EndLevel) { timer += Time.deltaTime; gamemodeTimer += Time.deltaTime; }

            if (Gamemode.GameMode == GameModes.Classic) { /* Do Nothing */ }


            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            if (!EndLevel)
            {
                //Debug.Log(gamemodeTimer);
                if (Gamemode.GameMode == GameModes.DeadlyRace)
                {
                    //GameObject.Find("Player/Canvas/UI/Gamemodes_UI/Deadly_Race_UI/Value_Txt").GetComponent<TMP_Text>().text = Mathf.Floor(timer / 60).ToString("00") + ":" + (timer % 60).ToString("00");

                    TMPTextStringLocalization _tmp = GameObject.Find("Player/Canvas/UI/Gamemodes_UI/Deadly_Race_UI/Value_Txt").GetComponent<TMPTextStringLocalization>();
                    _tmp.text_tmp.text = "<size=10>" + _tmp.ShowText.localization.GetString("wave") + ": </size>" + Mathf.Floor(timer / 60).ToString("00") + ":" + (timer % 60).ToString("00");
                    TMPTextStringLocalization _tmp2 = GameObject.Find("Player/Canvas/UI/Gamemodes_UI/Deadly_Race_UI/Best_Txt").GetComponent<TMPTextStringLocalization>();
                    _tmp2.text_tmp.text = "<size=10>" + _tmp2.ShowText.localization.GetString("Best") + ": </size>" + Mathf.Floor(TinySaveSystem.GetInt(sceneName + "_Value") / 60).ToString("00") + ":" + (TinySaveSystem.GetInt(sceneName + "_Value") % 60).ToString("00");

                    if (gamemodeTimer >= DeadlyRaceMode_EnemySpawnRate)
                    {
                        _DealtyRaceCounter++;

                        if (_DealtyRaceCounter % WaveMode_WaveBetweenBoss != 0)
                        {
                            // Spawn Normal Wave
                            for (int i = 0; i < (Random.Range(DeadlyRaceMode_EnemySpawnSize.x, DeadlyRaceMode_EnemySpawnSize.y + 1) + (int)(Random.Range(DeadlyRaceMode_EnemySpawnSize.x, DeadlyRaceMode_EnemySpawnSize.y + 1) * DeadlyRaceMode_MultiplierSpawn * WaveLevel)); i++)
                            {
                                float randomCategory = Random.Range(0f, 1f);
                                EnemyChangeStruct change = GetEnemyChangeStruct(_DealtyRaceCounter);
                                GameObject enemyObject;

                                if (randomCategory < change.CategorySeparator.x) // Common
                                {
                                    enemyObject = (GameObject)Instantiate(CommonEnemyList[UnityEngine.Random.Range(0, (CommonEnemyList.Count))], RandomNavmeshLocation(EnemySpawnRadius), Quaternion.identity);
                                    enemyObject.GetComponent<EnemyStates>().OrderWaypoints = TargetSelectType.PlayerTarget;
                                }
                                else if (randomCategory > change.CategorySeparator.x && randomCategory < change.CategorySeparator.y) // Uncommon
                                {
                                    enemyObject = (GameObject)Instantiate(UncommonEnemyList[UnityEngine.Random.Range(0, (UncommonEnemyList.Count))], RandomNavmeshLocation(EnemySpawnRadius), Quaternion.identity);
                                    enemyObject.GetComponent<EnemyStates>().OrderWaypoints = TargetSelectType.PlayerTarget;
                                }
                                else // Rare
                                {
                                    enemyObject = (GameObject)Instantiate(RareEnemyList[UnityEngine.Random.Range(0, (RareEnemyList.Count))], RandomNavmeshLocation(EnemySpawnRadius), Quaternion.identity);
                                    enemyObject.GetComponent<EnemyStates>().OrderWaypoints = TargetSelectType.PlayerTarget;
                                }
                            }
                        }
                        else
                        {
                            // Spawn Boss
                            GameObject BossObject = (GameObject)Instantiate(BossEnemyList[UnityEngine.Random.Range(0, (BossEnemyList.Count))], RandomNavmeshLocation(EnemySpawnRadius), Quaternion.identity);
                            BossObject.GetComponent<EnemyStates>().OrderWaypoints = TargetSelectType.PlayerTarget;

                            // Spawn Boss Wave
                            for (int i = 0; i < (Random.Range(DeadlyRaceMode_EnemySpawnSize.x, DeadlyRaceMode_EnemySpawnSize.y + 1) + (int)(Random.Range(DeadlyRaceMode_EnemySpawnSize.x, DeadlyRaceMode_EnemySpawnSize.y + 1) * DeadlyRaceMode_MultiplierSpawn * WaveLevel)) * WaveMode_BossWaveMultiplier; i++)
                            {
                                float randomCategory = Random.Range(0f, 1f);
                                EnemyChangeStruct change = GetEnemyChangeStruct(_DealtyRaceCounter);
                                GameObject enemyObject;

                                if (randomCategory < change.CategorySeparator.x) // Common
                                {
                                    enemyObject = (GameObject)Instantiate(CommonEnemyList[UnityEngine.Random.Range(0, (CommonEnemyList.Count))], RandomNavmeshLocation(EnemySpawnRadius), Quaternion.identity);
                                    enemyObject.GetComponent<EnemyStates>().OrderWaypoints = TargetSelectType.PlayerTarget;
                                }
                                else if (randomCategory > change.CategorySeparator.x && randomCategory < change.CategorySeparator.y) // Uncommon
                                {
                                    enemyObject = (GameObject)Instantiate(UncommonEnemyList[UnityEngine.Random.Range(0, (UncommonEnemyList.Count))], RandomNavmeshLocation(EnemySpawnRadius), Quaternion.identity);
                                    enemyObject.GetComponent<EnemyStates>().OrderWaypoints = TargetSelectType.PlayerTarget;
                                }
                                else // Rare
                                {
                                    enemyObject = (GameObject)Instantiate(RareEnemyList[UnityEngine.Random.Range(0, (RareEnemyList.Count))], RandomNavmeshLocation(EnemySpawnRadius), Quaternion.identity);
                                    enemyObject.GetComponent<EnemyStates>().OrderWaypoints = TargetSelectType.PlayerTarget;
                                }
                            }
                        }


                        gamemodeTimer = 0;

                        // Spawn Bonus Wave Item
                        BonusWaveItemDrop.SpawnDrop(Camera.main.transform, BonudWaveItemChange, BonusWaveItemSpawnRange, true);
                    }
                }

                if (Gamemode.GameMode == GameModes.WaveMode)
                {
                    TMPTextStringLocalization _tmp = GameObject.Find("Player/Canvas/UI/Gamemodes_UI/Wave_UI/Value_Txt").GetComponent<TMPTextStringLocalization>();
                    _tmp.text_tmp.text = "<size=10>" + _tmp.ShowText.localization.GetString("wave") + ": </size>" + WaveLevel;
                    TMPTextStringLocalization _tmp2 = GameObject.Find("Player/Canvas/UI/Gamemodes_UI/Wave_UI/Best_Txt").GetComponent<TMPTextStringLocalization>();
                    _tmp2.text_tmp.text = "<size=10>" + _tmp2.ShowText.localization.GetString("Best") + ": </size>" + TinySaveSystem.GetInt(sceneName + "_Value");

                    if (gamemodeTimer >= WaveMode_MaxWaveTime)
                    {
                        WaveLevel++;

                        if (WaveLevel % WaveMode_WaveBetweenBoss != 0)
                        {
                            // Spawn Normal Wave
                            for (int i = 0; i < (WaveMode_WaveSpawnSize + (int)(WaveMode_WaveSpawnSize * WaveMode_SpawnMultiplier * WaveLevel)); i++)
                            {
                                float randomCategory = Random.Range(0f, 1f);
                                EnemyChangeStruct change = GetEnemyChangeStruct(WaveLevel);
                                GameObject enemyObject;

                                if (randomCategory < change.CategorySeparator.x) // Common
                                {
                                    enemyObject = (GameObject)Instantiate(CommonEnemyList[UnityEngine.Random.Range(0, (CommonEnemyList.Count))], RandomNavmeshLocation(EnemySpawnRadius), Quaternion.identity);
                                    enemyObject.GetComponent<EnemyStates>().OrderWaypoints = TargetSelectType.PlayerTarget;
                                }
                                else if (randomCategory > change.CategorySeparator.x && randomCategory < change.CategorySeparator.y) // Uncommon
                                {
                                    enemyObject = (GameObject)Instantiate(UncommonEnemyList[UnityEngine.Random.Range(0, (UncommonEnemyList.Count))], RandomNavmeshLocation(EnemySpawnRadius), Quaternion.identity);
                                    enemyObject.GetComponent<EnemyStates>().OrderWaypoints = TargetSelectType.PlayerTarget;
                                }
                                else // Rare
                                {
                                    enemyObject = (GameObject)Instantiate(RareEnemyList[UnityEngine.Random.Range(0, (RareEnemyList.Count))], RandomNavmeshLocation(EnemySpawnRadius), Quaternion.identity);
                                    enemyObject.GetComponent<EnemyStates>().OrderWaypoints = TargetSelectType.PlayerTarget;
                                }
                            }
                        }
                        else
                        {
                            // Spawn Boss
                            GameObject BossObject = (GameObject)Instantiate(BossEnemyList[UnityEngine.Random.Range(0, (BossEnemyList.Count))], RandomNavmeshLocation(EnemySpawnRadius), Quaternion.identity);
                            BossObject.GetComponent<EnemyStates>().OrderWaypoints = TargetSelectType.PlayerTarget;

                            // Spawn Boss Wave
                            for (int i = 0; i < (WaveMode_WaveSpawnSize + (int)(WaveMode_WaveSpawnSize * WaveMode_SpawnMultiplier * WaveLevel)) * WaveMode_BossWaveMultiplier; i++)
                            {
                                float randomCategory = Random.Range(0f, 1f);
                                EnemyChangeStruct change = GetEnemyChangeStruct(WaveLevel);
                                GameObject enemyObject;

                                if (randomCategory < change.CategorySeparator.x) // Common
                                {
                                    enemyObject = (GameObject)Instantiate(CommonEnemyList[UnityEngine.Random.Range(0, (CommonEnemyList.Count))], RandomNavmeshLocation(EnemySpawnRadius), Quaternion.identity);
                                    enemyObject.GetComponent<EnemyStates>().OrderWaypoints = TargetSelectType.PlayerTarget;
                                }
                                else if (randomCategory > change.CategorySeparator.x && randomCategory < change.CategorySeparator.y) // Uncommon
                                {
                                    enemyObject = (GameObject)Instantiate(UncommonEnemyList[UnityEngine.Random.Range(0, (UncommonEnemyList.Count))], RandomNavmeshLocation(EnemySpawnRadius), Quaternion.identity);
                                    enemyObject.GetComponent<EnemyStates>().OrderWaypoints = TargetSelectType.PlayerTarget;
                                }
                                else // Rare
                                {
                                    enemyObject = (GameObject)Instantiate(RareEnemyList[UnityEngine.Random.Range(0, (RareEnemyList.Count))], RandomNavmeshLocation(EnemySpawnRadius), Quaternion.identity);
                                    enemyObject.GetComponent<EnemyStates>().OrderWaypoints = TargetSelectType.PlayerTarget;
                                }
                            }
                        }


                        gamemodeTimer = 0;

                        // Spawn Bonus Wave Item
                        BonusWaveItemDrop.SpawnDrop(Camera.main.transform, BonudWaveItemChange, BonusWaveItemSpawnRange, true);

                    }
                }

                if (Gamemode.GameMode == GameModes.ArcadeMode)
                {
                    TMPTextStringLocalization _tmp = GameObject.Find("Player/Canvas/UI/Gamemodes_UI/Arcade_UI/Value_Txt").GetComponent<TMPTextStringLocalization>();
                    _tmp.text_tmp.text = "<size=10>" + _tmp.ShowText.localization.GetString("score") + ": </size>" + GameManager.Instance.TotalPoints;
                    TMPTextStringLocalization _tmp2 = GameObject.Find("Player/Canvas/UI/Gamemodes_UI/Arcade_UI/Best_Txt").GetComponent<TMPTextStringLocalization>();
                    _tmp2.text_tmp.text = "<size=10>" + _tmp2.ShowText.localization.GetString("Best") + ": </size>" + TinySaveSystem.GetInt(sceneName + "_Value");
                }
            }
        }

        public EnemyChangeStruct GetEnemyChangeStruct(int waveLevel)
        {
            EnemyChangeStruct tmp = SpawnChangePercent[0];
            for (int i = 0; i < SpawnChangePercent.Count; i++) { if (SpawnChangePercent[i].WaveNumber < WaveLevel) { tmp = SpawnChangePercent[i]; } }
            return tmp;
        }

        public Vector3 RandomNavmeshLocation(float radius)
        {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
            randomDirection += GameManager.Instance.transform.parent.position;

            NavMeshHit hit;
            Vector3 finalPosition = Vector3.zero;


            if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
            {
                finalPosition = hit.position;
            }
            return finalPosition;
        }

        public static Vector3 GetRandomPoint()
        {
            Vector3 randomPos = UnityEngine.Random.insideUnitSphere * 666;

            NavMeshHit hit;
            NavMesh.SamplePosition(randomPos, out hit, 666, NavMesh.AllAreas);


            return hit.position;
        }


        #region Trophy in Script

        public void GenerateEndStats()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Trophy _tmpTrophy = TinySaveSystem.GetTrophy(sceneName);

            if (Gamemode.GameMode == GameModes.StoryMode)
            {
                PlayerSaveClass Save = new PlayerSaveClass();
                Save.Health = Camera.main.transform.parent.GetComponent<PlayerHealth>().health;
                Save.Armor = Camera.main.transform.parent.GetComponent<PlayerHealth>().armor;
                Save.Map = campaign.CheckNextLevel();

                WeaponSwitch ws = Camera.main.transform.parent.transform.Find("Weapons").GetComponent<WeaponSwitch>();

                // Ammo
                List<int> ammoPoll = new List<int>();

                for (int i = 0; i < GameManager.Instance.AmmoCount.Count; i++)
                {
                    ammoPoll.Add(GameManager.Instance.AmmoCount[i].StartAmmo);
                }

                Save.Ammo = GameManager.Instance.ArrayToString(ammoPoll.ToArray());

                // All Weaoin
                List<string> all = new List<string>();
                for (int i = 0; i < ws.AllWeaponList.Count; i++)
                {
                    if (ws.AllWeaponList[i] != null) { all.Add(ws.AllWeaponList[i].name.Replace("(Clone)", "")); }
                    else { all.Add("Empty"); }
                }
                Save.AllWeapon = GameManager.Instance.ArrayToString(all.ToArray());

                // Primary Slot
                List<string> primary = new List<string>();
                for (int i = 0; i < ws.PrimarySlot.Count; i++)
                {
                    if (ws.PrimarySlot[i] != null) { primary.Add(ws.PrimarySlot[i].name.Replace("(Clone)", "")); }
                    else { primary.Add("Empty"); }
                }
                Save.PrimarySlot = GameManager.Instance.ArrayToString(primary.ToArray());

                // Secoundary Slot
                List<string> secoundary = new List<string>();
                for (int i = 0; i < ws.SecoundarySlot.Count; i++)
                {
                    if (ws.SecoundarySlot[i] != null) { secoundary.Add(ws.SecoundarySlot[i].name.Replace("(Clone)", "")); }
                    else { secoundary.Add("Empty"); }
                }
                Save.SecoundarySlot = GameManager.Instance.ArrayToString(secoundary.ToArray());

                // Melle Slot
                List<string> melle = new List<string>();
                for (int i = 0; i < ws.MelleSlot.Count; i++)
                {
                    if (ws.MelleSlot[i] != null) { melle.Add(ws.MelleSlot[i].name.Replace("(Clone)", "")); }
                    else { melle.Add("Empty"); }
                }
                Save.MelleSlot = GameManager.Instance.ArrayToString(melle.ToArray());

                // Throwable Slot
                List<string> throwable = new List<string>();
                for (int i = 0; i < ws.ThrowableSlot.Count; i++)
                {
                    if (ws.ThrowableSlot[i] != null) { throwable.Add(ws.ThrowableSlot[i].name.Replace("(Clone)", "")); }
                    else { throwable.Add("Empty"); }
                }
                Save.ThrowableSlot = GameManager.Instance.ArrayToString(throwable.ToArray());



                TinySaveSystem.SetPlayerSave("StoryMode_Save", Save);
            }

            // Set Time and Cursor
            Time.timeScale = 1f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Disable Player
            GameObject player = GameObject.FindGameObjectWithTag("Player").gameObject;
            player.GetComponent<PlayerMovement>().enabled = false;
            player.GetComponent<PlayerHealth>().enabled = false;

            // Enable Panel and Disable Timer
            EndLevel = true;
            PlayerUIManager.Instance.EndScreen_Panel.SetActive(true);
            PlayerUIManager.Instance.EndScreen_LevelNameTxt.ShowText.key = campaign.FindKeyFromScene();

            _tmpTrophy.Requirement_1 = CheckTrophyRequirement(PlayerUIManager.Instance.EndScreen_Requirement_1, Gamemode.GetRequirement(1));
            _tmpTrophy.Requirement_2 = CheckTrophyRequirement(PlayerUIManager.Instance.EndScreen_Requirement_2, Gamemode.GetRequirement(2));
            _tmpTrophy.Requirement_3 = CheckTrophyRequirement(PlayerUIManager.Instance.EndScreen_Requirement_3, Gamemode.GetRequirement(3));
            _tmpTrophy.Requirement_4 = CheckTrophyRequirement(PlayerUIManager.Instance.EndScreen_Requirement_4, Gamemode.GetRequirement(4));
            _tmpTrophy.Requirement_5 = CheckTrophyRequirement(PlayerUIManager.Instance.EndScreen_Requirement_5, Gamemode.GetRequirement(5));

            //_tmpTrophy.Dev_Check_Requirements();


            // Show Time
            PlayerUIManager.Instance.EndScreen_TimeTxt.text = Mathf.Floor(timer / 60).ToString("00") + ":" + (timer % 60).ToString("00");
            // Show Collected Items
            if (Items.Count == 0) { PlayerUIManager.Instance.EndScreen_ItemsTxt.text = "100%"; }
            else { PlayerUIManager.Instance.EndScreen_ItemsTxt.text = Mathf.Floor((float)foundItems / Items.Count * 100) + "%"; }
            // Show Discored Secret Rooms
            if (SecretRoom.Count == 0) { PlayerUIManager.Instance.EndScreen_SecretsTxt.text = "100%"; }
            else { PlayerUIManager.Instance.EndScreen_SecretsTxt.text = Mathf.Floor((float)foundSecret / SecretRoom.Count * 100) + "%"; }
            // Show Killed Enemy
            if (Enemy.Count == 0) { PlayerUIManager.Instance.EndScreen_EnemyTxt.text = "100%"; }
            else { PlayerUIManager.Instance.EndScreen_EnemyTxt.text = Mathf.Floor((float)foundEnemy / Enemy.Count * 100) + "%"; }
            // Show Points
            PlayerUIManager.Instance.EndScreen_PointsTxt.text = GameManager.Instance.TotalPoints.ToString();



            // Generate Trophy
            GenerateTrophy(_tmpTrophy);
            // If Better Save
            if (_tmpTrophy.GetCount() > TinySaveSystem.GetTrophy(sceneName).GetCount()) { TinySaveSystem.SetTrophy(sceneName, _tmpTrophy); }

            if (Gamemode.GameMode != GameModes.Classic)
            {
                if (Gamemode.GameMode == GameModes.DeadlyRace) { if (TinySaveSystem.GetInt(sceneName + "_Value") < (int)timer) { TinySaveSystem.SetInt(sceneName + "_Value", (int)timer); } }
                if (Gamemode.GameMode == GameModes.WaveMode) { if (TinySaveSystem.GetInt(sceneName + "_Value") < WaveLevel) { TinySaveSystem.SetInt(sceneName + "_Value", WaveLevel); } }
                if (Gamemode.GameMode == GameModes.ArcadeMode) { if (TinySaveSystem.GetInt(sceneName + "_Value") < GameManager.Instance.TotalPoints) { TinySaveSystem.SetInt(sceneName + "_Value", GameManager.Instance.TotalPoints); } }
            }

            // Add Points
            GameSettingsManger _gm = Resources.Load("Game_Settings") as GameSettingsManger;
            DifficultyManager _dm = Resources.Load("Difficulty") as DifficultyManager;
            switch (_tmpTrophy.GetCount())
            {
                case 1:
                    TinySaveSystem.SetInt("unlockable_point", TinySaveSystem.GetInt("unlockable_point") + (int)(_gm.NoneTrophyPoints * _dm.CurrentDifficultyLevel().pointsMultiplier));
                    break;
                case 2:
                    TinySaveSystem.SetInt("unlockable_point", TinySaveSystem.GetInt("unlockable_point") + (int)(_gm.BronzeTrophyPoints * _dm.CurrentDifficultyLevel().pointsMultiplier));
                    break;
                case 3:
                    TinySaveSystem.SetInt("unlockable_point", TinySaveSystem.GetInt("unlockable_point") + (int)(_gm.SilverTrophyPoints * _dm.CurrentDifficultyLevel().pointsMultiplier));
                    break;
                case 4:
                    TinySaveSystem.SetInt("unlockable_point", TinySaveSystem.GetInt("unlockable_point") + (int)(_gm.GoldTrophyPoints * _dm.CurrentDifficultyLevel().pointsMultiplier));
                    break;
                case 5:
                    TinySaveSystem.SetInt("unlockable_point", TinySaveSystem.GetInt("unlockable_point") + (int)(_gm.DiamondTrophyPoints * _dm.CurrentDifficultyLevel().pointsMultiplier));
                    break;
                default:
                    TinySaveSystem.SetInt("unlockable_point", TinySaveSystem.GetInt("unlockable_point") + (int)(_gm.PlatiniumTrophyPoints * _dm.CurrentDifficultyLevel().pointsMultiplier));
                    break;
            }
            //this.Log($"You get {_tmpTrophy.GetCount()} point(s)");

            // Next Level Btn
            string checkerNextLevel = campaign.CheckNextLevel();
            //this.Log(checkerNextLevel);

            if (checkerNextLevel != "Main Scene")
            {
                PlayerUIManager.Instance.EndScreen_NextLevelBtn.SetActive(true);
                PlayerUIManager.Instance.EndScreen_NextLevelBtn.GetComponent<Button>().onClick.AddListener(delegate () { LoadingScreen.instance.loadingScreen(checkerNextLevel); });
            }
            else { PlayerUIManager.Instance.EndScreen_NextLevelBtn.SetActive(false); }


            // Disable & Enable Objects
            foreach (Transform child in player.transform)
            {
                if (child.tag != "MainCamera") child.gameObject.SetActive(false);
                if (child.name == "Main Panels") child.gameObject.SetActive(true);
            }
        }

        public void BackToMenu()
        {
            LoadingScreen.instance.loadingScreen("Main Scene");
        }

        void GenerateTrophy(Trophy trophy)
        {
            switch (trophy.GetCount())
            {
                case 1:
                    PlayerUIManager.Instance.EndScreen_TrophyTxt.ShowText.key = "bronze";
                    break;
                case 2:
                    PlayerUIManager.Instance.EndScreen_TrophyTxt.ShowText.key = "silver";
                    break;
                case 3:
                    PlayerUIManager.Instance.EndScreen_TrophyTxt.ShowText.key = "gold";
                    break;
                case 4:
                    PlayerUIManager.Instance.EndScreen_TrophyTxt.ShowText.key = "platinium";
                    break;
                case 5:
                    PlayerUIManager.Instance.EndScreen_TrophyTxt.ShowText.key = "diamond";
                    break;
                default:
                    PlayerUIManager.Instance.EndScreen_TrophyTxt.ShowText.key = "none";
                    break;
            }
        }

        bool CheckTrophyRequirement(Transform _element, TrophyRequirementClass _requirement)
        {
            switch (_requirement.RequirementType)
            {
                case TrophyRequirement.CompleteLevel:
                    //_element.gameObject.SetActive(false);
                    return true;

                case TrophyRequirement.ItemsPercent:
                    _element.gameObject.SetActive(true);
                    _element.Find("Name").GetComponent<TMPTextStringLocalization>().ShowText.key = "items";
                    _element.Find("Value").GetComponent<TMP_Text>().text = $"{Mathf.Floor((float)foundItems / Items.Count * 100)}% / {_requirement.RequirementValue}%";
                    if (Items.Count == 0) { _element.Find("Value").GetComponent<TMP_Text>().text = $"100% / {_requirement.RequirementValue}%"; return true; }
                    if (Mathf.Floor((float)foundItems / Items.Count * 100) >= _requirement.RequirementValue)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case TrophyRequirement.Items:
                    _element.gameObject.SetActive(true);
                    _element.Find("Name").GetComponent<TMPTextStringLocalization>().ShowText.key = "items";
                    _element.Find("Value").GetComponent<TMP_Text>().text = $"{foundItems} / {_requirement.RequirementValue}";
                    if (foundItems >= _requirement.RequirementValue)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case TrophyRequirement.EnemyPercent:
                    _element.gameObject.SetActive(true);
                    _element.Find("Name").GetComponent<TMPTextStringLocalization>().ShowText.key = "enemy";
                    _element.Find("Value").GetComponent<TMP_Text>().text = $"{Mathf.Floor((float)foundEnemy / Enemy.Count * 100)}% / {_requirement.RequirementValue}%";
                    if (Enemy.Count == 0) { _element.Find("Value").GetComponent<TMP_Text>().text = $"100% / {_requirement.RequirementValue}%"; return true; }
                    if (Mathf.Floor((float)foundEnemy / Enemy.Count * 100) >= _requirement.RequirementValue)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case TrophyRequirement.Enemy:
                    _element.gameObject.SetActive(true);
                    _element.Find("Name").GetComponent<TMPTextStringLocalization>().ShowText.key = "enemy";
                    _element.Find("Value").GetComponent<TMP_Text>().text = $"{foundEnemy}% / {_requirement.RequirementValue}";
                    if (foundEnemy >= _requirement.RequirementValue)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case TrophyRequirement.SecretsPercent:
                    _element.gameObject.SetActive(true);
                    _element.Find("Name").GetComponent<TMPTextStringLocalization>().ShowText.key = "secrets";
                    _element.Find("Value").GetComponent<TMP_Text>().text = $"{Mathf.Floor((float)foundSecret / SecretRoom.Count * 100)}% / {_requirement.RequirementValue}%";
                    if (SecretRoom.Count == 0) { _element.Find("Value").GetComponent<TMP_Text>().text = $"100% / {_requirement.RequirementValue}%"; return true; }
                    if (Mathf.Floor((float)foundSecret / SecretRoom.Count * 100) >= _requirement.RequirementValue)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case TrophyRequirement.Secrets:
                    _element.gameObject.SetActive(true);
                    _element.Find("Name").GetComponent<TMPTextStringLocalization>().ShowText.key = "secrets";
                    _element.Find("Value").GetComponent<TMP_Text>().text = $"{foundSecret}% / {_requirement.RequirementValue}";
                    if (foundSecret >= _requirement.RequirementValue)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case TrophyRequirement.MinimumScore:
                    _element.gameObject.SetActive(true);
                    _element.Find("Name").GetComponent<TMPTextStringLocalization>().ShowText.key = "score";
                    _element.Find("Value").GetComponent<TMP_Text>().text = $"{GameManager.Instance.TotalPoints} / {_requirement.RequirementValue}";
                    if (GameManager.Instance.TotalPoints >= _requirement.RequirementValue)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case TrophyRequirement.MinimumWave:
                    _element.gameObject.SetActive(true);
                    _element.Find("Name").GetComponent<TMPTextStringLocalization>().ShowText.key = "wave";
                    _element.Find("Value").GetComponent<TMP_Text>().text = $"{WaveLevel} / {_requirement.RequirementValue}";
                    if (WaveLevel >= _requirement.RequirementValue)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case TrophyRequirement.MoreTimeThan:
                    _element.gameObject.SetActive(true);
                    _element.Find("Name").GetComponent<TMPTextStringLocalization>().ShowText.key = "survivedTime";
                    _element.Find("Value").GetComponent<TMP_Text>().text = $"{Mathf.Floor(timer / 60).ToString("00") + ":" + (timer % 60).ToString("00")} > {Mathf.Floor(_requirement.RequirementValue / 60).ToString("00") + ":" + (_requirement.RequirementValue % 60).ToString("00")}";
                    if (timer >= _requirement.RequirementValue)
                    {
                        return true;
                    }
                    else
                    {
                        _element.Find("Value").GetComponent<TMP_Text>().text = $"{Mathf.Floor(timer / 60).ToString("00") + ":" + (timer % 60).ToString("00")} > {Mathf.Floor(_requirement.RequirementValue / 60).ToString("00") + ":" + (_requirement.RequirementValue % 60).ToString("00")}";
                        return false;
                    }

                case TrophyRequirement.LessTimeThan:
                    _element.gameObject.SetActive(true);
                    _element.Find("Name").GetComponent<TMPTextStringLocalization>().ShowText.key = "time";
                    _element.Find("Value").GetComponent<TMP_Text>().text = $"{Mathf.Floor(timer / 60).ToString("00") + ":" + (timer % 60).ToString("00")} < {Mathf.Floor(_requirement.RequirementValue / 60).ToString("00") + ":" + (_requirement.RequirementValue % 60).ToString("00")}";
                    if (timer <= _requirement.RequirementValue)
                    {
                        return true;
                    }
                    else
                    {
                        _element.Find("Value").GetComponent<TMP_Text>().text = $"<s>{Mathf.Floor(timer / 60).ToString("00") + ":" + (timer % 60).ToString("00")} < {Mathf.Floor(_requirement.RequirementValue / 60).ToString("00") + ":" + (_requirement.RequirementValue % 60).ToString("00")}";
                        return false;
                    }

                default:
                    _element.gameObject.SetActive(false);
                    return false;
            }
        }

        #endregion


    }

    #region Trophy

    [System.Serializable]
    public class Trophy
    {
        public bool Requirement_1, Requirement_2, Requirement_3, Requirement_4, Requirement_5;

        public int GetCount()
        {
            int _tmp = 0;

            if (Requirement_1) _tmp++;
            if (Requirement_2) _tmp++;
            if (Requirement_3) _tmp++;
            if (Requirement_4) _tmp++;
            if (Requirement_5) _tmp++;

            return _tmp;
        }

        public void Dev_Check_Requirements()
        {
            //Debug.Log($"{Requirement_1}, {Requirement_2}, {Requirement_3}, {Requirement_4}, {Requirement_5}");
        }
    }

    [System.Serializable]
    public enum GameModes
    {
        StoryMode = -1,
        Classic = 0,
        DeadlyRace = 1,
        WaveMode = 2,
        ArcadeMode = 3,
        Labolatory = 4
    }

    [System.Serializable]
    public class GameModeManager
    {
        public GameModes GameMode;
        // Trophy Classic
        public TrophyRequirementClass ClassicTrophy1 = new TrophyRequirementClass(TrophyRequirement.CompleteLevel, 0);
        public TrophyRequirementClass ClassicTrophy2 = new TrophyRequirementClass(TrophyRequirement.EnemyPercent, 100);
        public TrophyRequirementClass ClassicTrophy3 = new TrophyRequirementClass(TrophyRequirement.ItemsPercent, 100);
        public TrophyRequirementClass ClassicTrophy4 = new TrophyRequirementClass(TrophyRequirement.SecretsPercent, 100);
        public TrophyRequirementClass ClassicTrophy5 = new TrophyRequirementClass(TrophyRequirement.LessTimeThan, 360);
        // Trophy Deadly Race
        public TrophyRequirementClass DeadlyRaceTrophy1 = new TrophyRequirementClass(TrophyRequirement.MoreTimeThan, 120);
        public TrophyRequirementClass DeadlyRaceTrophy2 = new TrophyRequirementClass(TrophyRequirement.MoreTimeThan, 180);
        public TrophyRequirementClass DeadlyRaceTrophy3 = new TrophyRequirementClass(TrophyRequirement.MoreTimeThan, 300);
        public TrophyRequirementClass DeadlyRaceTrophy4 = new TrophyRequirementClass(TrophyRequirement.MoreTimeThan, 600);
        public TrophyRequirementClass DeadlyRaceTrophy5 = new TrophyRequirementClass(TrophyRequirement.MoreTimeThan, 1200);
        // Trophy Wave Mode
        public TrophyRequirementClass WaveTrophy1 = new TrophyRequirementClass(TrophyRequirement.MinimumWave, 5);
        public TrophyRequirementClass WaveTrophy2 = new TrophyRequirementClass(TrophyRequirement.MinimumWave, 10);
        public TrophyRequirementClass WaveTrophy3 = new TrophyRequirementClass(TrophyRequirement.MinimumWave, 20);
        public TrophyRequirementClass WaveTrophy4 = new TrophyRequirementClass(TrophyRequirement.MinimumWave, 35);
        public TrophyRequirementClass WaveTrophy5 = new TrophyRequirementClass(TrophyRequirement.MinimumWave, 50);
        // Trophy Arcade Mode
        public TrophyRequirementClass ArcadeModeTrophy1 = new TrophyRequirementClass(TrophyRequirement.MinimumScore, 5000);
        public TrophyRequirementClass ArcadeModeTrophy2 = new TrophyRequirementClass(TrophyRequirement.MinimumScore, 1000);
        public TrophyRequirementClass ArcadeModeTrophy3 = new TrophyRequirementClass(TrophyRequirement.MinimumScore, 5000);
        public TrophyRequirementClass ArcadeModeTrophy4 = new TrophyRequirementClass(TrophyRequirement.MinimumScore, 10000);
        public TrophyRequirementClass ArcadeModeTrophy5 = new TrophyRequirementClass(TrophyRequirement.MinimumScore, 25000);

        public TrophyRequirementClass GetRequirement(int index)
        {
            switch (GameMode)
            {
                case GameModes.DeadlyRace:
                    if (index == 1) return DeadlyRaceTrophy1;
                    else if (index == 2) return DeadlyRaceTrophy2;
                    else if (index == 3) return DeadlyRaceTrophy3;
                    else if (index == 4) return DeadlyRaceTrophy4;
                    else return DeadlyRaceTrophy5;

                case GameModes.WaveMode:
                    if (index == 1) return WaveTrophy1;
                    else if (index == 2) return WaveTrophy2;
                    else if (index == 3) return WaveTrophy3;
                    else if (index == 4) return WaveTrophy4;
                    else return WaveTrophy5;

                case GameModes.ArcadeMode:
                    if (index == 1) return ArcadeModeTrophy1;
                    else if (index == 2) return ArcadeModeTrophy2;
                    else if (index == 3) return ArcadeModeTrophy3;
                    else if (index == 4) return ArcadeModeTrophy4;
                    else return ArcadeModeTrophy5;

                default:
                    if (index == 1) return ClassicTrophy1;
                    else if (index == 2) return ClassicTrophy2;
                    else if (index == 3) return ClassicTrophy3;
                    else if (index == 4) return ClassicTrophy4;
                    else return ClassicTrophy5;
            }
        }

        public string GetTrophyName(int index)
        {
            switch (index)
            {
                case 1:
                    return "bronze";
                case 2:
                    return "silver";
                case 3:
                    return "gold";
                case 4:
                    return "platinium";
                case 5:
                    return "diamond";
                default:
                    return "none";
            }
        }
    }

    [System.Serializable]
    public class TrophyRequirementClass
    {
        public TrophyRequirementClass(TrophyRequirement type, float value)
        {
            RequirementType = type;
            RequirementValue = value;
        }

        public TrophyRequirement RequirementType;
        public float RequirementValue;
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(GameModeManager))]
    public class GameModeManagerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);


            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.PropertyField(property.FindPropertyRelative("GameMode"), new GUIContent("Level Game Mode"));
            EditorGUILayout.BeginVertical("helpbox");
            if (property.FindPropertyRelative("GameMode").enumValueIndex == 0 || property.FindPropertyRelative("GameMode").enumValueIndex == 1)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("ClassicTrophy1"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("ClassicTrophy2"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("ClassicTrophy3"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("ClassicTrophy4"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("ClassicTrophy5"));
            }
            if (property.FindPropertyRelative("GameMode").enumValueIndex == 2)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("DeadlyRaceTrophy1"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("DeadlyRaceTrophy2"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("DeadlyRaceTrophy3"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("DeadlyRaceTrophy4"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("DeadlyRaceTrophy5"));
            }
            if (property.FindPropertyRelative("GameMode").enumValueIndex == 3)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("WaveTrophy1"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("WaveTrophy2"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("WaveTrophy3"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("WaveTrophy4"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("WaveTrophy5"));
            }
            if (property.FindPropertyRelative("GameMode").enumValueIndex == 4)
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("ArcadeModeTrophy1"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("ArcadeModeTrophy2"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("ArcadeModeTrophy3"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("ArcadeModeTrophy4"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("ArcadeModeTrophy5"));
            }
            if (property.FindPropertyRelative("GameMode").enumValueIndex == 5)
            {
                TinyGUI.InfoBox("\nLabolatory Gamemome doesn't have a trophy\n", "PhysicsRaycaster Icon");
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0;
        }
    }

    [CustomPropertyDrawer(typeof(TrophyRequirementClass))]
    public class TrophyRequirementClassDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (property.FindPropertyRelative("RequirementType").enumValueIndex == 0)
            {
                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, 18), property.FindPropertyRelative("RequirementType"), new GUIContent($"{label.text}"));
            }
            else
            {
                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width - 67, 18), property.FindPropertyRelative("RequirementType"), new GUIContent($"{label.text}"));
                EditorGUI.PropertyField(new Rect(position.x + position.width - 65, position.y, 65, 18), property.FindPropertyRelative("RequirementValue"), GUIContent.none);
            }
            EditorGUI.EndProperty();
        }
    }

#endif

    public enum TrophyRequirement
    {
        CompleteLevel = 0,
        ItemsPercent = 1,
        EnemyPercent = 2,
        SecretsPercent = 3,
        LessTimeThan = 7,
        MoreTimeThan = 8,
        MinimumScore = 9,
        MinimumWave = 10,
        Items = 4,
        Enemy = 5,
        Secrets = 6
    }

    #endregion

    /// Custom Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(LevelManager))]
    public class LevelManagerEditor : Editor
    {
        public bool CommonEnemy, UncommonEnemy, RareEnemy, BossEnemy, StartObject, EnemyChange, ArcadeMode_Start;
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            LevelManager manager = (LevelManager)target;
            TinyGUI.DrawScript<LevelManager>("Level Manager Script", target);

            EditorGUI.indentLevel = 0;

            if (!Application.isPlaying)
            {
                if (manager.PrimaryWeapon.Count < 9) { manager.PrimaryWeapon.Add(null); }
                if (manager.SecoundaryWeapon.Count < 9) { manager.SecoundaryWeapon.Add(null); }
                if (manager.MelleWeapon.Count < 9) { manager.MelleWeapon.Add(null); }
                if (manager.ThrowableWeapon.Count < 9) { manager.ThrowableWeapon.Add(null); }
            }

            TinyGUI.BeginBoxGroup("Campaing Scriptable Object");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("campaign"), new GUIContent("Campaign"));
            if (TinyGUI.IconButton("SceneLoadIn", " Load", GUILayout.Width(EditorGUIUtility.singleLineHeight * 5)))
            {
                TinyDebug.Info("Coming Soon");
            }
            EditorGUILayout.EndHorizontal();
            TinyGUI.EndBoxGroup();

            TinyGUI.BeginBoxGroup("Gamemode Settings", "& Trophy Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Gamemode"), new GUIContent("Gamemode"));
            TinyGUI.EndBoxGroup();

            TinyGUI.BeginBoxGroup("Map Start Layout", "W Story mode dotyczy tylko pierwszej mapy");
            GameSettingsManger _gm = Resources.Load("Game_Settings") as GameSettingsManger;
            if (_gm.inventoryType == InventoryType.Normal)
            {
                EditorGUILayout.LabelField("Primart Slot", EditorStyles.miniBoldLabel);
                for (int i = 0; i < manager.PrimaryWeapon.Count; i++)
                {
                    if (i < _gm.PrimaryWeaponCount) EditorGUILayout.PropertyField(serializedObject.FindProperty("PrimaryWeapon").GetArrayElementAtIndex(i), new GUIContent($"Primary Slot {(i + 1)}"));
                }
                EditorGUILayout.LabelField("Secondary Slot", EditorStyles.miniBoldLabel);
                for (int i = 0; i < manager.SecoundaryWeapon.Count; i++)
                {
                    if (i < _gm.SecoundaryWeaponCount) EditorGUILayout.PropertyField(serializedObject.FindProperty("SecoundaryWeapon").GetArrayElementAtIndex(i), new GUIContent($"Secoundary Slot {(i + 1)}"));
                }
                EditorGUILayout.LabelField("Melle Slot", EditorStyles.miniBoldLabel);
                for (int i = 0; i < manager.MelleWeapon.Count; i++)
                {
                    if (i < _gm.MelleWeaponCount) EditorGUILayout.PropertyField(serializedObject.FindProperty("MelleWeapon").GetArrayElementAtIndex(i), new GUIContent($"Melle Slot {(i + 1)}"));
                }
                EditorGUILayout.LabelField("Thrownable Slot", EditorStyles.miniBoldLabel);
                for (int i = 0; i < manager.ThrowableWeapon.Count; i++)
                {
                    if (i < _gm.ThrowableWeaponCount) EditorGUILayout.PropertyField(serializedObject.FindProperty("ThrowableWeapon").GetArrayElementAtIndex(i), new GUIContent($"Throwable Slot {(i + 1)}"));
                }
            }
            else
            {
                EditorGUI.indentLevel = 1;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ClassicWeapon"), new GUIContent($"Start Weapon"));
                EditorGUI.indentLevel = 0;
            }
            TinyGUI.EndBoxGroup();

            if (manager.Gamemode.GameMode == GameModes.ArcadeMode)
            {
                TinyGUI.BeginBoxGroup("Arcade Mode Settings", "Desc");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ArcadeMode_StartItemCount"), new GUIContent("Start Item Count"));
                TinyGUI.ShowArray(serializedObject, "ArcadeMode_StartItemList", "Spawn Start Item", ref ArcadeMode_Start);
                TinyGUI.EndBoxGroup();
            }

            // Deadly Race & Wave Mode
            if (manager.Gamemode.GameMode == GameModes.DeadlyRace || manager.Gamemode.GameMode == GameModes.WaveMode)
            {
                TinyGUI.BeginBoxGroup("Deadly Race & Wave Mode Settings", "Desc");
                if (manager.Gamemode.GameMode == GameModes.WaveMode)
                {
                    TinyGUI.Guideline("Wave Info (Doesn't count Boss Wave)",
                        $"Wave 1:   Wave Size: {manager.WaveMode_WaveSpawnSize + (int)(manager.WaveMode_WaveSpawnSize * manager.WaveMode_SpawnMultiplier * 0)}",
                        $"Wave 5:   Wave Size  {manager.WaveMode_WaveSpawnSize + (int)(manager.WaveMode_WaveSpawnSize * manager.WaveMode_SpawnMultiplier * 4)}",
                        $"Wave 10:  Wave Size: {manager.WaveMode_WaveSpawnSize + (int)(manager.WaveMode_WaveSpawnSize * manager.WaveMode_SpawnMultiplier * 9)}",
                        $"Wave 25:  Wave Size: {manager.WaveMode_WaveSpawnSize + (int)(manager.WaveMode_WaveSpawnSize * manager.WaveMode_SpawnMultiplier * 24)}",
                        $"Wave 50:  Wave Size: {manager.WaveMode_WaveSpawnSize + (int)(manager.WaveMode_WaveSpawnSize * manager.WaveMode_SpawnMultiplier * 49)}",
                        $"Wave 100: Wave Size: {manager.WaveMode_WaveSpawnSize + (int)(manager.WaveMode_WaveSpawnSize * manager.WaveMode_SpawnMultiplier * 99)}");
                    TinyGUI.Guideline("Boss Wave",
                        $"Wave {manager.WaveMode_WaveBetweenBoss * 1}:    Old: {manager.WaveMode_WaveSpawnSize + (int)(manager.WaveMode_WaveSpawnSize * manager.WaveMode_SpawnMultiplier * (manager.WaveMode_WaveBetweenBoss * 1 - 1))}   |   New: Boss + {(int)((manager.WaveMode_WaveSpawnSize + (int)(manager.WaveMode_WaveSpawnSize * manager.WaveMode_SpawnMultiplier * (manager.WaveMode_WaveBetweenBoss * 1 - 1))) * manager.WaveMode_BossWaveMultiplier)} Enemy",
                        $"Wave {manager.WaveMode_WaveBetweenBoss * 2}:   Old: {manager.WaveMode_WaveSpawnSize + (int)(manager.WaveMode_WaveSpawnSize * manager.WaveMode_SpawnMultiplier * (manager.WaveMode_WaveBetweenBoss * 2 - 1))}   |   New: Boss + {(int)((manager.WaveMode_WaveSpawnSize + (int)(manager.WaveMode_WaveSpawnSize * manager.WaveMode_SpawnMultiplier * (manager.WaveMode_WaveBetweenBoss * 2 - 1))) * manager.WaveMode_BossWaveMultiplier)} Enemy",
                        $"Wave {manager.WaveMode_WaveBetweenBoss * 3}:   Old: {manager.WaveMode_WaveSpawnSize + (int)(manager.WaveMode_WaveSpawnSize * manager.WaveMode_SpawnMultiplier * (manager.WaveMode_WaveBetweenBoss * 3 - 1))}   |   New: Boss + {(int)((manager.WaveMode_WaveSpawnSize + (int)(manager.WaveMode_WaveSpawnSize * manager.WaveMode_SpawnMultiplier * (manager.WaveMode_WaveBetweenBoss * 3 - 1))) * manager.WaveMode_BossWaveMultiplier)} Enemy",
                        $"Wave {manager.WaveMode_WaveBetweenBoss * 4}:   Old: {manager.WaveMode_WaveSpawnSize + (int)(manager.WaveMode_WaveSpawnSize * manager.WaveMode_SpawnMultiplier * (manager.WaveMode_WaveBetweenBoss * 4 - 1))}   |   New: Boss + {(int)((manager.WaveMode_WaveSpawnSize + (int)(manager.WaveMode_WaveSpawnSize * manager.WaveMode_SpawnMultiplier * (manager.WaveMode_WaveBetweenBoss * 4 - 1))) * manager.WaveMode_BossWaveMultiplier)} Enemy",
                        $"Wave {manager.WaveMode_WaveBetweenBoss * 5}:   Old: {manager.WaveMode_WaveSpawnSize + (int)(manager.WaveMode_WaveSpawnSize * manager.WaveMode_SpawnMultiplier * (manager.WaveMode_WaveBetweenBoss * 5 - 1))}   |   New: Boss + {(int)((manager.WaveMode_WaveSpawnSize + (int)(manager.WaveMode_WaveSpawnSize * manager.WaveMode_SpawnMultiplier * (manager.WaveMode_WaveBetweenBoss * 5 - 1))) * manager.WaveMode_BossWaveMultiplier)} Enemy");

                    TinyGUI.BeginBoxGroup("Wave Settings");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("WaveMode_MaxWaveTime"), new GUIContent("Max Wave Time"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("WaveMode_WaveSpawnSize"), new GUIContent("Start Wave Size (Wave 1)"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("WaveMode_SpawnMultiplier"), new GUIContent("Wave Size Multiplier"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("WaveMode_WaveBetweenBoss"), new GUIContent("Wave Between Boss Wave"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("WaveMode_BossWaveMultiplier"), new GUIContent("Boss Wave Size Multiplier"));
                    TinyGUI.EndBoxGroup();
                }
                if (manager.Gamemode.GameMode == GameModes.DeadlyRace)
                {
                    TinyGUI.Guideline("Spawn Info (Doesn't count Boss Spawn)",
                        $"Spawn 1:     Enemy: {manager.DeadlyRaceMode_EnemySpawnSize.x + (int)(manager.DeadlyRaceMode_EnemySpawnSize.x * manager.DeadlyRaceMode_MultiplierSpawn * 0)}-{manager.DeadlyRaceMode_EnemySpawnSize.y + (int)(manager.DeadlyRaceMode_EnemySpawnSize.y * manager.DeadlyRaceMode_MultiplierSpawn * 0)}",
                        $"Spawn 5:     Enemy: {manager.DeadlyRaceMode_EnemySpawnSize.x + (int)(manager.DeadlyRaceMode_EnemySpawnSize.x * manager.DeadlyRaceMode_MultiplierSpawn * 4)}-{manager.DeadlyRaceMode_EnemySpawnSize.y + (int)(manager.DeadlyRaceMode_EnemySpawnSize.y * manager.DeadlyRaceMode_MultiplierSpawn * 4)}",
                        $"Spawn 10:    Enemy: {manager.DeadlyRaceMode_EnemySpawnSize.x + (int)(manager.DeadlyRaceMode_EnemySpawnSize.x * manager.DeadlyRaceMode_MultiplierSpawn * 9)}-{manager.DeadlyRaceMode_EnemySpawnSize.y + (int)(manager.DeadlyRaceMode_EnemySpawnSize.y * manager.DeadlyRaceMode_MultiplierSpawn * 9)}",
                        $"Spawn 25:    Enemy: {manager.DeadlyRaceMode_EnemySpawnSize.x + (int)(manager.DeadlyRaceMode_EnemySpawnSize.x * manager.DeadlyRaceMode_MultiplierSpawn * 24)}-{manager.DeadlyRaceMode_EnemySpawnSize.y + (int)(manager.DeadlyRaceMode_EnemySpawnSize.y * manager.DeadlyRaceMode_MultiplierSpawn * 24)}",
                        $"Spawn 50:    Enemy: {manager.DeadlyRaceMode_EnemySpawnSize.x + (int)(manager.DeadlyRaceMode_EnemySpawnSize.x * manager.DeadlyRaceMode_MultiplierSpawn * 49)}-{manager.DeadlyRaceMode_EnemySpawnSize.y + (int)(manager.DeadlyRaceMode_EnemySpawnSize.y * manager.DeadlyRaceMode_MultiplierSpawn * 49)}",
                        $"Spawn 100:   Enemy: {manager.DeadlyRaceMode_EnemySpawnSize.x + (int)(manager.DeadlyRaceMode_EnemySpawnSize.x * manager.DeadlyRaceMode_MultiplierSpawn * 99)}-{manager.DeadlyRaceMode_EnemySpawnSize.y + (int)(manager.DeadlyRaceMode_EnemySpawnSize.y * manager.DeadlyRaceMode_MultiplierSpawn * 99)}");

                    TinyGUI.Guideline("Boss Spawn",
                        $"Spawn {manager.DeadlyRaceMode_SpawnBetweenBoss * 1}:   Old: {manager.DeadlyRaceMode_EnemySpawnSize.x + (int)(manager.DeadlyRaceMode_EnemySpawnSize.x * manager.DeadlyRaceMode_MultiplierSpawn * (manager.DeadlyRaceMode_SpawnBetweenBoss * 1 - 1))}-{manager.DeadlyRaceMode_EnemySpawnSize.y + (int)(manager.DeadlyRaceMode_EnemySpawnSize.y * manager.DeadlyRaceMode_MultiplierSpawn * (manager.DeadlyRaceMode_SpawnBetweenBoss * 1 - 1))}  |   New: Boss + {(int)((manager.DeadlyRaceMode_EnemySpawnSize.x + (int)(manager.DeadlyRaceMode_EnemySpawnSize.x * manager.DeadlyRaceMode_MultiplierSpawn * (manager.DeadlyRaceMode_SpawnBetweenBoss * 1 - 1))) * manager.DeadlyRaceMode_BossSpawnMultiplier)}-{(int)((manager.DeadlyRaceMode_EnemySpawnSize.y + (int)(manager.DeadlyRaceMode_EnemySpawnSize.y * manager.DeadlyRaceMode_MultiplierSpawn * (manager.DeadlyRaceMode_SpawnBetweenBoss * 1 - 1))) * manager.DeadlyRaceMode_BossSpawnMultiplier)} Enemy",
                        $"Spawn {manager.DeadlyRaceMode_SpawnBetweenBoss * 2}:   Old: {manager.DeadlyRaceMode_EnemySpawnSize.x + (int)(manager.DeadlyRaceMode_EnemySpawnSize.x * manager.DeadlyRaceMode_MultiplierSpawn * (manager.DeadlyRaceMode_SpawnBetweenBoss * 2 - 1))}-{manager.DeadlyRaceMode_EnemySpawnSize.y + (int)(manager.DeadlyRaceMode_EnemySpawnSize.y * manager.DeadlyRaceMode_MultiplierSpawn * (manager.DeadlyRaceMode_SpawnBetweenBoss * 2 - 1))}  |   New: Boss + {(int)((manager.DeadlyRaceMode_EnemySpawnSize.x + (int)(manager.DeadlyRaceMode_EnemySpawnSize.x * manager.DeadlyRaceMode_MultiplierSpawn * (manager.DeadlyRaceMode_SpawnBetweenBoss * 2 - 1))) * manager.DeadlyRaceMode_BossSpawnMultiplier)}-{(int)((manager.DeadlyRaceMode_EnemySpawnSize.y + (int)(manager.DeadlyRaceMode_EnemySpawnSize.y * manager.DeadlyRaceMode_MultiplierSpawn * (manager.DeadlyRaceMode_SpawnBetweenBoss * 2 - 1))) * manager.DeadlyRaceMode_BossSpawnMultiplier)} Enemy",
                        $"Spawn {manager.DeadlyRaceMode_SpawnBetweenBoss * 3}:   Old: {manager.DeadlyRaceMode_EnemySpawnSize.x + (int)(manager.DeadlyRaceMode_EnemySpawnSize.x * manager.DeadlyRaceMode_MultiplierSpawn * (manager.DeadlyRaceMode_SpawnBetweenBoss * 3 - 1))}-{manager.DeadlyRaceMode_EnemySpawnSize.y + (int)(manager.DeadlyRaceMode_EnemySpawnSize.y * manager.DeadlyRaceMode_MultiplierSpawn * (manager.DeadlyRaceMode_SpawnBetweenBoss * 3 - 1))}  |   New: Boss + {(int)((manager.DeadlyRaceMode_EnemySpawnSize.x + (int)(manager.DeadlyRaceMode_EnemySpawnSize.x * manager.DeadlyRaceMode_MultiplierSpawn * (manager.DeadlyRaceMode_SpawnBetweenBoss * 3 - 1))) * manager.DeadlyRaceMode_BossSpawnMultiplier)}-{(int)((manager.DeadlyRaceMode_EnemySpawnSize.y + (int)(manager.DeadlyRaceMode_EnemySpawnSize.y * manager.DeadlyRaceMode_MultiplierSpawn * (manager.DeadlyRaceMode_SpawnBetweenBoss * 3 - 1))) * manager.DeadlyRaceMode_BossSpawnMultiplier)} Enemy",
                        $"Spawn {manager.DeadlyRaceMode_SpawnBetweenBoss * 4}:   Old: {manager.DeadlyRaceMode_EnemySpawnSize.x + (int)(manager.DeadlyRaceMode_EnemySpawnSize.x * manager.DeadlyRaceMode_MultiplierSpawn * (manager.DeadlyRaceMode_SpawnBetweenBoss * 4 - 1))}-{manager.DeadlyRaceMode_EnemySpawnSize.y + (int)(manager.DeadlyRaceMode_EnemySpawnSize.y * manager.DeadlyRaceMode_MultiplierSpawn * (manager.DeadlyRaceMode_SpawnBetweenBoss * 4 - 1))}  |   New: Boss + {(int)((manager.DeadlyRaceMode_EnemySpawnSize.x + (int)(manager.DeadlyRaceMode_EnemySpawnSize.x * manager.DeadlyRaceMode_MultiplierSpawn * (manager.DeadlyRaceMode_SpawnBetweenBoss * 4 - 1))) * manager.DeadlyRaceMode_BossSpawnMultiplier)}-{(int)((manager.DeadlyRaceMode_EnemySpawnSize.y + (int)(manager.DeadlyRaceMode_EnemySpawnSize.y * manager.DeadlyRaceMode_MultiplierSpawn * (manager.DeadlyRaceMode_SpawnBetweenBoss * 4 - 1))) * manager.DeadlyRaceMode_BossSpawnMultiplier)} Enemy",
                        $"Spawn {manager.DeadlyRaceMode_SpawnBetweenBoss * 5}:   Old: {manager.DeadlyRaceMode_EnemySpawnSize.x + (int)(manager.DeadlyRaceMode_EnemySpawnSize.x * manager.DeadlyRaceMode_MultiplierSpawn * (manager.DeadlyRaceMode_SpawnBetweenBoss * 5 - 1))}-{manager.DeadlyRaceMode_EnemySpawnSize.y + (int)(manager.DeadlyRaceMode_EnemySpawnSize.y * manager.DeadlyRaceMode_MultiplierSpawn * (manager.DeadlyRaceMode_SpawnBetweenBoss * 5 - 1))}  |   New: Boss + {(int)((manager.DeadlyRaceMode_EnemySpawnSize.x + (int)(manager.DeadlyRaceMode_EnemySpawnSize.x * manager.DeadlyRaceMode_MultiplierSpawn * (manager.DeadlyRaceMode_SpawnBetweenBoss * 5 - 1))) * manager.DeadlyRaceMode_BossSpawnMultiplier)}-{(int)((manager.DeadlyRaceMode_EnemySpawnSize.y + (int)(manager.DeadlyRaceMode_EnemySpawnSize.y * manager.DeadlyRaceMode_MultiplierSpawn * (manager.DeadlyRaceMode_SpawnBetweenBoss * 5 - 1))) * manager.DeadlyRaceMode_BossSpawnMultiplier)} Enemy");

                    TinyGUI.BeginBoxGroup("Deadly Race Settings");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DeadlyRaceMode_EnemySpawnRate"), new GUIContent("Enemy Spawn Rate"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DeadlyRaceMode_EnemySpawnSize"), new GUIContent("Enemy Spawn Size"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DeadlyRaceMode_MultiplierSpawn"), new GUIContent("Spawn Size Multiplier"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DeadlyRaceMode_SpawnBetweenBoss"), new GUIContent("Spawn Between Boss"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DeadlyRaceMode_BossSpawnMultiplier"), new GUIContent("Boss Spawn Multiplier"));
                    TinyGUI.EndBoxGroup();
                }

                TinyGUI.BeginBoxGroup("Enemy Settings");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("EnemySpawnRadius"), new GUIContent("Spawn Radius from Player"));
                TinyGUI.ShowArray(serializedObject, "SpawnChangePercent", "Spawn Change Percent", ref EnemyChange);
                TinyGUI.Title("Enemy Spawn Category");
                TinyGUI.ShowArray(serializedObject, "CommonEnemyList", "Common Enemy List", ref CommonEnemy);
                TinyGUI.ShowArray(serializedObject, "UncommonEnemyList", "Uncommon Enemy List", ref UncommonEnemy);
                TinyGUI.ShowArray(serializedObject, "RareEnemyList", "Rare Enemy List", ref RareEnemy);
                TinyGUI.ShowArray(serializedObject, "BossEnemyList", "Boss Enemy List", ref BossEnemy);
                TinyGUI.EndBoxGroup();


                TinyGUI.BeginBoxGroup("Spawn Object Settings");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("StartObjectSpawnCount"), new GUIContent("Start Object Spawn Count"));
                TinyGUI.ShowArray(serializedObject, "StartObjectSpawnList", "Start Object Spawn List", ref StartObject);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BonusWaveItemSpawnRange"), new GUIContent("Bonus Wave Item Spawn Range"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BonusWaveItemDrop"), new GUIContent("Bonus Wave Item Drop"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BonudWaveItemChange"), new GUIContent("Bonus Wave Item Change"));
                TinyGUI.EndBoxGroup();
                TinyGUI.EndBoxGroup();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomPropertyDrawer(typeof(EnemyChangeStruct))]
    public class EnemyChangeStructDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            Rect waveRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Rect label1 = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width / 3, EditorGUIUtility.singleLineHeight);
            Rect label2 = new Rect(position.x + (position.width / 3), position.y + EditorGUIUtility.singleLineHeight, position.width / 3, EditorGUIUtility.singleLineHeight);
            Rect label3 = new Rect(position.x + ((position.width / 3) * 2), position.y + EditorGUIUtility.singleLineHeight, position.width / 3, EditorGUIUtility.singleLineHeight);
            Rect sliderRect = new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight * 2), position.width, EditorGUIUtility.singleLineHeight);

            // Draw fields - you might want to customize these based on your needs
            EditorGUI.PropertyField(waveRect, property.FindPropertyRelative("WaveNumber"), new GUIContent("From the Wave"));

            float x = property.FindPropertyRelative("CategorySeparator").vector2Value.x;
            float y = property.FindPropertyRelative("CategorySeparator").vector2Value.y;
            float percentCom = x * 100;
            float percentUncom = (y - x) * 100;
            float percentRare = (1 - y) * 100;

            EditorGUI.LabelField(label1, new GUIContent($"Common [{percentCom.ToString("00.0")}%]"), EditorStyles.centeredGreyMiniLabel);
            EditorGUI.LabelField(label2, new GUIContent($"Uncommon [{percentUncom.ToString("00.0")}%]"), EditorStyles.centeredGreyMiniLabel);
            EditorGUI.LabelField(label3, new GUIContent($"Rare [{percentRare.ToString("00.0")}%]"), EditorStyles.centeredGreyMiniLabel);

            EditorGUI.MinMaxSlider(sliderRect, ref x, ref y, 0, 1);
            property.FindPropertyRelative("CategorySeparator").vector2Value = new Vector2(x, y);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3;
        }
    }

#endif


    [System.Serializable]
    public class EnemyChangeStruct
    {
        public int WaveNumber;
        public Vector2 CategorySeparator;
    }
}