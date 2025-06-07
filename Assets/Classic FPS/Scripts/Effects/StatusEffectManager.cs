using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HellishBattle.Effects
{
    public class StatusEffectManager : MonoBehaviour
    {
        public bool showUI;
        public Transform uiContainer;
        public GameObject uiPrefab;
        public List<StatusEffectManagerClass> effects;

        public UnityEvent onActivate, onDeactivate;

        void Start()
        {
            if (showUI)
            {
                // Remove all Children
                foreach (Transform child in uiContainer)
                {
                    Destroy(child.gameObject);
                }

                // Spawn All Effects
                foreach (StatusEffectManagerClass effect in effects)
                {
                    GameObject ui = Instantiate(uiPrefab, uiContainer);
                    effect.uiImage = ui.GetComponent<Image>();
                    effect.uiDuration = ui.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
                    ui.SetActive(false);
                }
            }

            ApplyEffect(effects[0].effect);
        }

        void Update()
        {
            // Modify Status Effect Lifetime
            foreach (StatusEffectManagerClass statusEffect in effects)
            {
                if (statusEffect.active)
                {
                    statusEffect.activeTime -= Time.deltaTime;

                    if (statusEffect.activeTime < 0)
                    {
                        statusEffect.active = false;
                        statusEffect.activeTime = 0;
                        statusEffect.onInactive.Invoke();
                        statusEffect.uiImage.gameObject.SetActive(false);
                    }

                    if (showUI)
                    { // Show in UI
                        int min = Mathf.FloorToInt(statusEffect.activeTime / 60);
                        int sec = Mathf.FloorToInt(statusEffect.activeTime % 60);
                        statusEffect.uiDuration.text = string.Format("{0:00}:{1:00}", min, sec);
                    }
                }
            }
        }

        #region Apply Status Effect
        public void ApplyEffect(StatusEffectData statusEffectData, float duration)
        {
            if (effects != null)
            {
                StatusEffectManagerClass effectClass = effects.Find(x => x.effect == statusEffectData);
                if (effectClass != null)
                {
                    // Effect is already in the list, you can activate or update it
                    effectClass.active = true;
                    effectClass.activeTime = duration;
                    effectClass.onActive.Invoke();

                    if (showUI) effectClass.uiImage.gameObject.SetActive(true);
                    if (showUI && effectClass.effect.Icon) effectClass.uiImage.sprite = effectClass.effect.Icon;

                    onActivate.Invoke();
                }
            }
            else
            {
                Debug.LogError("Status Effect Object is Empty");
            }
        }

        public void ApplyEffect(StatusEffectData statusEffectData)
        {
            if (effects != null)
            {
                ApplyEffect(statusEffectData, statusEffectData.LifeTime);
            }
            else
            {
                Debug.LogError("Status Effect Object is Empty");
            }
        }
        #endregion

        #region Remove Status Effect
        public void RemoveEffect(StatusEffectData statusEffectData)
        {
            if (effects != null)
            {
                StatusEffectManagerClass effectClass = effects.Find(x => x.effect == statusEffectData);
                if (effectClass != null && effectClass.active)
                {
                    effectClass.active = false;
                    effectClass.activeTime = 0;
                    effectClass.onInactive.Invoke();
                    onDeactivate.Invoke();
                }
            }
            else
            {
                Debug.LogError("Status Effect Object is Empty");
            }
        }

        public void RemoveAllEffects()
        {
            foreach (StatusEffectManagerClass effect in effects)
            {
                if (effect.active)
                {
                    effect.onInactive.Invoke();
                }
                effect.active = false;
                effect.activeTime = 0;
            }
            onDeactivate.Invoke();
        }
        #endregion

        #region Get Values By ID
        public List<string> IsActive(string id)
        {
            foreach (StatusEffectManagerClass effect in effects)
            {
                if (effect.effect.ID == id && effect.active)
                {
                    return effect.effect.GetValues();
                }
            }

            return null;
        }
        #endregion
    }

    [System.Serializable]
    public class StatusEffectManagerClass
    {
        public StatusEffectData effect;
        public bool active;
        public float activeTime;
        public UnityEvent onActive, onInactive;

        [HideInInspector] public Image uiImage;
        [HideInInspector] public TMPro.TMP_Text uiDuration;
    }
}
