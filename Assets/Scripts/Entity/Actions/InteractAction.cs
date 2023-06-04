using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Entity
{
    public class InteractAction : EntityAction
    {
        public InteractAction(CharacterEntity RefEntity, Cell TargetCell)
            : base(RefEntity, TargetCell)
        {
        }

        public override void ExecuteAction()
        {
            TargetCell.AttachedInteract.Interact(this.RefEntity as PlayerBehavior);
            EndAction();
        }

        public override object[] GetDatas()
        {
            return new object[0];
        }

        public override void SetDatas(object[] Datas) { }
    }
}