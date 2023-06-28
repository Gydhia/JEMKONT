using DownBelow.Entity;
using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DownBelow.Spells
{

    public class Spell_ShareAlteration : Spell<SpellData_Alteration>
    {
        public Spell_ShareAlteration(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, TargettingCondition targCond, CastingCondition castCond) : base(CopyData, RefEntity, TargetCell, ParentSpell, targCond,castCond)
        {
        }

        public override async Task DoSpellBehavior()
        {
            await base.DoSpellBehavior();

            foreach (var x in TargetEntities) {
                x.AddAlterations(RefEntity.Alterations.FindAll(x => x.GetType() == LocalData.GetType()));
			}
        }
    }
}
