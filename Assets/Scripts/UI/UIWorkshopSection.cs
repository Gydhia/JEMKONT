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
            workshop.Storage.OnStorageItemChanged -= _refreshWorkshop;

            this.CurrentWorkshop = workshop;

            this.WorkshopName.text = workshop.WorkshopName();
            this.UIStorage.SetStorageAndShow(workshop.Storage, isRealPlayer);
        }

        private void _refreshWorkshop(ItemEventData Data)
        {
            Debug.Log("Item : " + Data.ItemData.ItemPreset.ItemName + " added into furnace");
        }

        public void OnClickCraft()
        {

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