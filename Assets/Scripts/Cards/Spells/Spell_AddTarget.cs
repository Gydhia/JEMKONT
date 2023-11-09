using DownBelow.Entity;
using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells
{
    /// <summary>
    /// Does nothing more than storing cells in result
    /// </summary>
    public class Spell_AddTarget : Spell<SpellData>
    {
        public Spell_AddTarget(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData, int Cost) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData, Cost)
        {
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();

            GetTargets(TargetCell);
            Result.TargetedCells.AddRange(TargetedCells);

            EndAction();
        }
    }

}
