using DownBelow;
using DownBelow.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Inventory
{
    public class InventoryItem
    {
        #region EVENTS
        public event ItemEventData.Event OnItemChanged;

        public void FireItemChanged()
        {
            this.OnItemChanged?.Invoke(new ItemEventData(this));
        }
        #endregion

        public ItemPreset ItemPreset;
        public int Quantity { get; private set; }
        public int Slot;

        public void RemoveQuantity(int quantity = -1)
        {
            this.Quantity -= quantity == -1 ? this.Quantity : quantity;
            if (this.Quantity <= 0)
                this.ItemPreset = null;

            this.FireItemChanged();
        }

        public void AddQuantity(int quantity)
        {
            this.Quantity += quantity;
            this.FireItemChanged();
        }

        public void Init(ItemPreset RefItem, int Slot, int Quantity)
        {
            this.ItemPreset = RefItem;
            this.Slot = Slot;
            this.Quantity = Quantity;
        }
    }

    [Serializable]
    public struct ItemData
    {
        public Guid Name { get; set; }
        public int Quantity { get; set; }

        public ItemData(Guid Name, int Quantity)
        {
            this.Name = Name;
            this.Quantity = Quantity;
        }
    }
}
