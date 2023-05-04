using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DownBelow.Spells
{
    public class Spell_ThisOnesMine : Spell<SpellData_Summon>
    {
        public Spell_ThisOnesMine(SpellData CopyData, CharacterEntity RefEntity, Cell TargetCell, Spell ParentSpell, SpellCondition ConditionData, int Cost) : base(CopyData, RefEntity, TargetCell, ParentSpell, ConditionData, Cost)
        {
        }
        public override void ExecuteAction()
        {
            base.ExecuteAction();
            GetTargets(TargetCell);
            List<Cell> cellsToCheck = new List<Cell>
            {
                TargetCell,
                RefEntity.EntityCell
            };
            ///Algo à vérifier, car il y a des cas étranges.
            var pathto = GridManager.Instance.FindPath(RefEntity, TargetCell.PositionInGrid, Range: 1);
            if (pathto != null)
            {
                pathto = pathto.FindAll(x => !cellsToCheck.Contains(x));
                if (pathto.Count() > 0)
                {
                    cellsToCheck.AddRange(pathto);
                }
            }
            foreach (var cell in cellsToCheck)
            {
                foreach (var cellneighbour in GridManager.Instance.GetNormalNeighbours(cell, cell.RefGrid)
                    .Where(x => x.Datas.state == CellState.Walkable && !cellsToCheck.Contains(x)))
                {
                    GameManager.Instance.StartCoroutine(Spell_SummonNCE.SummonNCE(cellneighbour, (SpellData_Summon)LocalData, RefEntity));
                }
            }

            EndAction();
        }
    }
}