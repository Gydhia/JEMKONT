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
        public Cell FromCell;
        public ItemPreset Item;
        public int Quantity;
        public int ToSlot;
        public int FromSlot;
        public bool OverUI;

        public DropItemAction(CharacterEntity RefEntity, Cell TargetCell) 
            : base(RefEntity, TargetCell)
        {
        }

        public virtual void Init(Cell FromCell, ItemPreset Item, int Quantity, bool OverUI, int ToSlot = -1, int FromSlot = -1)
        {
            this.FromCell = FromCell;
            this.Item = Item;
            this.Quantity = Quantity;
            this.OverUI = OverUI;
            this.ToSlot = ToSlot;
            this.FromSlot = FromSlot;
        }

        public override void ExecuteAction()
        {
            var player = this.RefEntity as PlayerBehavior;

            // Dropping over UI
            if (this.OverUI)
            {
                // We assume that a null cell means playerInventory
                var fromStorage = FromCell == null ? player.PlayerInventory : ((InteractableStorage)FromCell.AttachedInteract).Storage;
                var toStorage = TargetCell == null ? player.PlayerInventory : ((InteractableStorage)TargetCell.AttachedInteract).Storage;

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
                FromCell == null ? null : FromCell.PositionInGrid.GetData(),
                Item.UID,
                Quantity, 
                OverUI,
                ToSlot, 
                FromSlot
            };
        }

        public override void SetDatas(object[] Datas)
        {
            if (Datas[0] != null)
            {
                int[] pos = Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(Datas[0].ToString());
                this.FromCell = this.RefGrid.Cells[pos[0], pos[1]];
            }
            
            this.Item = SettingsManager.Instance.ItemsPresets[Guid.Parse((string)Datas[1])];
            this.Quantity = Convert.ToInt32(Datas[2]);
            this.OverUI = (bool)Datas[3];
            this.ToSlot = Convert.ToInt32(Datas[4]);
            this.FromSlot = Convert.ToInt32(Datas[5]);
        }
    }
}