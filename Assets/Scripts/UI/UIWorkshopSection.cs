using DownBelow.Events;
using DownBelow.GridSystem;
using DownBelow.Managers;
using DownBelow.UI.Inventory;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI
{
    public class UIWorkshopSection : MonoBehaviour
    {
        public UIStorage UIStorage;
        public InteractableWorkshop CurrentWorkshop;

        public TextMeshProUGUI WorkshopName;

        public Button CraftButton;

        public UIInventoryItem InItem;
        public UIInventoryItem OutItem;
        public UIInventoryItem FuelItem;

        public Transform ScrollWheel;
        public Transform InnerScrollWheel;

        public void Init()
        {
            this.gameObject.SetActive(false);
        }

        public void ShowWorkshop(InteractableWorkshop workshop, bool isRealPlayer)
        {
            if(this.CurrentWorkshop != null)
            {
                this.CurrentWorkshop.Storage.OnStorageItemChanged -= _refreshWorkshop;
            }

            this.CurrentWorkshop = workshop;

            this.CurrentWorkshop.Storage.OnStorageItemChanged += _refreshWorkshop;


            this.WorkshopName.text = workshop.WorkshopName();
            this.UIStorage.SetStorageAndShow(workshop.Storage, isRealPlayer);

            this.InItem.OnlyAcceptedItem = workshop.InputItem;
            this.FuelItem.OnlyAcceptedItem = workshop.FuelItem;
            this.OutItem.CanOnlyTake = true;

            this._refreshWorkshop(null);
        }

        private void _refreshWorkshop(ItemEventData Data)
        {
            this.CraftButton.interactable = this.InItem.SelfItem.ItemPreset != null && this.FuelItem.SelfItem.ItemPreset != null;
        }

        public void OnClickCraft()
        {
            if(this.InItem.SelfItem.ItemPreset != null)
            {
                for (int i = 0; i < this.InItem.TotalQuantity / 3; i++)
                {
                    NetworkManager.Instance.GiftOrRemoveStorageItem(this.CurrentWorkshop, this.CurrentWorkshop.OutputItem, 1, this.OutItem.Slot);
                    NetworkManager.Instance.GiftOrRemoveStorageItem(this.CurrentWorkshop, this.InItem.SelfItem.ItemPreset, -3, this.InItem.Slot);
                    NetworkManager.Instance.GiftOrRemoveStorageItem(this.CurrentWorkshop, this.FuelItem.SelfItem.ItemPreset, -1, this.FuelItem.Slot);
                }
            }

            this._refreshWorkshop(null);
        }

        public void OpenPanel()
        {
            this.gameObject.SetActive(true);
        }

        public void ClosePanel()
        {
            if(this.CurrentWorkshop != null)
            {
                this.CurrentWorkshop.Storage.OnStorageItemChanged -= _refreshWorkshop;
                this.CurrentWorkshop = null;
            }

            this.gameObject.SetActive(false);


        }
    }
}