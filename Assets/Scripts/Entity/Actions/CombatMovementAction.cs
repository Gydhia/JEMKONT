using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Entity
{
    public class CombatMovementAction : MovementAction
    {
        public CombatMovementAction(CharacterEntity RefEntity, Cell TargetCell) : base(RefEntity, TargetCell)
        {
        }

        public override bool AllowedToProcess()
        {
            return this.RefEntity.Speed >= (this.calculatedPath.Count - 1);
        }

        public override void MoveWithPath()
        {
            if (this.RefEntity.CurrentGrid.IsCombatGrid)
            {
                if (this.RefEntity.IsMoving || !this.AllowedToProcess())
                {
                    EndAction();
                    return;
                }

                // -1 since the own entity's cell is included
                this.RefEntity.ApplyStat(EntityStatistics.Speed, -(this.calculatedPath.Count - 1));
            }

            base.MoveWithPath();
        }

        public override void EndAction()
        {
            Managers.GridManager.Instance.CalculatePossibleCombatMovements(this.RefEntity);
            base.EndAction();
        }
    }

}