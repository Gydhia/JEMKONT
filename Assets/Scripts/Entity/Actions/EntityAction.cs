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
        // Since we have the reference here, we can remove ourself without any help
        [HideInInspector]
        public List<EntityAction> RefBuffer;

        [HideInInspector]
        public CharacterEntity RefEntity;
        [HideInInspector]
        public Cell TargetCell;
        protected System.Action EndCallback;

        public virtual bool AllowedToProcess() { return true; }

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

        /// <summary>
        /// Notifies the buffer that the action is over. Normally, when overriding, you NEED to call the base.
        /// </summary>
        public virtual void EndAction()
        {
            // Remove this action since we ended it, or it'll create an infinite loop
            this.RefBuffer.Remove(this);
            PoolManager.Instance.CellIndicatorPool.HideActionIndicators(this);

            if (this.EndCallback != null)
            { 
                Debug.Log($"Before invoking endcallback\n{this.EndCallback.Method.Name}.\n{GameManager.Instance.BufferStatus()}");
                this.EndCallback.Invoke();
                Debug.Log($"Invoked endcallback\n{this.EndCallback.Method.Name}.\n{GameManager.Instance.BufferStatus()}");
            }
        }

        public abstract object[] GetDatas();
        public abstract void SetDatas(object[] Datas);
    }
}
