using UnityEngine;
using UnityEngine.UI;

namespace HellishBattle.Player
{
    public class FlashScreen : MonoBehaviour
    {
        [Header("Damage")]
        public Sprite overlayDamage;
        public Color colorDamage;
        public float durationDamage;

        [Header("Health")]
        public Sprite overlayHealth;
        public Color colorHealth;
        public float durationHealth;

        [Header("Armor")]
        public Sprite overlayArmor;
        public Color colorArmor;
        public float durationArmor;

        [Header("Ammo")]
        public Sprite overlayAmmo;
        public Color colorAmmo;
        public float durationAmmo;

        Image flashScreen;
        int screenNumber; // 0 = Damage, 1 = Health, 2 = Armor, 3 = Ammo
        GameSettingsManger _gm;

        void Start()
        {
            flashScreen = GetComponent<Image>();
            _gm = (GameSettingsManger)Resources.Load("Game_Settings");
        }

        void Update()
        {
            if (flashScreen.color.a > 0)
            {
                if (screenNumber == 0 && _gm.EnablePlayerDamageOverlay)
                {
                    Color invisible = new Color(colorDamage.r, colorDamage.g, colorDamage.b, 0);
                    flashScreen.color = Color.Lerp(flashScreen.color, invisible, durationDamage * Time.deltaTime);
                }
                if (screenNumber == 1 && _gm.EnablePlayerBonusOverlay)
                {
                    Color invisible = new Color(colorHealth.r, colorHealth.g, colorHealth.b, 0);
                    flashScreen.color = Color.Lerp(flashScreen.color, invisible, durationHealth * Time.deltaTime);
                }
                if (screenNumber == 2 && _gm.EnablePlayerBonusOverlay)
                {
                    Color invisible = new Color(colorArmor.r, colorArmor.g, colorArmor.b, 0);
                    flashScreen.color = Color.Lerp(flashScreen.color, invisible, durationArmor * Time.deltaTime);
                }
                if (screenNumber == 3 && _gm.EnablePlayerBonusOverlay)
                {
                    Color invisible = new Color(colorAmmo.r, colorAmmo.g, colorAmmo.b, 0);
                    flashScreen.color = Color.Lerp(flashScreen.color, invisible, durationAmmo * Time.deltaTime);
                }
            }
        }

        public void TookDamage()
        {
            flashScreen.sprite = overlayDamage;
            flashScreen.color = colorDamage;
            screenNumber = 0;
        }

        public void HealthBonus()
        {
            flashScreen.sprite = overlayHealth;
            flashScreen.color = colorHealth;
            screenNumber = 1;
        }

        public void ArmorBonus()
        {
            flashScreen.sprite = overlayArmor;
            flashScreen.color = colorArmor;
            screenNumber = 2;
        }

        public void AmmoBonus()
        {
            flashScreen.sprite = overlayAmmo;
            flashScreen.color = colorAmmo;
            screenNumber = 3;
        }
    }
}