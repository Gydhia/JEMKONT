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
        public Spell_TheList(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, TargettingCondition targCond, CastingCondition castCond) : base(CopyData, RefEntity, TargetCell, ParentSpell, targCond,castCond)
        {
        }

        public override async Task DoSpellBehavior()
        {
            await base.DoSpellBehavior();
            foreach(var target in TargetEntities)
            {
                ((PlayerBehavior)RefEntity).theList++;
                target.ApplyStat(EntityStatistics.Health, -10 * ((PlayerBehavior)RefEntity).theList);
            }
            
        }
    }

}
