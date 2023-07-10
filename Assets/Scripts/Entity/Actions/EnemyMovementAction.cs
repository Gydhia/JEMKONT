using DownBelow.GridSystem;
using DownBelow.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using UnityEngine;

namespace DownBelow.Entity
{
    public class EnemyMovementAction : CombatMovementAction
    {
        public MovementType Type;

        public EnemyMovementAction(CharacterEntity RefEntity, Cell TargetCell) 
            : base(RefEntity, TargetCell)
        {
        }

        public virtual void Init(MovementType Type)
        {
            Debug.Log($"init: {Type}");
            this.Type = Type;
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
				case MovementType.Straight: 
                    path = this.MovementStraight(); break;
                case MovementType.Flee:
                    path = this.MovementFlee();
                    break;
				case MovementType.Kite:
				case MovementType.StraightToRange:
				default: 
                    path = this.MovementStraightToRange(); break;
			}
 
			if (path == null || path.Count == 0)
                return null;

            if(path.Count >= RefEntity.Speed)
                path.RemoveRange(RefEntity.Speed, (path.Count - 1) - (RefEntity.Speed - 1));

            return path;
		}

		private List<Cell> MovementStraight()
        {
            if (TargetCell == null || TargetCell.EntityIn == null || TargetCell.EntityIn.EntityCell == null)
                return null;

            GridPosition targPosition = TargetCell.EntityIn.EntityCell.PositionInGrid;
            return GridManager.Instance.FindPath(this.RefEntity, targPosition, false, 1);
        }

        public bool IsValidDirection(Border direction)
        {
            return direction == Border.Left || direction == Border.Right ||
                   direction == Border.Top || direction == Border.Bottom;
        }

        public Cell GetNewPosition(Cell currentPosition, Border direction)
        {
            int newLatitude = currentPosition.Datas.heightPos;
            int newLongitude = currentPosition.Datas.widthPos;

            switch (direction)
            {
                case Border.Left:
                    newLongitude--;
                    break;
                case Border.Right:
                    newLongitude++;
                    break;
                case Border.Top:
                    newLatitude++;
                    break;
                case Border.Bottom:
                    newLatitude--;
                    break;
            }

            return currentPosition.RefGrid.Cells[newLatitude, newLongitude];
        }

        /// <summary>
        /// Will Go straight to the target stops when in range
        /// </summary>
        private List<Cell> MovementStraightToRange()
        {
            if (TargetCell == null || TargetCell.EntityIn == null || TargetCell.EntityIn.EntityCell == null)
                return null;

            GridPosition targPosition = TargetCell.EntityIn.EntityCell.PositionInGrid;
            return GridManager.Instance.FindPath(this.RefEntity, targPosition, false, this.RefEntity.Range);
        }

        public List<Cell> MovementFlee()
        {
            // For now it's gonna be a random cell
            var newTarget = this.TargetCell.RefGrid.Cells.RandomWalkable(GameManager.Instance.SaveName + CombatManager.Instance.EntityTurnRotation);

            if (newTarget == null)
                return null;

            return GridManager.Instance.FindPath(this.RefEntity, newTarget.PositionInGrid);
        }

        public override object[] GetDatas()
        {
            return new object[1] { this.Type.ToString() };
        }

        public override void SetDatas(object[] Datas)
        {
            this.Type = (MovementType)System.Enum.Parse(typeof(MovementType), Datas[0].ToString());
        }
	}

}
