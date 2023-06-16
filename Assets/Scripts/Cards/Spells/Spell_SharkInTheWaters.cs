using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
namespace DownBelow.Spells
{
    public class Spell_SharkInTheWaters : Spell
    {
        public Spell_SharkInTheWaters(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, TargettingCondition targCond, CastingCondition castCond) : base(CopyData, RefEntity, TargetCell, ParentSpell, targCond, castCond)
        {
        }

        public override async Task DoSpellBehavior()
        {
            await base.DoSpellBehavior();
            var hurtEnemies = CombatManager.Instance.PlayingEntities.Where(x => !x.IsAlly && x.Health < x.MaxHealth).ToList();
            foreach (var item in hurtEnemies)
            {
                item.ApplyStat(EntityStatistics.Health, Mathf.Max(item.MaxHealth - item.Health, 0) * -1);
            }
        }
    }

}
