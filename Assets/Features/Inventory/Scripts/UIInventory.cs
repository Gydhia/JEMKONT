using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Jemkont.Managers;
using Jemkont.UI.Inventory;

namespace DownBelow.Inventory
{
    public class UIInventory : MonoBehaviour
    {
        private List<UIInventoryItem> Items;

        private void OnEnable()
        {
            InventoryManager.Instance.OnItemAdded += OnItemAdded;
            InventoryManager.Instance.OnItemRemoved += OnItemRemoved;
        }

        private void OnDisable()
        {
            InventoryManager.Instance.OnItemAdded -= OnItemAdded;
            InventoryManager.Instance.OnItemRemoved -= OnItemRemoved;
        }

        private void OnItemAdded(ItemPreset item)
        {

        }

        private void OnItemRemoved(ItemPreset item)
        {

        }
    }

}
