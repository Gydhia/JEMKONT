using DownBelow.Entity;
using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DownBelow.Spells
{
    public class SpellData_ChillingDrought : SpellData
    {
        public int BaseDamage;
        public int DamagePenaltyPerCard;
    }

    public class Spell_ChillingDrought : Spell<SpellData_ChillingDrought>
    {
        public Spell_ChillingDrought(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData)   
        {
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();

            //GetTargets(TargetCell)[0].ApplyStat(LocalData.BaseDamage; 
            // cant do it for now, need hand of the refentity

            EndAction();
        }
    }
}
