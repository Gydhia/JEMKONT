using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Entity
{
    public abstract class EntityAction
    {
        protected CharacterEntity RefEntity;
        protected System.Action EndCallback;

        public virtual void ExecuteAction(CharacterEntity RefEntity, System.Action EndCallback)
        {
            this.RefEntity = RefEntity;
            this.EndCallback = EndCallback;
        }

        public virtual void EndAction()
        {
            if (this.EndCallback != null)
                this.EndCallback.Invoke();
        }

    }
}
