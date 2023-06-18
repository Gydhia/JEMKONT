using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Inventory;
using DownBelow.UI.Inventory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DownBelow.UI.Inventory
{
    [Serializable]
    public class BaseStorage
    {
        public Cell RefCell;
        public InventoryItem[] StorageItems;
        public int MaxSlots;

        #region EVENTS
        public event ItemEventData.Event OnStorageItemChanged;

        public void FireStorageItemChanged(InventoryItem Item)
        {
            this.OnStorageItemChanged?.Invoke(new ItemEventData(Item));
        }
        #endregion

        public void Init(StoragePreset preset, Cell RefCell)
        {
            this.RefCell = RefCell;
            this.Init(preset.MaxSlots);
        }

        public void Init(int slots, Cell RefCell)
        {
            this.RefCell = RefCell;
            this.Init(slots);
        }

        public void Init(int slots)
        {
            this.MaxSlots = slots;
            this.StorageItems = new InventoryItem[slots];

            for (int i = 0; i < this.StorageItems.Length; i++)
                this.StorageItems[i] = new InventoryItem();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="preset"></param>
        /// <param name="quantity"></param>
        /// <param name="preferredSlot"></param>
        /// <param name="addAll"></param>
        /// <returns>The quantity that couldn't be added to the stack.</returns>
        public int TryAddItem(ItemPreset preset, int quantity, int preferredSlot = -1, bool addAll = true)
        {

            int remaining = quantity;
            int slot = preferredSlot != -1 ? preferredSlot : _getAvailableSlot(preset, true);

            // NO slot of THIS item
            if (slot == -1 || this.StorageItems[slot].ItemPreset == null)
            {
                slot = preferredSlot != -1 ? preferredSlot : _getAvailableSlot();

                // NO remaining slots
                if (slot == -1)
                    return remaining;
                // ADD into empty slot
                else
                {
                    int nbToAdd = remaining >= preset.MaxStack ? preset.MaxStack : remaining;

                    this.StorageItems[slot].Init(preset, slot, nbToAdd);
                    remaining -= nbToAdd;
                    this.FireStorageItemChanged(this.StorageItems[slot]);

                    return (remaining > 0 && addAll) ? this.TryAddItem(preset, remaining) : remaining;
                }
            }
            // EXISTING slot of THIS item, or SPECIFIED slot
            else
            {
                int free = preset.MaxStack - this.StorageItems[slot].Quantity;
                if (free == 0) return quantity;
                free = free > remaining ? remaining : free;

                this.StorageItems[slot].AddQuantity(free);
                remaining -= free;

                this.FireStorageItemChanged(this.StorageItems[slot]);

                return (remaining > 0 && addAll) ? this.TryAddItem(preset, remaining) : remaining;
            }
        }


        private int _getAvailableSlot(ItemPreset preset, bool toAdd)
        {
            for (int i = 0; i < this.StorageItems.Length; i++)
                if (this.StorageItems[i] != null && this.StorageItems[i].ItemPreset == preset && (!toAdd || this.StorageItems[i].Quantity < preset.MaxStack))
                    return i;
            return -1;
        }


        private int _getAvailableSlot()
        {
            for (int i = 0; i < this.StorageItems.Length; i++)
                if (this.StorageItems[i].ItemPreset == null)
                    return i;
            return -1;
        }


        /// <summary>
        /// To remove item to the storage
        /// </summary>
        /// <param name="item">The item preset</param>
        /// <param name="quantity">The number to remove, -1 if everything</param>
        public void RemoveItem(ItemPreset preset, int quantity, int preferredSlot = -1)
        {
            int slot = preferredSlot != -1 ? preferredSlot : this._getAvailableSlot(preset, false);

            if (slot != -1)
            {
                this.StorageItems[slot].RemoveQuantity(quantity);
            }
        }

        public bool HasResources(ItemPreset preset, int quantity)
        {
            int foundQuantity = 0;
            for (int i = 0; i < this.StorageItems.Length; i++)
            {
                if (this.StorageItems[i] != null && this.StorageItems[i].ItemPreset == preset)
                {
                    foundQuantity += this.StorageItems[i].Quantity;
                }
            }

            return foundQuantity >= quantity;
        }
    }

    public struct StorageData
    {
        public Dictionary<Guid, int> StoredItems { get; set; }

        public StorageData(Dictionary<Guid, int> StoredItems)
        {
            this.StoredItems = StoredItems;
        }
    }
}

