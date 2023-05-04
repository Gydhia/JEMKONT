using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace DownBelow.Spells
{
    public class Spell_SharkInTheWaters : Spell
    {
        public Spell_SharkInTheWaters(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData, int Cost) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData, Cost)
        {
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();
            var hurtEnemies = CombatManager.Instance.PlayingEntities.Where(x => !x.IsAlly && x.Health < x.MaxHealth ).ToList();
            foreach (var item in hurtEnemies)
            {
                item.ApplyStat(EntityStatistics.Health, item.MaxHealth - item.Health);
            }
            EndAction();
        }
    }

}
