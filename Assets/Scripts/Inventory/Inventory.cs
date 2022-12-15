using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Inventory
{
    public class Inventory : MonoBehaviour
    {
        public List<InventoryItem> Items;
        public int Slots;

        public void Init(int slots)
        {
            this.Slots = slots;
        }

    }

}
