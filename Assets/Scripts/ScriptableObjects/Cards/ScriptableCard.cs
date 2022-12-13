using Jemkont.Spells;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Jemkont.Mechanics
{
    public enum ECardType { Melee, Ranged, Special }

    [CreateAssetMenu(menuName = "Card")]
    public class ScriptableCard : SerializedScriptableObject
    {
        public string Title;
        public int Cost;
        [TextArea] public string Description;
        public Sprite IllustrationImage;

        public Spell[] Spells;
        private void OnValidate() {
            Title = name;
        }
    }

    public class CardComparer : IComparer<ScriptableCard>
    {
        public int Compare(ScriptableCard x, ScriptableCard y)
        {
            return x.Title.CompareTo(y.Title);
        }
    }

}
