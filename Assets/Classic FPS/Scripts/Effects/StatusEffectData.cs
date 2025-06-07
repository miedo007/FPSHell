using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HellishBattle.Effects
{
    public abstract class StatusEffectData : ScriptableObject
    {
        [Tooltip("ID jest u¿ywane do wykrywania czy dany Status Effect jest aktywny")]
        public string ID;

        [Tooltip("Nazwa mo¿e byæ wyœwietlania w GUI")]
        public string Name;

        [Tooltip("Icon")]
        public Sprite Icon;

        [Tooltip("Type")]
        public StatusEffectType Type;

        [Tooltip("LifeTime")]
        [Suffix("Sekund")] public float LifeTime = 30;

        abstract public List<string> GetValues();
    }

    [System.Serializable]
    public enum StatusEffectType
    {
        Positive = 0,
        Neutral = 1,
        Negative = 2
    }
}
