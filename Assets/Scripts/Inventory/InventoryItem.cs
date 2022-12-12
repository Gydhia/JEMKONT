using DownBelow;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Inventory
{
    public class InventoryItem : MonoBehaviour
    {
        public ItemPreset RefItem;
        public int Quantity;

        public void Init(ItemPreset preset, int quantity = 1)
        {
            RefItem = preset;
            this.Quantity = quantity;
        }
    }

    [Serializable]
    public struct ItemData
    {
        public Guid Name { get; set; }
        public int Quantity { get; set; }

        public ItemData(Guid Name, int Quantity)
        {
            this.Name = Name;
            this.Quantity = Quantity;
        }
    }
}
