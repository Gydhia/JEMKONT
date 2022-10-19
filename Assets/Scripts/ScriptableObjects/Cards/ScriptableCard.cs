using Jemkont.Spells;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Jemkont.Mechanics
{
    public enum EChipType { Melee, Ranged, Special }

    [CreateAssetMenu(menuName = "Chip")]
    public class ScriptableCard : SerializedScriptableObject
    {
        public string Title;
        public int Cost;
        [TextArea] public string Description;
        public Sprite IllustrationImage;

        public Spell[] Spells;

        public void ExecuteSpells(GridSystem.Cell target)
        {
            //ScriptableCardExtension.Instance.ExecuteSpells(target, this.Spells);
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
