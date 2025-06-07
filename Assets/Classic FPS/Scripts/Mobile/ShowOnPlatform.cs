using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HellishBattle.Mobile
{
    public class ShowOnPlatform : MonoBehaviour
    {
        public bool ShowOnPC = true;
        public bool ShowOnMobile = true;

        public void Awake()
        {
            // Mobile
            if (ShowOnMobile == false && (Application.isMobilePlatform))
            {
                transform.gameObject.SetActive(false);
            }


            // PC or Editor
            if (ShowOnPC == false && (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.OSXPlayer
                || Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.OSXEditor))
            {
                transform.gameObject.SetActive(false);
            }
        }
    }
}
