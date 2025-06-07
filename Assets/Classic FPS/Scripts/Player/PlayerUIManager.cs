using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HellishBattle.Player.UI
{
    public class PlayerUIManager : MonoBehaviour
    {
        [HideInInspector]
        private static PlayerUIManager _instance;
        public static PlayerUIManager Instance
        {
            get { return _instance; }

        }

        [Line("END SCREEN UI")]
        public GameObject EndScreen_Panel;
        public GameObject EndScreen_NextLevelBtn;
        public GameObject EndScreen_BackToMenuBtn;
        public TMP_Text EndScreen_TimeTxt;
        public TMP_Text EndScreen_ItemsTxt;
        public TMP_Text EndScreen_SecretsTxt;
        public TMP_Text EndScreen_EnemyTxt;
        public TMP_Text EndScreen_PointsTxt;
        public Transform EndScreen_Requirement_1;
        public Transform EndScreen_Requirement_2;
        public Transform EndScreen_Requirement_3;
        public Transform EndScreen_Requirement_4;
        public Transform EndScreen_Requirement_5;
        public TMPTextStringLocalization EndScreen_LevelNameTxt;
        public TMPTextStringLocalization EndScreen_TrophyTxt;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
        }
    }
}
