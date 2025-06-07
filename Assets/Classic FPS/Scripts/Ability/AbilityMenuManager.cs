using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HellishBattle.SaveSystem;

namespace HellishBattle.Ability
{
    public class AbilityMenuManager : MonoBehaviour
    {

        [Line("Prefabs")]
        public GameObject SkillPrefab;

        [Line("Ability")]
        public GameObject AbilityContainer;
        public GameObject AbilityWindow;
        public TMPTextStringLocalization SelectedAbilityLocalization;
        public Image AbilityIcon;

        [Line("Passive Skill")]
        public GameObject PassiveSkillContainer;
        public GameObject PassiveSkillWindow;
        public TMPTextStringLocalization SelectedPassiveSkillLocalization;
        public Image PassiveSkillIcon;

        [Line("Passive Skill")]
        public ReorderableList<TMP_Text> PointTexts;
        AbilityManager am;
        int selectedPassiveSkill = 0;
        int selectedAbility = 0;

        private void Awake()
        {
            am = (AbilityManager)Resources.Load("Ability");
            selectedPassiveSkill = TinySaveSystem.GetInt("selectedPassiveSkill");
            selectedAbility = TinySaveSystem.GetInt("selectedAbility");
            LocalizationUpdate();
        }

        public void LocalizationUpdate()
        {
            selectedPassiveSkill = TinySaveSystem.GetInt("selectedPassiveSkill");
            selectedAbility = TinySaveSystem.GetInt("selectedAbility");

            SelectedPassiveSkillLocalization.ShowText.localization = am.passiveAbilityList[selectedPassiveSkill].skill.Name.localization;
            SelectedPassiveSkillLocalization.ShowText.key = am.passiveAbilityList[selectedPassiveSkill].skill.Name.key;
            PassiveSkillIcon.sprite = am.passiveAbilityList[selectedPassiveSkill].skill.Icon;

            SelectedAbilityLocalization.ShowText.localization = am.AbilityList[selectedAbility].skill.Name.localization;
            SelectedAbilityLocalization.ShowText.key = am.AbilityList[selectedAbility].skill.Name.key;
            AbilityIcon.sprite = am.AbilityList[selectedAbility].skill.Icon;

            // 
            for (int i = 0; i < am.passiveAbilityList.Count; i++)
            {
                if (am.passiveAbilityList[i].UnlockedAtStart) TinySaveSystem.SetBool($"passive_{i}_unlock", true);
            }
            for (int i = 0; i < am.AbilityList.Count; i++)
            {
                if (am.AbilityList[i].UnlockedAtStart) TinySaveSystem.SetBool($"ability_{i}_unlock", true);
            }
        }

        private void Update()
        {
            //
            for (int i = 0; i < PointTexts.List.Count; i++)
            {
                PointTexts.List[i].text = $"{TinySaveSystem.GetInt("unlockable_point")} P";
            }
        }

