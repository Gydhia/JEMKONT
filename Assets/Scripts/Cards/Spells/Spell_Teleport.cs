using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace DownBelow.Spells
{
    /// <summary>
    /// Teleports RefEntity to the closest walkable cell
    /// </summary>
    public class Spell_Teleport : Spell<SpellData>
    {
        public Spell_Teleport(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData, int Cost) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData, Cost)
        {
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();
            var cellToTP = TargetCell;


            RefEntity.Teleport(cellToTP, Result);
            EndAction();
        }
    }
}
