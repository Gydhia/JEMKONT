using DownBelow.Entity;
using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Tilemaps;
using UnityEngine;
namespace DownBelow.Spells
{
    public class Spell_StatsOnResult : Spell<SpellData_Stats>
    {
        public Spell_StatsOnResult(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData, int Cost) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData, Cost)
        {
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();

            GetTargets(TargetCell);

            foreach (CharacterEntity target in Result.TargetedCells.FindAll(x => x.EntityIn != null).Select(x => x.EntityIn))
            {
                target.ApplyStat(LocalData.Statistic,
                    LocalData.StatAmount * (LocalData.IsNegativeEffect ?  -1 : 1));
            }

            ExecuteAction();
        }
    }
}
