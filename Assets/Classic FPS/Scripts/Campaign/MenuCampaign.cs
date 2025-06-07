using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HellishBattle.Level;
using HellishBattle.SaveSystem;

namespace HellishBattle
{
    namespace Campaign
    {
        public class MenuCampaign : MonoBehaviour
        {
            [Line("Base")]
            public LevelCategory category;
            public CampaignManager campaign;

            [Line("Select Gamemode")]
            public GameObject GamemodeContainer;
            public GameObject GamemodePrefab;
            public GameObject ChapterPanel;

            [Line("Chapter  ")]
            public GameObject ChapterContainer;
            public GameObject ChapterPrefab;

            [Line("Level Selector")]
            public GameObject LevelContainer;
            public GameObject LevelPanel;
            public GameObject LevelPrefab;

            int _selectedGamemode;

            [ShowIf("category", (int)LevelCategory.StoryMode)] public StringLocalization TrophyLocalization;
            [ShowIf("category", (int)LevelCategory.StoryMode)] public GameObject LoadBtn;
            [ShowIf("category", (int)LevelCategory.StoryMode)] public GameObject SelectLevelBtn;
            [ShowIf("category", (int)LevelCategory.StoryMode)] public TMP_Text PlatiniumTxt;
            [ShowIf("category", (int)LevelCategory.StoryMode)] public TMP_Text DiamondTxt;
            [ShowIf("category", (int)LevelCategory.StoryMode)] public TMP_Text GoldTxt;
            [ShowIf("category", (int)LevelCategory.StoryMode)] public TMP_Text SilverTxt;
            [ShowIf("category", (int)LevelCategory.StoryMode)] public TMP_Text BronzeTxt;

            public void Start()
            {
                // Disable Panels
                LevelPanel.SetActive(false);
                if (category == LevelCategory.Gamemode) ChapterPanel.SetActive(false);
                // Reset Containers
                if(ChapterContainer != null) foreach (Transform child in ChapterContainer.transform) { GameObject.Destroy(child.gameObject); }
                if (LevelContainer != null) foreach (Transform child in LevelContainer.transform) { GameObject.Destroy(child.gameObject); }
                if (GamemodeContainer != null) foreach (Transform child in GamemodeContainer.transform) { GameObject.Destroy(child.gameObject); }

                // Generate Chapters
                if (category == LevelCategory.Chapter)
                {
                    for (int i = 0; i < campaign.chapter.Count; i++)
                    {
                        int j = i;
                        GameObject rocketInstantiated = (GameObject)Instantiate(ChapterPrefab, ChapterContainer.transform);
                        rocketInstantiated.GetComponent<TMPTextStringLocalization>().ShowText.localization = campaign.localization;
                        rocketInstantiated.GetComponent<TMPTextStringLocalization>().ShowText.key = campaign.chapter[i].key;

                        Button btn = rocketInstantiated.GetComponent<Button>();
                        btn.onClick.AddListener(delegate () { GenerateLevelList(j); });
                    }
                }

                // Generate Gamemodes
                else if (category == LevelCategory.Gamemode)
                {
                    /* Generate Gamemodes */
                    for (int i = 0; i < campaign.GameModes.Count; i++)
                    {
                        int index = i;
                        GameObject GamemodeBTN = (GameObject)Instantiate(GamemodePrefab, GamemodeContainer.transform);
                        GamemodeBTN.GetComponent<TMPTextStringLocalization>().ShowText.localization = campaign.localization;
                        GamemodeBTN.GetComponent<TMPTextStringLocalization>().ShowText.key = campaign.GameModes[i].key;

                        Button btn = GamemodeBTN.GetComponent<Button>();
                        btn.onClick.AddListener(delegate () { GameModesGenerateChapters(index); });
                    }
                }

                // Generate Story Mode
                else if (category == LevelCategory.StoryMode)
                {
                    for (int i = 0; i < campaign.storymode.Count; i++)
                    {
                        int j = i;
                        GameObject rocketInstantiated = (GameObject)Instantiate(ChapterPrefab, ChapterContainer.transform);
                        rocketInstantiated.GetComponent<TMPTextStringLocalization>().ShowText.localization = campaign.localization;
                        rocketInstantiated.GetComponent<TMPTextStringLocalization>().ShowText.key = campaign.storymode[i].key;

                        Button btn = rocketInstantiated.GetComponent<Button>();
                        btn.onClick.AddListener(delegate () { GenerateLevelList(j); });
                    }
                }
            }

