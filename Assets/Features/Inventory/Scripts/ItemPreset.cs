using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DownBelow.Inventory;
using Jemkont.Managers;
using Sirenix.OdinInspector;
using System;

namespace DownBelow
{
    [CreateAssetMenu(fileName = "Item", menuName = "DownBelow/ScriptableObject/Item", order = 1)]
    public class ItemPreset : SerializedScriptableObject
    {
        [ReadOnly]
        public Guid UID;

        //Properties
        public ItemTypes Type => type;
        public string ItemName => itemName; 
        public Sprite InventoryIcon => inventoryIcon;
        public Outline ItemPrefab => itemPrefab;

        //Fields
        [SerializeField] private ItemTypes type;
        [OnValueChanged("_updateUID")][SerializeField] private string itemName;
        [SerializeField] private Sprite inventoryIcon;
        [SerializeField] private Outline itemPrefab;

        private void _updateUID()
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(ItemName));
                this.UID = new Guid(hash);
            }
        }
    }

}
 