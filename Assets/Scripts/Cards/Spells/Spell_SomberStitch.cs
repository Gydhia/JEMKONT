using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Spells.Alterations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells
{
    public class Spell_SomberStitch : Spell<SpellData>
    {
        public Spell_SomberStitch(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData, int Cost) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData, Cost)
        {
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();

            Result.TargetedCells[0].EntityIn.AddAlteration(new SomberStitch(-1, TargetedCells[1].EntityIn));
            Result.TargetedCells[1].EntityIn.AddAlteration(new SomberStitch(-1, TargetedCells[0].EntityIn));

            EndAction();
        }
    }

}
