using DownBelow;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Managers
{
    public enum ItemTypes
    {
        EQUIPEMENT = 0,
        CONSOMMABLE = 1,
        RESSOURCE = 2,
        THROWABLE = 3,
        COUNT

    }

    public class InventoryManager : _baseManager<InventoryManager>
    {

        //Properties
        public List<ItemPreset> Inventory => inventory;

        //Fields
        [HideInInspector]
        [SerializeField]
        private List<ItemPreset> inventory;

        public Action<ItemPreset> OnItemAdded;
        public Action<ItemPreset> OnItemRemoved;


        public void AddItemToInventory(ItemPreset item)
        {
            inventory.Add(item);
            OnItemAdded?.Invoke(item);
        }

        public void RemoveItemFromIventory(ItemPreset item)
        {
            inventory.Remove(item);
            OnItemRemoved(item);
        }
    }

}