            public void Update()
            {
                if (category == LevelCategory.StoryMode)
                {
                    LoadBtn.SetActive(TinySaveSystem.HasKey("StoryMode_Save"));
                    if (TinySaveSystem.GetPlayerSave("StoryMode_Save").Map == "Main Scene")
                    {
                        LoadBtn.SetActive(false);
                    }
                    SelectLevelBtn.SetActive(TinySaveSystem.HasKey("StoryMode_Save"));
                }
            }

            public void LoadPlayerSave()
            {
                PlayerSaveClass Save = TinySaveSystem.GetPlayerSave("StoryMode_Save");

                LoadingScreen.instance.loadingScreen(Save.Map);
            }

            public void NewPlayerSave()
            {
                for (int i = 0; i < TinySaveSystem.data.items.Count; i++)
                {
                    if (TinySaveSystem.data.items[i].Key == "StoryMode_Save")
                    {
                        TinySaveSystem.data.items.Remove(TinySaveSystem.data.items[i]);
                        TinySaveSystem.SaveToDisk();
                    }
                }
                LoadingScreen.instance.loadingScreen(campaign.storymode[0].Levels[0].sceneName);
            }

            /// <summary>
            /// Funkcja S³u¿y do generowania dostêpnych rozdzia³ów dla wybranego Gamemode
            /// </summary>
            /// <param name="mode"></param>
            public void GameModesGenerateChapters(int mode)
            {
                if (category == LevelCategory.Gamemode)
                {
                    // Formatting
                    ChapterPanel.SetActive(true);
                    foreach (Transform child in ChapterContainer.transform) { GameObject.Destroy(child.gameObject); }
                    foreach (Transform child in LevelContainer.transform) { GameObject.Destroy(child.gameObject); }

                    _selectedGamemode = mode;

                    for (int i = 0; i < campaign.GameModes[mode].Chapters.Count; i++)
                    {
                        int j = i;
                        GameObject rocketInstantiated = (GameObject)Instantiate(ChapterPrefab, ChapterContainer.transform);
                        rocketInstantiated.GetComponent<TMPTextStringLocalization>().ShowText.localization = campaign.localization;
                        rocketInstantiated.GetComponent<TMPTextStringLocalization>().ShowText.key = campaign.GameModes[mode].Chapters[i].key;

                        Button btn = rocketInstantiated.GetComponent<Button>();
                        btn.onClick.AddListener(delegate () { GenerateLevelList(j); });
                    }
                }
            }

