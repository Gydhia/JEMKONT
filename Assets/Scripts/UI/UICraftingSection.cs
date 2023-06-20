using DownBelow.Events;
using DownBelow.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DownBelow.UI
{
    public class UICraftingSection : MonoBehaviour
    {

        public Transform ItemsHolder;
        public UICraftingItem CraftingItemPrefab;

        public List<UICraftingItem> CraftingItems = new List<UICraftingItem>();

        public void Init()
        {
            foreach (var cPreset in SettingsManager.Instance.CraftRepices)
            {
                this.CraftingItems.Add(Instantiate(this.CraftingItemPrefab, this.ItemsHolder));
                this.CraftingItems[^1].Init(cPreset);
            }

            PlayerInputs.player_tab.performed += _togglePanel;
            GameManager.Instance.OnEnteredGrid += _closePanel;

            GameManager.RealSelfPlayer.PlayerInventory.OnStorageItemChanged += RefreshRecipesCraft;

            this.gameObject.SetActive(false);
        }

        private void RefreshRecipesCraft(ItemEventData Data)
        {
            foreach (var crafting in this.CraftingItems)
            {
                crafting.RefreshCanCraft();
            }
        }

        private void _closePanel(EntityEventData Data)
        {
            this.gameObject.SetActive(false);   
        }

        private void _togglePanel(InputAction.CallbackContext context)
        {
            this.gameObject.SetActive(!this.gameObject.activeSelf);
        }
    }
}

