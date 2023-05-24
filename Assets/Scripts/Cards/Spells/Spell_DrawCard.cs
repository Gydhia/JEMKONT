using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells
{
    public class SpellData_Card : SpellData
    {
        [Tooltip("Keep to null of should draw random card")]
        public ScriptableCard SpecificCard;
    }

    public class Spell_DrawCard : Spell<SpellData_Card>
    {
        public Spell_DrawCard(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData, int Cost) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData, Cost)
        {
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();

            //Need to separate card draw
            if (this.RefEntity is PlayerBehavior player)
            {
                if(LocalData.SpecificCard != null)
                {
                    player.Deck.CreateAndDrawSpecificCard(LocalData.SpecificCard);
                }
                else
                {
                    player.Deck.DrawCard();
                }
            }

            EndAction();
        }
    }

}
