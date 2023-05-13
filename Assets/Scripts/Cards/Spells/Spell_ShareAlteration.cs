using DownBelow.Entity;
using DownBelow.GridSystem;
using ExternalPropertyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells
{

    public class Spell_ShareAlteration : Spell<SpellData_Alteration>
    {
        public Spell_ShareAlteration(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData, int Cost) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData, Cost)
        {
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();

            GetTargets(TargetCell)
                .ForEach(x=>x.AddAlterations(RefEntity.Alterations.FindAll(x=>x.GetType() == LocalData.GetType())));

            EndAction();
        }
    }
}
