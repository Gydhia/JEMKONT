using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI
{
    public class UIResourceItem : MonoBehaviour
    {
        // Store datas
        public ItemPreset Preset;
        public int Quantity;

        public Image ResourceIcon;
        public TextMeshProUGUI AmountText;

        public void Init(ItemPreset preset, int count, bool hasResources)
        {
            this.Preset = preset;
            this.Quantity = count;

            this.ResourceIcon.sprite = preset.InventoryIcon;
            this.AmountText.text = count.ToString();
            
            this.AmountText.color = hasResources ? Color.white : Color.red;
        }
    }
}