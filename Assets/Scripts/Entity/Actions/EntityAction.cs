using DownBelow.GridSystem;
using DownBelow.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Entity
{
    public abstract class EntityAction
    {
        public CharacterEntity RefEntity;
        public Cell TargetCell;
        protected System.Action EndCallback;

        public EntityAction(CharacterEntity RefEntity, Cell TargetCell)
        {
            this.RefEntity = RefEntity;
            this.TargetCell = TargetCell;
        }

        public void SetCallback(System.Action EndCallback)
        {
            this.EndCallback = EndCallback;
        }

        public abstract void ExecuteAction();

        public virtual void EndAction()
        {
            // Remove this action since we ended it, or it'll create an infinite loop
            GameManager.Instance.RemoveTopFromBuffer(this.RefEntity);

            if (this.EndCallback != null)
                this.EndCallback.Invoke();
        }

        public abstract object[] GetDatas();
        public abstract void SetDatas(object[] Datas);

    }
}
