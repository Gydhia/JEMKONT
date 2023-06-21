using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Mechanics
{
    [CreateAssetMenu(fileName = "DialogPreset", menuName = "DownBelow/DialogPreset", order = 2)]
    public class DialogPreset : SerializedScriptableObject
    {
        public Sprite TalkerIcon;
        [TextArea]
        public List<string> Dialogs;
    }
}