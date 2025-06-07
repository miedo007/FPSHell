using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


namespace HellishBattle
{
    public class SettingsHoverUI : MonoBehaviour
    {
        [Line("Resolution Settings")]
        public RectTransform OptionList;
        public GameObject InfoPanel;
        [Line("Info Text")]
        public TMP_Text Name;
        public TMP_Text Description;

        private void Start()
        {
            ChangeUI("", "");
        }

        public void Update()
        {
            if (Camera.main.aspect < 1.5f)
            {
                OptionList.offsetMax = new Vector2(-16, OptionList.offsetMax.y);
                InfoPanel.SetActive(false);
            }
            else
            {
                OptionList.offsetMax = new Vector2(-174, OptionList.offsetMax.y);
                InfoPanel.SetActive(true);
            }
        }

        public void ChangeUI(string title, string description)
        {
            if (title != null) { Name.text = title; Name.gameObject.SetActive(true); }
            else { Name.text = ""; Name.gameObject.SetActive(false); }

            if (description != null) { Description.text = description; Description.gameObject.SetActive(true); }
            else { Description.text = ""; Description.gameObject.SetActive(false); }
        }
    }
}
