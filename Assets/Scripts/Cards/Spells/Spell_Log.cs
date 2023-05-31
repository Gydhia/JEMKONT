using DownBelow.Entity;
using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DownBelow.Spells
{
    public class Spell_Log : Spell<SpellData>
    {
        public Spell_Log(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData)
        {
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();
            Debug.LogWarning("Spell Is bugged. Watch out.");
            EndAction();
        }
    }
}
