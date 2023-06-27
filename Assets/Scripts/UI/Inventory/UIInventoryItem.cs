using DownBelow.Entity;
using DownBelow.Events;
using DownBelow.Inventory;
using DownBelow.Managers;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

namespace DownBelow.UI.Inventory
{
    public class UIInventoryItem : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public static UIInventoryItem LastHoveredItem;

        public Tooltipable Tooltipable;

        [FoldoutGroup("Facultative parameters")]
        public ItemPreset OnlyAcceptedItem = null;
        [FoldoutGroup("Facultative parameters")]
        public bool CanOnlyTake = false;


        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI quantity;
        [SerializeField] private Button selfButton;

        public int TotalQuantity = 0;
        public int Slot;
        public UIStorage SelfStorage;

        public InventoryItem SelfItem => this.SelfStorage.Storage.StorageItems[this.Slot];

        public Image SelectedImage;

        [HideInInspector] public ItemEventData Data;

        private Transform _parentAfterDrag;
        private Vector3 _positionAfterDrag;

        protected virtual bool DroppableOverUI => true;
        protected virtual bool DroppableOverWorld => true;

        private void Start()
        {
            this.RemoveItem();
        }

        public void Init(InventoryItem Item, UIStorage refStorage, int slot, bool OnlyTake)
        {
            this.gameObject.SetActive(true);

            this.CanOnlyTake = OnlyTake;
            this.SelfStorage = refStorage;
            this.Slot = slot;

            if(this.SelfItem != null)
            {
                this.SelfItem.OnItemChanged -= RefreshItem;
            }
            this.SelfItem.OnItemChanged += RefreshItem;

            if (this.SelfItem.ItemPreset != null)
            {
                this.icon.sprite = Item.ItemPreset.InventoryIcon;
                this.icon.gameObject.SetActive(true);
                this.TotalQuantity = Item.Quantity;
                this.quantity.text = this.TotalQuantity.ToString();
            } 
            else
            {
                if (this.OnlyAcceptedItem != null)
                {
                    this.icon.sprite = this.OnlyAcceptedItem.InventoryIcon;
                    this.icon.gameObject.SetActive(true);

                    this.icon.color = new Color(this.icon.color.r, this.icon.color.g, this.icon.color.b, 0.1f);
                }
                else
                {
                    this.icon.gameObject.SetActive(false);
                }

                this.quantity.text = string.Empty;
                this.TotalQuantity = 0;
            }
        }

        public void RefreshItem(ItemEventData Data)
        {
            if (Data.ItemData.Quantity > 0)
            {
                this.Data = Data;
                this.icon.sprite = this.Data.ItemData.ItemPreset.InventoryIcon;
                this.icon.color = new Color(this.icon.color.r, this.icon.color.g, this.icon.color.b, 1f);

                if (!this.icon.gameObject.activeInHierarchy)
                    this.icon.gameObject.SetActive(true);
                this.TotalQuantity = this.Data.ItemData.Quantity;
                this.quantity.text = this.TotalQuantity.ToString();
   
            } 
            else
            {
                if(this.OnlyAcceptedItem != null)
                {
                    this.icon.sprite = this.OnlyAcceptedItem.InventoryIcon;
                    this.icon.gameObject.SetActive(true);

                    this.icon.color = new Color(this.icon.color.r, this.icon.color.g, this.icon.color.b, 0.1f);
                }
                else
                {
                    this.icon.sprite = null;
                    this.icon.gameObject.SetActive(false);
                }
                
                this.quantity.text = string.Empty;
                this.TotalQuantity = 0;
            }

            this.RefreshTooltipable();
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

        public void SelectedSlot(bool value)
        {
            SelectedImage.color = new Color(SelectedImage.color.r, SelectedImage.color.g, SelectedImage.color.b, value ? 1 : 0);
        }

        public void RemoveItem()
        {
            if(this.OnlyAcceptedItem == null)
            {
                this.icon.gameObject.SetActive(false);
            }
            this.quantity.text = string.Empty;
            this.TotalQuantity = 0;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (this.TotalQuantity == 0)
                return;

            this._positionAfterDrag = selfButton.transform.position;
            this._parentAfterDrag = selfButton.transform.parent;
            selfButton.transform.SetParent(UIManager.Instance.DragItemParent);
            selfButton.transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (this.TotalQuantity == 0)
                return;

            icon.raycastTarget = false;
            selfButton.image.raycastTarget = false;
            if (selfButton.targetGraphic != null)
                selfButton.targetGraphic.raycastTarget = false;
            selfButton.transform.position = Mouse.current.position.ReadValue();
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            if (this.TotalQuantity == 0)
                return;
            if (this.DroppableOverWorld && GridManager.Instance.LastHoveredCell != null && GridManager.Instance.LastHoveredCell.RefGrid == GameManager.SelfPlayer.EntityCell.RefGrid && GridManager.Instance.LastHoveredCell.Datas.state.HasFlag(GridSystem.CellState.Walkable))
            {
                this.dropOverWorld(eventData);
            } 
            else if (this.DroppableOverUI && LastHoveredItem != null && LastHoveredItem != this)
            {
                this.dropOverUI(eventData);
            }

            icon.raycastTarget = true;
            selfButton.image.raycastTarget = true;
            if (selfButton.targetGraphic != null)
                selfButton.targetGraphic.raycastTarget = true;
            selfButton.transform.position = this._positionAfterDrag;
            selfButton.transform.SetParent(this._parentAfterDrag);
        }
        protected virtual void dropOverUI(PointerEventData eventData)
        {
            if (LastHoveredItem && LastHoveredItem != this && LastHoveredItem.SelfItem.ItemPreset == null)
            {
                if (LastHoveredItem.OnlyAcceptedItem != null && LastHoveredItem.OnlyAcceptedItem != this.SelfItem.ItemPreset)
                    return;

                if (!LastHoveredItem.CanOnlyTake)
                {
                    var action = new DropItemAction(GameManager.RealSelfPlayer, LastHoveredItem.SelfStorage.Storage.RefCell);
                    action.Init(
                        this.SelfStorage.Storage.RefCell,
                        this.SelfItem.ItemPreset,
                        this.TotalQuantity,
                        true,
                        LastHoveredItem.Slot,
                        this.Slot
                     );

                    NetworkManager.Instance.EntityAskToBuffAction(action);
                }
            }
        }
        protected virtual void dropOverWorld(PointerEventData eventData)
        {
            if(this.SelfItem.ItemPreset is ToolItem)
            {
                var action = new DropItemAction(GameManager.RealSelfPlayer, GridManager.Instance.LastHoveredCell);
                action.Init(
                    null,
                    SelfItem.ItemPreset,
                    SelfItem.Quantity,
                    false);

                NetworkManager.Instance.EntityAskToBuffAction(action);
            }
        }

        public virtual void RefreshTooltipable()
        {
            if (this.Tooltipable != null)
            {
                if (this.SelfItem.ItemPreset == null)
                {
                    this.Tooltipable.enabled = false;
                }
                else
                {
                    this.Tooltipable.enabled = true;
                    this.Tooltipable.Text = this.SelfItem.ItemPreset.Description;
                    this.Tooltipable.Title = this.SelfItem.ItemPreset.ItemName;
                }
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

        private void OnDestroy()
        {
            if(this.SelfStorage != null && this.SelfStorage.Storage != null && this.SelfStorage.Storage.StorageItems.Length > 0)
            {
                this.SelfItem.OnItemChanged -= RefreshItem;
            }
        }
    }
}