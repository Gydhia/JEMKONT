using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Spells.Alterations;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DownBelow.Spells
{
    public class Spell_SomberStitch : Spell<SpellData>
    {
        public Spell_SomberStitch(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData)
        {
        }

        public override async Task DoSpellBehavior()
        {
            await base.DoSpellBehavior();
            GetTargets(TargetCell);
            TargetedCells[0].EntityIn.AddAlteration(new SomberStitch(-1, TargetedCells[1].EntityIn));
            TargetedCells[1].EntityIn.AddAlteration(new SomberStitch(-1, TargetedCells[0].EntityIn));

            EndAction();
        }
    }

}
