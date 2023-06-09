using DownBelow.GridSystem;
namespace DownBelow.Entity
{
    public class PickupItemAction : EntityAction
    {
        public ItemPreset Item;
        public int quantity;

        public PickupItemAction(CharacterEntity RefEntity, Cell TargetCell) : base(RefEntity, TargetCell)
        {
            this.Item = TargetCell.ItemContained.ItemPreset;
            this.quantity = TargetCell.ItemContained.Quantity;
        }

        public override void ExecuteAction()
        {
            TargetCell.TryPickUpItem((PlayerBehavior)RefEntity);
            this.EndAction();
        }

        public override object[] GetDatas()
        {
            return new object[0];
        }

        public override void SetDatas(object[] Datas) { }
    }

}