using DownBelow.Entity;
using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace DownBelow.Spells
{
    public class Spell_TeleportStoredToCaster : Spell<SpellData>
    {
        public Spell_TeleportStoredToCaster(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData)
        {
        }

        public override void ExecuteAction()
        {
            base.ExecuteAction();

            GetTargets(TargetCell);

            foreach (CharacterEntity target in Result.TargetedCells.FindAll(x => x.EntityIn != null).Select(x => x.EntityIn))
            {
                target.SmartTeleport(RefEntity.EntityCell, Result);
            }

            EndAction();
        }
    }
}
