using DownBelow.GridSystem;
using DownBelow.Managers;

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
            // Only the local player should execute the UI action
            if (CurrentRessource != null && CurrentRessource.isMature && this.RefEntity == GameManager.RealSelfPlayer)
            {
                UIManager.Instance.GatherSection.StartInteract(this, 3);
                var player = this.RefEntity as PlayerBehavior;

                player.FireGatheringStarted(CurrentRessource);
            }
            else
            {
                EndAction();
            }
        }

        // This should normally only be called locally
        public void OnGatherEnded()
        {
            var resAction = new GatherResourceAction(this.RefEntity, this.TargetCell);

            ResourcePreset rPreset = this.CurrentRessource.LocalPreset;
            System.Random generator = new System.Random(this.RefEntity.UID.GetHashCode());
            resAction.Init(generator.Next(rPreset.MinGathering, rPreset.MaxGathering));

            NetworkManager.Instance.EntityAskToBuffAction(resAction);
            var player = this.RefEntity as PlayerBehavior;

            player.FireGatheringEnded(CurrentRessource);

            this.EndAction();
        }

        public override object[] GetDatas()
        {
            return new object[0];
        }

        public override void SetDatas(object[] Datas) { }
    }
}
