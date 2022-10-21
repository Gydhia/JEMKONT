using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DownBelow.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
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

        private void OnItemAdded(Item item)
        {

        }

        private void OnItemRemoved(Item item)
        {

        }
    }

}
