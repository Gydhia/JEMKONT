using DownBelow.Entity;
using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.CinemachineOrbitalTransposer;

namespace DownBelow.Spells
{


    public class Spell_ConvertAllStatIntoAnother : Spell<SpellData_TradeStats>
    {
        public Spell_ConvertAllStatIntoAnother(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData)
        {
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();

            var targets = GetTargets(TargetCell);

            foreach (var target in targets)
            {

                int realAmount = LocalData.Statistic.MinValue() - target.Statistics[LocalData.Statistic];
                if (!LocalData.GivesAlteration)
                {
                    target.ApplyStat(LocalData.Statistic, realAmount);
                } else
                {

                    target.AddAlteration(new BuffAlteration(LocalData.duration, realAmount, LocalData.Statistic));
                }

                int amountToTrade = Mathf.Abs(Mathf.CeilToInt(realAmount * LocalData.convertRatio));
                if (!LocalData.ConvertsIntoAlteration)
                {
                    target.ApplyStat(LocalData.StatToTradeFor, amountToTrade);
                } else
                {
                    target.AddAlteration(new BuffAlteration(LocalData.duration, amountToTrade, LocalData.StatToTradeFor));
                }

            }

            EndAction();
        }
    }
}