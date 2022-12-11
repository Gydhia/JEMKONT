using DownBelow;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jemkont.UI.Inventory
{
    public class UIInventoryItem : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI quantity;
        [SerializeField] private Button selfButton;

        public void Init(ItemPreset preset, int quantity)
        {
            icon.sprite = preset.InventoryIcon;
            this.quantity.text = quantity.ToString();

        }
    }
}