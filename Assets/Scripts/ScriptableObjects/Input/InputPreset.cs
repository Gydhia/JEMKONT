using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.GameData
{
    [CreateAssetMenu(menuName = "Presets/InputPreset")]
    public class InputPreset : SerializedScriptableObject
    {
        [Header("Cursors")]
        public Texture2D CardCursor;
    }

}
