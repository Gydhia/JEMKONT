using DownBelow.GridSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Entity
{
    public class EnemyAction : EntityAction
    {
        public Action EndTurnCallback = null;
        Action Behavior;

        public EnemyAction(CharacterEntity RefEntity, Cell TargetCell, Action ExecuteAction) : base(RefEntity, TargetCell)
        {
            this.RefEntity = RefEntity;
            this.TargetCell = TargetCell;
            this.Behavior = ExecuteAction;
        }

        public EnemyAction(CharacterEntity RefEntity, Cell TargetCell) : base(RefEntity, TargetCell)
        {
            this.RefEntity = RefEntity;
            this.TargetCell = TargetCell;
        }

        public override void ExecuteAction()
        {
            this.Behavior();
            /*if (this.RefEntity.CurrentGrid.IsCombatGrid)
            {
                if (this.RefEntity.IsMoving || !this.AllowedToProcess())
                {
                    EndAction();
                    return;
                }

                // -1 since the own entity's cell is included
                this.RefEntity.ApplyStat(EntityStatistics.Speed, -(this.calculatedPath.Count - 1));
            }

            base.MoveWithPath();*/
            this.EndAction();
        }

        public override object[] GetDatas()
        {
            object[] Datas = new object[3];
            Datas[0] = this.RefBuffer;
            Datas[1] = this.RefEntity;
            Datas[2] = this.TargetCell;

            return Datas;
        }

        public override void SetDatas(object[] Datas)
        {
            throw new NotImplementedException();
        }

    }
}