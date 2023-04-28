using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DownBelow.Entity.EnemyEntity;

namespace DownBelow.Entity
{
    public class EnemyMovementAction : CombatMovementAction
    {
        public MovementType Type;

        public EnemyMovementAction(CharacterEntity RefEntity, Cell TargetCell, MovementType MovementType) 
            : base(RefEntity, TargetCell)
        {
            this.Type = MovementType;
        }

        public override void ExecuteAction()
        {
            // Should be a TargettingAction that return the cell to target, aka the entity
            this.ProcessContextAction();

            base.ExecuteAction();
        }

        protected override List<Cell> GetProcessedPath()
        {
            List<Cell> path;
            switch (this.Type)
            {
                case MovementType.Straight: path = this.MovementStraight(); break;
                case MovementType.StraightToRange:
                default: path = this.MovementStraightToRange(); break;
            }

            if (path == null || path.Count == 0)
                return null;

            path.RemoveRange(RefEntity.Speed, (path.Count - 1) - (RefEntity.Speed - 1));
            return path;
        }

        private List<Cell> MovementStraight()
        {
            GridPosition targPosition = TargetCell.EntityIn.EntityCell.PositionInGrid;
            return GridManager.Instance.FindPath(TargetCell.EntityIn, targPosition);
        }

        /// <summary>
        /// Will Go straight to the target stops when in range
        /// </summary>
        private List<Cell> MovementStraightToRange()
        {
            GridPosition targPosition = TargetCell.EntityIn.EntityCell.PositionInGrid;
            return GridManager.Instance.FindPath(this.RefEntity, targPosition, false, this.RefEntity.Range);
        }
    }

}
