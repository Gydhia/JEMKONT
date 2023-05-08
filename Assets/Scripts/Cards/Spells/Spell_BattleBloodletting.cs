using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DownBelow.Spells
{
    public class Spell_BattleBloodletting : Spell<SpellData>
    {
        public Spell_BattleBloodletting(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData, int Cost) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData, Cost)
        {
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();

            CharacterEntity target = GetTargets(TargetCell)[0];
            int healAmount = target.Health / 2;

            target.ApplyStat(EntityStatistics.Health, -healAmount);
            CombatManager.Instance.PlayingEntities.FindAll(x => x.IsAlly && x != target)
                .ForEach((x)=>x.ApplyStat(EntityStatistics.Health,healAmount));

            EndAction();
        }

    }

}