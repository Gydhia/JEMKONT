using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.GameData
{
    [CreateAssetMenu(fileName = "CombatPreset", menuName = "Jemkont/Combat Preset", order = 1)]
    public class CombatPreset : SerializedScriptableObject
    {
        [Tooltip("Time in seconds for a turn"), Range(0f, 360f)]
        public float TurnTime = 60f;

        [Range(0f, 10f)]
        public int CardsToDrawAtStart = 3;

        [Range(0f, 10f)]
        public int CardsToDrawAtTurn = 3;

        [Range(0f, 2f)]
        public float DelayBetweenActions = 0.4f;
    }
}
