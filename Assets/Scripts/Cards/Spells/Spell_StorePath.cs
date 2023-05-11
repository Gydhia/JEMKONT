using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DownBelow.Spells
{

    public class Spell_StorePath : Spell<SpellData>
    {
        public Spell_StorePath(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData, int Cost) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData, Cost)
        {
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();

            //Need path between two target cells in result

            var start = Result.TargetedCells[0];
            var end = Result.TargetedCells[1];

           // GridManager.Instance.FindPath()
           //TODO : PAth with two cells

            EndAction();
        }
    }
}