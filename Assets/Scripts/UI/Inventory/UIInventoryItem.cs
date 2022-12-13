using DownBelow;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DownBelow.UI.Inventory
{
    public class UIInventoryItem : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI quantity;
        [SerializeField] private Button selfButton;

        public void Init(ItemPreset preset, int quantity)
        {
            if(preset != null)
            {
                this.icon.sprite = preset.InventoryIcon;
                this.quantity.text = quantity.ToString();
            }
            else
            {
                this.icon.sprite = null;
                this.quantity.text = string.Empty;
            }
        }
    }
}