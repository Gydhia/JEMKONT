using DownBelow.GridSystem;

namespace DownBelow.Entity
{
    public class GatherResourceAction : ProgressiveAction
    {
        public InteractableResource CurrentRessource = null;
        public int GivenResources;

        public GatherResourceAction(CharacterEntity RefEntity, Cell TargetCell)
            : base(RefEntity, TargetCell)
        {
            this.CurrentRessource = (InteractableResource)TargetCell.AttachedInteract;
        }

        public void Init(int givenResources)
        {
            this.GivenResources = givenResources;
        }

        public override void ExecuteAction()
        {
            var player = this.RefEntity as PlayerBehavior;

            CurrentRessource.Interact(player);
            player.TakeResources(CurrentRessource.LocalPreset.ResourceItem, this.GivenResources);

            this.EndAction();
        }


        public override object[] GetDatas()
        {
            return new object[1] { this.GivenResources };
        }

        public override void SetDatas(object[] Datas) 
        {
            this.GivenResources = int.Parse(Datas[0].ToString());
        }
    }
}
