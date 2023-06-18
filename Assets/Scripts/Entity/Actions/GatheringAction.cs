using DownBelow.GridSystem;
using DownBelow.Managers;
using Photon.Realtime;
using System.Linq;

namespace DownBelow.Entity
{
    public class GatheringAction : PendularAction
    {
        public InteractableResource CurrentRessource = null;

        public GatheringAction(CharacterEntity RefEntity, Cell TargetCell)
            : base(RefEntity, TargetCell)
        {
            this.CurrentRessource = (InteractableResource)TargetCell.AttachedInteract;
        }

        public override void ExecuteAction()
        {
            if (GameManager.CurrentAvailableResources <= 0)
            {
                UIManager.Instance.DatasSection.ShowWarningText("It seems that this land is exhausted...");
                EndAction();
            }
            // Only the local player should execute the UI action
            else if (CurrentRessource != null && CurrentRessource.isMature && this.RefEntity == GameManager.RealSelfPlayer)
            {
                UIManager.Instance.GatherSection.StartInteract(this, this.requiredTicks);
            }
        }

        public override void LocalTick(bool result)
        {
            base.LocalTick(result);

            if (result)
            {
                var player = this.RefEntity as PlayerBehavior;

                var resTool = player.ActiveTools.First(t => t.Class == this.CurrentRessource.LocalPreset.GatherableBy);
                player.Animator.SetTrigger(resTool.GatherAnim);
            }
        }

        protected override void OnFinalTick()
        {
            if(this.succeededTicks > 0)
            {
                var player = this.RefEntity as PlayerBehavior;

                ResourcePreset rPreset = this.CurrentRessource.LocalPreset;

                // IMPORTANT : Use a seed for local random
                System.Random generator = new System.Random(this.RefEntity.UID.GetHashCode());
                int nbResourcers = generator.Next(rPreset.MinGathering, rPreset.MaxGathering);
                nbResourcers *= this.succeededTicks;

                this.CurrentRessource.Interact(player);
                GameManager.Instance.FireResourceGathered(this.CurrentRessource);

                if(this.RefEntity == GameManager.RealSelfPlayer)
                {
                    NetworkManager.Instance.GiftOrRemovePlayerItem(player.UID, this.CurrentRessource.LocalPreset.ResourceItem, nbResourcers);
                }
            }

            this.EndAction();
        }
    }
}
