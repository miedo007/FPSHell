using UnityEngine;
using UnityEngine.EventSystems;


namespace HellishBattle
{
    public class SettingsHoverDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public SettingsHoverUI UI;
        public LocalizedString Name;
        public LocalizedString Description;

        public void OnPointerEnter(PointerEventData eventData)
        {
            UI.ChangeUI(Name.GetLocalization(), Description.GetLocalization());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UI.ChangeUI("", "");
        }
    }
}
