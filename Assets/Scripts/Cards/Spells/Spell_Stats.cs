using DownBelow.Entity;
using DownBelow.GridSystem;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Spells
{
    public class SpellData_Stats : SpellData
    {
        [FoldoutGroup("STATS Spell Datas")]
        public bool IsNegativeEffect = false;

        [FoldoutGroup("STATS Spell Datas")]
        public EntityStatistics Statistic = EntityStatistics.None;

        [FoldoutGroup("STATS Spell Datas")]
        public int StatAmount = 1;
    }

    public class Spell_Stats : Spell<SpellData_Stats>
    {
        public Spell_Stats(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell)
            : base(CopyData, RefEntity, TargetCell, ParentSpell) { }

        public override void ExecuteAction()
        {
            base.ExecuteAction();

            var targets = this.GetTargets(TargetCell);

            foreach (var target in targets)
            {
                target.ApplyStat(
                    LocalData.Statistic,
                    LocalData.StatAmount * (LocalData.IsNegativeEffect ? -1 : 1)
                );
            }
        }
    }
}
