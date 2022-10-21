using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DownBelow.Inventory;
namespace DownBelow
{
    [CreateAssetMenu(fileName = "Item", menuName = "DownBelow/ScriptableObject/Item", order = 1)]
    public class Item : ScriptableObject
    {
        //Properties
        public ItemTypes Type => type;
        public string ItemName => itemName; 
        public Sprite InventoryIcon => inventoryIcon;

        //Fields
        [SerializeField] private ItemTypes type;
        [SerializeField] private string itemName;
        [SerializeField] private Sprite inventoryIcon; 
    }

}
 