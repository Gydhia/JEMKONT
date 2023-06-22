using DownBelow.Events;
using DownBelow.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DownBelow.UI
{
    public class UIEnchantSection : MonoBehaviour
    {
        public Transform ItemsHolder;
        public UIEnchantItem EnchantItemPrefab;

        public List<UIEnchantItem> EnchantItems = new List<UIEnchantItem>();

        public void Init()
        {
            foreach (var tPreset in CardsManager.Instance.AvailableTools)
            {
                this.EnchantItems.Add(Instantiate(this.EnchantItemPrefab, this.ItemsHolder));
                this.EnchantItems[^1].Init(tPreset);
            }

            GameManager.RealSelfPlayer.PlayerInventory.OnStorageItemChanged += RefreshRecipesCraft;

            this.gameObject.SetActive(false);
        }

        private void RefreshRecipesCraft(ItemEventData Data)
        {
            foreach (var crafting in this.EnchantItems)
            {
                crafting.RefreshCanCraft();
            }
        }

        public void ClosePanel()
        {
            this.gameObject.SetActive(false);
        }

        public void OpenPanel()
        {
            this.gameObject.SetActive(true);

            foreach (var enchantItem in this.EnchantItems)
            {
                // Only enable it if it's owned by the local player
                enchantItem.LockImage.gameObject.SetActive(enchantItem.RefTool.ActualPlayer != GameManager.RealSelfPlayer); 
            }
        }
    }
}
