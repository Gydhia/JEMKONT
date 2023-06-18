using DownBelow.Events;
using DownBelow.Inventory;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DownBelow.UI.Inventory
{
    public class UIStorage : MonoBehaviour
    {
        public BaseStorage Storage;
        public Transform ItemsHolder;

        public List<UIInventoryItem> Items = new List<UIInventoryItem>();

        public void SetStorageAndShow(BaseStorage storageRef, bool show = true)
        {
            if (this.Storage != null)
                this.Storage.OnStorageItemChanged -= _refreshStorage;

            // We cancel if no new storage
            if (storageRef == null)
                return;

            this.Storage = storageRef;

            this.Storage.OnStorageItemChanged += _refreshStorage;

            if (show)
            {
                this.gameObject.SetActive(true);
            }

            this._init();
        }

        private void _refreshStorage(ItemEventData Data)
        {
            this.Items[Data.ItemData.Slot].RefreshItem(Data);
        }

        /// <summary>
        ///  To update the view when newly set storage
        /// </summary>
        private void _init()
        {
            int gap = this.Items.Count <= 0 ? 0 : this.Items.Count - this.Storage.MaxSlots ;

            for (int r = 0; r <= gap; r++)
            {
                this.Items[r + this.Storage.MaxSlots - 1].gameObject.SetActive(false);
            }

            for (int i = 0; i < this.Storage.StorageItems.Length; i++)
            {
                if (this.Items.Count <= i)
                {
                    this.Items.Add(Instantiate(SettingsManager.Instance.GameUIPreset.ItemPrefab, ItemsHolder));
                }

                this.Items[i].Init(this.Storage.StorageItems[i], this, i);
            }

        }

        public void HideStorage()
        {
            this.gameObject.SetActive(false);
        }
    }
}