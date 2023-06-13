using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DownBelow.Entity;

namespace DownBelow.GameData
{
    [CreateAssetMenu(fileName = "GridsPreset", menuName = "DownBelow/BasePresets/Grids Preset", order = 1)]
    public class GridsPreset : SerializedScriptableObject
    {
        [Range(1f, 50f)]
        public float CellsSize = 10f;

        [Range(0f, 2f)]
        public float CellsEdgeOffset = 0.1f;

        [Range(0.01f, 2f)]
        public float TimeToCrossCell = 0.12f;

        public GameObject GridShader;

        public ArrowRenderer SpellArrowPrefab;

        public ParticleSystem CombatEntracePrefab;

        public ParticleSystem PlayerSwitchPrefab;
    }

}
