using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HellishBattle.Effects
{
    [CreateAssetMenu(fileName = "MovementEffect", menuName = "Hellish Battle/Effect List/MovementEffect", order = 24)]
    public class MovementEffect : StatusEffectData
    {
        [Range(0.1f, 1.9f)] public float SpeedMultiplier = 1;

        public override List<string> GetValues()
        {
            List<string> values = new List<string>();
            values.Add(SpeedMultiplier.ToString());
            return values;
        }
    }
}