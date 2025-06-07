using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using HellishBattle.SaveSystem;

namespace HellishBattle.Ability
{
    public class PlayerAbility : MonoBehaviour
    {
        [Line("Ability UI")]
        public Image AbilityIcon;
        public Image AbilityCooldownImage;
        public TMP_Text AbilityCooldownText;
        [Space(10)]
        public Image PassiveSkillIcon;

        [Line("Preview to Selected Ability")]
        public BaseAbilityScript SelectedAbility;
        public PassiveAbilitySO SelectedPassive;


        private TinyInput tinyInput;
        private float timer;

        private void Awake()
        {
            tinyInput = InputManager.Instance.input;
            tinyInput.Player.Ability.started += ctx => { ActivateAbility(); };
        }

        public void OnDisable() { tinyInput.Disable(); }

        private void Start()
        {
            AbilityManager am = (AbilityManager)Resources.Load("Ability");
            SelectedAbility = am.AbilityList[TinySaveSystem.GetInt("selectedAbility")].skill;
            SelectedPassive = am.passiveAbilityList[TinySaveSystem.GetInt("selectedPassiveSkill")].skill;

            AbilityIcon.sprite = SelectedAbility.Icon;
            PassiveSkillIcon.sprite = SelectedPassive.Icon;
            AbilityCooldownImage.fillAmount = 0;
            AbilityCooldownText.text = "";
        }

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer > 0)
            {
                AbilityCooldownImage.fillAmount = timer / SelectedAbility.cooldown;
                AbilityCooldownText.text = ((int)timer + 1).ToString();
            }
            else
            {
                AbilityCooldownImage.fillAmount = 0;
                AbilityCooldownText.text = "";
            }
        }

        void ActivateAbility()
        {
            if (timer < 0)
            {
                SelectedAbility.ActivateAbility();
                timer = SelectedAbility.cooldown;
            }
            else
            {
                TinyDebug.Warning(this, "Ability not Available Now!");
            }
        }
    }
}