using DownBelow.Events;
using DownBelow.UI.Inventory;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Inventory
{
    public class BaseStorage
    {
        #region EVENTS
        public event ItemEventData.Event OnItemChanged;

        public void FireItemChanged(ItemPreset Item, int Quantity) 
        {
            this.OnItemChanged?.Invoke(new ItemEventData(Item, Quantity));
        }
        #endregion

        public Dictionary<ItemPreset, int> StorageItems= new Dictionary<ItemPreset, int>();
        public int MaxSlots;

        public void Init(StoragePreset preset)
        {
            this.MaxSlots = preset.MaxSlots;
        }

        /// <summary>
        /// To add an item to the storage
        /// </summary>
        /// <param name="item">The item preset</param>
        /// <param name="quantity">The number to add, -1 if infinite</param>
        public void AddItem(ItemPreset item, int quantity = -1)
        {
            if(quantity > 0)
            {
                if (this.StorageItems.ContainsKey(item))
                    StorageItems[item] += quantity;
                else
                    this.StorageItems.Add(item, quantity);
            } 
            else
                this.StorageItems[item] = -1;

            this.FireItemChanged(item, this.StorageItems[item]);
        }

        /// <summary>
        /// To remove item to the storage
        /// </summary>
        /// <param name="item">The item preset</param>
        /// <param name="quantity">The number to remove, -1 if everything</param>
        public void RemoveItem(ItemPreset item, int quantity = -1)
        {
            if (quantity > 0)
            {
                if (this.StorageItems.ContainsKey(item))
                {
                    this.StorageItems[item] += quantity;
                    if(this.StorageItems[item] <= 0)
                        this.StorageItems.Remove(item);
                }
                else
                    this.StorageItems.Add(item, quantity);
            }
            else
                this.StorageItems.Remove(item);

            if (this.StorageItems.ContainsKey(item))
                this.FireItemChanged(item, this.StorageItems[item]);
            else
                this.FireItemChanged(item, 0);
        }

        public int GetNumberByStacks()
        {
            int stackNumber = 0;
            foreach (KeyValuePair<ItemPreset, int> item in this.StorageItems)
                stackNumber += item.Value % item.Key.MaxStack;

            return stackNumber;
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

