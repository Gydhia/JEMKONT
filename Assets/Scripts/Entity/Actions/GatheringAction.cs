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

        public GatheringAction(CharacterEntity RefEntity, Cell TargetCell, InteractableResource CurrentRessource) 
            : base(RefEntity, TargetCell)
        {
            this.CurrentRessource = CurrentRessource;
        }

        public void Init(InteractableResource CurrentRessource)
        {
            this.CurrentRessource = CurrentRessource;
        }

        public override void ExecuteAction()
        {
            GameManager.Instance.StartCoroutine(this._gatherResource());
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
                    yield break;
                }
            }
            CurrentRessource.Interact(this.RefEntity as PlayerBehavior);

            ((PlayerBehavior)this.RefEntity).FireGatheringEnded(CurrentRessource);
        }

        public override object[] GetDatas()
        {
            return new object[] { this.CurrentRessource };
        }

        public override void SetDatas(object[] Datas)
        {
            this.CurrentRessource = Datas[0] as InteractableResource;
        }
    }
}
