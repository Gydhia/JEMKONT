using DownBelow.GridSystem;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Entity
{
    public class GatheringAction : ProgressiveAction
    {
        public InteractableResource CurrentRessource = null;

        public GatheringAction(CharacterEntity RefEntity, Cell TargetCell)
            : base(RefEntity, TargetCell)
        {
            this.CurrentRessource = (InteractableResource)TargetCell.AttachedInteract;
        }

        public override void ExecuteAction()
        {
            if (CurrentRessource != null && CurrentRessource.isMature)
                GameManager.Instance.StartCoroutine(this._gatherResource());
            else EndAction();
        }

        public IEnumerator _gatherResource()
        {
            ResourcePreset preset = CurrentRessource.InteractablePreset as ResourcePreset;
            ((PlayerBehavior)this.RefEntity).FireGatheringStarted(CurrentRessource);

            float timer = 0f;
            while (timer < preset.TimeToGather)
            {
                timer += Time.deltaTime;
                yield return null;

                if (this.abortAction)
                {
                    ((PlayerBehavior)this.RefEntity).FireGatheringCanceled(CurrentRessource);
                    this.abortAction = false;
                    EndAction();
                    yield break;
                }
            }
            CurrentRessource.Interact(this.RefEntity as PlayerBehavior);

            ((PlayerBehavior)this.RefEntity).FireGatheringEnded(CurrentRessource);

            if (!this.abortAction)
                EndAction();
        }

        public override object[] GetDatas()
        {
            return new object[0];
        }

        public override void SetDatas(object[] Datas)
        {

        }
    }
}
