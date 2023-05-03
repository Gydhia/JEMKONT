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

        public DropItemAction(CharacterEntity RefEntity, Cell TargetCell, string UID, short quantity, short preferedSlot = -1) : base(RefEntity, TargetCell)
        {
            var GUID = Guid.Parse(UID);
            this.Item = GridManager.Instance.ItemsPresets[GUID];
            this.quantity = quantity;
            this.preferedSlot = preferedSlot;
        }

        public override void ExecuteAction()
        {
            InventoryItem InvInt = new();
            InvInt.Init(Item, 0, (int)quantity);
            TargetCell.DropDownItem(InvInt);
            if(Item is ToolItem)
            {
                ((PlayerBehavior)RefEntity).PlayerSpecialSlot.RemoveItem(Item, quantity, preferedSlot);
            } else
            {
                ((PlayerBehavior)RefEntity).PlayerInventory.RemoveItem(Item, quantity, preferedSlot);
            }
            this.EndAction();
        }

        public override object[] GetDatas()
        {
            return new object[3] { Item.UID.ToString(), (short)quantity, (short)preferedSlot };
        }

        public override void SetDatas(object[] Datas)
        {
            Item = GridManager.Instance.ItemsPresets[Guid.Parse((string)Datas[0])];
            quantity = (short)Datas[1];
            preferedSlot = (short)Datas[2];
        }
    }
}