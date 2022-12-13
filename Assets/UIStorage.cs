using DownBelow.Events;
using DownBelow.Managers;
using DownBelow.UI.Inventory;
using System.Collections;
using System.Collections.Generic;
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

            this.Storage.OnItemChanged += _updateStorage;

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
        }

        /// <summary>
        /// To update the storage once set
        /// </summary>
        /// <param name="Data"></param>
        private void _updateStorage(ItemEventData Data)
        {

        }

        public void HideStorage()
        {
            this.gameObject.SetActive(false);
        }
    }
}