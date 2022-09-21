using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Jemkont.GameData
{
    [CreateAssetMenu(fileName = "GridsPreset", menuName = "Jemkont/Grids Preset", order = 1)]
    public class GridsPreset : SerializedScriptableObject
    {
        [Range(1f, 50f)]
        public float CellsSize = 10f;

        [Range(0f, 2f)]
        public float CellsEdgeOffset = 0.1f;
    }

}
