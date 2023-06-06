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

    public class Spell_StorePath : Spell<SpellData>
    {
        public Spell_StorePath(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData)
        {
        }

        public override async Task DoSpellBehavior()
        {
            await base.DoSpellBehavior();

            //Need path between two target cells in result?
            var start = Result.TargetedCells.Find(x => x.EntityIn != null).EntityIn;
            if (start == null)
            {
                Debug.LogError("Invalid Cast: Cast has no entity. (Spell_StorePath needs this)");
                EndAction();
            }
            var end = Result.TargetedCells.Find(x => x != start);
            List<Cell> path = GridManager.Instance.FindPath(start, end.PositionInGrid, true);
            Result.TargetedCells.Clear();
            Result.TargetedCells.AddRange(path.FindAll(x => x != start.EntityCell && x != end));

            EndAction();
        }
    }
}