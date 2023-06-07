using DownBelow.Entity;
using DownBelow.GridSystem;
using ExternalPropertyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DownBelow.Spells
{

    public class Spell_ShareAlteration : Spell<SpellData_Alteration>
    {
        public Spell_ShareAlteration(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData)
        {
        }

        public override async Task DoSpellBehavior()
        {
            await base.DoSpellBehavior();

            GetTargets(TargetCell)
                .ForEach(x=>x.AddAlterations(RefEntity.Alterations.FindAll(x=>x.GetType() == LocalData.GetType())));
        }
    }
}
