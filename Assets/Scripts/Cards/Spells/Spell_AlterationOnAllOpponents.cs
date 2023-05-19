using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.Mechanics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DownBelow.Spells
{
    public class Spell_AlterationOnAllOpponents : Spell<SpellData_Alteration>
    {
        public Spell_AlterationOnAllOpponents(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData, int Cost) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData, Cost)
        {
        }
        public override void ExecuteAction()
        {
            base.ExecuteAction();
            foreach (CharacterEntity entity in CombatManager.Instance.PlayingEntities.Where(e => !e.IsAlly))
            {
                LocalData.Alteration.Apply(entity);
            }
            EndAction();
        }
    }

}

