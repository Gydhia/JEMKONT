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
        public ScriptableCard card;
    }

    public class Spell_DrawSpecificCard : Spell<SpellData_Card>
    {
        public Spell_DrawSpecificCard(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData, int Cost) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData, Cost)
        {
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();

            //Need to separate card draw
            CardsManager.Instance.DrawCard(LocalData.card);  

            EndAction();
        }
    }

}
