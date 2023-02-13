using DownBelow;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DownBelow.UI.Inventory
{
    public class UIInventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI quantity;
        [SerializeField] private Button selfButton;

        public int TotalQuantity = 0;
        public ItemPreset ItemPreset;

        private Transform _parentAfterDrag;

        private void Start()
        {
            this.RemoveItem();
        }

        public void Init(ItemPreset preset, int quantity)
        {
            this.ItemPreset = preset;

            if(preset != null)
            {
                this.icon.sprite = preset.InventoryIcon;
                this.TotalQuantity = quantity;
                this.quantity.text = this.TotalQuantity.ToString();
            }
            else
            {
                this.icon.sprite = Managers.SettingsManager.Instance.GameUIPreset.ItemCase;
                this.quantity.text = string.Empty;
                this.TotalQuantity = 0;
            }
        }

        /// <summary>
        /// To update the quantity of UI Item. Negative to remove, positive to add
        /// </summary>
        /// <param name="quantity"></param>
        public void UpdateQuantity(int quantity)
        {
            this.TotalQuantity += quantity;
            this.quantity.text = this.TotalQuantity.ToString();
        }

        public void RemoveItem()
        {
            this.icon.sprite = Managers.SettingsManager.Instance.GameUIPreset.ItemCase;
            this.quantity.text = string.Empty;
            this.TotalQuantity = 0;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            this._parentAfterDrag = transform.parent;
            this.transform.SetParent(transform.root);
            this.transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            this.transform.position = Input.mousePosition;
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            transform.SetParent(this._parentAfterDrag);
        }

    }
}