using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.Mechanics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DownBelow.Spells
{
    public class Spell_AlterationOnAllOpponents : Spell<SpellData_Alteration>
    {
        public Spell_AlterationOnAllOpponents(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData)
        {
        }
        public override async Task DoSpellBehavior()
        {
            await base.DoSpellBehavior();
            foreach (CharacterEntity entity in CombatManager.Instance.PlayingEntities.Where(e => !e.IsAlly))
            {
                LocalData.Alteration.Apply(entity);
            }
            EndAction();
        }
    }

}

