using DownBelow.GridSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Entity
{
    public class MovementAction : EntityAction
    {
        private string _otherGrid;

        public void Init(CharacterEntity RefEntity, Cell TargetCell, string otherGrid)
        {
            this.RefEntity = RefEntity;
            this.TargetCell = TargetCell;

            this._otherGrid = otherGrid;
        }

        //public override void ExecuteAction(CharacterEntity RefEntity, Action EndCallback)
        //{
        //    base.ExecuteAction(RefEntity, EndCallback);


        //    // TODO: fcking do every movement processing here
        //    if (this.RefEntity is PlayerBehavior player)
        //        player.TryGoTo(this.TargetCell, player.Speed);
        //    else
        //        this.RefEntity.TryGoTo(TargetCell, RefEntity.Speed);
        //}

        public override void EndAction()
        {
            base.EndAction();
        }

    }

}