using DownBelow.Entity;
using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Entity
{
    public class AttackingAction : EntityAction
    {
        public AttackingAction(CharacterEntity RefEntity, Cell TargetCell) 
            : base(RefEntity, TargetCell)
        {
        }

        public override void ExecuteAction()
        {
            // Should be a TargettingAction that return the cell to target, aka the entity. Only for Enemies 
            if (this.contextAction != null)
            {
                Debug.Log(contextAction.TargetCell);
                this.ProcessContextAction();
            }
            
            if (this._isInRange())
            {
                this.RefEntity.Animator.SetTrigger("Attack");
                TargetCell.EntityIn.ApplyStat(EntityStatistics.Health, -(this.RefEntity.Strength));
            }

            this.EndAction();
        }

        private bool _isInRange()
        {
            if (TargetCell == null || TargetCell.EntityIn == null || TargetCell.EntityIn.EntityCell == null)
                return false;

            GridPosition targPosition = TargetCell.EntityIn.EntityCell.PositionInGrid;
            var path = GridManager.Instance.FindPath(this.RefEntity, targPosition, true);
            return path != null && path.Count <= this.RefEntity.Range;
        }



        public override object[] GetDatas()
        {
            return new object[0];
        }

        public override void SetDatas(object[] Datas) { }
    }
}