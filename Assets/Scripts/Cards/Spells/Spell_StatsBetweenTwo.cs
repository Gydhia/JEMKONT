using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static EODE.Wonderland.AnimatorRuntimeInfos;

namespace DownBelow.Spells
{
    public class Spell_StatsBetweenTwo : Spell<SpellData_Stats>
    {
        public Spell_StatsBetweenTwo(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData)
        {
        }

        public override async Task DoSpellBehavior()
        {
            await base.DoSpellBehavior();
            var targets = GetTargets(TargetCell);
            if (targets == null || targets.Count > 2)
            {

                return;
            }
            List<Cell> EntityCells = new List<Cell>() {
                targets[0].EntityCell,
                targets[1].EntityCell
            };
            //ArgumentNullException: Value cannot be null.
            //Parameter name: first
            var path = GridManager.Instance.FindPath(targets[0], targets[1].EntityCell.PositionInGrid, true).Except(EntityCells);
            foreach (var cell in path)
            {
                if (cell.EntityIn != null)
                {
                    cell.EntityIn.ApplyStat(LocalData.Statistic, LocalData.IsNegativeEffect ? -LocalData.StatAmount : LocalData.StatAmount);
                }
                if (cell.AttachedNCE != null && LocalData.Statistic == EntityStatistics.Health)
                {
                    cell.AttachedNCE.Hit();
                }
            }

        }
    }
}
