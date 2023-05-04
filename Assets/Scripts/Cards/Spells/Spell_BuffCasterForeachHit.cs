using DownBelow.Entity;
using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace DownBelow.Spells
{
    public class Spell_BuffCasterForeachHit : Spell<SpellData_Stats>
    {
        public Spell_BuffCasterForeachHit(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData, int Cost) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData, Cost)
        {
        }
        public override void ExecuteAction()
        {
            base.ExecuteAction();

            for (int i = 0;i < Result.DamagesDealt.Count();i++)
            {
                RefEntity.ApplyStat(
                    LocalData.Statistic,
                    LocalData.StatAmount * (LocalData.IsNegativeEffect ? -1 : 1)
                );
            }

            EndAction();
        }
    }

}
