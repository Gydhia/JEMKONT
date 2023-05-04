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
        public Spell_Stats(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData, int Cost)
            : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData, Cost) { }

        public override void ExecuteAction()
        {
            base.ExecuteAction();

            var targets = this.GetTargets(this.TargetCell);
            if(LocalData.Statistic == EntityStatistics.Health)
            {
                //This means it is a damaging spell. Then, the NCE is hit (we don't want it to be hit if we are lowering the defense or something.)
                foreach (NonCharacterEntity nce in NCEHits)
                {
                    nce.Hit();
                }
            }

            foreach (var target in targets)
            {
                target.ApplyStat(
                    LocalData.Statistic,
                    LocalData.StatAmount * (LocalData.IsNegativeEffect ? -1 : 1)
                );
            }

            this.EndAction();
        }
    }
}
