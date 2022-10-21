using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Inventory
{
    public class ChestStockage : MonoBehaviour
    {
        [SerializeField] private List<Item> itemsInChest = new List<Item>();
    }
}

