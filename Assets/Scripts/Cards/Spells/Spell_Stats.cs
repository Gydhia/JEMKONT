using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Spells.Alterations;
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

        [InfoBox("If this is ticked, it will create an alteration for the buff, and will not buff the stats directly (thus making the stat changes temporary, and not permanent for the battle.)")]
        [FoldoutGroup("STATS Spell Datas")]
        public bool GivesAlteration;
        [FoldoutGroup("STATS Spell Datas")]
        [ShowIf(nameof(GivesAlteration))]
        public int duration;

        public SpellData_Stats(bool isNegativeEffect, EntityStatistics statistic, int statAmount)
        {
            IsNegativeEffect = isNegativeEffect;
            Statistic = statistic;
            StatAmount = statAmount;
        }
    }

    public class SpellData_TradeStats : SpellData_Stats
    {
        [FoldoutGroup("STATS Spell Datas")]
        public EntityStatistics StatToTradeFor;
        [FoldoutGroup("STATS Spell Datas")]
        public float convertRatio = 1f;
        [InfoBox("If this is ticked, it will create an alteration for the buff, and will not buff the stats directly (thus making the stat changes temporary, and not permanent for the battle.)")]
        [FoldoutGroup("STATS Spell Datas")]
        public bool ConvertsIntoAlteration;
        [FoldoutGroup("STATS Spell Datas")]

        public SpellData_TradeStats(bool isNegativeEffect, EntityStatistics statistic, int statAmount, EntityStatistics StatToTradeFor, float convertRatio) : base(isNegativeEffect, statistic, statAmount)
        {
            this.StatToTradeFor = StatToTradeFor;
            this.convertRatio = convertRatio;
        }

    }

    public class Spell_Stats : Spell<SpellData_Stats>
    {
        public Spell_Stats(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData) { }

        public override void ExecuteAction()
        {
            base.ExecuteAction();

            var targets = this.GetTargets(this.TargetCell);
            if (LocalData.Statistic == EntityStatistics.Health && NCEHits != null)
            {
                //This means it is a damaging spell. Then, the NCE is hit (we don't want it to be hit if we are lowering the defense or something.)
                foreach (NonCharacterEntity nce in NCEHits)
                {
                    nce.Hit();
                }
            }
            int realAmount = LocalData.StatAmount * (LocalData.IsNegativeEffect ? -1 : 1);
            foreach (var target in targets)
            {

                if (!LocalData.GivesAlteration)
                {
                    target.ApplyStat(
                    LocalData.Statistic,
                    realAmount
                );
                } else
                {
                    target.AddAlteration(new BuffAlteration(LocalData.duration, realAmount, LocalData.Statistic));
                }
            }
            if (LocalData is SpellData_TradeStats trading)
            {
                int amountToTrade = Mathf.CeilToInt(realAmount * trading.convertRatio);

                foreach (var target in targets)
                {
                    if (!trading.ConvertsIntoAlteration)
                    {
                        target.ApplyStat(trading.StatToTradeFor, amountToTrade);
                    } else
                    {
                        target.AddAlteration(new BuffAlteration(LocalData.duration, amountToTrade, trading.Statistic));
                    }
                }
            }
            this.EndAction();
        }
    }
}
