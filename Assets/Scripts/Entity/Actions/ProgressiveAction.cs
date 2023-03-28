using DownBelow.GridSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Entity
{
    public abstract class ProgressiveAction : EntityAction
    {
        protected bool abortAction = false;
        public ProgressiveAction(CharacterEntity RefEntity, Cell TargetCell) : base(RefEntity, TargetCell)
        {
        }

        public virtual void AbortAction()
        {
            abortAction = true;
        }
    }
}