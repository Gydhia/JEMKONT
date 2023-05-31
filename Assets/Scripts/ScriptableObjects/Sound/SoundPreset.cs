using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace DownBelow.GameData
{
    [CreateAssetMenu(fileName = "SoundPreset", menuName = "Jemkont/Sound Preset", order = 5)]
    public class SoundPreset : SerializedScriptableObject
    {
        public AK.Wwise.Event audio_popup;
    }
}