        public void GeneratePassiveSkillLevel()
        {
            foreach (Transform child in PassiveSkillContainer.transform) { Destroy(child.gameObject); }
            for (int i = 0; i < am.passiveAbilityList.Count; i++)
            {
                int index = i;
                GameObject button = Instantiate(SkillPrefab, PassiveSkillContainer.transform);

                button.transform.Find("Name_Txt").GetComponent<TMP_Text>().text = am.passiveAbilityList[i].skill.Name.localization.GetString(am.passiveAbilityList[i].skill.Name.key);
                button.transform.Find("Desc_Txt").GetComponent<TMP_Text>().text = am.passiveAbilityList[i].skill.Description.localization.GetString(am.passiveAbilityList[i].skill.Description.key);
                button.transform.Find("Skill_Icon").GetComponent<Image>().sprite = am.passiveAbilityList[i].skill.Icon;

                if (TinySaveSystem.GetBool($"passive_{index}_unlock"))
                {
                    button.transform.Find("Pkt_Txt").GetComponent<TMP_Text>().text = "";
                    button.transform.Find("Skill_Icon").Find("Lock").GetComponent<Transform>().gameObject.SetActive(false);
                }
                else
                {
                    button.transform.Find("Pkt_Txt").GetComponent<TMP_Text>().text = am.passiveAbilityList[i].price + " p";
                    button.transform.Find("Skill_Icon").Find("Lock").GetComponent<Transform>().gameObject.SetActive(true);
                }

                button.transform.GetComponent<Button>().onClick.AddListener(() => ClickButton(index));

                void ClickButton(int id)
                {
                    if (TinySaveSystem.GetBool($"passive_{id}_unlock"))
                    {
                        TinySaveSystem.SetInt("selectedPassiveSkill", id);
                        PassiveSkillWindow.SetActive(false);
                        LocalizationUpdate();
                    }
                    else
                    {
                        if (TinySaveSystem.GetInt("unlockable_point") >= am.passiveAbilityList[id].price)
                        {
                            TinySaveSystem.SetBool($"passive_{id}_unlock", true);
                            TinySaveSystem.SetInt("unlockable_point", TinySaveSystem.GetInt("unlockable_point") - am.passiveAbilityList[id].price);
                            LocalizationUpdate();
                            GeneratePassiveSkillLevel();
                        }
                        else
                        {
                            TinyDebug.Warning("You need Points!");
                        }
                    }
                }
            }
        }
        public void GenerateSkillLevel()
        {
            foreach (Transform child in AbilityContainer.transform) { Destroy(child.gameObject); }
            for (int i = 0; i < am.AbilityList.Count; i++)
            {
                int index = i;

                //this.Test(TinySaveSystem.GetBool($"ability_{index}_unlock") + " " + index);
                GameObject button = Instantiate(SkillPrefab, AbilityContainer.transform);
                button.transform.Find("Name_Txt").GetComponent<TMP_Text>().text = am.AbilityList[i].skill.Name.localization.GetString(am.AbilityList[i].skill.Name.key);
                button.transform.Find("Desc_Txt").GetComponent<TMP_Text>().text = am.AbilityList[i].skill.Description.localization.GetString(am.AbilityList[i].skill.Description.key);
                button.transform.Find("Skill_Icon").GetComponent<UnityEngine.UI.Image>().sprite = am.AbilityList[i].skill.Icon;

                if (TinySaveSystem.GetBool($"ability_{index}_unlock"))
                {
                    button.transform.Find("Pkt_Txt").GetComponent<TMP_Text>().text = "";
                    button.transform.Find("Skill_Icon").Find("Lock").GetComponent<Transform>().gameObject.SetActive(false);
                }
                else
                {
                    button.transform.Find("Pkt_Txt").GetComponent<TMP_Text>().text = am.AbilityList[i].price + " p";
                    button.transform.Find("Skill_Icon").Find("Lock").GetComponent<Transform>().gameObject.SetActive(true);
                }

                button.transform.GetComponent<Button>().onClick.AddListener(() => ClickButton(index));

                void ClickButton(int id)
                {
                    if (TinySaveSystem.GetBool($"ability_{id}_unlock"))
                    {
                        TinySaveSystem.SetInt("selectedAbility", id);
                        AbilityWindow.SetActive(false);
                        LocalizationUpdate();
                    }
                    else
                    {
                        if (TinySaveSystem.GetInt("unlockable_point") >= am.passiveAbilityList[id].price)
                        {
                            TinySaveSystem.SetBool($"ability_{id}_unlock", true);
                            TinySaveSystem.SetInt("unlockable_point", TinySaveSystem.GetInt("unlockable_point") - am.AbilityList[id].price);
                            LocalizationUpdate();
                            GenerateSkillLevel();
                        }
                        else
                        {
                            TinyDebug.Warning("You need Points!");
                        }
                    }
                }

            }
        }
    }
}