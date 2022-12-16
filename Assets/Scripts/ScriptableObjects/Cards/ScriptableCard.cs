using DownBelow.Entity;
using DownBelow.Spells;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DownBelow.Mechanics
{
    public enum ECardType { Melee, Ranged, Special }

    [CreateAssetMenu(menuName = "Card")]
    public class ScriptableCard : SerializedScriptableObject
    {
        [ReadOnly]
        public Guid UID;
        [OnValueChanged("_updateUID")]
        public string Title;
        public int Cost;
        public CardType CardType;
        [TextArea] 
        public string Description;
        public Sprite IllustrationImage;

        public Spell[] Spells;
        private void OnValidate() {
            Title = name;
        }

        public void CastSpell(GridSystem.Cell cell)
        {
            Managers.NetworkManager.Instance.AskCastSpell(this, cell);
        }

        private void _updateUID()
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(Title));
                this.UID = new Guid(hash);
            }
        }

        public bool IsTrackable() => this.Spells.Length > 0 && this.Spells[0].ApplyToCell;
    
    }

    public enum CardType
    {
        None = 0,
        // Yellow
        Skill = 1,
        // Red
        Attack = 2,
        // Green or Blue
        Power = 3
    }

    public class CardComparer : IComparer<ScriptableCard>
    {
        public int Compare(ScriptableCard x, ScriptableCard y)
        {
            return x.Title.CompareTo(y.Title);
        }
    }

}
