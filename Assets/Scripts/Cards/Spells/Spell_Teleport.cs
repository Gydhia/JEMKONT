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

            //Could be changed to a "while(cellToTP != walkable)"?
            if (cellToTP.Datas.state != CellState.Walkable)
            {
                List<Cell> freeNeighbours = GridManager.Instance.GetNormalNeighbours(cellToTP, cellToTP.RefGrid)
                    .FindAll(x => x.Datas.state == CellState.Walkable)
                    .OrderByDescending(x => Math.Abs(x.PositionInGrid.latitude - RefEntity.EntityCell.PositionInGrid.latitude) + Math.Abs(x.PositionInGrid.longitude - RefEntity.EntityCell.PositionInGrid.longitude))
                    .ToList();
                //Someday will need a Foreach, but i just don't know what we need to check on the cells before tp'ing, so just tp on the farther one.
                cellToTP = freeNeighbours[0];
            }

            if(cellToTP.EntityIn != null)
            {
                Result.TeleportedTo.Add(cellToTP.EntityIn);
            }

            RefEntity.Teleport(cellToTP);
            EndAction();
        }

    }
}
