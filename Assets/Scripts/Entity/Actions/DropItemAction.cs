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

        public DropItemAction(CharacterEntity RefEntity, Cell TargetCell, string UID, short quantity) : base(RefEntity, TargetCell)
        {
            var GUID = Guid.Parse(UID);
            this.Item = GridManager.Instance.ItemsPresets[GUID];
            this.quantity = quantity;
        }

        public override void ExecuteAction()
        {
            InventoryItem InvInt = new();
            InvInt.Init(Item, 0, (int)quantity);
            TargetCell.DropDownItem(InvInt);
            this.EndAction();
        }

        public override object[] GetDatas()
        {
            return new object[2] { Item.UID.ToString(), (short)quantity };
            //Confused²
        }

        public override void SetDatas(object[] Datas)
        {
            Item = GridManager.Instance.ItemsPresets[Guid.Parse((string)Datas[0])];
            quantity = (short)Datas[1];
            //throw new System.NotImplementedException();
            //Confused
        }
    }
}