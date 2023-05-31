using DownBelow.GridSystem;
using DownBelow.Inventory;
using DownBelow.Managers;
using System;
using System.Linq;
using UnityEditor;

namespace DownBelow.Entity
{
    public class DropItemAction : EntityAction
    {
        public ItemPreset Item;
        public short quantity;
        public short preferedSlot;

        public DropItemAction(CharacterEntity RefEntity, Cell TargetCell, string UID, string quantity, string preferedSlot = "-1") : base(RefEntity, TargetCell)
        {
            var GUID = Guid.Parse(UID);
            this.Item = SettingsManager.Instance.ItemsPresets[GUID];
            short.TryParse(quantity, out this.quantity);
            short.TryParse(preferedSlot, out this.preferedSlot);
        }

        public override void ExecuteAction()
        {
            InventoryItem InvInt = new();
            InvInt.Init(Item, 0, (int)quantity);
            TargetCell.DropDownItem(InvInt);
            if(Item is ToolItem tool)
            {
                ((PlayerBehavior)this.RefEntity).RemoveActiveTool(tool);
                ((PlayerBehavior)RefEntity).PlayerSpecialSlots.RemoveItem(tool, quantity, preferedSlot);
            }
            else
            {
                ((PlayerBehavior)RefEntity).PlayerInventory.RemoveItem(Item, quantity, preferedSlot);
            }
            this.EndAction();
        }

        public override object[] GetDatas()
        {
            return new object[3] { Item.UID.ToString(), quantity.ToString(), preferedSlot.ToString() };
        }

        public override void SetDatas(object[] Datas)
        {
            Item = SettingsManager.Instance.ItemsPresets[Guid.Parse((string)Datas[0])];
            short.TryParse(Datas[1].ToString(), out quantity);
            short.TryParse(Datas[2].ToString(), out preferedSlot);
        }
    }
}