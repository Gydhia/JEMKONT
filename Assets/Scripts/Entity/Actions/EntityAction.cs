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

        /// <summary>
        /// Context action is a way to link an action to another to handle datas or something else.
        /// You can put whatever you want but don't forget to parse it
        /// </summary>
        [HideInInspector]
        public EntityAction ContextAction;

        protected List<Action> EndCallbacks = new List<Action>();

        public virtual bool AllowedToProcess() { return true; }

        public EntityAction(CharacterEntity RefEntity, Cell TargetCell)
        {
            this.RefEntity = RefEntity;
            this.TargetCell = TargetCell;
        }

        public void SetCallback(Action EndCallback)
        {
            this.EndCallbacks.Add(EndCallback);
        }

        public void SetContextAction(EntityAction ContextAction)
        {
            this.ContextAction = ContextAction;
        }
        public virtual void ProcessContextAction()
        {
            this.TargetCell = this.ContextAction.TargetCell;
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

            if (this.EndCallbacks != null)
            {
                foreach(Action callback in this.EndCallbacks)
                {
                    Debug.Log($"Before invoking endcallback\n{callback.Method.Name}.\n{GameManager.Instance.BufferStatus()}");
                    callback.Invoke();
                    Debug.Log($"Invoked endcallback\n{callback.Method.Name}.\n{GameManager.Instance.BufferStatus()}");
                }
            }
        }

        public abstract object[] GetDatas();
        public abstract void SetDatas(object[] Datas);
    }
}
