using DownBelow.Entity;
using DownBelow.Spells;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DownBelow.Mechanics
{
    public enum ECardType { Melee, Ranged, Special }

    [CreateAssetMenu(menuName = "Cards/SO_Card")]
    public class ScriptableCard : SerializedScriptableObject
    {
        #region SERIALIZATION
        [ReadOnly]
        public Guid UID;
        [OnValueChanged("_updateUID")]
        public string Title;
        private void _updateUID()
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(Title));
                this.UID = new Guid(hash);
            }
        }
        #endregion

        public int Cost;
        public CardType CardType;
        [TextArea] 
        public string Description;
        public Sprite IllustrationImage;

        public EClass Class;

        public Spell[] Spells;
        private void OnValidate() {
            Title = name;
        }

        public bool IsTrackable() => true; //this.Spells.Length > 0 && this.Spells[0].ApplyToCell;

        [HideInInspector]
        public int CurrentSpellTargetting = 0;

        public int GetNextTargettingSpellIndex()
        {
            for (int i = CurrentSpellTargetting + 1; i < this.Spells.Length; i++)
            {
                if (this.Spells[i].Data.RequiresTargetting)
                {
                    CurrentSpellTargetting = i;
                    return CurrentSpellTargetting;
                }
            }

            return -1;
        }
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
