using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Entity
{
    public abstract class EntityAction
    {
        protected CharacterEntity RefEntity;
        protected Cell TargetCell;
        protected System.Action EndCallback;

        public void SetCallback(System.Action EndCallback)
        {
            this.EndCallback = EndCallback;
        }

        public virtual void ExecuteAction()
        {

        }

        public virtual void EndAction()
        {
            if (this.EndCallback != null)
                this.EndCallback.Invoke();
        }

    }
}
