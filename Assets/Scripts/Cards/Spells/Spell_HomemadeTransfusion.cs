using DownBelow.Entity;
using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DownBelow.Spells
{
    public class SpellData_HomemadeTransfusion : SpellData
    {
        [Tooltip("Reminder: target is healed by X. If he is still hurt, he is dealt X*2 damage.")]
        public int HealingValue;
    }

    public class Spell_HomemadeTransfusion : Spell<SpellData_HomemadeTransfusion>
    {
        public Spell_HomemadeTransfusion(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, TargettingCondition targCond, CastingCondition castCond) : base(CopyData, RefEntity, TargetCell, ParentSpell, targCond,castCond)
        {
        }

        public override async Task DoSpellBehavior()
        {
            await base.DoSpellBehavior();
            var target = GetTargets(TargetCell)[0];
            target.ApplyStat(EntityStatistics.Health, LocalData.HealingValue);
            if (target.Health < target.MaxHealth)
            {
                target.ApplyStat(EntityStatistics.Health, -(LocalData.HealingValue*2));
            }
        }
    }

}

