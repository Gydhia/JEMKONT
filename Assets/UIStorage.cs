using DownBelow.Events;
using DownBelow.Managers;
using DownBelow.UI.Inventory;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DownBelow.Inventory
{
    public class UIStorage : MonoBehaviour
    {
        public BaseStorage Storage;
        public Transform ItemsHolder;

        public List<UIInventoryItem> Items = new List<UIInventoryItem>();

        public void SetStorageAndShow(BaseStorage storageRef)
        {
            // We unsubscribe from the precedent storage
            if (this.Storage != null)
                this.Storage.OnItemChanged -= _updateStorage;

            // We cancel if no new storage
            if (storageRef == null)
                return;

            this.Storage = storageRef;

            this.gameObject.SetActive(true);
            this._init();
        }

        /// <summary>
        ///  To update the view when newly set storage
        /// </summary>
        private void _init()
        {
            int gap = this.Items.Count <= 0 ?
                0 : this.Storage.MaxSlots - this.Items.Count;

            if(gap > 0)
            {
                for (int i = 0; i < gap; i++)
                {
                    this.Items.Add(Instantiate(SettingsManager.Instance.GameUIPreset.ItemPrefab, ItemsHolder));
                    this.Items[^1].Init(null, 0);
                }
            }
            else
            {
                for (int i = this.Items.Count - 1; i >= this.Items.Count + gap; i--)
                    this.Items[i].gameObject.SetActive(false);
            }

            int counter = 0;
            foreach (var item in this.Storage.StorageItems)
            {
                this.Items[counter].Init(item.Key, item.Value);
                counter++;
            }

            this.Storage.OnItemChanged += _updateStorage;
        }

        /// <summary>
        /// To update the storage once set
        /// </summary>
        /// <param name="Data"></param>
        private void _updateStorage(ItemEventData Data)
        {
            // Ugly for now, rework this method later
            List<UIInventoryItem> existingItems = this.Items.Where(i => i.ItemPreset == Data.Item).ToList();

            int stacksToAdd = Mathf.CeilToInt((float)Data.Quantity / Data.Item.MaxStack);
            int remainings = Data.Quantity;

            if (existingItems.Count == 0)
            {
                for (int i = 0; i < stacksToAdd; i++)
                {
                    int nbToAdd = remainings >= Data.Item.MaxStack ? Data.Item.MaxStack : remainings;
                    this.Items[_getAvailableSlot()].Init(Data.Item, nbToAdd);
                    remainings -= nbToAdd;
                }
            }
            else
            {
                foreach (var item in existingItems) {
                    if(item.TotalQuantity < Data.Item.MaxStack)
                    {
                        // If we removed this item
                        if(Data.Quantity == 0)
                        {
                            item.RemoveItem();
                            continue;
                        }

                        // If not enough or just the right amount to complete the stack
                        if(remainings < Data.Item.MaxStack - item.TotalQuantity)
                        {
                            item.UpdateQuantity(remainings);
                            remainings = 0;
                        }
                        // Or more to complete the stack
                        else
                        {
                            item.UpdateQuantity(Data.Quantity - item.TotalQuantity);
                            remainings -= Data.Quantity - item.TotalQuantity; 
                        }
                    }
                }
                if(remainings > 0)
                {
                    stacksToAdd = Mathf.CeilToInt(remainings / Data.Item.MaxStack);
                    for (int i = 0; i < stacksToAdd; i++)
                    {
                        int nbToAdd = remainings >= Data.Item.MaxStack ? Data.Item.MaxStack : remainings;
                        this.Items[_getAvailableSlot()].Init(Data.Item, nbToAdd);
                        remainings -= nbToAdd;
                    }
                }
            }
        }

        private int _getAvailableSlot()
        {
            for (int i = 0; i < this.Items.Count; i++)
                if (this.Items[i].ItemPreset == null)
                    return i;
            return -1;
        }

        public void HideStorage()
        {
            this.gameObject.SetActive(false);
        }
    }
}