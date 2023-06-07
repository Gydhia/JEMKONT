using DownBelow.Entity;
using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace DownBelow.Spells
{
    public class Spell_TheList : Spell<SpellData>
    {
        public Spell_TheList(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData)
        {
        }

        public override async Task DoSpellBehavior()
        {
            await base.DoSpellBehavior();
            GetTargets(TargetCell);
            foreach(var target in TargetEntities)
            {
                ((PlayerBehavior)RefEntity).theList++;
                target.ApplyStat(EntityStatistics.Health, -2 * ((PlayerBehavior)RefEntity).theList);
            }
            
        }
    }

}
