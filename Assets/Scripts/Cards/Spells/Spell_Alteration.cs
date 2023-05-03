using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Spells.Alterations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells
{
    public class SpellData_Alteration : SpellData
    {
        public Alteration Alteration;
    }

    public class Spell_Alteration : Spell<SpellData_Alteration>
    {
        public Spell_Alteration(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData, int Cost) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData, Cost)
        {
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();

            foreach (var entity in TargetEntities)
            {
                entity.AddAlteration(LocalData.Alteration);
            }        

            base.EndAction();
        }
    }

}
