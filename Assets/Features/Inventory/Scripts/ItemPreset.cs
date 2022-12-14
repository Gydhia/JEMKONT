using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DownBelow.Inventory;
using DownBelow.Managers;
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
        public ItemTypes Type;
        [OnValueChanged("_updateUID")]
        public string ItemName; 
        public Sprite InventoryIcon;
        public Outline ItemPrefab;

        public int MaxStack = 1;

        //Fields

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
 