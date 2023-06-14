using DownBelow.Events;
using DownBelow.GridSystem;
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
        public BaseStorage SelfStorage;

        public TextMeshProUGUI WorkshopName;

        public Button CraftButton;

        public UIInventoryItem InItem;
        public UIInventoryItem OutItem;
        public UIInventoryItem FuelItem;

        public Transform ScrollWheel;
        public Transform InnerScrollWheel;

        public void Init()
        {
            this.SelfStorage = new BaseStorage();
            this.SelfStorage.Init(3);
            this.UIStorage.SetStorageAndShow(this.SelfStorage, false);

            this.gameObject.SetActive(false);

            this.SelfStorage.OnStorageItemChanged += _refreshWorkshop;
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
            this.gameObject.SetActive(false);
        }
    }
}