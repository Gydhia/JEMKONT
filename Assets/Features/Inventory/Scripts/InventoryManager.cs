using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Inventory
{
    public enum ItemTypes
    {
        EQUIPEMENT = 0,
        CONSOMMABLE = 1,
        RESSOURCE = 2,
        THROWABLE = 3,
        COUNT

    }

    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance;

        //Properties
        public List<Item> Inventory => inventory;

        //Fields
        [HideInInspector]
        [SerializeField]
        private List<Item> inventory;

        public Action<Item> OnItemAdded;
        public Action<Item> OnItemRemoved;

        private void Awake()
        {
            Instance = this;
        }

        public void AddItemToInventory(Item item)
        {
            inventory.Add(item);
            OnItemAdded?.Invoke(item);
        }

        public void RemoveItemFromIventory(Item item)
        {
            inventory.Remove(item);
            OnItemRemoved(item);
        }
    }

}
