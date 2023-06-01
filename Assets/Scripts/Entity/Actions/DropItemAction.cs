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
        public Cell ToCell;
        public Cell FromCell;
        public ItemPreset Item;
        public short Quantity;
        public short ToSlot;
        public short FromSlot;
        public bool OverUI;

        public DropItemAction(CharacterEntity RefEntity, Cell ToCell, string FromCell, string UID, string quantity, string overUI, string toSlot = "-1", string fromSlot = "-1") 
            : base(RefEntity, ToCell)
        {
            var GUID = Guid.Parse(UID);
            
            if (string.IsNullOrEmpty(FromCell))
            {
                this.FromCell = null;
            }
            else
            {
                int[] pos = FromCell.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                this.FromCell = this.RefEntity.CurrentGrid.Cells[pos[0], pos[1]];
            }

            this.Item = SettingsManager.Instance.ItemsPresets[GUID];
            short.TryParse(quantity, out this.Quantity);
            short.TryParse(toSlot, out this.ToSlot);
            short.TryParse(fromSlot, out this.FromSlot);
            bool.TryParse(overUI, out this.OverUI);
        }

        public override void ExecuteAction()
        {
            var player = this.RefEntity as PlayerBehavior;

            // Dropping over UI
            if (this.OverUI)
            {
                bool fromPlayerToStorage = this.FromCell == null;
                var fromStorage = fromPlayerToStorage ? player.PlayerInventory : ((InteractableStorage)TargetCell.AttachedInteract).Storage;
                var toStorage = fromPlayerToStorage ? ((InteractableStorage)TargetCell.AttachedInteract).Storage : player.PlayerInventory;

                int remainings = toStorage.TryAddItem(this.Item, this.Quantity, this.ToSlot);
                fromStorage.RemoveItem(this.Item, this.Quantity - remainings, this.FromSlot);
            }
            // Dropping over grid
            else if(!OverUI && TargetCell.Datas.state == CellState.Walkable)
            {
                InventoryItem InvInt = new();
                InvInt.Init(Item, 0, (int)Quantity);
                TargetCell.DropDownItem(InvInt);
                if (Item is ToolItem tool)
                {
                    player.RemoveActiveTool(tool);
                    player.PlayerSpecialSlots.RemoveItem(tool, Quantity, ToSlot);
                }
                else
                {
                    player.PlayerInventory.RemoveItem(Item, Quantity, ToSlot);
                }
            }
                      
            this.EndAction();
        }

        public override object[] GetDatas()
        {
            return new object[6] {
                this.ToCell == null ? "" : (FromCell.PositionInGrid.latitude + "," + FromCell.PositionInGrid.longitude),
                Item.UID.ToString(),
                Quantity.ToString(), 
                OverUI.ToString(),
                ToSlot.ToString(), 
                FromSlot.ToString()
            };
        }

        public override void SetDatas(object[] Datas)
        {
            if (!string.IsNullOrEmpty(Datas[0].ToString()))
            {
                int[] pos = Datas[0].ToString().Split(';').Select(n => Convert.ToInt32(n)).ToArray();
                this.ToCell = this.TargetCell.RefGrid.Cells[pos[0], pos[1]];
            }
            Item = SettingsManager.Instance.ItemsPresets[Guid.Parse((string)Datas[1])];
            short.TryParse(Datas[2].ToString(), out Quantity);
            bool.TryParse(Datas[3].ToString(), out OverUI);
            short.TryParse(Datas[4].ToString(), out ToSlot);
            short.TryParse(Datas[5].ToString(), out FromSlot);
        }
    }
}