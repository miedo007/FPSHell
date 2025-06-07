using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace HellishBattle.Mobile
{
    public class MobileGUIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public ButtonType type;
        public UnityEvent invoke;
        bool isPressed;

        // Start is called before the first frame update
        public void Update()
        {
            if (isPressed) { invoke.Invoke(); }
            if (type == ButtonType.Press) { isPressed = false; }
        }
        public void OnPointerDown(PointerEventData data)
        {
            if (type == ButtonType.Bool) { isPressed = !isPressed; }
            else { isPressed = true; }
        }
        public void OnPointerUp(PointerEventData data)
        {
            if (type != ButtonType.Bool) { isPressed = false; }
        }

        public enum ButtonType
        {
            Press, Hold, Bool
        }
    }
}
