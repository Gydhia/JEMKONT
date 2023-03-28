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

        public override void MoveWithPath()
        {
            if (this.RefEntity.CurrentGrid.IsCombatGrid)
            {
                if (this.RefEntity.IsMoving)
                    return;

                this.RefEntity.ApplyStat(EntityStatistics.Speed, -this.calculatedPath.Count);
            }

            base.MoveWithPath();
        }
    }

}