using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.GameData
{
    [CreateAssetMenu(menuName = "DownBelow/BasePresets/InputPreset")]
    public class InputPreset : SerializedScriptableObject
    {
        [Header("Cursors")]
        public Texture2D CardCursor;
        [Header("Timings")]
        public float PathRequestDelay = 0.35f;
    }

}