            public void GenerateLevelList(int chapterID)
            {
                foreach (Transform child in LevelContainer.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }

                // Change Prefab Save
                if (category == LevelCategory.Chapter)
                {
                    for (int i = 0; i < campaign.chapter[chapterID].Levels.Count; i++)
                    {
                        int j = i;
                        GameObject rocketInstantiated = (GameObject)Instantiate(LevelPrefab, LevelContainer.transform);
                        rocketInstantiated.transform.Find("name_txt").GetComponent<TMPTextStringLocalization>().ShowText.key = campaign.chapter[chapterID].Levels[i].key;
                        Trophy _tmpTrophy = TinySaveSystem.GetTrophy(campaign.chapter[chapterID].Levels[i].sceneName);
                        rocketInstantiated.transform.Find("trophy_txt").GetComponent<TMPTextStringLocalization>().ShowText.key = GenerateTrophy(_tmpTrophy);

                        Button btn = rocketInstantiated.GetComponent<Button>();
                        btn.onClick.AddListener(delegate () { OpenLevel(campaign.chapter[chapterID].Levels[j].sceneName); });
                    }
                }

                else if (category == LevelCategory.Gamemode)
                {
                    for (int i = 0; i < campaign.GameModes[_selectedGamemode].Chapters[chapterID].Levels.Count; i++)
                    {
                        int index = i;
                        GameObject rocketInstantiated = (GameObject)Instantiate(LevelPrefab, LevelContainer.transform);
                        rocketInstantiated.transform.Find("name_txt").GetComponent<TMPTextStringLocalization>().ShowText.key = campaign.GameModes[_selectedGamemode].Chapters[chapterID].Levels[i].key;
                        Trophy _tmpTrophy = TinySaveSystem.GetTrophy(campaign.GameModes[_selectedGamemode].Chapters[chapterID].Levels[i].sceneName);
                        rocketInstantiated.transform.Find("trophy_txt").GetComponent<TMPTextStringLocalization>().ShowText.key = GenerateTrophy(_tmpTrophy);

                        Button btn = rocketInstantiated.GetComponent<Button>();
                        btn.onClick.AddListener(delegate () { OpenLevel(campaign.GameModes[_selectedGamemode].Chapters[chapterID].Levels[index].sceneName); });
                    }
                }

                else if (category == LevelCategory.StoryMode)
                {
                    for (int i = 0; i < campaign.storymode[chapterID].Levels.Count; i++)
                    {
                        int j = i;
                        GameObject rocketInstantiated = (GameObject)Instantiate(LevelPrefab, LevelContainer.transform);
                        rocketInstantiated.transform.Find("name_txt").GetComponent<TMPTextStringLocalization>().ShowText.key = campaign.storymode[chapterID].Levels[i].key;
                        Trophy _tmpTrophy = TinySaveSystem.GetTrophy(campaign.storymode[chapterID].Levels[i].sceneName);
                        rocketInstantiated.transform.Find("trophy_txt").GetComponent<TMPTextStringLocalization>().ShowText.key = GenerateTrophy(_tmpTrophy);

                        Button btn = rocketInstantiated.GetComponent<Button>();
                        btn.onClick.AddListener(delegate () { OpenLevel(campaign.storymode[chapterID].Levels[j].sceneName); });
                    }
                }
                LevelPanel.SetActive(true);

                void OpenLevel(string name)
                {
                    LoadingScreen.instance.loadingScreen(name);
                }
            }

            string GenerateTrophy(Trophy trophy)
            {
                switch (trophy.GetCount())
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

            public void GenerateTrophyInfo()
            {
                int Platinium = 0;
                int Diamond = 0;
                int Gold = 0;
                int Silver = 0;
                int Bronze = 0;
                int All = 0;

                // Gen Number
                for (int chapter = 0; chapter < campaign.storymode.Count; chapter++)
                {
                    for (int level = 0; level < campaign.storymode[chapter].Levels.Count; level++)
                    {
                        Trophy _tmpTrophy = TinySaveSystem.GetTrophy(campaign.storymode[chapter].Levels[level].sceneName);
                        if (_tmpTrophy.GetCount() == 5) { Platinium++; }
                        if (_tmpTrophy.GetCount() == 4) { Diamond++; }
                        if (_tmpTrophy.GetCount() == 3) { Gold++; }
                        if (_tmpTrophy.GetCount() == 2) { Silver++; }
                        if (_tmpTrophy.GetCount() == 1) { Bronze++; }
                        All++;
                    }
                }

                // ShowText
                PlatiniumTxt.text = $"{TrophyLocalization.GetString("platinium")}: {Platinium}/{All}";
                DiamondTxt.text = $"{TrophyLocalization.GetString("diamond")}: {Diamond}/{All}";
                GoldTxt.text = $"{TrophyLocalization.GetString("gold")}: {Gold}/{All}";
                SilverTxt.text = $"{TrophyLocalization.GetString("silver")}: {Silver}/{All}";
                BronzeTxt.text = $"{TrophyLocalization.GetString("bronze")}: {Bronze}/{All}";
            }

            public enum LevelCategory
            {
                StoryMode = 2, Chapter = 0, Gamemode = 1
            }
        }

    }
}
