using DownBelow.Events;
using DownBelow.Inventory;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DownBelow.UI.Inventory
{
    public class UIInventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public static UIInventoryItem LastHoveredItem;

        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI quantity;
        [SerializeField] private Button selfButton;

        public int TotalQuantity = 0;
        public int Slot;
        public UIStorage SelfStorage;

        public InventoryItem SelfItem => this.SelfStorage.Storage.StorageItems[this.Slot];

        private Transform _parentAfterDrag;
        private Vector3 _positionAfterDrag;

        private void Start()
        {
            this.RemoveItem();
        }

        public void Init(InventoryItem Item, UIStorage refStorage, int slot)
        {
            this.gameObject.SetActive(true);

            this.SelfStorage = refStorage;
            this.Slot = slot;

            this.SelfItem.OnItemChanged += RefreshItem;

            if (this.SelfItem.ItemPreset != null)
            {
                this.icon.sprite = Item.ItemPreset.InventoryIcon;
                this.TotalQuantity = Item.Quantity;
                this.quantity.text = this.TotalQuantity.ToString();
            } else
            {
                this.icon.sprite = Managers.SettingsManager.Instance.GameUIPreset.ItemCase;
                this.quantity.text = string.Empty;
                this.TotalQuantity = 0;
            }
        }

        public void RefreshItem(ItemEventData Data)
        {
            if (Data.ItemData.Quantity > 0)
            {
                this.icon.sprite = Data.ItemData.ItemPreset.InventoryIcon;
                this.TotalQuantity = Data.ItemData.Quantity;
                this.quantity.text = this.TotalQuantity.ToString();
            } else
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
            if (this.TotalQuantity == 0)
                return;

            this._positionAfterDrag = selfButton.transform.position;
            this._parentAfterDrag = selfButton.transform.parent;
            selfButton.transform.SetParent(transform.root);
            selfButton.transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (this.TotalQuantity == 0)
                return;

            selfButton.image.raycastTarget = false;
            if (selfButton.targetGraphic != null)
                selfButton.targetGraphic.raycastTarget = false;
            selfButton.transform.position = Input.mousePosition;
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            if (this.TotalQuantity == 0)
                return;

            processEndDrag(eventData);

            selfButton.image.raycastTarget = true;
            if (selfButton.targetGraphic != null)
                selfButton.targetGraphic.raycastTarget = true;
            selfButton.transform.position = this._positionAfterDrag;
            selfButton.transform.SetParent(this._parentAfterDrag);
        }
        protected virtual void processEndDrag(PointerEventData eventData)
        {
            if (LastHoveredItem && LastHoveredItem != this)
            {
                int remainings = LastHoveredItem.SelfStorage.Storage.TryAddItem(this.SelfItem.ItemPreset, this.TotalQuantity, LastHoveredItem.Slot);
                this.SelfStorage.Storage.RemoveItem(this.SelfItem.ItemPreset, this.TotalQuantity - remainings, this.Slot);
            }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            LastHoveredItem = this;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (LastHoveredItem == this)
                LastHoveredItem = null;
        }
    }
}