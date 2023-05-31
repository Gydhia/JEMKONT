using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using UnityEngine;

namespace DownBelow.Entity
{
    public class EnemyMovementAction : CombatMovementAction
    {
        public MovementType Type;

        public EnemyMovementAction(CharacterEntity RefEntity, Cell TargetCell, string Type) 
            : base(RefEntity, TargetCell)
        {
            this.Type = (MovementType)System.Enum.Parse(typeof(MovementType), Type);
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

        public override object[] GetDatas()
        {
            return new object[1] { this.Type.ToString() };
        }

        public override void SetDatas(object[] Datas)
        {
            base.SetDatas(Datas);
            this.Type = (MovementType)System.Enum.Parse(typeof(MovementType), Datas[0].ToString());
        }
    }

}
