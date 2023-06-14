using DownBelow.Managers;
using DownBelow.Mechanics;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI
{
    public class UICraftingItem : MonoBehaviour
    {
        private CraftPreset _craftPreset;
        public UIResourceItem ResourceItemPrefab;

        public List<UIResourceItem> ResourceItems = new List<UIResourceItem>();

        public Button CraftButton;
        public TextMeshProUGUI ItemName;
        public Image ItemImage;

        public void Init(CraftPreset preset)
        {
            this._craftPreset = preset;

            this.ItemName.text = preset.ItemName;
            this.ItemImage.sprite = preset.ItemIcon;

            bool hasAllResources = true;
            foreach (var craftKvp in preset.CraftRecipe)
            {
                bool hasResources = GameManager.RealSelfPlayer.PlayerInventory.HasResources(craftKvp.Key, craftKvp.Value);

                hasAllResources &= hasResources;                

                this.ResourceItems.Add(Instantiate(this.ResourceItemPrefab, this.transform));
                this.ResourceItems[^1].Init(craftKvp.Key, craftKvp.Value, hasResources);
            }

            this.CraftButton.interactable = hasAllResources;
        }

        public void RefreshCanCraft()
        {
            bool hasAllResources = true;
            foreach (var item in this.ResourceItems)
            {
                bool hasResource = GameManager.RealSelfPlayer.PlayerInventory.HasResources(item.Preset, item.Quantity);
                item.AmountText.color = hasResource ? Color.white : Color.red;

                hasAllResources &= hasResource;
            }

            this.CraftButton.interactable = hasAllResources;
        }

        public void OnClickCraft()
        {
            foreach (var itemKVP in this._craftPreset.CraftRecipe)
            {
                NetworkManager.Instance.GiftOrRemovePlayerItem(GameManager.RealSelfPlayer.UID, itemKVP.Key, -itemKVP.Value);
            }

            NetworkManager.Instance.GiftOrRemovePlayerItem(GameManager.RealSelfPlayer.UID, this._craftPreset.ToPlaceItem, 1);

        }
    }
}
