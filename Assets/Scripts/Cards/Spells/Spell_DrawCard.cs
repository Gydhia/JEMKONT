using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.Mechanics;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells
{
    public class SpellData_Card : SpellData
    {
        [Tooltip("Keep to null of should draw random card")]
        [FoldoutGroup("CARD Spell Datas")]
        public ScriptableCard SpecificCard;
        [FoldoutGroup("CARD Spell Datas")]
        public int DrawNumber = 1;
    }

    public class Spell_DrawCard : Spell<SpellData_Card>
    {
        public Spell_DrawCard(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData)
        {
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();

            //Need to separate card draw
            if (this.RefEntity is PlayerBehavior player)
            {
                for (int i = 0; i < LocalData.DrawNumber; i++)
                {
                    if (LocalData.SpecificCard != null)
                    {
                        player.Deck.CreateAndDrawSpecificCard(LocalData.SpecificCard);
                    }
                    else
                    {
                        player.Deck.DrawCard();
                    }
                }
            }

            EndAction();
        }
    }

}
