using DownBelow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Events
{
    public class ItemEventData : EventData<ItemEventData>
    {
        public ItemPreset Item;
        public int Quantity;

        public ItemEventData(ItemPreset Item, int Quantity)
        {
            this.Item = Item;
            this.Quantity = Quantity;
        }
    }
}
