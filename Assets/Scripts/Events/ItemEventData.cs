using DownBelow;
using DownBelow.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Events
{
    public class ItemEventData : EventData<ItemEventData>
    {
        public InventoryItem ItemData;

        public ItemEventData(InventoryItem ItemData)
        {
            this.ItemData = ItemData;
        }
    }
}
